using LetMeGo.Scripts.PathFindingAlgo;
using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Plugins.RuntimePathFinding.Scripts.PathfindingOperation
{
    [BurstCompile]
    public struct PathFindingConverterMapToPath : IJobParallelFor
    {
        [ReadOnly] public NativeList<int> nodeIndexPath;
        [ReadOnly] public NativeArray<NodeState> nodeMap;
        [WriteOnly] public NativeArray<NodeState> nodePath;

        public void Execute(int index)
        {
            nodePath[index] = nodeMap[nodeIndexPath[index]];
        }
    }
}