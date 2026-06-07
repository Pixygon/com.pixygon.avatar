using Pixygon.Actors;
using UnityEngine;

namespace Pixygon.Avatar {
    /// <summary>
    /// A humanoid body. Holds an <see cref="AvatarSpec"/>, builds it via a chosen
    /// <see cref="IAvatarRenderer"/>, and implements <see cref="IBody"/> so an <see cref="Actor"/> can
    /// wear it. Also usable standalone (a Mii customizer / a cutscene preview) without an Actor.
    /// </summary>
    public class Avatar : MonoBehaviour, IBody {
        [SerializeField] private AvatarCatalog _catalog;
        [Tooltip("A component implementing IAvatarRenderer (the 2D or 3D assembler).")]
        [SerializeField] private MonoBehaviour _rendererComponent;
        [Tooltip("Optional default race body-mode applied on Awake (biology defaults).")]
        [SerializeField] private AvatarRaceMode _race;

        public AvatarSpec Spec { get; private set; } = new AvatarSpec();
        public BodyKind Kind => BodyKind.Humanoid;

        private AvatarBuilder _builder;

        private void Awake() {
            _builder = new AvatarBuilder(_catalog, _rendererComponent as IAvatarRenderer);
            if (_race != null) _race.ApplyTo(Spec);
        }

        /// <summary>Replace the whole spec and rebuild.</summary>
        public void SetSpec(AvatarSpec spec) {
            Spec = spec ?? new AvatarSpec();
            Rebuild();
        }

        /// <summary>Apply a race body-mode (biology defaults) over the current spec, then rebuild.</summary>
        public void SetRace(AvatarRaceMode race) {
            _race = race;
            race?.ApplyTo(Spec);
            Rebuild();
        }

        /// <summary>Change a single slot and rebuild.</summary>
        public void SetPart(AvatarSlot slot, int partId) {
            Spec.Set(slot, partId);
            Rebuild();
        }

        public void Rebuild() => _builder?.Build(Spec);

        // ── IBody ──
        public void OnActorAttached(Actor actor) {
            // An Actor took this body. (Future: pull the race/spec from the actor's ActorDefinition.)
            Rebuild();
        }
    }
}
