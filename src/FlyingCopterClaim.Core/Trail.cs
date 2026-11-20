namespace FlyingCopterClaim.Core;

/// <summary>
/// The copter's unfinished trail: the pale line drawn while it is away from
/// claimed territory. Nothing in the trail counts toward the fill percentage
/// until the trail closes back onto claimed territory and <see cref="Board.ClaimEnclosed"/>
/// turns it (and the area it encloses) into claimed tiles.
/// </summary>
public sealed class Trail
{
    private readonly HashSet<Position> _cells = new();

    public int Count => _cells.Count;

    public bool IsEmpty => _cells.Count == 0;

    public IReadOnlyCollection<Position> Cells => _cells;

    public bool Contains(Position position) => _cells.Contains(position);

    public bool Add(Position position) => _cells.Add(position);

    public void Clear() => _cells.Clear();
}
