using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pixygon.Avatar.Editor {
    /// <summary>
    /// The Avatar Workbench — authoring avatar items should be effortless (founder 2026-08-12):
    /// one window to CREATE parts/garments, EDIT their meaning (slot, stats, lore, codex),
    /// PREVIEW them on a reference body live in the SceneView, and SYNC them to the server
    /// (GLB + baked manifest → /v1/avatar/assets → the web Studio). Ships in the package, so
    /// every Pixygon project has the same bench. Pixygon → Avatar Workbench.
    /// </summary>
    public class AvatarWorkbench : EditorWindow {
        private const string PrefFolder = "Pixygon.Workbench.Folder";
        private const string PrefBody = "Pixygon.Workbench.BodyPrefab";

        private Vector2 _listScroll, _editScroll;
        private string _search = "";
        private AvatarPart _selected;
        private GameObject _previewRoot;

        // The stat vocabulary garment authors reach for — label → stable catalog id.
        private static readonly (string label, int id, string key)[] CommonStats = {
            ("Defense", 40001, "Defense.Defense"),
            ("Magic Defense", 40002, "Defense.MagicDefense"),
            ("Evasion", 40003, "Defense.Evasion"),
            ("Move Speed", 90001, "Movement.MoveSpeed"),
            ("Sprint Speed", 90002, "Movement.SprintSpeed"),
            ("Temperature Resist", 160003, "Survival.TemperatureResist"),
            ("Endurance", 20007, "Attributes.Endurance"),
            ("Charisma", 20008, "Attributes.Charisma"),
            ("Perception", 20010, "Attributes.Perception"),
            ("Stealth", 100002, "PerceptionUtility.StealthLevel"),
        };

        [MenuItem("Pixygon/Avatar Workbench")]
        public static void Open() => GetWindow<AvatarWorkbench>("Avatar Workbench");

        private void OnDisable() => ClearPreview();

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawEditor();
            EditorGUILayout.EndHorizontal();
        }

        // ── left: every part in the project, searchable; creation buttons ──
        private void DrawList() {
            EditorGUILayout.BeginVertical(GUILayout.Width(230));
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            foreach (var part in AllParts()) {
                if (!string.IsNullOrEmpty(_search)
                    && !part.name.ToLowerInvariant().Contains(_search.ToLowerInvariant())) continue;
                var label = $"{(part is GarmentPart ? "🛡 " : "")}{part.name}  ·  {part._slot}";
                if (GUILayout.Toggle(_selected == part, label, "Button") && _selected != part)
                    Select(part);
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("+ New Part")) CreateAsset<AvatarPart>("Part");
            if (GUILayout.Button("+ New Garment")) CreateAsset<GarmentPart>("Garment");
            EditorGUILayout.EndVertical();
        }

        // ── right: the selected part's whole meaning + preview + sync ──
        private void DrawEditor() {
            EditorGUILayout.BeginVertical();
            if (_selected == null) {
                EditorGUILayout.HelpBox("Select a part, or create one. Set a reference body below — "
                    + "previews dress it right in the SceneView.", MessageType.Info);
                DrawBodyField();
                EditorGUILayout.EndVertical();
                return;
            }
            _editScroll = EditorGUILayout.BeginScrollView(_editScroll);
            var so = new SerializedObject(_selected);
            so.Update();
            EditorGUILayout.PropertyField(so.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(so.FindProperty("_slot"));
            EditorGUILayout.PropertyField(so.FindProperty("_prefab3D"));
            EditorGUILayout.PropertyField(so.FindProperty("_material3D"));
            EditorGUILayout.PropertyField(so.FindProperty("_tint"));
            EditorGUILayout.PropertyField(so.FindProperty("_sprite2D"));
            EditorGUILayout.PropertyField(so.FindProperty("_icon"));

            if (_selected is GarmentPart garment) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Consequence", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty("_stats"), true);
                // One-click stat rows — the vocabulary authors actually reach for.
                EditorGUILayout.BeginHorizontal();
                foreach (var (label, id, key) in CommonStats.Take(5))
                    if (GUILayout.Button("+" + label, EditorStyles.miniButton)) AddStat(garment, id, key);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                foreach (var (label, id, key) in CommonStats.Skip(5))
                    if (GUILayout.Button("+" + label, EditorStyles.miniButton)) AddStat(garment, id, key);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(so.FindProperty("_weight"));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Lore", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(so.FindProperty("_description"));
                EditorGUILayout.PropertyField(so.FindProperty("_lore"), true);
                EditorGUILayout.PropertyField(so.FindProperty("_codexSlug"));
                EditorGUILayout.PropertyField(so.FindProperty("_glbSource"));
            }
            if (so.ApplyModifiedProperties()) RefreshPreview();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawBodyField();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(_previewRoot != null ? "Refresh Preview" : "Preview on Body"))
                RefreshPreview();
            if (_previewRoot != null && GUILayout.Button("Clear Preview")) ClearPreview();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Save & Sync to Server", GUILayout.Height(28))) {
                AssetDatabase.SaveAssets();
                PixygonPartSync.SyncNow(_selected);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBodyField() {
            var current = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString(PrefBody, ""));
            var picked = (GameObject)EditorGUILayout.ObjectField("Reference body", current, typeof(GameObject), false);
            if (picked != current)
                EditorPrefs.SetString(PrefBody, picked != null ? AssetDatabase.GetAssetPath(picked) : "");
        }

        private static List<AvatarPart> AllParts() =>
            AssetDatabase.FindAssets("t:AvatarPart")
                .Select(g => AssetDatabase.LoadAssetAtPath<AvatarPart>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).OrderBy(p => p._slot).ThenBy(p => p.name).ToList();

        private void Select(AvatarPart part) {
            _selected = part;
            Selection.activeObject = part;
            RefreshPreview();
        }

        private void CreateAsset<T>(string baseName) where T : AvatarPart {
            var folder = EditorPrefs.GetString(PrefFolder, "Assets/AvatarParts");
            if (!AssetDatabase.IsValidFolder(folder)) {
                System.IO.Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }
            var asset = CreateInstance<T>();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Select(asset);
        }

        private static void AddStat(GarmentPart g, int id, string key) {
            Undo.RecordObject(g, "Add garment stat");
            var list = (g._stats ?? new GarmentPart.GarmentStat[0]).ToList();
            if (list.Any(s => s.statId == id)) return;
            list.Add(new GarmentPart.GarmentStat { statId = id, key = key, value = 1f });
            g._stats = list.ToArray();
            EditorUtility.SetDirty(g);
        }

        // ── SceneView preview: the reference body, dressed with the selected part ──
        private void RefreshPreview() {
            ClearPreview();
            if (_selected == null) return;
            _previewRoot = new GameObject("~AvatarWorkbenchPreview") { hideFlags = HideFlags.DontSave };
            var bodyPath = EditorPrefs.GetString(PrefBody, "");
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(bodyPath);
            if (body != null) {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(body, _previewRoot.transform);
                b.transform.localPosition = Vector3.zero;
            }
            if (_selected._prefab3D != null) {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(_selected._prefab3D, _previewRoot.transform);
                p.transform.localPosition = Vector3.zero;
                if (_selected._material3D != null)
                    foreach (var r in p.GetComponentsInChildren<Renderer>())
                        r.sharedMaterial = _selected._material3D;
            }
            SceneView.lastActiveSceneView?.Frame(new Bounds(Vector3.up, Vector3.one * 2.2f), false);
        }

        private void ClearPreview() {
            if (_previewRoot != null) DestroyImmediate(_previewRoot);
            _previewRoot = null;
        }
    }
}
