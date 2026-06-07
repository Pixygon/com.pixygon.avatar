namespace Pixygon.Avatar {
    /// <summary>
    /// Turns an avatar into visuals. Per-dimension implementations plug in here: a <b>3D</b> rig-
    /// assembler (URP — parents mesh parts to snap bones, swaps skin/coat materials, lerps body
    /// height; the salvageable Pixygon legacy <c>AvatarBuilder</c> logic lives in this impl) or a
    /// <b>2D</b> sprite-stack (micro). The <see cref="AvatarBuilder"/> resolves parts from the catalog;
    /// the renderer mounts them. The same <see cref="AvatarSpec"/> drives either.
    /// </summary>
    public interface IAvatarRenderer {
        /// <summary>Tear down the current body.</summary>
        void Clear();

        /// <summary>Set the base body mesh/sprite + overall scale (bodyHeight 0..1).</summary>
        void SetBody(AvatarPart body, float bodyHeight);

        /// <summary>Mount (or, if <paramref name="part"/> is null, clear) one slot.</summary>
        void SetPart(AvatarSlot slot, AvatarPart part);

        /// <summary>Finalize the assembly (rebuild combined meshes, bake the sprite atlas, etc.).</summary>
        void Commit();
    }
}
