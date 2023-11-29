using Plugins.RuntimePathFinding.Scripts;
using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.Character
{
    [BurstCompile]
    public struct PathConvertNodeToVector : IJobParallelFor
    {
        [ReadOnly] public NativeArray<NodeState> PathNode;
        [WriteOnly] public NativeArray<Vector3> NodePosition;

        public void Execute(int index)
        {
            NodePosition[index] = PathNode[index].GetGroundPosition;
        }
    }
}
