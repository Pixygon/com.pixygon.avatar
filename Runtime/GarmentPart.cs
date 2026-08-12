using System.Collections.Generic;
using Pixygon.Stats;
using UnityEngine;

namespace Pixygon.Avatar {
    /// <summary>
    /// A garment: clothing WITH consequence — armor, boots, cloaks. Extends <see cref="AvatarPart"/>
    /// (same slots, same 2D/3D render path, same catalog) with the layer clothing was missing:
    /// <b>stat modifiers</b> (defense, warmth, movement…), <b>lore</b>, the Codex link, and the
    /// canonical GLB source. Armor is not a separate system — it's clothing that matters.
    /// Equip/unequip is one mod-set on the wearer's StatBlock, keyed by slot so re-equipping
    /// cleanly replaces.
    /// </summary>
    [CreateAssetMenu(menuName = "Pixygon/Avatar/Garment", fileName = "Garment")]
    public class GarmentPart : AvatarPart {
        [System.Serializable]
        public struct GarmentStat {
            [Tooltip("Stable stat id from Pixygon.Stats.Stat (e.g. 40001 Defense, 160003 TemperatureResist).")]
            public int statId;
            [Tooltip("Human/wiki key, e.g. \"Defense.Defense\" — documentation, not identity.")]
            public string key;
            public float value;
        }

        [Header("Consequence (what wearing this DOES)")]
        [Tooltip("Flat modifiers applied to the wearer's StatBlock while worn.")]
        public GarmentStat[] _stats;
        [Tooltip("Carried weight while worn (pack encumbrance treats worn as half carried).")]
        public float _weight = 0.5f;

        [Header("Lore")]
        [TextArea] public string _description;
        public string[] _lore;
        [Tooltip("Codex slug this garment IS (code→Codex rule).")]
        public string _codexSlug;

        [Header("Portable asset")]
        [Tooltip("Path/URL of the canonical GLB (meters, +Z forward, origin at worn anchor, " +
                 "manifest baked in asset.extras.pixygonItem). Unity renders _prefab3D; " +
                 "browsers render this — SAME item.")]
        public string _glbSource;

        /// <summary>Apply this garment's modifiers to a wearer. Keyed by slot: equipping another
        /// garment in the same slot overwrites; <see cref="RemoveFrom"/> clears.</summary>
        public void ApplyTo(StatBlock block) {
            if (block == null || _stats == null) return;
            string source = ModSource();
            foreach (var s in _stats)
                block.Get(s.statId)?.SetMod(source, s.value);
        }

        public void RemoveFrom(StatBlock block) {
            if (block == null || _stats == null) return;
            string source = ModSource();
            foreach (var s in _stats)
                block.Get(s.statId)?.ClearMod(source);
        }

        private string ModSource() => "garment:" + _slot;

        /// <summary>The portable identity — bake into the GLB's extras or export as sidecar.</summary>
        public ItemManifest ToManifest() {
            var m = new ItemManifest {
                Id = GetFullID,
                Kind = "garment",
                Slot = _slot.ToString(),
                Title = string.IsNullOrEmpty(_displayName) ? name : _displayName,
                Description = _description,
                Lore = _lore,
                CodexSlug = _codexSlug,
            };
            if (_stats != null)
                foreach (var s in _stats)
                    m.Stats.Add(new ItemManifest.StatEntry { Id = s.statId, Key = s.key, Value = s.value });
            return m;
        }
    }
}
