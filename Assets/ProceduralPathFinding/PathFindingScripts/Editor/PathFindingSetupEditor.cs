#if UNITY_EDITOR
using Plugins.RuntimePathFinding.Scripts;
using ProceduralPathFinding.PathFindingScripts.Controller;
using UnityEditor;
using UnityEngine;

namespace UseFullManager.Scripts.Editor
{
    public class PathFindingSetupEditor : EditorWindow
    {
        [MenuItem("Window/ProcePath/Setup")]
        public static void ShowWindow()
        {
            if (FindObjectOfType(typeof(PathFindingController)) != null)
            {
                Debug.Log("ProcePath already in scene");
                return;
            }

            string[] managers = AssetDatabase.FindAssets("PathFindingController");
            if (managers.Length == 0)
            {
                Debug.Log("Not find the GameObejct PathFindingController in project");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(managers[0]);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Instantiate(go);
        }
    }
}
#endif