using Pixygon.ID;
using UnityEngine;

namespace Pixygon.Avatar {
    /// <summary>
    /// One catalog entry — a hair, a shirt, a tail, a skin shader. Identified by <see cref="IdObject"/>
    /// so it's stable across games and the wiki. Carries <b>per-dimension</b> assets so the same part
    /// renders in a 3D (URP) game or a 2D (micro) game. Adding a part = adding one of these assets to
    /// the catalog → every game that loads the catalog gets it.
    /// </summary>
    [CreateAssetMenu(menuName = "Pixygon/Avatar/Avatar Part", fileName = "Part")]
    public class AvatarPart : IdObject {
        public string _displayName;
        public AvatarSlot _slot;

        [Header("3D (URP games) — mesh parented to the rig's snap bone")]
        public GameObject _prefab3D;
        [Tooltip("Optional material/shader (skin type, coat, hair color tint…).")]
        public Material _material3D;
        [Tooltip("Optional tint applied to the material (skin tone / hair color).")]
        public Color _tint = Color.white;

        [Header("2D (micro / sprite games) — a layer in the sprite stack")]
        public Sprite _sprite2D;
        public int _sortingOrder;

        [Header("Unlock")]
        [Tooltip("If false, the part must be unlocked (SkinCard / NFT / account). Hook attaches here later.")]
        public bool _ownedByDefault = true;

        [Header("Icon")]
        public Sprite _icon;
    }
}
