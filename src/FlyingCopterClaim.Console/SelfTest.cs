using FlyingCopterClaim.Core;

namespace FlyingCopterClaim.Console;

/// <summary>
/// Runs the same checks as the xUnit tests in
/// tests/FlyingCopterClaim.Core.Tests/GameTests.cs, but as plain code so the
/// game rules can be verified with a single command even where the vstest
/// test-host is slow to start (as in this VM):
///   dotnet run --project src/FlyingCopterClaim.Console -- --selftest
/// The xUnit tests remain the canonical test suite — run them with
/// `dotnet test` on a normal dev machine.
/// </summary>
public static class SelfTest
{
    public static int Run()
    {
        var checks = new List<(string Name, Func<bool> Check)>
        {
            ("Starts at 0% with 3 lives", () =>
            {
                var g = new Game(Levels.Level1());
                return g.FilledPercent == 0 && g.Lives == 3 && g.State == GameState.Playing
                    && g.Copter.Position == new Position(20, 23);
            }),
            ("Trail alone claims nothing", () =>
            {
                var g = new Game(Levels.Level1());
                g.Step(Direction.Up);
                g.Step(Direction.Up);
                return g.FilledPercent == 0 && g.Trail.Count == 2
                    && g.Board.GetTile(new Position(20, 22)) == Tile.Trail;
            }),
            ("Closing a loop claims 44 whole tiles = 5%", () =>
            {
                var g = new Game(Levels.Level1());
                foreach (var _ in Enumerable.Range(0, 10)) g.Step(Direction.Left);
                foreach (var _ in Enumerable.Range(0, 4)) g.Step(Direction.Up);
                foreach (var _ in Enumerable.Range(0, 10)) g.Step(Direction.Right);
                foreach (var _ in Enumerable.Range(0, 4)) g.Step(Direction.Down);
                return g.Trail.IsEmpty && g.Board.ClaimedTiles == 44 && g.FilledPercent == 5;
            }),
            ("Copter cannot walk through walls", () =>
            {
                var g = new Game(Levels.Level1());
                foreach (var _ in Enumerable.Range(0, 7)) g.Step(Direction.Left);
                foreach (var _ in Enumerable.Range(0, 15)) g.Step(Direction.Up);
                return g.Copter.Position == new Position(13, 12);
            }),
            ("Enemy bounces off claimed territory", () =>
            {
                var board = new Board(10, 8, Array.Empty<Position>());
                var ball = new Enemy(EnemyKind.Ball, new Position(5, 5), new Position(0, 1));
                ball.Move(board, new Position(1, 1));
                ball.Move(board, new Position(1, 1));
                return ball.Position.Y != 7;
            }),
            ("Walking into an enemy costs a life and respawns", () =>
            {
                var g = new Game(Levels.Level1());
                foreach (var _ in Enumerable.Range(0, 5)) g.Step(Direction.Up);
                return g.Lives == 2 && g.Copter.Position == new Position(20, 23)
                    && g.Trail.IsEmpty && g.State == GameState.Playing;
            }),
            ("Losing all 3 lives ends the game", () =>
            {
                var g = new Game(Levels.Level1());
                for (var i = 0; i < 3; i++)
                {
                    foreach (var _ in Enumerable.Range(0, 5)) g.Step(Direction.Up);
                }

                return g.Lives == 0 && g.State == GameState.GameOver;
            }),
            ("Enemy hitting a trail tile is a hit", () =>
            {
                var board = new Board(10, 8, Array.Empty<Position>());
                board.SetTile(new Position(5, 6), Tile.Trail);
                var ball = new Enemy(EnemyKind.Ball, new Position(5, 5), new Position(0, 1));
                return ball.Move(board, new Position(1, 1)) == EnemyMoveResult.HitCopterOrTrail;
            }),
        };

        var failures = 0;
        foreach (var (name, check) in checks)
        {
            bool passed;
            try
            {
                passed = check();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"FAIL: {name} — threw {ex.GetType().Name}: {ex.Message}");
                failures++;
                continue;
            }

            System.Console.WriteLine(passed ? $"PASS: {name}" : $"FAIL: {name}");
            if (!passed)
            {
                failures++;
            }
        }

        System.Console.WriteLine(failures == 0
            ? $"Self-test: all {checks.Count} checks passed."
            : $"Self-test: {failures} of {checks.Count} checks FAILED.");
        return failures == 0 ? 0 : 1;
    }
}
