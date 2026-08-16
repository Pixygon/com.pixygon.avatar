using System.Threading.Tasks;
using UnityEngine;

namespace Pixygon.Avatar.Editor {
    /// <summary>
    /// The one place that touches GLTFast. Consumer projects' Assets/Editor scripts compile
    /// into the predefined Assembly-CSharp-Editor, which CANNOT reference glTFast's Export
    /// assembly (it is not auto-referenced) — so they must never name GLTFast types directly.
    /// They call this wrapper instead: this assembly references glTFast properly, and
    /// PIXYGON_GLTF is auto-defined here via the asmdef's versionDefines whenever
    /// com.unity.cloud.gltfast is installed. No manual Scripting Define Symbols, anywhere.
    /// Check <see cref="Available"/> (or just read Export's false return) for the no-toolchain case.
    /// </summary>
    public static class GlbExporter {
        /// <summary>True when the GLB toolchain (com.unity.cloud.gltfast) is installed.</summary>
        public static bool Available =>
#if PIXYGON_GLTF
            true;
#else
            false;
#endif

        /// <summary>Export a prefab/GameObject to a .glb at glbPath. False when the toolchain
        /// is absent or the export failed — callers log and move on.</summary>
        public static async Task<bool> Export(GameObject root, string sceneName, string glbPath) {
            if (root == null || string.IsNullOrEmpty(glbPath)) return false;
#if PIXYGON_GLTF
            var export = new GLTFast.Export.GameObjectExport();
            export.AddScene(new[] { root }, sceneName);
            return await export.SaveToFileAndDispose(glbPath);
#else
            await Task.CompletedTask;
            return false;
#endif
        }
    }
}
