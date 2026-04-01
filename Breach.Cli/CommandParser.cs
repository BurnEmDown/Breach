using Breach.Core;
using Breach.Core.Actions;

namespace Breach.Cli;

/// <summary>
/// Result of parsing a player's command-line input. Contains either a valid
/// <see cref="Action"/> ready for execution, or an <see cref="Error"/> message.
/// </summary>
internal sealed class ParseResult
{
    /// <summary>The parsed game action (null if parsing failed).</summary>
    public IGameAction? Action { get; }
    
    /// <summary>Error message if parsing failed (null if successful).</summary>
    public string?      Error  { get; }

    /// <summary>Constructor for a successful parse result.</summary>
    public ParseResult(IGameAction action) { Action = action; }
    
    /// <summary>Constructor for a failed parse result.</summary>
    public ParseResult(string error)       { Error  = error;  }
}

/// <summary>
/// Parses command-line input from the player and converts it into game actions.
/// Handles: move, switch, override commands with proper validation and error reporting.
/// </summary>
internal static class CommandParser
{
    /// <summary>
    /// Parses a command string entered by the active player.
    /// 
    /// Supported syntax:
    ///   move &lt;agentIndex&gt; &lt;row&gt;,&lt;col&gt;     — Move agent 0 or 1 to adjacent tile
    ///   switch                      — Swap tiles under both agents
    ///   override &lt;agentIndex&gt; &lt;slot&gt;     — Swap agent tile with player board
    /// 
    /// </summary>
    /// <param name="input">The raw command string.</param>
    /// <param name="player">The current player (for validation).</param>
    /// <returns>A ParseResult with either a valid action or an error message.</returns>
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

    /// <summary>Parses the "move" command: move &lt;agentIndex&gt; &lt;row&gt;,&lt;col&gt;</summary>
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

    /// <summary>Parses the "override" command: override &lt;agentIndex&gt; &lt;slot&gt;</summary>
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

    /// <summary>
    /// Attempts to parse a position string in "row,col" format (e.g., "1,2").
    /// Returns true if successful; false if format is invalid.
    /// </summary>
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
