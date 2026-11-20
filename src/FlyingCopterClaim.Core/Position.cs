namespace FlyingCopterClaim.Core;

/// <summary>A single tile position on the board grid. X is the column, Y is the row.</summary>
public readonly record struct Position(int X, int Y)
{
    public static Position operator +(Position position, Position offset) =>
        new(position.X + offset.X, position.Y + offset.Y);
}
