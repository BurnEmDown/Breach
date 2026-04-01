using Breach.Core.Actions;

namespace Breach.Core;

/// <summary>
/// Validates and applies player actions, manages action points, and advances turns.
/// </summary>
public sealed class GameEngine
{
    public GameState State { get; }

    public GameEngine(GameState initialState)
    {
        State = initialState;
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    public ActionResult Execute(IGameAction action)
    {
        var validation = Validate(action);
        if (!validation.IsSuccess)
            return validation;

        var cost = GetActionCost(action);

        if (State.ActionPointsRemaining < cost)
            return ActionResult.Failure(
                $"Not enough action points (have {State.ActionPointsRemaining}, need {cost}).");

        Apply(action);
        State.ActionPointsRemaining -= cost;

        if (State.ActionPointsRemaining == 0)
            AdvanceTurn();

        return ActionResult.Success();
    }

    // -----------------------------------------------------------------------
    // Validation
    // -----------------------------------------------------------------------

    private ActionResult Validate(IGameAction action)
    {
        if (action.Player != State.CurrentPlayer.Id)
            return ActionResult.Failure("It is not your turn.");

        return action switch
        {
            MoveAction m     => ValidateMove(m),
            SwitchAction s   => ValidateSwitch(s),
            OverrideAction o => ValidateOverride(o),
            _                => ActionResult.Failure("Unknown action type.")
        };
    }

    private ActionResult ValidateMove(MoveAction m)
    {
        if (m.AgentIndex is < 0 or > 1)
            return ActionResult.Failure("Agent index must be 0 or 1.");

        if (!m.Target.IsValid())
            return ActionResult.Failure($"Target position {m.Target} is out of bounds.");

        var agent = State.CurrentPlayer.Agents[m.AgentIndex];
        if (!agent.Position.IsOrthogonallyAdjacentTo(m.Target))
            return ActionResult.Failure(
                $"Position {m.Target} is not orthogonally adjacent to {agent.Position}.");

        // An agent cannot move to a position occupied by its own other agent.
        var otherAgent = State.CurrentPlayer.Agents[m.AgentIndex == 0 ? 1 : 0];
        if (otherAgent.Position == m.Target)
            return ActionResult.Failure("Cannot move onto a tile occupied by your own agent.");

        return ActionResult.Success();
    }

    private static ActionResult ValidateSwitch(SwitchAction _) =>
        // Switch is always valid as long as it is the player's turn (already checked).
        ActionResult.Success();

    private ActionResult ValidateOverride(OverrideAction o)
    {
        if (o.AgentIndex is < 0 or > 1)
            return ActionResult.Failure("Agent index must be 0 or 1.");

        if (o.PlayerBoardSlot is < 0 or >= PlayerBoard.Size)
            return ActionResult.Failure(
                $"Player board slot must be 0–{PlayerBoard.Size - 1}.");

        var agentPos = State.CurrentPlayer.Agents[o.AgentIndex].Position;
        if (State.Board[agentPos] is null)
            return ActionResult.Failure("There is no tile under the agent to override.");

        return ActionResult.Success();
    }

    // -----------------------------------------------------------------------
    // Cost
    // -----------------------------------------------------------------------

    private int GetActionCost(IGameAction action)
    {
        // Surcharge: if a Move ends on a tile with a rival agent, it costs 2 AP.
        if (action is MoveAction m && IsRivalOccupied(m.Target))
            return 2;

        return 1;
    }

    private bool IsRivalOccupied(Position pos)
    {
        return State.OpponentPlayer.Agents.Any(a => a.Position == pos);
    }

    // -----------------------------------------------------------------------
    // Application
    // -----------------------------------------------------------------------

    private void Apply(IGameAction action)
    {
        switch (action)
        {
            case MoveAction m:     ApplyMove(m);     break;
            case SwitchAction s:   ApplySwitch(s);   break;
            case OverrideAction o: ApplyOverride(o); break;
        }
    }

    private void ApplyMove(MoveAction m)
    {
        State.CurrentPlayer.Agents[m.AgentIndex].Position = m.Target;
    }

    private void ApplySwitch(SwitchAction _)
    {
        var agents = State.CurrentPlayer.Agents;
        State.Board.Swap(agents[0].Position, agents[1].Position);
    }

    private void ApplyOverride(OverrideAction o)
    {
        var agent     = State.CurrentPlayer.Agents[o.AgentIndex];
        var playerBoard = State.CurrentPlayer.Board;

        var boardTile       = State.Board[agent.Position];
        var playerBoardTile = playerBoard[o.PlayerBoardSlot];

        State.Board[agent.Position]       = playerBoardTile;
        playerBoard[o.PlayerBoardSlot]    = boardTile;
    }

    // -----------------------------------------------------------------------
    // Turn advancement
    // -----------------------------------------------------------------------

    private void AdvanceTurn()
    {
        State.IsFirstTurn             = false;
        State.CurrentPlayerIndex      = State.CurrentPlayerIndex == 0 ? 1 : 0;
        State.ActionPointsRemaining   = 2;
    }
}
