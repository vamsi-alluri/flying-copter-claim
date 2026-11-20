using FlyingCopterClaim.Core;

namespace FlyingCopterClaim.Core.Tests;

/// <summary>Tests for the base rules agreed with Vamz. No timer, no score, whole tiles only.</summary>
public sealed class GameTests
{
    private static Game NewGame() => new(Levels.Level1());

    [Fact]
    public void StartsAtZeroPercentWithThreeLives()
    {
        var game = NewGame();

        Assert.Equal(0, game.FilledPercent);
        Assert.Equal(3, game.Lives);
        Assert.Equal(GameState.Playing, game.State);
        Assert.Equal(new Position(20, 23), game.Copter.Position);
    }

    [Fact]
    public void TrailAloneClaimsNothing()
    {
        var game = NewGame();

        game.Step(Direction.Up);
        game.Step(Direction.Up);

        Assert.Equal(0, game.FilledPercent);
        Assert.Equal(2, game.Trail.Count);
        Assert.Equal(Tile.Trail, game.Board.GetTile(new Position(20, 22)));
    }

    [Fact]
    public void ClosingALoopClaimsWholeTilesAndRaisesIntegerPercent()
    {
        var game = NewGame();

        // Rectangle: left 10 along the border, up 4, right 10, down 4,
        // back onto the bottom border. (Avoids the ball at (20, 18).)
        foreach (var _ in Enumerable.Range(0, 10)) game.Step(Direction.Left);
        foreach (var _ in Enumerable.Range(0, 4)) game.Step(Direction.Up);
        foreach (var _ in Enumerable.Range(0, 10)) game.Step(Direction.Right);
        foreach (var _ in Enumerable.Range(0, 4)) game.Step(Direction.Down);

        Assert.True(game.Trail.IsEmpty);
        Assert.True(game.Board.ClaimedTiles > 0);
        // Interior of the loop (9 x 3 = 27) + the 17 trail tiles = 44 tiles.
        Assert.Equal(44, game.Board.ClaimedTiles);
        // Integer percent only: 44 * 100 / 801 = 5 (no decimals).
        Assert.Equal(5, game.FilledPercent);
        Assert.Equal(GameState.Playing, game.State);
    }

    [Fact]
    public void CopterCannotWalkThroughWalls()
    {
        var game = NewGame();

        // Walk left along the border to x=13, then up the x=13 column:
        // the wall segment at x=13, y=4..11 blocks the copter at y=12.
        foreach (var _ in Enumerable.Range(0, 7)) game.Step(Direction.Left); // to (13, 23)
        foreach (var _ in Enumerable.Range(0, 15)) game.Step(Direction.Up);

        Assert.Equal(new Position(13, 12), game.Copter.Position);
        var before = game.Copter.Position;
        game.Step(Direction.Up); // y=11, x=13 is a wall

        Assert.Equal(before, game.Copter.Position);
    }

    [Fact]
    public void EnemyBouncesOffClaimedTerritoryInsteadOfEnteringIt()
    {
        var board = new Board(10, 8, Array.Empty<Position>());
        var ball = new Enemy(EnemyKind.Ball, new Position(5, 5), new Position(0, 1));

        // Ball moves down to y=6, then the border (claimed) at y=7 blocks it.
        Assert.Equal(EnemyMoveResult.Moved, ball.Move(board, new Position(1, 1)));
        Assert.Equal(new Position(5, 6), ball.Position);
        ball.Move(board, new Position(1, 1));

        // It must not be on the claimed border row.
        Assert.NotEqual(7, ball.Position.Y);
    }

    [Fact]
    public void WalkingIntoAnEnemyCostsALifeAndRespawnsTheCopter()
    {
        var game = NewGame();
        var enemy = game.Enemies.First(e => e.Position == new Position(20, 18));

        // Walk straight up the x=20 column into that ball.
        foreach (var _ in Enumerable.Range(0, 5)) game.Step(Direction.Up);

        Assert.Equal(2, game.Lives);
        Assert.Equal(new Position(20, 23), game.Copter.Position);
        Assert.True(game.Trail.IsEmpty);
        Assert.Equal(GameState.Playing, game.State);
    }

    [Fact]
    public void LosingAllLivesEndsTheGame()
    {
        var game = NewGame();

        for (var i = 0; i < 3; i++)
        {
            foreach (var _ in Enumerable.Range(0, 5)) game.Step(Direction.Up);
        }

        Assert.Equal(0, game.Lives);
        Assert.Equal(GameState.GameOver, game.State);
    }

    [Fact]
    public void EnemyHittingTrailIsReportedAsHit()
    {
        var board = new Board(10, 8, Array.Empty<Position>());
        board.SetTile(new Position(5, 6), Tile.Trail);
        var ball = new Enemy(EnemyKind.Ball, new Position(5, 5), new Position(0, 1));

        Assert.Equal(EnemyMoveResult.HitCopterOrTrail, ball.Move(board, new Position(1, 1)));
    }

    [Fact]
    public void PercentageIsAlwaysAWholeNumber()
    {
        var game = NewGame();

        // No floating-point percentage exists anywhere in the core:
        // ClaimedPercent is an int, computed by integer division.
        Assert.IsType<int>(game.FilledPercent);
        Assert.Equal(0, game.FilledPercent);
    }
}
