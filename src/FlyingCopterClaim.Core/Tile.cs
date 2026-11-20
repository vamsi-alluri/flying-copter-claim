namespace FlyingCopterClaim.Core;

/// <summary>
/// What a board tile currently is.
/// <list type="bullet">
///   <item><see cref="Empty"/> — open playfield, can be claimed.</item>
///   <item><see cref="Wall"/> — border or interior wall; never claimable, blocks copter and enemies.</item>
///   <item><see cref="Claimed"/> — already owned territory; safe for the copter, blocks enemies.</item>
///   <item><see cref="Trail"/> — the copter's unfinished pale line; not claimed yet.</item>
/// </list>
/// </summary>
public enum Tile
{
    Empty,
    Wall,
    Claimed,
    Trail,
}
