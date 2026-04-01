using Breach.Core.Actions;

namespace Breach.Core;

/// <summary>
/// The heart of Breach's game logic. The GameEngine validates player actions,
/// applies them to the game state, manages action points, enforces turn structure,
/// and implements all gameplay rules (including the rival-agent surcharge).
/// 
/// Typical usage:
/// 1. Create a GameEngine with an initial GameState (from GameSetup.CreateInitialState())
/// 2. Call Execute() with each action
/// 3. Check the returned ActionResult for success or failure
/// 4. Repeat until game end (not yet implemented)
/// </summary>
public sealed class GameEngine
{
    /// <summary>The current game state, mutated as actions are executed.</summary>
    public GameState State { get; }

    /// <summary>
    /// Initializes the game engine with an initial state.
    /// </summary>
    /// <param name="initialState">The starting game state (usually from GameSetup).</param>
    public GameEngine(GameState initialState)
    {
        State = initialState;
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Executes a game action if it is valid and affordable in action points.
    /// On success, mutates the game state and may advance the turn.
    /// All core game rules (adjacency, surcharge, turn structure) are enforced here.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>An ActionResult indicating success or failure with a reason.</returns>
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

    /// <summary>
    /// Validates that the action is legal given the current game state.
    /// Checks: (1) it is the correct player's turn, (2) action-specific rules.
    /// </summary>
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

    /// <summary>
    /// Validates Move: target is in bounds, orthogonally adjacent,
    /// and not occupied by the same player's other agent.
    /// </summary>
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

    /// <summary>Validates Switch: always valid (no prerequisites).</summary>
    private static ActionResult ValidateSwitch(SwitchAction _) =>
        ActionResult.Success();

    /// <summary>
    /// Validates Override: agent index and slot are in range,
    /// and there is a tile under the agent to swap.
    /// </summary>
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

    /// <summary>
    /// Calculates the action point cost for an action.
    /// Most actions cost 1 AP, but Move onto a rival-occupied tile costs 2 AP (surcharge).
    /// </summary>
    private int GetActionCost(IGameAction action)
    {
        // Surcharge: if a Move ends on a tile with a rival agent, it costs 2 AP.
        if (action is MoveAction m && IsRivalOccupied(m.Target))
            return 2;

        return 1;
    }

    /// <summary>Returns true if a position is currently occupied by an opponent's agent.</summary>
    private bool IsRivalOccupied(Position pos)
    {
        return State.OpponentPlayer.Agents.Any(a => a.Position == pos);
    }

    // -----------------------------------------------------------------------
    // Application
    // -----------------------------------------------------------------------

    /// <summary>Applies the action's effects to the game state.</summary>
    private void Apply(IGameAction action)
    {
        switch (action)
        {
            case MoveAction m:     ApplyMove(m);     break;
            case SwitchAction s:   ApplySwitch(s);   break;
            case OverrideAction o: ApplyOverride(o); break;
        }
    }

    /// <summary>Moves the specified agent to the target position.</summary>
    private void ApplyMove(MoveAction m)
    {
        State.CurrentPlayer.Agents[m.AgentIndex].Position = m.Target;
    }

    /// <summary>Swaps the tiles under the current player's two agents.</summary>
    private void ApplySwitch(SwitchAction _)
    {
        var agents = State.CurrentPlayer.Agents;
        State.Board.Swap(agents[0].Position, agents[1].Position);
    }

    /// <summary>
    /// Swaps the tile under the specified agent with a tile on the player's board.
    /// </summary>
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

    /// <summary>
    /// Advances to the next player's turn. Clears the first-turn flag,
    /// switches players, and resets AP to 2 for all turns after the first.
    /// </summary>
    private void AdvanceTurn()
    {
        State.IsFirstTurn             = false;
        State.CurrentPlayerIndex      = State.CurrentPlayerIndex == 0 ? 1 : 0;
        State.ActionPointsRemaining   = 2;
    }
}
