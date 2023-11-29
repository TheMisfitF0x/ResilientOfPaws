using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ProceduralPathFinding.PathFindingScripts.PathfindingOperation
{
    [BurstCompile]
    public struct PathFindingInvertPathData : IJobParallelFor
    {
        [WriteOnly] public NativeArray<NodeState> nodesPathInverted;

        [ReadOnly] public NativeArray<NodeState> nodesPath;
        [ReadOnly] public int lengthArray;
        
        public void Execute(int index)
        {
            nodesPathInverted[index] = nodesPath[lengthArray - index];
        }
    }
}
