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
  with every kill. Each wave starts faster and lays more eggs than the last. Only the lowest
  chicken in each column lays eggs, never too close to the ship.
- Dive bombers from wave 2: a chicken wobbles as a warning, then swoops at the ship, drops an aimed
  egg and flies back into the formation.
- Gift boxes: catch one for 8 s of Spread, Lightning (piercing) or Fireball, or a 12 s shield
  bubble that absorbs one hit. Contents come from a shuffled bag, so every run differs. Each weapon is a `WeaponConfig` asset.
- A chicken reaching the lose line just above the ship ends the run.
- Mother Hen after wave four: 75 health and a health bar. She fires aimed five-egg volleys and
  charged rings of eggs, then enrages at half health, rains eggs and calls a row of chicks that
  shield her and dive-bomb the ship. Her helmet cracks and then her armour breaks as she takes
  damage, and she goes out in a chain of blasts and one big explosion. Her health is a vertical bar
  on the right edge. Short camera cues mark her entrance, enrage and defeat.
- Chickens pop in a burst of feathers (a world-space particle system); Mother Hen sheds feathers on
  every hit and bursts into a feather cloud.
- The gameplay view is letterboxed to 16:9, so every resolution shows the same playfield.
- Three lives, 1.5 s respawn and 2.5 s of blinking protection, score and saved high score.
- Menu with How To Play and Quit, and a living background of drifting chickens; wave intro, HUD,
  pause (also on focus loss), Game Over and Victory screens with fade-in transitions, restart.

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
  Art/          Art made for this project: feather and shield bubble
  Audio/        Audio made for this project: the boss siren
  Config/       ScriptableObjects: GameBalance, MotherHen, Food, Gifts, Audio,
                Waves/ (four waves + boss escort) and Weapons/ (Ion, Spread, Lightning, Fireball)
  Prefabs/      Boss, Chickens, Food, Menu (attract mode), Projectiles, UI (RunUI) and VFX
  Scenes/       SampleScene — the whole game lives in one scene
  Scripts/
    Core/       Singleton<T>, IGameManager, IDamageable, TrackedPool<T>, Bezier, enums, constants
    Config/     The ScriptableObject types
    Managers/   GameManager, WaveManager, PickupManager, AudioManager and the bullet/egg pools
    Gameplay/   Player and weapons, chickens and their formation helpers, Mother Hen, projectiles, pickups
    Effects/    Visual feedback only: feathers, explosions, camera cues, letterbox, background, menu flock
    UI/         Menus, HUD, How To Play and presenters
  ThirdParty/   Everything not made for this project: Chicken Invaders and Starbound art, Kenney and
                OpenGameArt audio, the Bungee font (sources and licences in ASSETS.md)
Docs/
  GDD.md        Game design document
  ASSETS.md     Where every asset came from, and its licence
```

## Design notes

- `GameManager` owns the rules. Other systems report what happened through `IGameManager`, and the
  UI, audio and presenters only listen to its events, so they cannot change score or lives by accident.
- One `Managers` object holds the focused managers. The UI, Mother Hen and the menu flock are prefabs
  placed in the scene with their references set in the Inspector.
- `Singleton<T>` is used only for the four objects many scripts need: `GameManager`,
  `PlayerController`, `AudioManager` and `FeatherBursts`.
- `WaveManager` tracks the chickens it creates instead of searching the scene every frame, and
  announces each kill once (`OnChickenKilled`) for food and feathers to react to.
- Bullets, eggs and pickups each own a `TrackedPool<T>`: Unity's `ObjectPool<T>`, prewarmed at load,
  plus release-everything and a guard against releasing the same object twice.
- Everything tunable lives in ScriptableObjects, so balancing never needs a recompile.
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
