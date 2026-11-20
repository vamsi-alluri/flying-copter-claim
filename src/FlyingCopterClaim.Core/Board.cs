namespace FlyingCopterClaim.Core;

/// <summary>
/// The grid the game is played on.
///
/// Rules baked into this class (agreed with Vamz, Level 1):
/// <list type="bullet">
///   <item>The outer border is claimed territory. It is safe ground, but it does
///   NOT count toward the fill percentage — the percentage is only about the
///   interior playfield.</item>
///   <item>Interior walls are never claimable and block the copter and enemies.</item>
///   <item>Only whole tiles count. There is no partial-tile coverage in this
///   version; a tile is either empty, trail, or claimed.</item>
///   <item>The percentage is a whole integer: <c>claimed * 100 / claimable</c>,
///   using integer division. No decimals or floating point in the display.</item>
/// </list>
/// </summary>
public sealed class Board
{
    private readonly Tile[,] _tiles;

    public Board(int width, int height, IEnumerable<Position> interiorWalls)
    {
        if (width < 5 || height < 5)
        {
            throw new ArgumentException("Board must be at least 5x5 tiles.", nameof(width));
        }

        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        // Outer border: claimed safe ground.
        for (var x = 0; x < width; x++)
        {
            _tiles[x, 0] = Tile.Claimed;
            _tiles[x, height - 1] = Tile.Claimed;
        }

        for (var y = 0; y < height; y++)
        {
            _tiles[0, y] = Tile.Claimed;
            _tiles[width - 1, y] = Tile.Claimed;
        }

        foreach (var wall in interiorWalls)
        {
            if (!IsInside(wall) || IsBorder(wall))
            {
                throw new ArgumentException($"Wall position {wall} must be inside the border.", nameof(interiorWalls));
            }

            _tiles[wall.X, wall.Y] = Tile.Wall;
        }

        // Claimable = every interior tile that is not a wall. The border is
        // excluded on purpose, so the percentage starts at 0%.
        ClaimableTiles = 0;
        for (var x = 1; x < width - 1; x++)
        {
            for (var y = 1; y < height - 1; y++)
            {
                if (_tiles[x, y] != Tile.Wall)
                {
                    ClaimableTiles++;
                }
            }
        }
    }

    public int Width { get; }

    public int Height { get; }

    /// <summary>Interior tiles that can ever be claimed (excludes walls and the border).</summary>
    public int ClaimableTiles { get; }

    /// <summary>Interior tiles already claimed.</summary>
    public int ClaimedTiles { get; private set; }

    /// <summary>Whole-number fill percentage, 0–100. Integer division only.</summary>
    public int ClaimedPercent => ClaimableTiles == 0 ? 100 : ClaimedTiles * 100 / ClaimableTiles;

    public bool IsInside(Position position) =>
        position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;

    public bool IsBorder(Position position) =>
        position.X == 0 || position.Y == 0 || position.X == Width - 1 || position.Y == Height - 1;

    public Tile GetTile(Position position) => _tiles[position.X, position.Y];

    public void SetTile(Position position, Tile tile) => _tiles[position.X, position.Y] = tile;

    public bool IsEmpty(Position position) => IsInside(position) && GetTile(position) == Tile.Empty;

    /// <summary>
    /// Turns the trail and every empty area it encloses into claimed territory.
    /// An empty tile is enclosed when no enemy can reach it: flood-fill from
    /// every enemy through empty tiles only (walls, claimed tiles and the trail
    /// are barriers). Whatever the flood fill cannot reach is enclosed and is
    /// claimed, whole tile by whole tile. Trail tiles themselves become claimed.
    /// Returns the number of newly claimed tiles.
    /// </summary>
    public int ClaimEnclosed(IEnumerable<Position> enemyPositions, Trail trail)
    {
        var reachable = new bool[Width, Height];
        var queue = new Queue<Position>();

        foreach (var enemy in enemyPositions)
        {
            if (IsInside(enemy) && GetTile(enemy) == Tile.Empty && !reachable[enemy.X, enemy.Y])
            {
                reachable[enemy.X, enemy.Y] = true;
                queue.Enqueue(enemy);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var offset in new[] { new Position(1, 0), new Position(-1, 0), new Position(0, 1), new Position(0, -1) })
            {
                var next = current + offset;
                if (IsInside(next) && !reachable[next.X, next.Y] && GetTile(next) == Tile.Empty)
                {
                    reachable[next.X, next.Y] = true;
                    queue.Enqueue(next);
                }
            }
        }

        var claimed = 0;

        // Enclosed empty tiles: not reachable from any enemy.
        for (var x = 1; x < Width - 1; x++)
        {
            for (var y = 1; y < Height - 1; y++)
            {
                if (_tiles[x, y] == Tile.Empty && !reachable[x, y])
                {
                    _tiles[x, y] = Tile.Claimed;
                    ClaimedTiles++;
                    claimed++;
                }
            }
        }

        // The trail itself becomes claimed territory.
        foreach (var cell in trail.Cells)
        {
            if (IsInside(cell) && GetTile(cell) == Tile.Trail)
            {
                _tiles[cell.X, cell.Y] = Tile.Claimed;
                if (!IsBorder(cell))
                {
                    ClaimedTiles++;
                    claimed++;
                }
            }
        }

        return claimed;
    }

    /// <summary>Erases an unfinished trail (used when a life is lost).</summary>
    public void ClearTrail(Trail trail)
    {
        foreach (var cell in trail.Cells)
        {
            if (IsInside(cell) && GetTile(cell) == Tile.Trail)
            {
                _tiles[cell.X, cell.Y] = Tile.Empty;
            }
        }

        trail.Clear();
    }
}
