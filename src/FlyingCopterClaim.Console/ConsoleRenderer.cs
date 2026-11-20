using System.Text;
using FlyingCopterClaim.Core;

namespace FlyingCopterClaim.Console;

/// <summary>
/// Text rendering only. It reads the game state and draws it — it never
/// changes game rules. A graphical renderer (Blazor / Godot) can replace
/// this class without touching <see cref="Game"/>.
/// </summary>
public static class ConsoleRenderer
{
    public static void Render(Game game)
    {
        System.Console.SetCursorPosition(0, 0);
        var output = new StringBuilder();

        output.AppendLine("FLYING COPTER CLAIM  —  Level 1 (base version)");
        output.AppendLine($"Filled: {game.FilledPercent}%  (target {game.Level.TargetPercent}%)   " +
                          $"Lives: {new string('♥', Math.Max(0, game.Lives))}{new string('♡', Math.Max(0, game.Level.StartingLives - game.Lives))}   " +
                          $"Trail: {game.Trail.Count} tiles");
        output.AppendLine("Move: Arrow keys or WASD   |   R: restart   |   Q: quit");
        output.AppendLine();

        for (var y = 0; y < game.Board.Height; y++)
        {
            for (var x = 0; x < game.Board.Width; x++)
            {
                var position = new Position(x, y);
                output.Append(CharFor(game, position));
            }

            output.AppendLine();
        }

        output.AppendLine();
        output.AppendLine(game.State switch
        {
            GameState.Won => "LEVEL CLEAR! You reached the target. Press R to play again, Q to quit.",
            GameState.GameOver => "GAME OVER — out of lives. Press R to try again, Q to quit.",
            _ => "Draw a loop with your trail and close it on claimed ground to claim the area inside.",
        });

        System.Console.Write(output.ToString());
    }

    private static char CharFor(Game game, Position position)
    {
        if (game.Copter.Position == position)
        {
            return 'C';
        }

        var enemy = game.Enemies.FirstOrDefault(e => e.Position == position);
        if (enemy is not null)
        {
            return enemy.Symbol;
        }

        return game.Board.GetTile(position) switch
        {
            Tile.Wall => '#',
            Tile.Claimed => '=',
            Tile.Trail => '+',
            _ => '.',
        };
    }
}
