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
| Targets | Windows standalone, Android |

## How to run

1. Open the project folder with Unity Hub using editor version `6000.3.20f1`.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.

## Controls

| Action | Keyboard | Gamepad | Android |
|---|---|---|---|
| Move | A / D or arrow keys | Left stick | On-screen stick |
| Fire | Hold Space | Hold south button | On-screen button |
| Restart | R on the result screen | East button | Play Again |

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
    Gameplay/   PlayerController, ScrollingBackground
    Managers/   GameManager
Docs/
  GDD.md        Game design document
  ASSETS.md     Where every asset came from, and its licence
```

## Design notes

- `GameManager` owns the rules. UI and audio only listen to its events, so they cannot change score
  or lives by accident.
- Managers derive from a generic `Singleton<T>` base class.
- Enemies register themselves with their manager instead of the manager searching the scene.
- Balance values live in ScriptableObjects, so tuning does not need a recompile.
- The background is a **tiled** sprite whose Size is a whole number of tiles. Artwork is never
  scaled non-uniformly.

## Assets

The sprites are third-party artwork from InterAction studios' *Chicken Invaders*, used for
coursework only and not presented as original work. Full per-file sources and the licence position
are in [`Docs/ASSETS.md`](Docs/ASSETS.md).

## Documentation

- [Game Design Document](Docs/GDD.md)
- [Asset sources and licences](Docs/ASSETS.md)
