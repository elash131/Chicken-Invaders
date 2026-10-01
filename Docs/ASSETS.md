# Assets — sources, licences and import settings

The single record of where every asset came from. Referenced from [`GDD.md`](GDD.md) §6.

---

## Licence position

**Every sprite in this project is ripped artwork from InterAction studios' commercial *Chicken
Invaders***, obtained from The Spriters Resource and the game's fan wiki. Per-file sources are in
the tables below.

Fandom's wiki *text* is CC-BY-SA; the uploaded game images are **not** — they carry **no reuse
licence**, and neither do sprite-rip archives. They are used here for **coursework only**, declared
as third-party rather than presented as licensed. **No public build may ship them.**

**Replacement plan.** For any public release, swap in CC0 art and rename the game. Kenney's
[Space Shooter Redux](https://kenney.nl/assets/space-shooter-redux) (CC0) covers the ship,
projectiles and UI directly; [Chicken Sprites by Shepardskin](https://opengameart.org/content/chicken-sprites)
(CC0) covers the enemies. Every sprite is referenced through a prefab, scene or config field,
so this is an asset swap, not a code change.

**Open question for the lecturer:** is ripped artwork acceptable for this submission? If not, the
swap above is a day's work and no code changes.

---

## Import settings

Two classes of art, deliberately set differently:

| | Sheets & pixel art (ship, chickens, projectiles) | Pre-rendered art (food, starfield) |
|---|---|---|
| Filter Mode | **Point** — preserves hard edges | **Bilinear** — preserves gradients |
| Draw Mode | Simple | **Tiled** for the starfield |
| Compression | None | Normal Quality |

Mip maps off everywhere. Wrap Mode **Repeat** on the starfield (**Full Rect** mesh type, or tiling
shows seams), Clamp elsewhere.

**Pixels Per Unit** differs per sheet because the source art does: ship **40**, chickens and
projectiles **100**, food **128**, starfield **100**. Sorting layers back → front: `Background` →
`Pickups` → `Projectiles` → `Chickens` → `Player` → `VFX` → `UI`.

> The background is Tiled with **Size set to a whole number of tiles**, never scaled. A non-uniform
> `localScale` on artwork stretches it — that is what cost marks on the mid-semester project.

---

## Inventory — `Assets/ThirdParty/ChickenInvaders/` and `Assets/ThirdParty/Starbound/`

### Sprites

| File | Contents | PPU | Use |
|---|---|---|---|
| `hero_ship.png` | 8 banking poses, 45 × 37 each | 40 | The player. Pose picked from input, not animated |
| `chicken-wings.png` | 50 frames — body, wings and feet together | 100 | Enemy base layer, flap animation |
| `chicken-body-leotard2.png` | 48 colour variants, 64 × 69 | 100 | Costume overlay — one colour per chicken type |
| `chicken-face.png` | 225 head frames, ~35 × 42 | 100 | Head layer |
| `egg.png` / `eggbreak.png` | 4 eggs / break frames | 100 | Enemy projectile and its impact |
| `bulletIon.png` | Beam frames | 100 | Default Ion blaster |
| `bulletNeutron.png` | Green plasma, 3 sizes | 100 | Spread gift weapon |
| `bullet-bolt3.png` | 8 cyan lightning frames (re-sliced 4 × 2) | 100 | Lightning gift weapon, animated |
| `bullet-bolt1.png` | 2 fireball frames | 100 | Fireball gift weapon |
| `…Starbound - Monsters - Unique - Unusual Gift Box.png` | 7 gift box frames + small ones | 40, Point filter | Gift pickup, animated |
| `flare-my.png` | 4 coloured flares | 100 | Hit and explosion VFX |
| `…Astronaut Chicken.png` | 3 whole chickens + parts | 100 | Mother Hen: three damage stages, feet and wings |
| `…GUI - Logo.png` | Title logo | 100 | Main menu |

> `hero_ship.png` is derived from the *Authentic Hero* rip below: the original had **no alpha**
> (a black background), so the black was keyed out. The unmodified original is kept beside it.

### Food — `Assets/ThirdParty/ChickenInvaders/Sprites/Food/`

Ready to use as single sprites:

| File | px | Use |
|---|---|---|
| `CI3Leg.png` | 42 × 70 | Feast Streak food, streak 1–2 |
| `TwinLegs.png` | 56 × 47 | Feast Streak food, streak 3–4 |
| `CI3Roast.png` | 92 × 71 | Feast Streak food, streak 5–6 |
| `PlainBurger` → `CheeseBurger` → `TomatoCheeseBurger` → `TLCBurger` → `DoubleBurger` → `TripleBurger` → `QuadBurger` | 62 × 52 up to 70 × 136 | The burger ladder, streak 7 up to 13+ |
| `RedHerring.png` | 69 × 64 | The rare red herring, worth nothing |

The other food sprites from the fan wiki (corn, pizza, popcorn, sweets, vegetables, pumpkin,
mistletoe and the herring sheets) were unused and have been removed from the project.

### Backgrounds

| File | px | Use |
|---|---|---|
| `…Chicken Invaders 3 - Background - Starfield.png` | 1524 × 1524, seamless | Tiled scrolling background. One tile = 15.24 world units at PPU 100 |

---

## Per-file sources

### The Spriters Resource
The gift box is the one sprite not from Chicken Invaders: *Starbound* (Chucklefish), from
spriters-resource.com — PC, Starbound, Monsters, Unique, "Unusual Gift Box". Same position as the
other ripped art: coursework only, credited, no reuse licence.


| File | Source |
|---|---|
| `PC _ Computer - Chicken Invaders - Playable Characters - Authentic Hero.png` | spriters-resource.com — Chicken Invaders, Playable Characters |
| `PC _ Computer - Chicken Invaders 4 - Enemies - Astronaut Chicken.png` | spriters-resource.com — Chicken Invaders 4, Enemies |
| `PC _ Computer - Chicken Invaders 4 - GUI - Logo.png` | spriters-resource.com — Chicken Invaders 4, GUI |
| `PC _ Computer - Chicken Invaders 3 - Background - Starfield.png` | spriters-resource.com — Chicken Invaders 3, Backgrounds |
| `chicken-wings.png`, `chicken-body-leotard2.png`, `chicken-face.png` | Chicken Invaders sprite rips |
| `egg.png`, `eggbreak.png`, `bullet*.png`, `flare-my.png` | Chicken Invaders sprite rips |

### Fan wiki — food

Downloaded 2026-09-05 from `chickeninvaders.fandom.com/wiki/Food`. The CDN served WebP under `.png`
names; the files were decoded and re-saved as real RGBA PNGs. Base URL for every row below is
`https://static.wikia.nocookie.net/chickeninvaders/images/`.

| File | Path under that base |
|---|---|
| `CI3Leg.png` | `3/39/CI3Leg.png` |
| `CI3Roast.png` | `5/5c/CI3Roast.png` |
| `TwinLegs.png` | `2/21/TwinLegs.png` |
| `PlainBurger.png` | `8/8c/PlainBurger.png` |
| `CheeseBurger.png` | `2/25/CheeseBurger.png` |
| `TomatoCheeseBurger.png` | `4/4b/TomatoCheeseBurger.png` |
| `TLCBurger.png` | `4/40/TLCBurger.png` |
| `DoubleBurger.png` | `9/92/DoubleBurger.png` |
| `TripleBurger.png` | `c/ce/TripleBurger.png` |
| `QuadBurger.png` | `f/f9/QuadBurger.png` |
| `Corny1.png` | `9/9f/Corny1.png` |
| `Corny2.png` | `e/e2/Corny2.png` |
| `RedHerring.png` | `0/0c/RedHerring.png` |
| `RedHerringOLD.png` | `7/78/RedHerringOLD.png` |
| `Red_herring.png` | `4/43/Red_herring.png` |
| `Pumpkin.png` | `6/64/Pumpkin.png` |
| `Mistletoe.png` | `c/c3/Mistletoe.png` |
| `MistletoeLeaf.png` | `0/04/MistletoeLeaf.png` |
| `PopcornTypes.png` | `f/f6/PopcornTypes.png` |
| `Popcorn_yellow.png` | `6/6f/Popcorn_yellow.png` |
| `Vege.png` | `d/d4/Vege.png` |
| `Fired.png` | `f/fa/Fired.png` |
| `Pizza.png` | `f/f4/Pizza.png` |
| `Sweet.png` | `4/49/Sweet.png` |
| `Ice_cream.png` | `2/22/Ice_cream.png` |

---

## Fonts and UI

The menu and HUD use **Bungee** throughout. Bungee and the included TextMesh Pro default
**Liberation Sans** resources are both SIL Open Font License 1.1. Bungee comes from the official
Google Fonts repository (`google/fonts/ofl/bungee`, downloaded 2026-09-30); its licence is stored
at `Assets/ThirdParty/Fonts/Bungee-OFL.txt`. Liberation Sans comes from the installed Unity uGUI 2.0.0
package's **TMP Essential Resources**;
its original notice is kept at `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`.
Only the font, fallback, settings, line-breaking/style data and required mobile SDF shader/includes
were imported. No examples, emoji sprites or additional Unity packages were added.

The title uses the purple/white logo already listed above, with an additional `MenuTitle` slice
that preserves existing slices and GUIDs. Buttons use Unity's built-in sliced UI sprite and colour
states stored in `RunUI.prefab`; they require no downloaded or generated bitmap artwork.

## Audio — `Assets/ThirdParty/Kenney/`, `Assets/ThirdParty/OpenGameArt/` and `Assets/Audio/`

All audio is free to use: everything is **CC0** except the chicken sound, which is **CC-BY 3.0** and
credited below. Downloaded 2026-10-01. Music was converted from WAV to OGG to keep the repository
small; music imports as **Streaming**, short effects as **Decompress On Load**.

| File | Source | Licence |
|---|---|---|
| `music_menu`, `music_game`, `music_boss`, `music_victory` | "5 Chiptunes (Action)" by **Juhani Junkala** — [opengameart.org/content/5-chiptunes-action](https://opengameart.org/content/5-chiptunes-action) (Title Screen, Level 1, Level 3, Ending) | CC0 |
| `jingle_victory`, `jingle_gameover`, `sfx_wave_clear` | Kenney **Music Jingles** — [kenney.nl/assets/music-jingles](https://kenney.nl/assets/music-jingles) (`jingles_NES12`, `NES11`, `NES09`) | CC0 |
| `sfx_shoot`, `sfx_egg_splat`, `sfx_player_explode`, `sfx_blast`, `sfx_big_boom` | Kenney **Sci-fi Sounds** — [kenney.nl/assets/sci-fi-sounds](https://kenney.nl/assets/sci-fi-sounds) (`laserSmall_000`, `slime_000`, `explosionCrunch_003/000`; the big boom layers `lowFrequency_explosion_000` with `explosionCrunch_004`) | CC0 |
| `sfx_boss_hit`, `sfx_glass_break`, `sfx_food` | Kenney **Impact Sounds** — [kenney.nl/assets/impact-sounds](https://kenney.nl/assets/impact-sounds) (`impactMetal_medium_000`; glass layers `impactGlass_heavy_001` + `_004`; `footstep_snow_000`) | CC0 |
| `sfx_egg_lay`, `sfx_ui_click` | Kenney **Interface Sounds** — [kenney.nl/assets/interface-sounds](https://kenney.nl/assets/interface-sounds) (`drop_002`, `click_001`) | CC0 |
| `sfx_respawn`, `sfx_wave_start`, `sfx_boss_charge` | Kenney **Digital Audio** — [kenney.nl/assets/digital-audio](https://kenney.nl/assets/digital-audio) (`powerUp1`, `zapThreeToneUp`, `phaserUp1`) | CC0 |
| `sfx_chicken_1..3` | "Chicken Sound Effect" by **IMadeIt** — [opengameart.org/content/chicken-sound-effect](https://opengameart.org/content/chicken-sound-effect), cut into three clucks and loudness-normalised | **CC-BY 3.0** — credit: *Chicken sound by IMadeIt (OpenGameArt)* |
| `sfx_food_herring` | Kenney **Digital Audio** (`phaserDown1`) | CC0 |
| `sfx_shoot_spread`, `sfx_shoot_lightning`, `sfx_shoot_fireball` | Kenney **Sci-fi Sounds** `laserRetro_002`, Kenney **Digital Audio** `zap1` (trimmed), Kenney **Sci-fi Sounds** `laserLarge_000` (trimmed, lowered) | CC0 |
| `sfx_gift_catch`, `sfx_weapon_expire` | Kenney **Digital Audio** (`powerUp10`, `phaserDown2`) | CC0 |
| `sfx_shield_break` | Kenney **Impact Sounds** `impactGlass_light_000` layered with Kenney **Sci-fi Sounds** `forceField_000` | CC0 |
| `Assets/Audio/sfx_boss_alarm` | Made for this project: a two-tone siren synthesised with ffmpeg | Original |

### Made for this project

| File | What | Licence |
|---|---|---|
| `Assets/Art/feather.png` | 96 × 32 white feather for the feather particle bursts, drawn procedurally | Original |
| `Assets/Art/shield.png` | 128 × 128 glowing cyan bubble for the shield, drawn procedurally | Original |

> "Free on itch.io" is not a licence name. `CC0 1.0`, `CC-BY 4.0`, `OFL 1.1` are. Paste the exact
> one into the table above.
