namespace FlyingCopterClaim.Core;

/// <summary>A level definition: board size, walls, enemies, win target and lives.</summary>
public sealed record LevelDefinition(
    int Number,
    int Width,
    int Height,
    int TargetPercent,
    int StartingLives,
    Position CopterSpawn,
    IReadOnlyList<Position> Walls,
    IReadOnlyList<Enemy> Enemies);

/// <summary>
/// Level definitions. Level 1 is deliberately very basic: a 40x24 board,
/// three short interior walls forming chambers with wide gaps, three balls
/// and two slow mines. No timer, no score.
/// </summary>
public static class Levels
{
    public static LevelDefinition Level1()
    {
        const int width = 40;
        const int height = 24;

        var walls = new List<Position>();

        // Vertical wall, left chamber (gap above y=4 and below y=11).
        for (var y = 4; y <= 11; y++)
        {
            walls.Add(new Position(13, y));
        }

        // Horizontal wall across the middle (gap to its right).
        for (var x = 14; x <= 25; x++)
        {
            walls.Add(new Position(x, 12));
        }

        // Vertical wall, right chamber (gap above y=12 and below y=19).
        for (var y = 12; y <= 19; y++)
        {
            walls.Add(new Position(26, y));
        }

        // Short top wall, right side.
        for (var x = 27; x <= 33; x++)
        {
            walls.Add(new Position(x, 5));
        }

        var enemies = new List<Enemy>
        {
            new(EnemyKind.Ball, new Position(10, 6), new Position(1, 1)),
            new(EnemyKind.Ball, new Position(30, 8), new Position(-1, 1)),
            new(EnemyKind.Ball, new Position(20, 18), new Position(1, -1)),
            new(EnemyKind.Mine, new Position(8, 18), new Position(1, 0), moveEveryTicks: 2),
            new(EnemyKind.Mine, new Position(32, 16), new Position(-1, 0), moveEveryTicks: 2),
        };

        return new LevelDefinition(
            Number: 1,
            Width: width,
            Height: height,
            TargetPercent: 60,
            StartingLives: 3,
            CopterSpawn: new Position(20, height - 1),
            Walls: walls,
            Enemies: enemies);
    }
}
