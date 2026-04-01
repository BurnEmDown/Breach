using Breach.Core;

namespace Breach.Cli;

/// <summary>
/// Responsible for rendering the Breach game state in ASCII format to the console.
/// Displays the main 3×3 board (with agent positions and tile colors),
/// and both players' boards below it.
/// </summary>
internal static class Renderer
{
    /// <summary>
    /// Prints the complete game state to the console, including the main board
    /// and both player boards.
    /// </summary>
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

    /// <summary>
    /// Prints the 3×3 main board with column/row labels, tile colors,
    /// and agent markers.
    /// </summary>
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
                var s    = tile is null ? "   " : tile.Color.ToAbbrev();
                Console.Write($" {s,5} |");
            }
            Console.WriteLine();

            Console.WriteLine("  +-------+-------+-------+");
        }
    }

    private static string AgentString(GameState state, Position pos)
    {
        // Find all agents occupying this position
        var agents = state.Players
            .SelectMany((p, pi) => p.Agents.Select((a, ai) => (player: pi + 1, agentIdx: ai, agent: a)))
            .Where(x => x.agent.Position == pos)
            .ToList();

        // Return formatted string: empty if no agents, "P1A0" if one agent, etc.
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

    /// <summary>Prints both players' boards below the main board.</summary>
    private static void PrintPlayerBoards(GameState state)
    {
        Console.Write("Player 1 board:  ");
        PrintPlayerBoard(state.Players[0].Board);
        Console.Write("Player 2 board:  ");
        PrintPlayerBoard(state.Players[1].Board);
    }

    /// <summary>
    /// Prints a single player's board slots (0-2) showing tiles or empty placeholders.
    /// Format: "0:[O] 1:[ --- ] 2:[G]" etc.
    /// </summary>
    private static void PrintPlayerBoard(PlayerBoard board)
    {
        for (var i = 0; i < PlayerBoard.Size; i++)
        {
            var tile = board[i];
            var s    = tile is null ? "[ --- ]" : $"[{tile.Color.ToAbbrev()}]";
            Console.Write($" {i}:{s}");
        }
        Console.WriteLine();
    }
}
