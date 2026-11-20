namespace FlyingCopterClaim.Core;

/// <summary>Enemy kind. Balls are fast and diagonal; mines are slower and orthogonal.</summary>
public enum EnemyKind
{
    Ball,
    Mine,
}

/// <summary>What happened when an enemy tried to move.</summary>
public enum EnemyMoveResult
{
    Moved,
    Stayed,
    /// <summary>The enemy ran into the copter or its unfinished trail — a life is lost.</summary>
    HitCopterOrTrail,
}

/// <summary>
/// A bouncing enemy. Enemies roam empty tiles only: walls, claimed territory
/// and the trail are barriers they bounce off. (In this base version mines also
/// roam the empty playfield, just slower — mines roaming claimed territory is a
/// later-level idea, not Level 1.)
/// </summary>
public sealed class Enemy
{
    public Enemy(EnemyKind kind, Position spawn, Position velocity, int moveEveryTicks = 1)
    {
        Kind = kind;
        Spawn = spawn;
        Position = spawn;
        Velocity = velocity;
        MoveEveryTicks = Math.Max(1, moveEveryTicks);
    }

    public EnemyKind Kind { get; }

    public Position Spawn { get; }

    public Position Position { get; private set; }

    /// <summary>Step per move, e.g. (1, 1) for a diagonal ball, (1, 0) for a mine.</summary>
    public Position Velocity { get; private set; }

    /// <summary>Move once every N game ticks. Balls use 1, mines use 2.</summary>
    public int MoveEveryTicks { get; }

    private int _ticksSinceMove;

    /// <summary>Symbol used by the console renderer.</summary>
    public char Symbol => Kind == EnemyKind.Ball ? 'o' : '*';

    public void Reset()
    {
        Position = Spawn;
        _ticksSinceMove = 0;
    }

    public EnemyMoveResult Move(Board board, Position copterPosition)
    {
        _ticksSinceMove++;
        if (_ticksSinceMove < MoveEveryTicks)
        {
            return EnemyMoveResult.Stayed;
        }

        _ticksSinceMove = 0;

        var result = TryStep(board, copterPosition);
        if (result != EnemyMoveResult.Stayed)
        {
            // Moved, or hit the copter/trail (a life is lost) — never retry a hit.
            return result;
        }

        // Bounced off a wall/claimed tile: reverse direction and try once more.
        Velocity = new Position(-Velocity.X, -Velocity.Y);
        return TryStep(board, copterPosition);
    }

    private EnemyMoveResult TryStep(Board board, Position copterPosition)
    {
        var next = Position + Velocity;

        if (next == copterPosition)
        {
            return EnemyMoveResult.HitCopterOrTrail;
        }

        if (!board.IsInside(next))
        {
            return EnemyMoveResult.Stayed;
        }

        var tile = board.GetTile(next);
        if (tile == Tile.Trail)
        {
            return EnemyMoveResult.HitCopterOrTrail;
        }

        if (tile != Tile.Empty)
        {
            return EnemyMoveResult.Stayed;
        }

        Position = next;
        return EnemyMoveResult.Moved;
    }
}
