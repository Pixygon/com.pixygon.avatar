using System.Collections.Generic;
using Pixygon.ID;
using UnityEngine;

namespace Pixygon.Avatar {
    /// <summary>
    /// A <b>race = a body mode</b>: a bundle of biology defaults. Reptillian = scale skin + tail +
    /// claws; Ydrast = gills + webbed hands/feet + slimy scale; Skyfolk = feathers/wings; Humen =
    /// baseline. <see cref="_morphWeight"/> &lt; 1 gives the lighter "proto-forms" (e.g. Caul in
    /// Veilwalkers) that grow more pronounced in later games.
    /// </summary>
    [CreateAssetMenu(menuName = "Pixygon/Avatar/Avatar Race Mode", fileName = "RaceMode")]
    public class AvatarRaceMode : IdObject {
        public string _displayName;

        [System.Serializable]
        public struct SlotPart {
            public AvatarSlot slot;
            public AvatarPart part;
        }

        [Tooltip("The biology parts this race fills in (skin shader, tail, claws, gills…).")]
        public List<SlotPart> _biology = new();

        [Range(0f, 1f)]
        [Tooltip("How pronounced the animal traits are. Proto-forms < 1; mainland forms approach 1.")]
        public float _morphWeight = 1f;

        /// <summary>Stamp this race's biology defaults onto a spec (overwrites the biology slots).</summary>
        public void ApplyTo(AvatarSpec spec) {
            if (spec == null) return;
            foreach (var b in _biology)
                if (b.part != null) spec.Set(b.slot, b.part.GetFullID);
        }
    }
}
