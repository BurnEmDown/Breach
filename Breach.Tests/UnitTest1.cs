using Breach.Core;
using Breach.Core.Actions;

namespace Breach.Tests;

public class GameSetupTests
{
    [Fact]
    public void InitialState_OrangeDiagonal()
    {
        var state = GameSetup.CreateInitialState();
        Assert.Equal(TileColor.Orange, state.Board[0, 0]!.Primary);
        Assert.Equal(TileColor.Orange, state.Board[1, 1]!.Primary);
        Assert.Equal(TileColor.Orange, state.Board[2, 2]!.Primary);
    }

    [Fact]
    public void InitialState_GreenPositions()
    {
        var state = GameSetup.CreateInitialState();
        Assert.Equal(TileColor.Green, state.Board[0, 1]!.Primary);
        Assert.Equal(TileColor.Green, state.Board[0, 2]!.Primary);
        Assert.Equal(TileColor.Green, state.Board[1, 0]!.Primary);
    }

    [Fact]
    public void InitialState_PurplePositions()
    {
        var state = GameSetup.CreateInitialState();
        Assert.Equal(TileColor.Purple, state.Board[1, 2]!.Primary);
        Assert.Equal(TileColor.Purple, state.Board[2, 0]!.Primary);
        Assert.Equal(TileColor.Purple, state.Board[2, 1]!.Primary);
    }

    [Fact]
    public void InitialState_Player1AgentPositions()
    {
        var state = GameSetup.CreateInitialState();
        var p1 = state.Players[0];
        Assert.Contains(p1.Agents, a => a.Position == new Position(0, 0));
        Assert.Contains(p1.Agents, a => a.Position == new Position(2, 2));
    }

    [Fact]
    public void InitialState_Player2AgentPositions()
    {
        var state = GameSetup.CreateInitialState();
        var p2 = state.Players[1];
        Assert.Contains(p2.Agents, a => a.Position == new Position(0, 2));
        Assert.Contains(p2.Agents, a => a.Position == new Position(2, 0));
    }

    [Fact]
    public void InitialState_FirstPlayerHasOneAP()
    {
        var state = GameSetup.CreateInitialState();
        Assert.Equal(0, state.CurrentPlayerIndex);
        Assert.Equal(1, state.ActionPointsRemaining);
        Assert.True(state.IsFirstTurn);
    }
}

public class MoveActionTests
{
    private static GameEngine NewEngine() => new(GameSetup.CreateInitialState());

    [Fact]
    public void Move_ValidAdjacentTile_Succeeds()
    {
        var engine = NewEngine();
        // Player 1, agent 0 is at (0,0). Move right to (0,1).
        var result = engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 1)));
        Assert.True(result.IsSuccess);
        Assert.Equal(new Position(0, 1), engine.State.Players[0].Agents[0].Position);
    }

    [Fact]
    public void Move_ConsumesOneAP_ThenEndsTurn()
    {
        var engine = NewEngine();
        // First turn has 1 AP — a single move should end the turn.
        engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 1)));
        Assert.Equal(1, engine.State.CurrentPlayerIndex); // now Player 2's turn
        Assert.Equal(2, engine.State.ActionPointsRemaining);
    }

    [Fact]
    public void Move_NonAdjacentPosition_Fails()
    {
        var engine = NewEngine();
        var result = engine.Execute(new MoveAction(PlayerId.One, 0, new Position(2, 2)));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Move_OutOfBounds_Fails()
    {
        var engine = NewEngine();
        var result = engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 3)));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Move_WrongPlayer_Fails()
    {
        var engine = NewEngine();
        var result = engine.Execute(new MoveAction(PlayerId.Two, 0, new Position(0, 1)));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Move_OntoRivalAgent_CostsTwoAP()
    {
        // Advance past first turn so Player 2 has 2 AP.
        var state  = GameSetup.CreateInitialState();
        var engine = new GameEngine(state);

        // Player 1 uses their 1 AP; turn passes to Player 2 with 2 AP.
        engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 1)));
        Assert.Equal(PlayerId.Two, engine.State.CurrentPlayer.Id);
        Assert.Equal(2, engine.State.ActionPointsRemaining);

        // Player 2 agent 0 is at (0,2). Player 1 agent 0 (just moved to (0,1)).
        // Move P2/A0 left to (0,1) — occupied by P1 agent → costs 2 AP.
        // Spending all 2 AP ends the turn immediately (AP resets to 2 for the next player).
        var result = engine.Execute(new MoveAction(PlayerId.Two, 0, new Position(0, 1)));
        Assert.True(result.IsSuccess);
        // The 2-AP surcharge consumed all of P2's AP, so the turn advanced back to P1.
        Assert.Equal(PlayerId.One, engine.State.CurrentPlayer.Id);
    }
}

public class SwitchActionTests
{
    [Fact]
    public void Switch_SwapsTilesUnderAgents()
    {
        // Give Player 1 a full 2-AP turn by advancing past the first turn.
        var state  = GameSetup.CreateInitialState();
        var engine = new GameEngine(state);

        // Burn P1's first turn (1 AP).
        engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 1)));
        // Burn P2's turn.
        engine.Execute(new MoveAction(PlayerId.Two, 0, new Position(0, 1)));

        // Now P1 has 2 AP. Agents are at (0,1) and (2,2).
        // Record tiles before switch.
        var p1     = engine.State.Players[0];
        var pos0   = p1.Agents[0].Position;
        var pos1   = p1.Agents[1].Position;
        var tile0Before = engine.State.Board[pos0];
        var tile1Before = engine.State.Board[pos1];

        var result = engine.Execute(new SwitchAction(PlayerId.One));
        Assert.True(result.IsSuccess);
        Assert.Equal(tile0Before, engine.State.Board[pos1]);
        Assert.Equal(tile1Before, engine.State.Board[pos0]);
    }
}

public class OverrideActionTests
{
    [Fact]
    public void Override_SwapsBoardTileWithPlayerBoardTile()
    {
        // Skip first-turn constraint by advancing.
        var state  = GameSetup.CreateInitialState();
        var engine = new GameEngine(state);
        engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 1)));  // end P1 turn
        engine.Execute(new MoveAction(PlayerId.Two, 0, new Position(0, 1)));  // end P2 turn

        // P1 now has 2 AP. Agent 0 is at (0,1), player board slot 0 has an Orange tile.
        var p1          = engine.State.Players[0];
        var agentPos    = p1.Agents[0].Position;
        var boardBefore = engine.State.Board[agentPos];
        var pbBefore    = p1.Board[0];

        var result = engine.Execute(new OverrideAction(PlayerId.One, 0, 0));
        Assert.True(result.IsSuccess);
        Assert.Equal(pbBefore,    engine.State.Board[agentPos]);
        Assert.Equal(boardBefore, p1.Board[0]);
    }

    [Fact]
    public void Override_InvalidSlot_Fails()
    {
        var state  = GameSetup.CreateInitialState();
        var engine = new GameEngine(state);
        engine.Execute(new MoveAction(PlayerId.One, 0, new Position(0, 1)));
        engine.Execute(new MoveAction(PlayerId.Two, 0, new Position(0, 1)));

        var result = engine.Execute(new OverrideAction(PlayerId.One, 0, 5));
        Assert.False(result.IsSuccess);
    }
}
