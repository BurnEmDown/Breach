namespace Breach.Core;

/// <summary>
/// A goal tile containing a level and a color-pattern requirement. The pattern
/// matches anywhere on the main board based on relative offsets.
/// </summary>
public sealed class GoalTile
{
    public string Id { get; }
    public string Name { get; }
    public GoalLevel Level { get; }
    public IReadOnlyList<GoalRequirementCell> Requirements { get; }

    public GoalTile(string id, string name, GoalLevel level, IEnumerable<GoalRequirementCell> requirements)
    {
        Id = string.IsNullOrWhiteSpace(id)
            ? throw new ArgumentException("Goal id cannot be blank.", nameof(id))
            : id;

        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Goal name cannot be blank.", nameof(name))
            : name;

        Level = level;

        var requirementList = requirements?.ToArray()
            ?? throw new ArgumentNullException(nameof(requirements));

        GoalValidation.Validate(level, requirementList);
        Requirements = requirementList;
    }

    public override string ToString() => $"{Name} ({Level})";
}
