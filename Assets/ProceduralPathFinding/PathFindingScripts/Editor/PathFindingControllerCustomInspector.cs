using ProceduralPathFinding.PathFindingScripts.Controller;
using UnityEditor;
using UnityEngine;

namespace Plugins.RuntimePathFinding.Scripts.Editor
{
    [CustomEditor (typeof (PathFindingController))]
    public class PathFindingControllerCustomInspector : UnityEditor.Editor
    {
        private bool _debugDisplay;
        private bool _debugWithClick;

        public override void OnInspectorGUI()
        {
            Color defaultBackgroundColor = GUI.backgroundColor;
            if (_debugDisplay)
            {
                GUI.backgroundColor = Color.green;
            }
        
            if (GUILayout.Button("DisplayDebug"))
            {
                _debugDisplay = !_debugDisplay;
                _debugWithClick = _debugWithClick && _debugDisplay;
                PathFindingController.displayDebug = _debugDisplay;
                PathFindingController.debugWithClick = _debugWithClick;
            }

            GUI.backgroundColor = defaultBackgroundColor;

            if (_debugDisplay)
            {
                if (_debugWithClick)
                {
                    GUI.backgroundColor = Color.green;
                }
                
                if (GUILayout.Button("Debug with click"))
                {
                    _debugWithClick = !_debugWithClick;
                    PathFindingController.debugWithClick = _debugWithClick;
                }
                GUI.backgroundColor = defaultBackgroundColor;

                if (_debugWithClick)
                {
                    if (GUILayout.Button("Check next Node"))
                    {
                        PathFindingController.canCheckNextNode = true;
                    }
                }
            }
            else
            {
                _debugWithClick = false;
            }          
            
            GUI.backgroundColor = defaultBackgroundColor;
            DrawDefaultInspector ( );
            serializedObject.ApplyModifiedProperties ( );
        }


        public void OnEnable()
        {
            _debugDisplay = PathFindingController.displayDebug;
            _debugWithClick = PathFindingController.debugWithClick;
        }
    }
}
