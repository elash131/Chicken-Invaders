# Cluck Invaders

A 2D arcade fixed shooter. Your ship is pinned to the bottom of the screen and moves left and right
only. Above it a formation of chickens drifts sideways, steps down at each wall, and lays eggs.
Every chicken you shoot makes the rest of the flock faster. Clear four waves, then beat the Mother
Hen.

Final project for **Unity 101 for CS Students**, The Academic College of Tel-Aviv Yaffo.

---

## Requirements

| | |
|---|---|
| Unity | **6000.3.20f1** (Unity 6 LTS) |
| Render pipeline | URP 2D |
| Packages | Input System, TextMeshPro, Unity UI |
| Targets | Windows PC standalone only |

## How to run

1. Open the project folder with Unity Hub using editor version `6000.3.20f1`.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.

## Implementation status

Playable from menu to victory:

- Horizontal ship with banking poses, held fire on a cooldown, pooled Ion bullets.
- Four chicken formations (2–5 rows × 5) that fly in, sweep, step down at the walls and speed up
  with every kill. Only the lowest chicken in each column lays eggs, never too close to the ship.
- A chicken reaching the lose line just above the ship ends the run.
- Mother Hen after wave four: 30 health and a health bar. She fires aimed three-egg volleys, then
  enrages at half health and rains eggs. Her helmet cracks and then her armour breaks as she takes
  damage, and she goes out in a chain of blasts and one big explosion. Her health is a vertical bar
  on the right edge. Short camera cues mark her entrance, enrage and defeat.
- Chickens pop in a burst of feathers (a world-space particle system); Mother Hen sheds feathers on
  every hit and bursts into a feather cloud.
- The gameplay view is letterboxed to 16:9, so every resolution shows the same playfield.
- Three lives, 1.5 s respawn and 2.5 s of blinking protection, score and saved high score.
- Menu, wave intro, HUD, pause (also on focus loss), Game Over and Victory screens, restart.

- Music for menu, waves, boss and victory with crossfades, sound effects for every event, muffled
  music while paused, and a remembered mute toggle.

- **Feast Streak:** every chicken drops food, and fast kills upgrade it — drumstick, twin legs,
  roast, then a burger that stacks taller with every extra kill (50 up to 1500 points). Mother Hen
  bursts into a feast of burgers and roasts when she is defeated.

## Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | A / D or arrow keys | Left stick |
| Fire | Hold Space | Hold south button |
| Restart (result screen) | R | East button |
| Pause / resume | Escape | Start button |
| Mute / unmute | M | Select button |

Losing focus pauses an active run; returning to the window requires explicit resume. Gameplay input
is blocked in menus, pause, respawn and result screens. Between waves the ship can move and
collect food, but not shoot. Fire must be released after a UI transition
before shooting again. Game Over ignores restart for 0.5 seconds.

## Project structure

```
Assets/
  Animations/   Chicken flap clip and its controller
  Art/          Sprites and the tiled starfield background
  Audio/        Music and SFX (CC0 / CC-BY, see ASSETS.md)
  Config/       GameBalance, MotherHen, Audio and the four Wave assets
  Fonts/        Bungee (SIL OFL)
  Prefabs/      Boss, Chickens, Projectiles, UI (RunUI) and VFX
  Scenes/       SampleScene — the whole game lives in one scene
  Scripts/
    Core/       Singleton base class, IGameManager, GameState, constants
    Config/     GameBalanceConfig, WaveConfig, BossConfig, AudioConfig
    Gameplay/   Player, projectiles, chickens, boss and background
    Managers/   GameManager, WaveManager, pools, UI and presenters
Docs/
  GDD.md        Game design document
  ASSETS.md     Where every asset came from, and its licence
```

## Design notes

- `GameManager` owns the rules. Other systems report what happened through `IGameManager`, and the
  UI and presenters only listen to its events, so they cannot change score or lives by accident.
- One `Managers` object holds the focused managers. The UI and Mother Hen are prefabs placed in the
  scene with their references set in the Inspector.
- Only `GameManager` and `PlayerController` use the generic `Singleton<T>` base.
- `WaveManager` tracks the chickens it creates instead of searching the scene every frame.
- Player bullets, enemy eggs and food use separate `ObjectPool`s, prewarmed at load.
- Balance values live in ScriptableObjects, so tuning does not need a recompile.
- The background is a tiled sprite; artwork is never scaled non-uniformly.

## Assets

The sprites are third-party artwork from InterAction studios' *Chicken Invaders*, used for
coursework only and not presented as original work. Full per-file sources and the licence position
are in [`Docs/ASSETS.md`](Docs/ASSETS.md). Music and sound effects are free assets (CC0), except the
chicken sound by IMadeIt (CC-BY 3.0); sources and credits are in the same file.

## Testing

- [ ] Full run in the Editor: menu → four waves → Mother Hen → Victory → Play Again
- [ ] Game Over by losing all lives, and by letting the flock reach the lose line
- [ ] Pause with Escape and by switching windows; resume continues the same phase
- [ ] UI at 16:9, 4:3 and a tall window
- [ ] Windows standalone build runs outside Unity
- [ ] Gamepad controls

## Documentation

- [Game Design Document](Docs/GDD.md)
- [Asset sources and licences](Docs/ASSETS.md)
