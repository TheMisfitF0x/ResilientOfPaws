using System;
using LetMeGo.Scripts.PathFindingAlgo;
using Plugins.RuntimePathFinding.Scripts;
using Plugins.RuntimePathFinding.Scripts.PathfindingOperation;
using ProceduralPathFinding.PathFindingScripts.Controller;
using ProceduralPathFinding.PathFindingScripts.PathfindingOperation;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.Character
{
    public struct PathFindingData
    {
        #region Variables
        public NativeList<NodeState> nodeStates;
        public PathFindingController.PathProcess pathFindingProcess;

        public Vector3 targetPoint;
        public Vector3 startPoint;

        public float timeDelayCheckNode;
        public float distanceToCheckWithNodePosition;
        public float lastDistance;
        public int currentIndexOnArrayPath;
        public int maxNodeToCheckOnPath;

        private Vector3 _directionToNode;
        private bool _isDataValide;
        #endregion

        #region Public
        public void DisposeData()
        {
            #if UNITY_EDITOR
            try
            {
                nodeStates.Dispose();
            }
            catch 
            {
               
            }            
            #else
            nodeStates.Dispose();
            #endif
            _isDataValide = false;
        }

       /* public void UpdateComponentEntityConversion(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            DynamicBuffer<NodeState> buffers = entityManager.GetBuffer<NodeState>(entity);
            buffers.Clear();
            buffers.AddRange(nodeStates);

            entityManager.RemoveComponent<PathEntityData>(entity);
            entityManager.AddComponentData(entity, new PathEntityData()
            {
                pathFindingProcess = pathFindingProcess
            });
        }*/
        
        /// <summary>
        /// Adding nodes on current list of nodes (path to the target)
        /// </summary>
        /// <param name="nodes">nodes to add</param>
        /// <param name="currentProcess">current path statut</param>
        public void AddPathNode(NativeArray<NodeState> nodes, PathFindingController.PathProcess currentProcess)
        {
            _isDataValide = true;
            pathFindingProcess = currentProcess;
            PathFindingInvertPathData reOrderPath = new PathFindingInvertPathData()
            {
                nodesPath = nodes,
                nodesPathInverted = new NativeArray<NodeState>(nodes.Length, Allocator.TempJob),
                lengthArray = nodes.Length - 1
            };

            JobHandle jobHandle = reOrderPath.Schedule(nodes.Length, 64);
            jobHandle.Complete();

            nodeStates.AddRange(reOrderPath.nodesPathInverted);
            reOrderPath.nodesPathInverted.Dispose();
        }
        
        /// <summary>
        /// check if nodes on path has new status
        /// </summary>
        /// <returns>True if one of nodes has new status</returns>
        public bool CheckIfPathNodesSwitchType()
        {
            if (!_isDataValide)
            {
                return false;
            }
            
            int length = nodeStates.Length;
            for (int i = currentIndexOnArrayPath; i < Mathf.Min(currentIndexOnArrayPath + maxNodeToCheckOnPath, length); i++)
            {
                if (nodeStates[i].CheckIfNodeTypeHasNewValue())
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Current direction between current position to the current nodes target
        /// </summary>
        /// <param name="groundPosition">player ground position</param>
        /// <param name="typeNodeDirection">type direction calcul</param>
        /// <param name="nodeSize">size nodes</param>
        /// <param name="distance">distance between position and nodes</param>
        /// <returns>direction to go</returns>
        public Vector3 CurrentDirectionWithDistance(Vector3 groundPosition, TypeNodeDirection typeNodeDirection, Vector2 nodeSize, out float distance)
        {
            if (currentIndexOnArrayPath == nodeStates.Length)
            {
                distance = -1;
                return Vector3.zero;
            }

            Vector3 currentTargetGround = nodeStates[currentIndexOnArrayPath].GetGroundPosition;
            Vector3 direction = currentTargetGround - groundPosition;
            distance = direction.magnitude;

            if (currentIndexOnArrayPath > 0 && typeNodeDirection == TypeNodeDirection.BetweenLastNodeToNextNode)
            {
                Vector3 dirNode = currentTargetGround - nodeStates[currentIndexOnArrayPath - 1].GetGroundPosition;
                float dotValue = Vector3.Dot(dirNode, direction);
                if (dotValue  > 0)
                {
                    direction = dirNode;
                }
            }

            float distCurrentIndex = Vector3.Distance(currentTargetGround, groundPosition);
            if (currentIndexOnArrayPath < nodeStates.Length - 1)
            {
                lastDistance =  distCurrentIndex;
                
                float distNextIndex = Vector3.Distance(nodeStates[currentIndexOnArrayPath + 1].GetGroundPosition, groundPosition);
                if(distCurrentIndex - Mathf.Max(nodeSize.x, nodeSize.y) * 0.5f >= distNextIndex || distance <= distanceToCheckWithNodePosition)
                {
                    lastDistance = float.MaxValue;
                    currentIndexOnArrayPath++;
                    _directionToNode = direction;
                }
            }
            else if (distCurrentIndex >= lastDistance)
            {
                currentIndexOnArrayPath++;
                return Vector3.zero;
            }
            else
            {
                lastDistance = distCurrentIndex;
            }
            return direction;
        }

        /// <summary>
        /// Get current nodes 
        /// </summary>
        /// <returns></returns>
        public NodeState CurrentNodeState()
        {
            return nodeStates[currentIndexOnArrayPath];
        }
        #endregion
    }
}
