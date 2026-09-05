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
(CC0) covers the enemies. Every sprite is referenced through a prefab field or the editor scene
builder, so this is an asset swap, not a code change.

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

## Inventory — `Assets/Art/`

### Sprites

| File | Contents | PPU | Use |
|---|---|---|---|
| `hero_ship.png` | 8 banking poses, 45 × 37 each | 40 | The player. Pose picked from input, not animated |
| `chicken-wings.png` | 50 frames — body, wings and feet together | 100 | Enemy base layer, flap animation |
| `chicken-body-leotard2.png` | 48 colour variants, 64 × 69 | 100 | Costume overlay — one colour per chicken type |
| `chicken-face.png` | 225 head frames, ~35 × 42 | 100 | Head layer |
| `egg.png` / `eggbreak.png` | 4 eggs / break frames | 100 | Enemy projectile and its impact |
| `bulletIon.png` | Beam frames | 100 | Player projectile (the one used) |
| `bulletNeutron`, `bulletFork`, `bullet-bolt1/3/4` | Alternate weapons | 100 | Unused — kept for the boss or a later pass |
| `flare-my.png` | 4 coloured flares | 100 | Hit and explosion VFX |
| `…Astronaut Chicken.png` | 3 whole chickens + parts | 100 | Alternative enemy look |
| `…GUI - Logo.png` | Title logo | 100 | Main menu |

> `hero_ship.png` is derived from the *Authentic Hero* rip below: the original had **no alpha**
> (a black background), so the black was keyed out. The unmodified original is kept beside it.

### Food — `Assets/Art/Sprites/Food/`

Ready to use as single sprites:

| File | px | Use |
|---|---|---|
| `CI3Leg.png` | 42 × 70 | **The drumstick pickup** — start here |
| `TwinLegs.png` | 56 × 47 | Larger drop |
| `CI3Roast.png` | 92 × 71 | Rare drop |
| `PlainBurger` → `CheeseBurger` → `TomatoCheeseBurger` → `TLCBurger` → `DoubleBurger` → `TripleBurger` → `QuadBurger` | 62 × 52 up to 70 × 136 | A seven-tier ladder, if a streak mechanic is ever added |
| `Corny1/2.png` | ~33 × 48 | Small common drop |
| `RedHerring.png` | 69 × 64 | A joke pickup worth nothing |
| `Pumpkin`, `Mistletoe`, `MistletoeLeaf` | — | Seasonal, optional |

**Collection sheets** (several items in one image, need Sprite Mode → Multiple → Slice → Automatic;
they are *not* uniform grids): `Fired`, `Ice_cream`, `Pizza`, `PopcornTypes`, `Popcorn_yellow`,
`Red_herring`, `Sweet`, `Vege`.

> ⚠️ **`RedHerringOLD.png` has no transparency** — it renders as a rectangular box. Use
> `RedHerring.png` instead.

### Backgrounds

| File | px | Use |
|---|---|---|
| `…Chicken Invaders 3 - Background - Starfield.png` | 1524 × 1524, seamless | Tiled scrolling background. One tile = 15.24 world units at PPU 100 |

---

## Per-file sources

### The Spriters Resource

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

## Still to source

| Asset | Where to look | What to record |
|---|---|---|
| UI font | Google Fonts | The exact family and its licence (usually OFL 1.1) |
| SFX ×6 — laser, cluck, splat, pickup, player death, wave clear | [freesound.org](https://freesound.org/) filtered to **CC0**, [pixabay](https://pixabay.com/sound-effects/), [mixkit](https://mixkit.co/free-sound-effects/game/) | Per-clip URL and licence name |
| Music — gameplay loop, optional boss loop | Same three sites | Per-clip URL and licence name |

Keep one-shots under ~1 second or they feel laggy. Import Type: **Decompress On Load** for the
short effects, **Streaming** for music.

> "Free on itch.io" is not a licence name. `CC0 1.0`, `CC-BY 4.0`, `OFL 1.1` are. Paste the exact
> one into the table above.
