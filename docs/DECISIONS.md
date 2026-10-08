# Decisions Log

One entry per decision: what was decided and why. Newest at the top. Before changing something listed here, read the reason. If the reason no longer holds, say so and add a new entry instead of silently undoing it.

Format: **Date. Decision.** Reason. Status (Active, Superseded, Open).

---

## Open questions (not yet decided)

- **Which branch is the main line?** Work currently lives on `claude/port-main-fixes`, which combines `ai-sandbox` (player health, ADS, sprint/jump, EnemyAI) with the fixes from PR #1. `main` is behind. Once the combined branch is tested in Unity, it should become `main`.
- **Player health and enemy health are separate systems.** `PlayerController` has its own health fields and implements `IDamageable` directly. Enemies use `HealthSystem`. Unifying them would let UI, damage effects, and loot logic work the same for both, but it was not done yet because it would change working player code.
- **How will 1000+ enemies be simulated in Unity?** Per-enemy `NavMeshAgent` will not scale. Candidates: flow field plus lightweight movement, or Unity DOTS/ECS. Decide before the horde stage.

---

## 2026-10-08. Project memory lives in the repo
`CLAUDE.md`, `docs/DESIGN.md`, and this file hold the project's goals and decisions so any new session can pick up the context. Reason: Claude has no memory between chats, so anything not written down gets forgotten or re-decided. Status: Active.

## 2026-10-08. The goal is "original PS4 Destiny, but zombies"
Every design decision is checked against that. See `docs/DESIGN.md`. Status: Active.

## 2026-10-08. Enemy health is a single component: `HealthSystem`
Exposes `Damaged` and `Died` events, `Heal()`, and a `destroyOnDeath` option. Other scripts (like `EnemyRewardTarget`) react to events instead of tracking health themselves. Reason: one source of truth means loot, UI, and effects can all listen to the same events. Status: Active (enemies only, see open questions).

## 2026-10-08. Shots ignore trigger colliders and find the damage target from the hit collider's parents
`Physics.Raycast(..., QueryTriggerInteraction.Ignore)` and `hit.collider.GetComponentInParent<IDamageable>()`. Reason: hits on child colliders (limbs, armor) must still damage the root object, and trigger zones (like loot magnets) must not block shots. Status: Active.

## 2026-10-08. Loot that starts flying toward the player keeps flying
`LootItem` latches `isMagnetized` once triggered and guards against collecting twice. Reason: previously loot froze in mid-air if the player stepped out of range, and could be collected twice in a frame. Status: Active.

## 2026-10-08. Temporary materials created at runtime are cleaned up
Hit-impact markers call `Destroy(rend.material, 0.2f)`. Reason: accessing `.material` creates a copy that is not destroyed with the object, which leaks memory over a long session. Status: Active.

## 2026-10-08. All damage goes through `IDamageable` and `DamagePayload`
Payload carries amount, damage type (Physical, Fire, Toxic, Electric, Frost), hit point, and hit normal. Reason: lets weapons, enemies, and hazards damage anything without knowing what it is, and leaves room for elemental effects. Status: Active.

## Earlier decisions (from design conversations, before the Unity code)

- **Server-authoritative from the start.** Even in single-player, keep simulation separate from display so it can move to a server later. Reason: retrofitting is very expensive, and it is the base of every anti-cheat measure.
- **Build in stages, each playable.** Single-player combat, then horde scale, then one boss, then co-op, then PvP, then backend and economy, then three worlds. Reason: protects against spending a year on infrastructure for a game nobody has confirmed is fun.
- **Rust-style persistent worlds before Destiny-style instances.** Reason: far simpler, same fantasy, and the dedicated-server work carries over.
- **Co-op PvE before PvP.** Reason: zombies tolerate lag, players do not. PvP netcode is the hardest part.
- **No custom hardware or locked OS for anti-cheat.** Reason: cost, no audience, and it still does not stop input spoofing or collusion. Use server authority, information hiding, detection, and off-the-shelf anti-cheat later.
- **Horde techniques:** one shared flow field, spatial hash separation, simulation level of detail, flat data arrays, GPU instancing with per-instance variation, staggered updates. Reason: proven in browser prototypes to keep cost nearly flat as zombie count rises.
- **Simple cartoonish art style, with variety from per-instance data** (size, tint, accessories) rather than many models. Reason: cheap to render and reads clearly in a crowd. Revisit if the art direction changes.
- **Boss design:** server-side state machine, phases, telegraphed attacks, scaled to group size, loot rolled on the server with contribution thresholds. Reason: fair, exploit-resistant, and fits the horde tech (adds are cheap).
- **Cross-world economy: central backend owns the truth, transactions are atomic.** Reason: prevents duplication exploits and keeps three worlds consistent.
