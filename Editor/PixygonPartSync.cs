using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Pixygon.Avatar.Editor {
    /// <summary>
    /// Make it once, it spreads everywhere: saving an AvatarPart/Garment in Unity pushes it to
    /// the Pixygon server (/v1/avatar/assets — the library behind the web Avatar Studio), so the
    /// Mii-like editor on the websites sees it immediately. Auto-sync is a toggle (Pixygon →
    /// Avatar Sync); "Sync Selected Now" for manual pushes. Auth = the pearl API key
    /// (PIXYGON_API_KEY env or ~/.config/dyson-swarm/config.toml). Uploads need a GLB — with
    /// PIXYGON_GLTF + com.unity.cloud.gltfast the part's prefab exports and gets its manifest
    /// baked into extras (GlbManifestBaker) before upload; without it, sync tells you what's
    /// missing instead of half-uploading.
    /// </summary>
    public class PixygonPartSync : AssetPostprocessor {
        private const string PrefAuto = "Pixygon.AvatarSync.Auto";
        private const string PrefUrl = "Pixygon.AvatarSync.Url";
        private const string DefaultUrl = "https://api.pixygon.com/v1/avatar/assets";

        [MenuItem("Pixygon/Avatar Sync/Auto-Sync on Save", true)]
        private static bool AutoValidate() {
            Menu.SetChecked("Pixygon/Avatar Sync/Auto-Sync on Save", EditorPrefs.GetBool(PrefAuto, false));
            return true;
        }

        [MenuItem("Pixygon/Avatar Sync/Auto-Sync on Save")]
        private static void ToggleAuto() =>
            EditorPrefs.SetBool(PrefAuto, !EditorPrefs.GetBool(PrefAuto, false));

        [MenuItem("Pixygon/Avatar Sync/Sync Selected Now")]
        private static void SyncSelected() {
            foreach (var obj in Selection.objects)
                if (obj is AvatarPart part) Sync(part);
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom) {
            if (!EditorPrefs.GetBool(PrefAuto, false)) return;
            foreach (var path in imported) {
                if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) continue;
                var part = AssetDatabase.LoadAssetAtPath<AvatarPart>(path);
                if (part != null) Sync(part);
            }
        }

        private static async void Sync(AvatarPart part) {
            var key = ApiKey();
            if (string.IsNullOrEmpty(key)) {
                Debug.LogWarning("[AvatarSync] No API key (PIXYGON_API_KEY or ~/.config/dyson-swarm/config.toml) — skipped.");
                return;
            }
            var garment = part as GarmentPart;
            var manifest = garment != null ? garment.ToManifest() : new ItemManifest {
                Id = part.GetFullID,
                Kind = "part",
                Slot = part._slot.ToString(),
                Title = string.IsNullOrEmpty(part._displayName) ? part.name : part._displayName,
            };

            string glbPath = null;
#if PIXYGON_GLTF
            if (part._prefab3D != null) {
                glbPath = Path.Combine(Path.GetTempPath(), part.name + ".glb");
                var export = new GLTFast.Export.GameObjectExport();
                export.AddScene(new[] { part._prefab3D }, part.name);
                await export.SaveToFileAndDispose(glbPath);
                if (!GlbManifestBaker.Bake(glbPath, manifest.ToJson(), out var bakeError))
                    Debug.LogWarning($"[AvatarSync] {part.name}: manifest not baked ({bakeError}) — uploading plain GLB.");
            }
#endif
            if (glbPath == null || !File.Exists(glbPath)) {
                Debug.LogWarning($"[AvatarSync] {part.name}: no GLB to upload "
                    + "(needs _prefab3D + com.unity.cloud.gltfast + PIXYGON_GLTF define). Manifest only exists locally.");
                return;
            }

            try {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.Add("x-api-key", key);
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(string.IsNullOrEmpty(part._displayName) ? part.name : part._displayName), "name");
                form.Add(new StringContent("mesh"), "type");
                form.Add(new StringContent(garment != null ? "clothing" : "other"), "category");
                form.Add(new StringContent(part._slot.ToString().ToLowerInvariant()), "slot");
                form.Add(new StringContent("skinned"), "attachType");
                var file = new ByteArrayContent(File.ReadAllBytes(glbPath));
                file.Headers.Add("Content-Type", "model/gltf-binary");
                form.Add(file, "file", part.name + ".glb");
                var url = EditorPrefs.GetString(PrefUrl, DefaultUrl);
                var res = await http.PostAsync(url, form);
                Debug.Log($"[AvatarSync] {part.name} → {url}: {(int)res.StatusCode} {res.ReasonPhrase}");
            } catch (Exception e) {
                Debug.LogWarning($"[AvatarSync] {part.name}: upload failed — {e.Message}");
            }
        }

        private static string ApiKey() {
            var env = Environment.GetEnvironmentVariable("PIXYGON_API_KEY");
            if (!string.IsNullOrEmpty(env)) return env;
            try {
                var toml = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config", "dyson-swarm", "config.toml");
                if (File.Exists(toml)) {
                    var match = Regex.Match(File.ReadAllText(toml), "(?:api_)?key\\s*=\\s*\"([^\"]+)\"");
                    if (match.Success) return match.Groups[1].Value;
                }
            } catch { /* fall through — caller logs */ }
            return null;
        }
    }
}
