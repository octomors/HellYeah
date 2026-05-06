using UnityEngine;
using UnityEditor;

public class EnableGPUInstancing : MonoBehaviour
{
    [MenuItem("Tools/Enable GPU Instancing для всех материалов")]
    static void EnableInstancingForAllMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && !mat.enableInstancing)
            {
                mat.enableInstancing = true;
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ GPU Instancing включён для {count} материалов");
    }
}