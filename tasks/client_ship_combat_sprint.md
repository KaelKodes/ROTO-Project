# Task Briefing: Ship Combat Sprint

**Agent:** 🟢 Client Agent  
**Priority:** High — targeting Friday playtest  
**Date:** 2026-03-05

---

## Current State (What's Already Built)

The combat system is **further along than the board suggests**. Here's what exists:

### Turrets (Manned Weapons) — `TurretComponent.cs` (430 lines)
- **6 types:** Repeater, Cannon, LongCannon, Laser, Bomber, GearCannon
- Full mount/dismount (`[F]`), mouse aim with Pivot node, crosshair overlay
- Each subclass has `Fire()` → spawns `Projectile`, sends `NetFireProjectile` to server
- Muzzle flash particle VFX on fire
- All wired into `ComponentPlacer.CreateComponent` (lines 606–641)

### Stationary Cannons (Helm-Fired) — `StationaryCannonComponent.cs`
- **6 types:** KineticCannon, LongCannon, GearCannon, Repeater, Laser, Bomber
- Base class `SpawnProjectile()` helper with networking
- `ShipController.FireAllStationaryCannons()` — fires all at once from helm
- All wired into `ComponentPlacer.CreateComponent` (lines 642–676)

### Projectiles — `Projectile.cs` (234 lines)
- `Area3D` moving forward at configurable speed
- `OnBodyEntered` / `OnAreaEntered` → finds parent `ShipController` → routes damage
- `RouteDamageToShip()` — priority: nearest `ShipComponent` within 2m, then structural segment
- Trail particle effect, visual scene loading (`cannonball.tscn`, `arrow.tscn`)
- `DamageDecalManager` — scorch marks on hit surfaces (64 max, pooled)
- `FloatingDamageNumber` — yellow damage numbers, pop + drift + fade

### Ship Damage — `ShipController.cs` (lines 55–222)
- `TakeDamageAtPoint()` — finds nearest segment, applies material integrity reduction
- `DamageNearestPartInSegment()` — hits panels first, then frames
- Panel/frame HP tracking, destruction when HP ≤ 0
- **Cascade destruction:** frame death checks if panels lost bridging support
- `SpawnStructuralBreakVFX()` — explosion burst particles

### Component HP — `ShipComponent.cs`
- `TakeDamage()` with damage flash, smoke at low HP
- `OnComponentDestroyed()` — detaches, spawns droppable `CraftablePart`, destruction VFX
- `InitHP()` based on hull metal durability

### Networking (Client → Server → Broadcast)
- `NetworkClient.SendFireProjectile()` → `NetFireProjectile`
- Server `CombatManager.TrySpawnProjectile()` — validates distance, creates `ServerProjectile`
- `CombatManager.Tick()` — moves projectiles, checks distance-based collisions against player positions
- Server broadcasts `NetProjectileSpawn` → client spawns visual

---

## What Needs Work

### 1. Upgrade Ship Components (In Progress)
**Goal:** Components should visually and mechanically scale with crafting quality/metal type.

- [ ] Component HP should scale with the hull metal's durability value (partially done via `InitHP`)
- [ ] Weapon damage should scale with `power` parameter (turret subclasses use hardcoded values like `120f`, `250f` — should use `power * multiplier`)
- [ ] Visual feedback for component quality tier (glow intensity, particle effects)

### 2. Ship Projectiles (In Progress)
**Goal:** Projectiles should feel distinct per weapon type and work reliably.

- [ ] Verify `res://models/projectiles/arrow.tscn` and `cannonball.tscn` exist — if not, create simple placeholder scenes
- [ ] Differentiate projectile visuals per damage type (Kinetic=cannonball, Energy=glowing bolt, Explosive=bomb trail)
- [ ] Projectile speed should vary per weapon type (repeater fast/weak, bomber slow/strong)
- [ ] Test: fire from turret → projectile spawns → hits another ship → damage registers → decal + floating number appear
- [ ] Test: stationary cannons fire from helm input → same damage loop

### 3. Ship Damage (In Progress)
**Goal:** Damage should be visible, consequential, and lead to ship death.

- [ ] Test cascade destruction: destroy a frame → connected panels should collapse
- [ ] Ship total HP / death state — what happens when the ship is fully destroyed?
  - Suggest: sum all frame HP as total structural integrity. At 0% → ship "sinks" (disable physics, drift downward, respawn at beacon)
- [ ] Damage visualization: panels should show damage state (tint darker as HP drops)
- [ ] Component destruction should affect ship stats (destroy engine → less thrust)

---

## Constraints
- **Don't touch** `ROTO.Shared` without announcing — changes ripple to server + island creator
- **Don't touch** server code (`ProjectAmbitionServer`) — Server Agent owns that
- The projectile visual scenes should be simple (low-poly sphere/bolt with emission material) — we can polish models later
- Focus on *working loop* first, *pretty* second

## Verification
1. Build a ship with a helm, turrets, stationary cannons, and frames/panels
2. Mount a turret → aim → fire → see projectile fly → hit a target ship
3. Fire stationary cannons from helm → same result
4. Target ship shows: floating damage number, scorch decal, HP reduction
5. Destroy enough structure → panels cascade-collapse
6. No crashes, no orphaned nodes
