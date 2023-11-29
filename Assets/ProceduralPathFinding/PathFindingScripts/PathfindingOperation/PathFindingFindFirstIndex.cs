using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.PathfindingOperation
{
    [BurstCompile]
    public struct PathFindingFindFirstIndex
    {
        [ReadOnly] public NativeArray<NodeState> nodeStates;
        [ReadOnly] public Vector3 positionStart;

        private float _currentDist;
        private int _currentNearestIndex;

        public int Execute()
        {
            _currentDist = int.MaxValue;

            int middleLength = (int) (nodeStates.Length * 0.5f);
            _currentNearestIndex = middleLength;
            for (int i = 0; i < middleLength; i++)
            {
                if(IsFirstNodeCheck(middleLength + i))
                {
                    return  middleLength + i;
                }

                if (IsFirstNodeCheck(middleLength - i))
                {
                    return middleLength - i;
                }
            }

            return _currentNearestIndex;
        }

        private bool IsFirstNodeCheck(int nodeIndex)
        {
            NodeState node = nodeStates[nodeIndex];
            
            float distToPoStartHorizontale = Mathf.Max(Mathf.Abs(positionStart.x - node.GetPosition.x),Mathf.Abs(positionStart.z - node.GetPosition.z));
            float distPosStartVertical = Mathf.Abs(positionStart.y - node.GetPosition.y);

            if (distToPoStartHorizontale + distToPoStartHorizontale < distPosStartVertical)
            {
                _currentNearestIndex = nodeIndex;
                _currentDist = distToPoStartHorizontale + distToPoStartHorizontale;
            }
            
            return distToPoStartHorizontale <= node.GetNodeData.nodeSize.x * 0.5f && distPosStartVertical <= node.GetNodeData.nodeSize.y * 0.5f;
        }
    }
}
