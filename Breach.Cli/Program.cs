using Breach.Core;
using Breach.Core.Actions;
using Breach.Cli;

// Setup
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== BREACH ===");
Console.WriteLine("A 2-player abstract strategy board game.");
Console.WriteLine();
Console.WriteLine("Commands:");
Console.WriteLine("  move <agent 0|1> <row>,<col>   — Move an agent orthogonally");
Console.WriteLine("  switch                         — Swap tiles under both agents");
Console.WriteLine("  override <agent 0|1> <slot 0-2> — Swap agent tile with player board");
Console.WriteLine("  quit                           — Exit game");
Console.WriteLine();

// Initialize game
var state  = GameSetup.CreateInitialState();
var engine = new GameEngine(state);

// Main game loop
while (true)
{
    Renderer.PrintState(state);

    Console.Write($"Player {(state.CurrentPlayerIndex + 1)} [{state.ActionPointsRemaining} AP] > ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;
    if (input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    // Parse and execute the command
    var result = CommandParser.Parse(input, state.CurrentPlayer.Id);
    if (result.Action is null)
    {
        Console.WriteLine($"  ! {result.Error}");
        continue;
    }

    // Execute the action and report the result
    var actionResult = engine.Execute(result.Action);
    if (!actionResult.IsSuccess)
        Console.WriteLine($"  ! {actionResult.FailureReason}");
}

Console.WriteLine();
Console.WriteLine("Thanks for playing Breach!");
