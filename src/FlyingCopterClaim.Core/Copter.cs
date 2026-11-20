namespace FlyingCopterClaim.Core;

/// <summary>
/// The player's copter. It only knows where it is and where it started;
/// movement rules and claiming live in <see cref="Game"/> so they stay testable.
/// </summary>
public sealed class Copter
{
    public Copter(Position spawn)
    {
        Spawn = spawn;
        Position = spawn;
    }

    /// <summary>Where the copter respawns after losing a life.</summary>
    public Position Spawn { get; }

    public Position Position { get; private set; }

    public void MoveTo(Position position) => Position = position;

    public void Respawn() => Position = Spawn;
}
