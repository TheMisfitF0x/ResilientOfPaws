using System;
using ProceduralPathFinding.PathFindingScripts.Character;
using UnityEditor;
using UnityEngine;

namespace Plugins.RuntimePathFinding.Scripts.Editor
{
    [CustomEditor (typeof (PathFindingComponent))]
    public class PathComponentCustomInspector : UnityEditor.Editor
    {
        public float sliderValuePositionLeft;
        public float sliderValuePositionRight;

        public void OnEnable()
        {
            PathFindingComponent pathFindingComponent = ((PathFindingComponent)target);

            sliderValuePositionLeft = pathFindingComponent.debugDisplayPosition.x;
            sliderValuePositionRight = pathFindingComponent.debugDisplayPosition.y;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector ( );
            serializedObject.ApplyModifiedProperties ( );

            EditorGUILayout.LabelField("Min Val: 0 / Max Val: 1");
            
            EditorGUILayout.MinMaxSlider(ref sliderValuePositionLeft, ref sliderValuePositionRight, 0, 1);

            ((PathFindingComponent)target).debugDisplayPosition = new Vector2(sliderValuePositionLeft, sliderValuePositionRight);
        }
    }
}
