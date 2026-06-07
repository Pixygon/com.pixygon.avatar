namespace Pixygon.Avatar {
    // ── Engine-portable core (no UnityEngine). ──

    /// <summary>
    /// Every slot an avatar can fill — <b>biology</b> (the body itself; race-modes set these),
    /// <b>hair</b>, <b>clothing</b>, and <b>accessories</b>. A slot holds one part id (or 0 = empty).
    /// Append-only: new slots go at the end. Array-style data (tools/abilities/cyberware) is handled
    /// separately, not as single slots.
    /// </summary>
    public enum AvatarSlot {
        // Biology (a race is a bundle of these — Reptillian = scale skin + tail + claws, etc.)
        Body = 0,
        SkinType,   // skin shader/material kind: smooth / scale / slime / fur
        SkinTone,
        Eyes,
        Ears,
        Tail,
        Claws,
        Gills,
        Webbing,
        Horns,
        Wings,
        Snout,
        Coat,       // fur/scale coat overlay

        // Hair
        Hair,
        HairColor,

        // Clothing
        Shirt,
        Pants,
        Shoes,
        Jacket,
        Headgear,
        Socks,
        Gloves,

        // Accessories / held
        AccessoryHead,
        AccessoryBody,
        AccessoryLapel,
        Offhand,
    }
}
