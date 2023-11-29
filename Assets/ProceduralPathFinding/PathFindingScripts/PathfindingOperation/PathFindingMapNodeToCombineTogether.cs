using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ProceduralPathFinding.PathFindingScripts.PathfindingOperation
{
    [BurstCompile]
    public struct PathFindingMapNodeToCombineTogether : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<NodeState> MapsNodes;
        [ReadOnly] public NativeArray<NodeState> NodesToAdd;

        [ReadOnly] public int StartMapNodeIndex;
            
        public void Execute(int index)
        {
            if (index + StartMapNodeIndex >= MapsNodes.Length || index >= NodesToAdd.Length)
            {
                return;
            }

            NodeState node = NodesToAdd[index];
            node.UpdateNodeDataIndexList(index + StartMapNodeIndex);
                
            MapsNodes[index + StartMapNodeIndex] = node;
        }
    }
}