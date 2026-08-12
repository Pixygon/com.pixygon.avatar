using System.Collections.Generic;
using UnityEngine;

namespace Pixygon.Avatar {
    /// <summary>
    /// The attachType:"skinned" contract, implemented: a part's SkinnedMeshRenderers rebind to a
    /// shared skeleton's bones BY NAME (CONVENTION.md). Used by the Workbench preview and by
    /// runtime dressing alike — one rebind rule everywhere.
    /// </summary>
    public static class SkinnedRebinder {
        /// <summary>Rebind every SkinnedMeshRenderer under <paramref name="part"/> to bones found
        /// under <paramref name="skeletonRoot"/>. Returns the number of bones that could NOT be
        /// matched (0 = perfect bind; fix mismatches with AvatarPart fix.boneMap server-side).</summary>
        public static int Rebind(GameObject part, Transform skeletonRoot) {
            if (part == null || skeletonRoot == null) return -1;
            var byName = new Dictionary<string, Transform>();
            foreach (var t in skeletonRoot.GetComponentsInChildren<Transform>(true))
                if (!byName.ContainsKey(t.name)) byName.Add(t.name, t);

            int missing = 0;
            foreach (var smr in part.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
                var bones = smr.bones;
                for (int i = 0; i < bones.Length; i++) {
                    if (bones[i] != null && byName.TryGetValue(bones[i].name, out var match)) bones[i] = match;
                    else missing++;
                }
                smr.bones = bones;
                if (smr.rootBone != null && byName.TryGetValue(smr.rootBone.name, out var root))
                    smr.rootBone = root;
            }
            return missing;
        }
    }
}
