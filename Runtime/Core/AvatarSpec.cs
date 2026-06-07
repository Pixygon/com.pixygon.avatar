using System.Collections.Generic;

namespace Pixygon.Avatar {
    /// <summary>
    /// Engine-portable description of an assembled avatar: which part id sits in each
    /// <see cref="AvatarSlot"/>, plus continuous params (body height). The renderer turns this into
    /// visuals; the persisted <c>AvatarData</c> (com.pixygon.saving) converts to/from one of these.
    /// No UnityEngine here.
    /// </summary>
    public sealed class AvatarSpec {
        private readonly Dictionary<AvatarSlot, int> _parts = new();

        /// <summary>0..1 — lerps overall body scale (the legacy builder used ~0.85×..1.15×).</summary>
        public float BodyHeight = 0.5f;

        public int Get(AvatarSlot slot) => _parts.TryGetValue(slot, out var v) ? v : 0;

        public void Set(AvatarSlot slot, int partId) {
            if (partId <= 0) _parts.Remove(slot);
            else _parts[slot] = partId;
        }

        public bool Has(AvatarSlot slot) => _parts.TryGetValue(slot, out var v) && v > 0;

        public IReadOnlyDictionary<AvatarSlot, int> Parts => _parts;

        public AvatarSpec Clone() {
            var s = new AvatarSpec { BodyHeight = BodyHeight };
            foreach (var kv in _parts) s._parts[kv.Key] = kv.Value;
            return s;
        }
    }
}
