# Pixygon — Avatar

The **humanoid body** for an Actor — a Mii-like, ID-based, customizable avatar.
Humanoids = `Actor` + `Avatar`. New clothing/parts are catalog assets, so they
spread to every game; **races are body *modes*** (biology), not just outfits.

## Why it's shaped this way

- **Engine-portable core.** `Pixygon.Avatar.Core` (`noEngineReferences`) holds the
  `AvatarSlot` enum + `AvatarSpec` (the resolved part-per-slot description) — pure C#.
- **Thin Unity adapter.** `Pixygon.Avatar` adds the authoring assets + builder +
  renderer seam. The same `AvatarSpec` renders in **2D** (micro sprite-stack) or
  **3D** (URP rig) via `IAvatarRenderer`.
- **Salvaged** from the legacy Pixygon `AvatarBuilder` (ID parts, snap-points,
  material swaps, body-height, NPC rig-strip) — decoupled from rendering this time.
- **Data stays in the save.** The persisted spec is `AvatarData` (com.pixygon.saving);
  `AvatarDataExtensions` bridges it ↔ `AvatarSpec` so avatars stay per-game-save
  (curated / local / profile modes per game).

```
com.pixygon.avatar/
└── Runtime/
    ├── Core/   →  Pixygon.Avatar.Core   ⚙️ engine-free
    │   ├── AvatarSlot.cs   biology + hair + clothing + accessory slots
    │   └── AvatarSpec.cs   part-per-slot + body-height (the portable description)
    └── (root) →  Pixygon.Avatar          Unity adapter
        ├── AvatarPart.cs        : IdObject — a part (slot + 2D/3D assets + unlock)
        ├── AvatarRaceMode.cs    : IdObject — a race = biology defaults + morph weight
        ├── AvatarCatalog.cs     the game's wardrobe (+ race modes)
        ├── IAvatarRenderer.cs   the 2D/3D assembler seam
        ├── AvatarBuilder.cs     resolves a spec against the catalog → renderer
        ├── Avatar.cs            MonoBehaviour + IBody (an actor's humanoid body)
        └── AvatarDataExtensions.cs   AvatarData ↔ AvatarSpec
```

## Key concepts

- **`AvatarSlot`** — biology (`Body`, `SkinType`, `Tail`, `Claws`, `Gills`, `Webbing`,
  `Horns`, `Wings`…), hair, clothing, accessories.
- **`AvatarRaceMode`** — a race as a **body mode**: Reptillian = scale skin + tail +
  claws; Ydrast = gills + webbing + slimy scale; Humen = baseline. `_morphWeight < 1`
  = the lighter **proto-forms** (e.g. Caul in Veilwalkers).
- **`AvatarPart`** — one catalog entry, with **per-dimension** assets so the same part
  works in a 2D or 3D game. Add a part = add one asset → every game gets it.

## Dependencies

`com.pixygon.idsystem`, `com.pixygon.saving` (AvatarData), `com.pixygon.actors` (IBody).

## Usage

```csharp
var avatar = GetComponent<Avatar>();
avatar.SetRace(reptillianMode);                 // biology defaults
avatar.SetSpec(saved.AvatarData.ToSpec());      // load the player's saved look
avatar.SetPart(AvatarSlot.Headgear, hatId);     // change one slot
```

## Status

`0.1.0` — MVP scaffold. **Next:** concrete `Avatar3DRenderer` (port the legacy
rig-assembler: snap bones, material swaps, body-height) + `Avatar2DRenderer` (sprite
stack for micro); the **AvatarCustomizer** UI; `SkinCard`/NFT unlock wiring; biology
fields on `AvatarData` (or a side-car) so race morphs persist. `.meta` files generate
on first Unity import.
