using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pixygon.Avatar {
    /// <summary>
    /// The portable item manifest — one item's ENTIRE identity (stats, lore, placement, casting)
    /// as plain data, bakeable into a glTF/GLB's <c>asset.extras.pixygonItem</c> (or a .json
    /// sidecar) so the SAME file renders and MEANS the same in Unity, a browser, or the future
    /// Pixygon Engine. Engine-free; <see cref="ToJson"/> writes deterministic, culture-invariant
    /// JSON. Schema is APPEND-ONLY (bump <see cref="Schema"/> only for breaking changes).
    ///
    /// GLB convention (see README): meters · Y-up · +Z forward · origin at the GRIP for held
    /// items, the WORN ANCHOR for garments · PBR textures embedded · manifest in asset.extras.
    /// </summary>
    public sealed class ItemManifest {
        public const int Schema = 1;

        public int Id;               // IdObject.GetFullID — stable across games
        public string Kind;          // "garment" | "weapon" | "consumable" | ...
        public string Slot;          // AvatarSlot name for garments; hand for weapons
        public string Title;
        public string Description;
        public string[] Lore;
        public string CodexSlug;     // code→Codex rule: the entry this item IS

        public readonly List<StatEntry> Stats = new List<StatEntry>();
        public struct StatEntry {
            public int Id;           // Pixygon.Stats stable id (e.g. 40001 Defense)
            public string Key;       // human/wiki key, e.g. "Defense.Defense"
            public float Value;
        }

        // Worn placement (garments/weapons hung on the body).
        public bool WornOnBack;
        public float[] WornOffset = { 0f, 0f, 0f };
        public float[] WornEuler = { 0f, 0f, 0f };

        // Casting implement (staff/wand/grimoire).
        public bool CastingImplement;
        public float CastPower = 1f;

        public string ToJson() {
            var sb = new StringBuilder(512);
            sb.Append("{\"pixygonItem\":{");
            sb.Append("\"schema\":").Append(Schema);
            sb.Append(",\"id\":").Append(Id);
            Str(sb, "kind", Kind);
            Str(sb, "slot", Slot);
            Str(sb, "title", Title);
            Str(sb, "description", Description);
            if (Lore != null && Lore.Length > 0) {
                sb.Append(",\"lore\":[");
                for (int i = 0; i < Lore.Length; i++) {
                    if (i > 0) sb.Append(',');
                    Quote(sb, Lore[i]);
                }
                sb.Append(']');
            }
            Str(sb, "codexSlug", CodexSlug);
            if (Stats.Count > 0) {
                sb.Append(",\"stats\":[");
                for (int i = 0; i < Stats.Count; i++) {
                    if (i > 0) sb.Append(',');
                    sb.Append("{\"id\":").Append(Stats[i].Id);
                    Str(sb, "key", Stats[i].Key);
                    sb.Append(",\"value\":").Append(F(Stats[i].Value)).Append('}');
                }
                sb.Append(']');
            }
            sb.Append(",\"placement\":{\"wornOnBack\":").Append(WornOnBack ? "true" : "false");
            sb.Append(",\"offset\":").Append(Vec(WornOffset));
            sb.Append(",\"euler\":").Append(Vec(WornEuler)).Append('}');
            if (CastingImplement)
                sb.Append(",\"castingImplement\":true,\"castPower\":").Append(F(CastPower));
            sb.Append(",\"glb\":{\"units\":\"meters\",\"up\":\"+Y\",\"forward\":\"+Z\",\"origin\":\"")
              .Append(Kind == "weapon" ? "grip" : "anchor").Append("\"}");
            sb.Append("}}");
            return sb.ToString();
        }

        private static string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);
        private static string Vec(float[] v) =>
            "[" + F(v[0]) + "," + F(v[1]) + "," + F(v[2]) + "]";
        private static void Str(StringBuilder sb, string name, string value) {
            if (string.IsNullOrEmpty(value)) return;
            sb.Append(",\"").Append(name).Append("\":");
            Quote(sb, value);
        }
        private static void Quote(StringBuilder sb, string s) {
            sb.Append('"');
            foreach (var c in s) {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c == '\n') sb.Append("\\n");
                else if (c < 0x20) sb.Append(' ');
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }
}
