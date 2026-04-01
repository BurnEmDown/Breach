using Breach.Core;

namespace Breach.Cli;

internal static class Renderer
{
    public static void PrintState(GameState state)
    {
        Console.WriteLine();
        PrintBoard(state);
        Console.WriteLine();
        PrintPlayerBoards(state);
        Console.WriteLine();
    }

    // -----------------------------------------------------------------------
    // Main board
    // -----------------------------------------------------------------------

    private static void PrintBoard(GameState state)
    {
        Console.WriteLine("     0       1       2");
        Console.WriteLine("  +-------+-------+-------+");

        for (var row = 0; row < 3; row++)
        {
            // Agent row
            Console.Write($"{row} |");
            for (var col = 0; col < 3; col++)
            {
                var pos = new Position(row, col);
                Console.Write($" {AgentString(state, pos),5} |");
            }
            Console.WriteLine();

            // Tile color row
            Console.Write("  |");
            for (var col = 0; col < 3; col++)
            {
                var pos  = new Position(row, col);
                var tile = state.Board[pos];
                var s    = tile is null ? "   " : $"{tile.Primary.ToAbbrev()}{tile.Secondary1.ToAbbrev()}{tile.Secondary2.ToAbbrev()}";
                Console.Write($" {s,5} |");
            }
            Console.WriteLine();

            Console.WriteLine("  +-------+-------+-------+");
        }
    }

    private static string AgentString(GameState state, Position pos)
    {
        var agents = state.Players
            .SelectMany((p, pi) => p.Agents.Select((a, ai) => (player: pi + 1, agentIdx: ai, agent: a)))
            .Where(x => x.agent.Position == pos)
            .ToList();

        return agents.Count switch
        {
            0 => "     ",
            1 => $" P{agents[0].player}A{agents[0].agentIdx} ",
            _ => string.Join("", agents.Select(x => $"P{x.player}A{x.agentIdx}"))
        };
    }

    // -----------------------------------------------------------------------
    // Player boards
    // -----------------------------------------------------------------------

    private static void PrintPlayerBoards(GameState state)
    {
        Console.Write("Player 1 board:  ");
        PrintPlayerBoard(state.Players[0].Board);
        Console.Write("Player 2 board:  ");
        PrintPlayerBoard(state.Players[1].Board);
    }

    private static void PrintPlayerBoard(PlayerBoard board)
    {
        for (var i = 0; i < PlayerBoard.Size; i++)
        {
            var tile = board[i];
            var s    = tile is null ? "[ --- ]" : $"[{tile.Primary.ToAbbrev()}{tile.Secondary1.ToAbbrev()}{tile.Secondary2.ToAbbrev()}]";
            Console.Write($" {i}:{s}");
        }
        Console.WriteLine();
    }
}
