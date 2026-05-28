using UnityEditor;
using UnityEngine;

public static class SaveTools
{
    [MenuItem("Tools/Reset Save")]
    private static void ResetSave()
    {
        SaveService.ResetFiles();

        if (Application.isPlaying)
        {
            GameManager manager = Object.FindAnyObjectByType<GameManager>();
            if (manager != null)
            {
                manager.LoadFromDisk();
            }
        }

        Debug.Log("Save files were reset.");
    }
}
