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

The prototype includes movement/banking, scrolling background, pooled Ion bullets, and four
chicken formations with entry animations, descent and speed increases per kill. An explicit run
state controls input, damage, wave progression, pause, respawn and result transitions. A simple
UGUI/TMP interface provides Play, Resume, results and a return to the menu.
It uses the existing purple/white title logo, Bungee for all menu and HUD text, and gold/violet buttons.
`Assets/Resources/RunUI.prefab` contains the authored Canvas and serialized UI references, following
the same pattern as the course examples. It is instantiated on entering Play Mode, so the scene is untouched.

Egg attacks, the lose-line trigger, boss gameplay, audio and final presentation are still planned.
After wave four, the prototype enters BossFight and displays a placeholder with a Main menu button;
it does not award victory. Damage/lose-line/boss-defeat commands are ready for those future systems.
The respawn flow includes 1.5 seconds off-screen and 2.5 seconds of protection, but no enemy currently
triggers player damage during normal gameplay.

## Controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | A / D or arrow keys | Left stick |
| Fire | Hold Space | Hold south button |
| Restart (result screen) | R | East button |
| Pause / resume | Escape | Start button |

Losing focus pauses an active run; returning to the window requires explicit resume. Gameplay input
is blocked in menus, intros, pause, respawn and result screens. Fire must be released after a UI
transition before shooting again. Game Over ignores restart for 0.5 seconds.

## Project structure

```
Assets/
  Animations/   Chicken flap clip and its controller
  Art/          Sprites and the tiled starfield background
  Editor/       Editor tools that build the scene and rebuild animation clips
  Prefabs/      Chicken prefab and its colour variants
  Scenes/       SampleScene — the whole game lives in one scene
  Scripts/
    Core/       Singleton base class, interfaces, constants
    Config/     GameBalanceConfig, WaveConfig
    Gameplay/   Player, projectiles, chickens, factory and background
    Managers/   GameManager, WaveManager, ProjectilePool, GameUIManager
Docs/
  GDD.md        Game design document
  ASSETS.md     Where every asset came from, and its licence
```

## Design notes

- `GameManager` owns the rules. UI and audio only listen to its events, so they cannot change score
  or lives by accident.
- Only GameManager and PlayerController use the generic `Singleton<T>` base; other systems use references.
- WaveManager tracks the chickens it creates instead of searching the scene every frame.
- Balance values live in ScriptableObjects, so tuning does not need a recompile.
- The background is a **tiled** sprite whose Size is a whole number of tiles. Artwork is never
  scaled non-uniformly.

## Assets

The sprites are third-party artwork from InterAction studios' *Chicken Invaders*, used for
coursework only and not presented as original work. Full per-file sources and the licence position
are in [`Docs/ASSETS.md`](Docs/ASSETS.md).

## Validation (2026-09-30)

- An isolated Unity Editor copy passed 51 automated run-state and serialized-UI checks, including pause, respawn,
  four-wave progression, result commands, menu return and five retries. The original scene was not changed.
- Menu renders were visually checked at 1080x1920, 1920x1080 and 1024x768.
- Damage and boss defeat were invoked through integration commands: egg attacks and the boss are
  not implemented yet. Physical gamepad input, a Windows player build and profiling remain untested.
- The batch Editor emitted a separate search-index exception; the gameplay validation completed successfully.

## Documentation

- [Game Design Document](Docs/GDD.md)
- [Asset sources and licences](Docs/ASSETS.md)
