using Breach.Core;
using Breach.Core.Actions;
using Breach.Cli;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== BREACH ===");
Console.WriteLine("Commands:  move <agent> <row>,<col>   |  switch   |  override <agent> <slot>   |  quit");
Console.WriteLine();

var state  = GameSetup.CreateInitialState();
var engine = new GameEngine(state);

while (true)
{
    Renderer.PrintState(state);

    Console.Write($"Player {(state.CurrentPlayerIndex + 1)} [{state.ActionPointsRemaining} AP] > ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;
    if (input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    var result = CommandParser.Parse(input, state.CurrentPlayer.Id);
    if (result.Action is null)
    {
        Console.WriteLine($"  ! {result.Error}");
        continue;
    }

    var actionResult = engine.Execute(result.Action);
    if (!actionResult.IsSuccess)
        Console.WriteLine($"  ! {actionResult.FailureReason}");
}

Console.WriteLine("Thanks for playing Breach!");
