using Breach.Core;
using Breach.Core.Actions;

namespace Breach.Cli;

internal sealed class ParseResult
{
    public IGameAction? Action { get; }
    public string?      Error  { get; }

    public ParseResult(IGameAction action) { Action = action; }
    public ParseResult(string error)       { Error  = error;  }
}

internal static class CommandParser
{
    /// <summary>
    /// Parses a command string entered by the current player.
    /// Supported syntax:
    ///   move &lt;agentIndex&gt; &lt;row&gt;,&lt;col&gt;
    ///   switch
    ///   override &lt;agentIndex&gt; &lt;slot&gt;
    /// </summary>
    public static ParseResult Parse(string input, PlayerId player)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return new ParseResult("Empty command.");

        return parts[0].ToLowerInvariant() switch
        {
            "move"     => ParseMove(parts, player),
            "switch"   => new ParseResult(new SwitchAction(player)),
            "override" => ParseOverride(parts, player),
            _          => new ParseResult($"Unknown command '{parts[0]}'. Use: move, switch, override, quit.")
        };
    }

    private static ParseResult ParseMove(string[] parts, PlayerId player)
    {
        if (parts.Length != 3)
            return new ParseResult("Usage: move <agent 0|1> <row>,<col>");

        if (!int.TryParse(parts[1], out var agentIdx) || agentIdx is < 0 or > 1)
            return new ParseResult("Agent index must be 0 or 1.");

        if (!TryParsePosition(parts[2], out var pos))
            return new ParseResult("Position must be row,col (e.g. 1,2).");

        return new ParseResult(new MoveAction(player, agentIdx, pos));
    }

    private static ParseResult ParseOverride(string[] parts, PlayerId player)
    {
        if (parts.Length != 3)
            return new ParseResult("Usage: override <agent 0|1> <slot 0-2>");

        if (!int.TryParse(parts[1], out var agentIdx) || agentIdx is < 0 or > 1)
            return new ParseResult("Agent index must be 0 or 1.");

        if (!int.TryParse(parts[2], out var slot) || slot is < 0 or >= PlayerBoard.Size)
            return new ParseResult($"Slot must be 0–{PlayerBoard.Size - 1}.");

        return new ParseResult(new OverrideAction(player, agentIdx, slot));
    }

    private static bool TryParsePosition(string s, out Position pos)
    {
        pos = default;
        var idx = s.IndexOf(',');
        if (idx < 0) return false;
        if (!int.TryParse(s[..idx], out var row)) return false;
        if (!int.TryParse(s[(idx + 1)..], out var col)) return false;
        pos = new Position(row, col);
        return true;
    }
}
