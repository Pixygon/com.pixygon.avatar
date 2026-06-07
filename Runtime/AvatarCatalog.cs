using System.Collections.Generic;
using UnityEngine;

namespace Pixygon.Avatar {
    /// <summary>
    /// The set of <see cref="AvatarPart"/>s + <see cref="AvatarRaceMode"/>s a game ships with. A game
    /// declares exactly which slice of the shared wardrobe it uses; the customizer + builder look parts
    /// up here by stable full id.
    /// </summary>
    [CreateAssetMenu(menuName = "Pixygon/Avatar/Avatar Catalog", fileName = "AvatarCatalog")]
    public class AvatarCatalog : ScriptableObject {
        [SerializeField] private List<AvatarPart> _parts = new();
        [SerializeField] private List<AvatarRaceMode> _races = new();

        private Dictionary<int, AvatarPart> _byId;

        public IReadOnlyList<AvatarPart> Parts => _parts;
        public IReadOnlyList<AvatarRaceMode> Races => _races;

        public AvatarPart Part(int fullId) {
            if (fullId <= 0) return null;
            Build();
            return _byId.TryGetValue(fullId, out var p) ? p : null;
        }

        public IEnumerable<AvatarPart> PartsForSlot(AvatarSlot slot) {
            foreach (var p in _parts)
                if (p != null && p._slot == slot) yield return p;
        }

        private void Build() {
            if (_byId != null) return;
            _byId = new Dictionary<int, AvatarPart>();
            foreach (var p in _parts)
                if (p != null) _byId[p.GetFullID] = p;
        }

        public void Invalidate() => _byId = null;

#if UNITY_EDITOR
        private void OnValidate() => Invalidate();
#endif
    }
}
