namespace FlyingCopterClaim.Core;

/// <summary>Four-way movement for the copter. Enemies use their own diagonal/orthogonal vectors.</summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public static class DirectionExtensions
{
    public static Position ToOffset(this Direction direction) => direction switch
    {
        Direction.Up => new Position(0, -1),
        Direction.Down => new Position(0, 1),
        Direction.Left => new Position(-1, 0),
        Direction.Right => new Position(1, 0),
        _ => new Position(0, 0),
    };
}
