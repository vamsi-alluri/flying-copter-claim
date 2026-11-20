namespace FlyingCopterClaim.Core;

public enum GameState
{
    Playing,
    Won,
    GameOver,
}

/// <summary>
/// The whole game loop in one testable class, with no console / UI code in it.
/// That separation is deliberate: this core can later be driven by a Blazor
/// WebAssembly page or a Godot C# scene without changing the rules.
///
/// Loop:
/// <list type="number">
///   <item>Copter moves one tile (<see cref="Step"/>). Leaving claimed territory
///   into empty tiles lays a trail; returning to claimed territory closes it.</item>
///   <item>Closing a trail claims the enclosed area (see <see cref="Board.ClaimEnclosed"/>).</item>
///   <item>Enemies move on <see cref="Tick"/> and bounce off walls and claimed tiles.</item>
///   <item>A ball/mine hitting the copter or its unfinished trail costs a life;
///   the trail is erased and the copter respawns. At 0 lives the game is over.</item>
///   <item>Reaching <see cref="LevelDefinition.TargetPercent"/> wins the level.</item>
/// </list>
/// Intentionally NOT in this version: timer, score, partial-tile coverage.
/// </summary>
public sealed class Game
{
    public Game(LevelDefinition level)
    {
        Level = level;
        Board = new Board(level.Width, level.Height, level.Walls);
        Copter = new Copter(level.CopterSpawn);
        Trail = new Trail();
        // Fresh enemy instances per game so a level definition stays reusable.
        Enemies = level.Enemies
            .Select(e => new Enemy(e.Kind, e.Spawn, e.Velocity, e.MoveEveryTicks))
            .ToList();
        Lives = level.StartingLives;
        State = GameState.Playing;
    }

    public LevelDefinition Level { get; }

    public Board Board { get; }

    public Copter Copter { get; }

    public Trail Trail { get; }

    public IReadOnlyList<Enemy> Enemies { get; }

    public int Lives { get; private set; }

    public GameState State { get; private set; }

    /// <summary>Whole-number fill percentage (see <see cref="Board.ClaimedPercent"/>).</summary>
    public int FilledPercent => Board.ClaimedPercent;

    /// <summary>Tiles claimed by the most recent close, 0 if the last move did not close anything.</summary>
    public int LastClaimedTiles { get; private set; }

    /// <summary>Move the copter one tile. Blocked by walls and the board edge.</summary>
    public void Step(Direction direction)
    {
        if (State != GameState.Playing)
        {
            return;
        }

        LastClaimedTiles = 0;
        var next = Copter.Position + direction.ToOffset();

        if (!Board.IsInside(next) || Board.GetTile(next) == Tile.Wall)
        {
            return;
        }

        // Walking into an enemy costs a life immediately.
        if (Enemies.Any(e => e.Position == next))
        {
            LoseLife();
            return;
        }

        var tile = Board.GetTile(next);

        if (tile == Tile.Claimed)
        {
            Copter.MoveTo(next);
            if (!Trail.IsEmpty)
            {
                LastClaimedTiles = Board.ClaimEnclosed(Enemies.Select(e => e.Position), Trail);
                Trail.Clear();
                if (Board.ClaimedPercent >= Level.TargetPercent)
                {
                    State = GameState.Won;
                }
            }

            return;
        }

        if (tile == Tile.Empty)
        {
            Copter.MoveTo(next);
            Board.SetTile(next, Tile.Trail);
            Trail.Add(next);
            return;
        }

        // Stepping back along the unfinished trail: just move, add nothing.
        Copter.MoveTo(next);
    }

    /// <summary>Advance the enemies one tick. Call this on a timer from the UI, not per keypress.</summary>
    public void Tick()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        foreach (var enemy in Enemies)
        {
            if (enemy.Move(Board, Copter.Position) == EnemyMoveResult.HitCopterOrTrail)
            {
                LoseLife();
                return;
            }
        }
    }

    private void LoseLife()
    {
        Lives--;
        Board.ClearTrail(Trail);
        Copter.Respawn();
        foreach (var enemy in Enemies)
        {
            enemy.Reset();
        }

        State = Lives <= 0 ? GameState.GameOver : GameState.Playing;
    }
}
