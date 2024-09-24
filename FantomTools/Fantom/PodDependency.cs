using System.Text;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a dependency for a pod
/// </summary>
/// <param name="Name">The name of the pod this pod depends on</param>
/// <param name="Constraints">The constraints of the dependency</param>
public readonly record struct PodDependency(string Name, params DependencyConstraint[] Constraints)
{
    /// <summary>
    /// Creates a dependency from a dependency string
    /// </summary>
    /// <param name="dependency">The dependency string</param>
    /// <returns>The dependency</returns>
    public static PodDependency Parse(string dependency)
    {
        var parser = new DependencyParser(dependency.Trim());
        return parser.Parse();
    }

    /// <summary>
    /// Converts a dependency to a dependency string
    /// </summary>
    /// <returns>The dependency string</returns>
    public override string ToString()
    {
        return $"{Name} {string.Join(", ", Constraints.Select(x => x.ToString()))}";
    }

    private class DependencyParser(string dependency)
    {
        private int _position = 0;
        private bool Ended => _position >= dependency.Length;
        private char Current => Ended ? '\0' : dependency[_position];

        public PodDependency Parse()
        {
            var name = Name();
            var constraints = new List<DependencyConstraint> { Constraint() };
            while (Current == ',')
            {
                Consume();
                ConsumeSpaces();
                constraints.Add(Constraint());
            }
            return new PodDependency(name, constraints.ToArray());
        }
        private string Name()
        {
            var s = new StringBuilder();
            while (Current != ' ')
            {
                if (Ended) throw new Exception("Unexpected end to dependency, expected space then version!");
                s.Append(Current);
                Consume();
            }
            ConsumeSpaces();
            if (Ended) throw new Exception("Unexpected end to dependency, expected version!");
            return s.ToString();
        }

        private DependencyConstraint Constraint()
        {
            var startVersion = Version();
            ConsumeSpaces();
            switch (Current)
            {
                case '+':
                    Consume();
                    ConsumeSpaces();
                    return new DependencyConstraint(startVersion, true);
                case '-':
                {
                    Consume();
                    ConsumeSpaces();
                    var endVersion = Version();
                    ConsumeSpaces();
                    return new DependencyConstraint(startVersion, endVersion);
                }
                default:
                    return new DependencyConstraint(startVersion);
            }
        }

        private Version Version()
        {
            var s = new StringBuilder();
            while (char.IsDigit(Current) || Current == '.')
            {
                s.Append(Current);
                Consume();
            }
            return new Version(s.ToString());
        }
        
        private void Consume()
        {
            if (!Ended) _position++;
        }

        private void ConsumeSpaces()
        {
            while (char.IsWhiteSpace(Current)) Consume();
        }
    }
}