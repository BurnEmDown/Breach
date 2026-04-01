namespace Breach.Core;

public sealed class Agent
{
    public PlayerId Owner { get; }
    public Position Position { get; set; }

    public Agent(PlayerId owner, Position position)
    {
        Owner    = owner;
        Position = position;
    }

    public Agent Clone() => new(Owner, Position);
}
