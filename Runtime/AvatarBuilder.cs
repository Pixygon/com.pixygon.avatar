namespace Pixygon.Avatar {
    /// <summary>
    /// Resolves an <see cref="AvatarSpec"/> against an <see cref="AvatarCatalog"/> and drives an
    /// <see cref="IAvatarRenderer"/> to assemble the body. This is the salvaged Pixygon
    /// <c>AvatarBuilder</c> pattern (ID-based parts → snap-points → material swaps → body-height),
    /// but the *rendering* is behind the renderer seam so 2D and 3D share this logic.
    /// </summary>
    public sealed class AvatarBuilder {
        private readonly AvatarCatalog _catalog;
        private readonly IAvatarRenderer _renderer;

        public AvatarBuilder(AvatarCatalog catalog, IAvatarRenderer renderer) {
            _catalog = catalog;
            _renderer = renderer;
        }

        public void Build(AvatarSpec spec) {
            if (spec == null || _catalog == null || _renderer == null) return;

            _renderer.Clear();
            _renderer.SetBody(_catalog.Part(spec.Get(AvatarSlot.Body)), spec.BodyHeight);

            foreach (var kv in spec.Parts) {
                if (kv.Key == AvatarSlot.Body) continue; // body handled above
                _renderer.SetPart(kv.Key, _catalog.Part(kv.Value));
            }

            _renderer.Commit();
        }
    }
}
