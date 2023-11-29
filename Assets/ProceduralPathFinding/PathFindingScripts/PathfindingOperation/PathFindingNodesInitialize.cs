using LetMeGo.Scripts.PathFindingAlgo;
using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.PathfindingOperation
{
    [BurstCompile]
    public struct PathFindingNodesInitialize : IJobParallelFor
    {
        [WriteOnly] public NativeArray<NodeState> nodes;
        
        [ReadOnly] public Vector3 centerPosition;
        [ReadOnly] public Vector3 positionTarget;
        [ReadOnly] public Vector3 positionStart;
            
        [ReadOnly] public LayerMask layerWalkable;
        [ReadOnly] public LayerMask layerObstacle;
        [ReadOnly] public AxeType axeToUse;

        [ReadOnly] public float nodeVerticalSize;
        [ReadOnly] public float nodeHorizontaleSize;
        [ReadOnly] public float percentageGroundCheck;

        [ReadOnly] public int borderSize;
        [ReadOnly] public int travelCost;
        [ReadOnly] public int mapParentIndex;
        [ReadOnly] public int sizeDiffBetweenMapAndLimit;
        
        [ReadOnly] public bool isRaycast2D;
            
        public void Execute(int index)
        {
            NodeState node = new NodeState();
            node.InitNodeState(
                new NodeState.NodeData(centerPosition, index, borderSize, new Vector2(nodeHorizontaleSize, nodeVerticalSize), layerWalkable, layerObstacle),
                positionTarget, positionStart, percentageGroundCheck, travelCost, mapParentIndex, isRaycast2D, axeToUse);
           
            nodes[index] = node;
        }
    }
}