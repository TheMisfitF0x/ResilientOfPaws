using LetMeGo.Scripts.PathFindingAlgo;
using UnityEditor;

namespace Plugins.RuntimePathFinding.Scripts.Editor
{
    [CustomEditor (typeof (PathSettingOnCharacter))]
    public class PathSettingOnCharacterCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector ( );
            serializedObject.ApplyModifiedProperties ( );
        }
    }
}
