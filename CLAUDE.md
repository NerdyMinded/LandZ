# LandZ

A first-person zombie shooter built in Unity.

**The goal: the original PS4 Destiny experience, but zombies.** Every decision should serve that. Read these before making design or architecture choices:

- `docs/DESIGN.md` for the vision, pillars, scope, and stages
- `docs/DECISIONS.md` for what was decided and why (do not undo these without saying so)

## About the developer

Self-taught, basic coder. Explain what you changed and why in plain language, avoid jargon, and give step-by-step instructions for anything done outside the code (Unity editor, GitHub Desktop). Do not assume knowledge of Git commands. Keep scripts small and readable so the developer can learn from them.

## Tech

- Unity 6000.5.4f1, Universal Render Pipeline, Input System package, AI Navigation package
- Scripts are in `Assets/Scripts/`
- Main scene: `Assets/Scenes/SampleScene.unity`

## Code map

| File | What it does |
|---|---|
| `DamageData.cs` | `DamageType` enum, `DamagePayload` struct, and the `IDamageable` interface. Everything that can be hurt implements `IDamageable`. |
| `HealthSystem.cs` | Health for enemies and props. Has `Damaged` and `Died` events, `Heal()`, and a `destroyOnDeath` option. |
| `EnemyRewardTarget.cs` | Listens for `HealthSystem.Died` and drops a loot prefab. Requires a `HealthSystem`. |
| `EnemyAI.cs` | Enemy detects the player, chases with a `NavMeshAgent` (or direct movement as a fallback), and melee attacks. Tunable fields: detection radius, move speed, attack range, attack damage, attack cooldown. |
| `LootItem.cs` | Loot pickup. Magnetizes toward the player once in range and keeps flying. Guards against double collection. |
| `PlayerController.cs` | First-person movement (walk, sprint, jump), aim-down-sights, hitscan shooting, muzzle flash and hit impact effects, and the player's own health, health bar, death, and respawn. |

How damage flows: the player raycasts from the camera, finds an `IDamageable` on the hit collider's parents, and sends a `DamagePayload`. Enemies hit the player the same way through `EnemyAI.AttackPlayer`.

Known quirk: the player has its own health inside `PlayerController`, separate from `HealthSystem`. See open questions in `docs/DECISIONS.md`.

## Rules for working in this repo

1. **Never push straight to `main`.** Work on a branch named `claude/<short-description>`. The developer merges after testing.
2. **You cannot run Unity here.** You cannot compile or play the game, so never claim something works. Say what was changed and give the developer a specific way to test it in Unity (what to click, what they should see).
3. **Always commit `.meta` files** alongside new or moved assets and scripts. A missing `.meta` breaks references in Unity.
4. **Do not hand-edit `.unity` scene or `.prefab` files** unless asked, and say so when you do. They are easy to corrupt. Prefer making values editable in the Inspector (public or `[SerializeField]` fields) and telling the developer which setting to change.
5. **Keep scripts small and single-purpose.** One job per file. Use events and interfaces instead of scripts reaching into each other.
6. **No per-frame allocations** in code that will run for many enemies (no `new` in `Update`, no `GetComponent` in `Update`, no LINQ). Cache references.
7. **Keep simulation separate from display** (state and rules in one place, visuals in another) so the game can become server-authoritative later.
8. **Before large changes, check `docs/DESIGN.md`.** If a request conflicts with the goal or the staged plan, say so and ask.
9. **After finishing work, update `docs/DECISIONS.md`** with any decision made and the reason, and update the code map above if a file was added or its job changed.

## Files to ignore

- `Assets/_Recovery/` is Unity's crash-recovery scene backup. Do not build on it.
- `test.txt` in the repo root is an empty leftover file.

## Branches (as of 2026-10-08)

- `main`: behind. Has the PR #1 fixes but not the player and enemy work.
- `ai-sandbox`: player health, ADS, sprint/jump, `EnemyAI`.
- `claude/port-main-fixes`: `ai-sandbox` plus the PR #1 fixes. The intended next `main` once tested.
