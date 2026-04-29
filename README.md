# Destructible Wall Demo

A Unity tech demo showcasing real-time mesh fracturing using projectile physics and the EzySlice library.

---

## Controls

### Camera Movement

| Input | Action |
|---|---|
| `W A S D E Q` | Move |
| `Left Shift` + move | Move fast |
| `Right Mouse Button` + drag | Look around |

### Shooting

| Input | Action |
|---|---|
| `Left Mouse Button` | Fire a projectile toward the cursor |

Aim at a wall and click to launch a projectile. Projectiles auto-destroy after 5 seconds if they don't hit anything.

### Reset

| Input | Action |
|---|---|
| `SPACE` | Reset all walls and clean up debris |

---

## Features

- **Real-time mesh fracturing** — walls shatter into physics-driven chunks on projectile impact using the EzySlice library
- **Impact-driven fracture pattern** — fracture seeds are distributed radially from the point of impact, producing denser breakage near the hit and larger pieces toward the edges
- **Physics chunks** — each chunk is a fully simulated rigid body with a convex mesh collider, mass, and realistic collision response
- **Free-look camera** — smooth 6-DOF camera with hold-to-rotate mouse look and a sprint modifier
- **Projectile launcher** — raycast-aimed projectile firing with configurable velocity, impact force, and impact radius
- **Wall reset system** — snapshots every wall's transform and material state at startup and can restore all walls and destroy all debris at any time
