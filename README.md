# Flying Copter Claim

Vamz's first game — a territory-claiming copter game in **C#**, inspired by
the classic AirXonix / Xonix idea (fly, draw a trail, close it off, claim the
area inside) with **original** art, name, textures and levels. Nothing is
copied from AirXonix: no copied name, artwork, assets, or level layouts.

Local version first. The web version comes later via WebAssembly — this
project is **not** a JavaScript game.

## The game (base version)

- A big board: a raised platform with golden walls and interior walls that
  make chambers and narrow passages.
- Your copter flies around and leaves a pale trail. **Nothing is claimed
  while you are drawing** — only when the trail closes back onto claimed
  territory does the enclosed area flip to claimed texture.
- Only **whole tiles** count, and only when a tile is fully inside a closed
  area. There is no partial-tile coverage in this version.
- The fill percentage is a **whole integer only** — no decimals, no floating
  point in the display. It is `claimed tiles × 100 / claimable tiles`
  (integer division). The outer border is safe claimed ground, but it does
  **not** count toward the percentage, so a new level starts at 0%.
- **Level 1 (very basic): fill 60% to win, 3 hearts/lives.** Lose a life
  when a ball or mine hits the copter or its unfinished trail; the trail is
  erased and the copter respawns on the bottom border. At 0 lives, game over.
- Enemies: glossy bouncing balls (fast, diagonal) and spiky mine-like
  enemies (slower, orthogonal). They bounce off walls and claimed territory.

### Intentionally NOT included (yet)

- **No timer.**
- **No score.**
- **No partial tiles** — a tile is empty, trail, or claimed, nothing in between.
- Mines roaming claimed territory (in this version mines also roam the empty
  playfield, just slower) and extra levels — those are later steps.

## Project layout

```text
FlyingCopterClaim.sln
src/
  FlyingCopterClaim.Core/      # Game rules only — no console/UI code.
    Position.cs                # Grid position
    Direction.cs               # Copter movement directions
    Tile.cs                    # Empty / Wall / Claimed / Trail
    Board.cs                   # Grid, walls, flood-fill claiming, integer %
    Copter.cs                  # Copter position + spawn
    Trail.cs                   # The unfinished trail
    Enemy.cs                   # Balls + mines, bouncing, hit detection
    Level.cs                   # Level definitions (Level 1 lives here)
    Game.cs                    # The whole loop: move, close, claim, lives, win/lose
  FlyingCopterClaim.Console/   # Text front-end. Replaceable by Blazor/Godot later.
    Program.cs                 # Game loop, input, --demo / --help
    ConsoleRenderer.cs         # Draws the board as text
tests/
  FlyingCopterClaim.Core.Tests/  # xUnit tests for the core rules
    GameTests.cs
```

The split is deliberate for maintainability: `FlyingCopterClaim.Core` knows
nothing about the console, so the same rules can later be driven by a Blazor
WebAssembly page or a Godot C# scene without changing the game logic.

## Requirements

- .NET 8 SDK (`dotnet --version` should print `8.x`). On Ubuntu 24.04:
  `sudo apt-get install -y dotnet-sdk-8.0`

## Build, test, run

From the project root (`~/workspace/flying-copter-claim/`):

```bash
dotnet build FlyingCopterClaim.sln
dotnet test FlyingCopterClaim.sln
dotnet run --project src/FlyingCopterClaim.Console -- --selftest
dotnet run --project src/FlyingCopterClaim.Console
```

Note: in the small shared VM used to build this base, `dotnet test`
(xUnit/vstest) could not connect to its test host and aborted after 90–300s
without running any tests — an environment issue, not a test failure.
The same 8 checks are also runnable directly, with no test host involved:

```bash
dotnet run --project src/FlyingCopterClaim.Console -- --selftest
# Expected: "Self-test: all 8 checks passed."
```

Run `dotnet test` on a normal dev machine; it should work there.

Verify the claim loop without playing by hand (scripted rectangle,
left → up → right → down, closing back on the border):

```bash
dotnet run --project src/FlyingCopterClaim.Console -- --demo
# Expected: Filled: 5%, 44 of 801 claimable tiles, 3 lives left, State: Playing
```

## Controls

| Key | Action |
| --- | --- |
| Arrow keys or WASD | Move the copter one tile |
| R | Restart the level |
| Q / Esc | Quit |

Board symbols: `C` copter, `o` ball, `*` mine, `#` wall, `=` claimed,
`+` trail, `.` empty.

## How claiming works (for the next developer)

1. The copter leaves claimed ground → every empty tile it enters becomes a
   `Trail` tile.
2. It returns to a `Claimed` tile → `Board.ClaimEnclosed` runs.
3. `ClaimEnclosed` flood-fills from every enemy through **empty tiles only**
   (walls, claimed tiles and the trail are barriers). Empty tiles the flood
   fill cannot reach are enclosed, so they — plus the trail tiles — become
   claimed, whole tile by whole tile.
4. If an enemy's next step would land on the copter or a trail tile, that is
   a hit: lose a life, erase the trail, respawn the copter, reset the enemies.

## Next steps

1. Play the console version and tune Level 1: board size, wall layout,
   enemy count/speed, and whether 60% feels right.
2. Decide the look: original claimed texture, copter art, ball/mine designs
   (no AirXonix assets).
3. Add Level 2+ definitions in `Level.cs` (same `LevelDefinition` shape).
4. Later: partial-tile coverage (the edge case Vamz noted from the
   screenshot), timer/score if wanted, then a Blazor WebAssembly front-end
   reusing `FlyingCopterClaim.Core` unchanged.
5. GitHub: Vamz owns the repo (`vamsi-alluri/flying-copter-claim`). Do not
   push from here unless he asks — see the repo pact in memory: only the
   initial commit goes directly to `main`; after that, Issues + PRs.
