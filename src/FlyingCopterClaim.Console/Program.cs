using FlyingCopterClaim.Core;

namespace FlyingCopterClaim.Console;

/// <summary>
/// Console front-end for Flying Copter Claim (base version).
/// Usage:
///   dotnet run --project src/FlyingCopterClaim.Console
///   dotnet run --project src/FlyingCopterClaim.Console -- --demo
///   dotnet run --project src/FlyingCopterClaim.Console -- --selftest
///   dotnet run --project src/FlyingCopterClaim.Console -- --help
/// --demo plays a scripted rectangle (up, left, down, back to the border)
/// and prints the board and the resulting percentage, so the claim loop can
/// be verified without playing by hand.
/// </summary>
public static class Program
{
    private const int TickMilliseconds = 180;

    public static int Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Contains("--help") || args.Contains("-h"))
        {
            System.Console.WriteLine("Flying Copter Claim — base version");
            System.Console.WriteLine("  Run the game:  dotnet run --project src/FlyingCopterClaim.Console");
            System.Console.WriteLine("  Scripted demo: dotnet run --project src/FlyingCopterClaim.Console -- --demo");
            System.Console.WriteLine("  Self-test:     dotnet run --project src/FlyingCopterClaim.Console -- --selftest");
            System.Console.WriteLine("  Controls: Arrow keys / WASD to move, R to restart, Q to quit.");
            System.Console.WriteLine("  Goal: fill 60% of the board. You have 3 lives. No timer, no score (yet).");
            return 0;
        }

        if (args.Contains("--demo"))
        {
            return RunDemo();
        }

        if (args.Contains("--selftest"))
        {
            return SelfTest.Run();
        }

        return RunInteractive();
    }

    private static int RunDemo()
    {
        var game = new Game(Levels.Level1());

        // Scripted loop, starting at the bottom border (20, 23):
        // left 10 along the border, up 4, right 10, down 4 — back onto the
        // border, closing the loop. (Deliberately avoids the ball at (20, 18).)
        var moves = new List<Direction>();
        moves.AddRange(Enumerable.Repeat(Direction.Left, 10));
        moves.AddRange(Enumerable.Repeat(Direction.Up, 4));
        moves.AddRange(Enumerable.Repeat(Direction.Right, 10));
        moves.AddRange(Enumerable.Repeat(Direction.Down, 4));

        foreach (var move in moves)
        {
            game.Step(move);
        }

        ConsoleRenderer.Render(game);
        System.Console.WriteLine();
        System.Console.WriteLine($"Demo finished. State: {game.State}");
        System.Console.WriteLine($"Claimed tiles: {game.Board.ClaimedTiles} of {game.Board.ClaimableTiles} claimable");
        System.Console.WriteLine($"Filled: {game.FilledPercent}% (integer percent, whole tiles only)");
        System.Console.WriteLine($"Lives left: {game.Lives}");
        return game.FilledPercent > 0 ? 0 : 1;
    }

    private static int RunInteractive()
    {
        var game = new Game(Levels.Level1());
        System.Console.CursorVisible = false;
        System.Console.Clear();

        try
        {
            while (true)
            {
                ConsoleRenderer.Render(game);

                var deadline = DateTime.UtcNow.AddMilliseconds(TickMilliseconds);
                while (DateTime.UtcNow < deadline)
                {
                    if (!System.Console.KeyAvailable)
                    {
                        Thread.Sleep(20);
                        continue;
                    }

                    var key = System.Console.ReadKey(intercept: true).Key;
                    switch (key)
                    {
                        case ConsoleKey.Q:
                        case ConsoleKey.Escape:
                            return 0;
                        case ConsoleKey.R:
                            game = new Game(Levels.Level1());
                            System.Console.Clear();
                            break;
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.W:
                            game.Step(Direction.Up);
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                            game.Step(Direction.Down);
                            break;
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.A:
                            game.Step(Direction.Left);
                            break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.D:
                            game.Step(Direction.Right);
                            break;
                    }

                    ConsoleRenderer.Render(game);
                }

                // Enemies move on their own timer, independent of keypresses.
                game.Tick();
            }
        }
        finally
        {
            System.Console.CursorVisible = true;
            System.Console.WriteLine();
        }
    }
}
