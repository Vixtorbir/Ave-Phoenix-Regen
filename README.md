# Ave Phoenix Regen

A Unity 6 remake of the 1980 arcade shoot'em up *Phoenix* (known in Spain as *Ave Phoenix*), playable on
Android and PC. I made it on my own as a birthday present for my father, who played the original in
arcades when he was young, so it ships with a story mode and a dedication screen written for him.

**[Download the Android APK](https://github.com/Vixtorbir/Ave-Phoenix-Regen/releases/latest)**

<!-- TODO: add a 20–30 s gameplay GIF here (boss fight + shield). -->

| | |
|---|---|
| **Role** | Solo project: design and programming |
| **Engine** | Unity 6 (6000.3.16f1), URP 2D, New Input System |
| **Platforms** | Android (APK, portrait) and Windows |
| **Language** | C# |
| **Status** | Playable: Story mode and endless Arcade mode |

Character portraits drawn by my brother.

## The game

- **Story mode:** two enemy waves, the Mothership boss and a victory sequence, framed by radio
  transmissions with typewriter dialogue.
- **Arcade mode:** endless waves that escalate over time, a boss every 10 waves and +1 life after a few
  rounds survived while damaged. High score is saved locally.
- **The shield** works like in the original: while it is up you are immune, but you cannot move or shoot,
  and it goes on cooldown when you release it.

## Controls

| Action | Keyboard | Gamepad | Touch (Android) |
|---|---|---|---|
| Move | WASD / Arrow keys | Left stick / D-pad | Drag anywhere |
| Fire | Automatic (Space also fires) | Automatic | Automatic |
| Shield (hold) | Left Shift | — | Tap the shield orb in the HUD |

## What to look at in the code

All gameplay code lives in [`Assets/_Project/Scripts`](Assets/_Project/Scripts).

- **Game flow state machine.** A persistent bootstrap scene owns a generic `StateMachine` / `IState` and a
  `SceneLoader` that swaps content scenes additively (Boot → Main Menu → Story / Arcade).
  [`Core/`](Assets/_Project/Scripts/Core)
- **The same state machine, reused for gameplay.** The shield runs on it (`Ready → Active → Cooldown`)
  and the boss has its own phase states (`Intro → Phase 1 → Phase 2 → Defeated`). Phase 2 starts when half
  of the boss barrier is destroyed, and the barrier segments and core take damage through an
  `IDamageable` contract.
  [`ShieldStates.cs`](Assets/_Project/Scripts/Gameplay/Player/ShieldStates.cs) ·
  [`BossStates.cs`](Assets/_Project/Scripts/Gameplay/Enemies/Boss/BossStates.cs)
- **Object pooling.** Enemies, player bullets, enemy projectiles, VFX and floating score popups use
  `UnityEngine.Pool.ObjectPool`. Pooled objects return themselves to their pool when they die or time
  out, instead of being destroyed and instantiated again.
  [`WaveManager.cs`](Assets/_Project/Scripts/Gameplay/Enemies/WaveManager.cs) ·
  [`Bullet.cs`](Assets/_Project/Scripts/Gameplay/Weapons/Bullet.cs)
- **Data-driven waves and dialogue.** Each wave is a `WaveDataSO` asset (enemy type, count, formation
  pattern such as grid, V-shape or alternating wings, spawn timing), and the story is a sequence of
  `StoryDialogueSO` assets, so content can change without touching code.
  [`Data/`](Assets/_Project/Scripts/Data)
- **One input entry point.** `PlayerInputReader` is the only script that talks to the Input System.
  Movement, shooting and shield read from it, which made adding touch controls (DPI-normalised drag
  and auto-fire) a change in one file.
  [`PlayerInputReader.cs`](Assets/_Project/Scripts/Gameplay/Player/PlayerInputReader.cs)
- **Procedural starfield.** The scrolling background is generated in code when the scene loads: a nebula
  made from periodic value noise plus stars drawn with wrap-around, so the texture tiles with no seams.
  The same seed always gives the same sky.
  [`StarfieldTexture.cs`](Assets/_Project/Scripts/Systems/StarfieldTexture.cs)
- **Game feel.** Hit flash, camera shake, floating scores and URP bloom/vignette.
  [`Systems/`](Assets/_Project/Scripts/Systems)

## Running it

1. Open the project with **Unity 6000.3.16f1** (Unity Hub will offer to install it).
2. Open `Assets/_Project/Scenes/00_Bootstrap.unity` and press Play. The game must start from this scene.

Or install the APK from [Releases](https://github.com/Vixtorbir/Ave-Phoenix-Regen/releases/latest) on an
Android phone (you may need to allow installing apps from unknown sources).

## What I would improve next

- The managers (`AudioManager`, `WaveManager`, `ArcadeModeManager`) are singletons that call each other
  directly. Next step: decouple them with events so systems can be tested in isolation.
- Story and Arcade mode share wave and boss logic in two places, which should be unified.
- Add Play Mode tests for the boss phases and the shield, and profile the game on a real low-end phone.

---

Fan-made, non-commercial remake. *Phoenix* and its original assets belong to their respective owners.

**Víctor Cárdenas** · Gameplay Programmer · [GitHub](https://github.com/Vixtorbir) ·
[LinkedIn](https://www.linkedin.com/in/victor-cardenas-gamedev/)
