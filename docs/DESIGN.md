# LandZ Design Document

> Status: first draft written by Claude from conversations with the developer.
> Items marked **(assumed)** are my reading of the vision and need the developer's confirmation.
> When this file and a request disagree, ask before building.

## The goal (one sentence)

**The original PS4 Destiny experience, but zombies.**

Every decision gets checked against that sentence: does this make the game feel more like launch-era Destiny with a zombie apocalypse, or less?

## What "original Destiny" means here (assumed)

These are the parts of Destiny 1 (2014-2015) the design leans on:

- **First-person gunplay that feels great.** Tight, responsive shooting is the foundation. Weapon feel (recoil, sound, hit feedback, aim-down-sights) matters more than weapon count.
- **The loot loop.** Kill things, they drop gear, gear makes you stronger, stronger lets you take on harder things. Loot should be visible, exciting, and worth chasing.
- **Distinct worlds to explore.** Destiny had separate destinations (Earth, Moon, Venus, Mars), each with its own look, enemies, and resources. This maps directly onto the planned three worlds.
- **Co-op as the heart.** Small fireteams working together in PvE.
- **Boss encounters as set pieces.** Strikes (small-team boss missions) and raids (large-team, mechanic-heavy bosses).
- **PvP as a separate arena mode.** Competitive play alongside the PvE world.
- **A social space.** A hub where players gather, trade, and prepare.
- **Abilities and class identity.** Powers beyond the gun.

## What zombies change

Destiny's enemies were organized factions (Fallen, Hive, Vex, Cabal). Here the enemy is the horde, and the horde is the technical and design hook:

- **Scale.** Hundreds to 1000+ zombies in an area, far more than most shooters attempt. See the horde architecture below.
- **Variety.** Zombies come in many speed and body variants, so a crowd stretches out when you run and reads as a living mass.
- **Zombies as the third faction.** In PvPvE, gunfire and boss roars attract hordes, which punishes camping and creates tension between players.
- **Boss fights.** Large zombie bosses with phases, telegraphed attacks, weak points, and summoned adds. The horde tech makes adds nearly free.

## Pillars

1. **Gunplay first.** If shooting doesn't feel good, nothing else matters.
2. **Scale is the signature.** The horde is what players should remember.
3. **Loot drives everything.** Every kill and every boss should pay out something that matters.
4. **Small and fun before big and complete.** Each stage below must be a playable game on its own.
5. **Server authority.** The server decides what happens. Clients display it. This keeps cheating and exploits manageable.

## Scope and stages

Build in layers. Each stage is playable and worth showing before the next one starts.

1. **Single-player combat loop** (current stage): player, enemies that fight back, health, loot, death and respawn.
2. **The horde:** scale enemy count up using the architecture below. Prove 1000+ zombies run smoothly.
3. **One boss** with phases, adds, and a loot drop.
4. **Co-op, 2-8 players** on one world, server-authoritative, player-hosted at first.
5. **PvP** inside that world.
6. **Backend:** accounts, persistent inventory, basic market.
7. **Second and third worlds** with different resources and hazards, linked by a shared economy.

Do not skip ahead. A fun single-world co-op game is a legitimate place to ship or share before anything after stage 5 is built.

## The three worlds and the economy (later stages)

- Three worlds, each with its own environment, resources, and hazards, so no world supplies everything and trade is natural.
- A central backend owns the economy truth. World servers request transactions and the backend approves them. Transactions must be atomic so items can never be duplicated or lost.
- Bosses are the main source of scarce goods, including goods needed in a different world.
- Loot sources (faucets) and loot sinks (repairs, consumables, taxes) must be balanced and monitored.

## Horde architecture (from the browser prototypes)

These techniques were proven in standalone browser tests and are the plan for the Unity version:

- **One flow field** for pathfinding. Every zombie reads the direction under its feet, so cost does not grow with zombie count. Distance uses weighted costs (10 straight, 14 diagonal) for rounder fronts. Close to the player, zombies steer directly at the player when they have line of sight.
- **Spatial hash** so zombies push apart from neighbors without checking every other zombie.
- **Simulation level of detail.** Near zombies get full simulation. Distant ones update every few frames or become cheap data.
- **Data-oriented storage.** Zombie state in flat arrays, not heavy objects.
- **GPU instancing** for rendering, with variation (size, tint, accessories) coming from per-instance data, not separate models.
- **Staggered updates and pooling** so AI work is spread across frames and nothing is allocated mid-game.
- **Speed variety:** 20 speed types from 0.67x to 1.33x, distributed on a bell curve centered on 1.0x.

**Important gap:** the current Unity `EnemyAI` uses one `NavMeshAgent` per enemy. That is fine for dozens of enemies but will not scale to 1000+. The horde stage will need either a flow-field approach or Unity's data-oriented tools. See DECISIONS.md.

## Multiplayer principles (later stages)

- The server is authoritative for movement, hits, loot, and inventory.
- Interest management: each player only receives data about nearby zombies, at a rate that drops with distance.
- Start Rust-style (one persistent world per server, roughly 16-64 players, player-hosted). Instanced Destiny-style activities come much later.
- Anti-cheat starts as design: never trust the client, never send data the client should not have, detect impossible stats on the server. Add off-the-shelf anti-cheat once there is a player base.
- PvE first. PvP is the hardest netcode problem and comes after co-op works.

## Open questions

- Which weapon types and how many for the first playable version?
- Is there a class or ability system, and when?
- Art direction: the earlier plan was simple, cartoonish models. Does that still hold?
- Target platform: PC (Steam Early Access was discussed) or console later?
- What is the story or setting that explains the three worlds?
- Where does the player respawn, and what does death cost?
