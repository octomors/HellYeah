#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "MeshReplacer", menuName = "Tools/Mesh Replacer")]
public class MeshReplacer : ScriptableObject
{
    [System.Serializable]
    public class SwapPair 
    {
        public Mesh oldMesh;
        public Mesh newMesh;
    }

    public List<SwapPair> pairs = new List<SwapPair>();

    [ContextMenu("Применить к активной сцене")]
    public void ApplyToScene()
    {
        if (pairs == null || pairs.Count == 0)
        {
            Debug.LogWarning("⚠️ Нет пар для замены. Настрой конфигурацию в Inspector.");
            return;
        }

        var lookup = new Dictionary<Mesh, Mesh>();
        foreach (var p in pairs)
            if (p.oldMesh != null && p.newMesh != null)
                lookup[p.oldMesh] = p.newMesh;

        // Определяем корень: префаб или сцена
        GameObject root = null;
        bool isPrefab = false;
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        
        if (prefabStage != null)
        {
            root = prefabStage.prefabContentsRoot;
            isPrefab = true;
            Debug.Log($"🔧 Работаю с префабом: {root.name}");
        }
        else
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            if (roots.Length == 0)
            {
                Debug.LogWarning("⚠️ Нет объектов в сцене!");
                return;
            }
            Debug.Log($"🎬 Работаю со сценой: {SceneManager.GetActiveScene().name}");
        }

        int replaced = 0;
        
        if (isPrefab)
        {
            // Обработка префаба
            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            foreach (var f in filters)
            {
                if (f.sharedMesh != null && lookup.TryGetValue(f.sharedMesh, out var newM))
                {
                    Undo.RecordObject(f, "Change Mesh");
                    f.sharedMesh = newM;
                    EditorUtility.SetDirty(f);
                    replaced++;
                }
            }
            // Сохраняем префаб
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
        else
        {
            // Обработка сцены
            foreach (var r in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var filters = r.GetComponentsInChildren<MeshFilter>(true);
                foreach (var f in filters)
                {
                    if (f.sharedMesh != null && lookup.TryGetValue(f.sharedMesh, out var newM))
                    {
                        Undo.RecordObject(f, "Change Mesh");
                        f.sharedMesh = newM;
                        EditorUtility.SetDirty(f);
                        replaced++;
                    }
                }
            }
        }

        Debug.Log($"✅ Готово. Заменено мешей: {replaced}");
    }
}

// === КНОПКА В ИНСПЕКТОРЕ ===
[CustomEditor(typeof(MeshReplacer))]
public class MeshReplacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MeshReplacer replacer = (MeshReplacer)target;
        
        GUILayout.Space(12);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🔄 ПРИМЕНИТЬ ЗАМЕНУ", GUILayout.Height(35), GUILayout.Width(250)))
        {
            replacer.ApplyToScene();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }
}
#endif