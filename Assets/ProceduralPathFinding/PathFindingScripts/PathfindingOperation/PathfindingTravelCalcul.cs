using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Collections;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.PathfindingOperation
{
    public struct PathfindingTravelCalcul
    {
        #region Variables
        public NativeList<NodeState> NodesPathSaved;
        public NativeArray<NodeState> MapNodes;
        public NativeList<int> NodesPath;

        [ReadOnly] public Vector3 PositionTarget;

        [ReadOnly] public int BorderSize;
        [ReadOnly] public int TotalMapVerticalIndex;
            
        [ReadOnly] public int IndexOfFirstNode;
        [ReadOnly] public int MapLengthNodes;
        [ReadOnly] public int NumberOfCaseCharacterCanFall;
        [ReadOnly] public float MAXVerticalSizeGroundAutoClimb;
        [ReadOnly] public int PresiceVerticalDetection;

        [ReadOnly] public float MAXAngleAvailableToWalkUp;
        [ReadOnly] public float MAXAngleAvailableToWalkDown;

        [ReadOnly] public bool CharacterCanFly;
        [ReadOnly] public bool CharacterCanFall;
        [ReadOnly] public bool MapDiffLimit;
        
        [WriteOnly] public bool FindPathToTarget;
        [WriteOnly] public bool UseDiagonalMove;
        [WriteOnly] public bool EndOfTravelCalcul;

        private float _costTargetToPath;
        private float _currentDistanceToTarget;
            
        private int _currentNodeIndexNearestTarget;
        #endregion
        
        public void Execute()
        {
            if (NodesPath.Length == 0)
            {
                EndOfTravelCalcul = false;
                _costTargetToPath = float.MaxValue;
                _currentDistanceToTarget = float.MaxValue;
                    
                _currentNodeIndexNearestTarget = -1;
            
                NodeState node = MapNodes[IndexOfFirstNode];
                node.SetUpFirstNode();
                MapNodes[IndexOfFirstNode] = node;
                
                NodesPath.Add(node.GetNodeData.nodeIndexOnList);
            }

            if (!LoopCurrentNodePath())
            {
                return;
            }
            
            PathFromParentNode();
            EndOfTravelCalcul = true;
        }
            
        public void DisposeNativeList()
        {
            NodesPath.Dispose();
            NodesPathSaved.Dispose();
            MapNodes.Dispose();
        }

        /// <summary>
        /// Check node around node parent index
        /// </summary>
        /// <param name="nodeParent">node index</param>
        private void CheckNeighborNode(int nodeParent)
        {
            Vector2Int nodeIndex = MapNodes[nodeParent].GetNodeData.nodeIndex;

            for (int i = -1; i < 2; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                
                bool neighborNodeWalkableI = false;
                Vector2Int iVector = new Vector2Int(nodeIndex.x + i, nodeIndex.y);
                if (IsNeighBorNodeAvailable(nodeParent, iVector, false, ref neighborNodeWalkableI))
                {
                    return;
                }

                for (int j = -1; j < 2; j++)
                {
                    if (j == 0)
                    {
                        continue;
                    }
                    
                    Vector2Int jVector = new Vector2Int(nodeIndex.x, nodeIndex.y + j);
                    bool neighborNodeWalkableJ = false;
                    if (IsNeighBorNodeAvailable(nodeParent, jVector, false, ref neighborNodeWalkableJ))
                    {
                        return;
                    }

                    if (neighborNodeWalkableI && neighborNodeWalkableJ && CheckNodeOnSide(nodeIndex, nodeParent, i, j))
                    {
                        return;
                    }
                }   
            }
        }

        /// <summary>
        /// Check node on side
        /// </summary>
        /// <param name="nodeIndex">node parent index position</param>
        /// <param name="nodeParent">node parent index</param>
        /// <param name="xValue">bonus x value</param>
        /// <param name="yValue">bonus y value</param>
        /// <returns>true is node is target position</returns>
        private bool CheckNodeOnSide(Vector2Int nodeIndex, int nodeParent, int xValue, int yValue)
        {
            if (UseDiagonalMove)
            {
                bool nodeWalkable = true;
                if (IsNeighBorNodeAvailable(nodeParent, new Vector2Int(nodeIndex.x + xValue, nodeIndex.y + yValue), true, ref nodeWalkable))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Check node vertical neighbor
        /// </summary>
        /// <param name="nodeParentNeighBor">node neighbor index</param>
        /// <param name="nodeParentIndex">node parent index</param>
        /// <param name="diagonalMove">is a diagonal move or not</param>
        /// <returns></returns>
        private bool CheckNodeWithCharacterFly(int nodeParentNeighBor, int nodeParentIndex, bool diagonalMove)
        {
            NodeState nodeParent = MapNodes[nodeParentNeighBor];
            NodeState.NodeType nodeParentType = nodeParent.GetNodeType;
            MapNodes[nodeParentNeighBor] = nodeParent;
            
            if (!CharacterCanFly)
            {
                if (CharacterCanFall && CheckCharacteFall(nodeParentIndex))
                {
                    return true;
                }
            }
            else if (nodeParentType == NodeState.NodeType.Empty && nodeParent.GetMapIndex > 0 && nodeParent.manualNodeAvailable)
            {
                if (CheckNodeConditionAvailableForPath(nodeParentNeighBor - MapLengthNodes, nodeParentIndex, diagonalMove))
                {
                    return true;
                }
            }
                    
            if (nodeParent.GetMapIndex < TotalMapVerticalIndex)
            {
                int upNodeIndex = nodeParentNeighBor + MapLengthNodes;
                NodeState nodeUp = MapNodes[upNodeIndex];
                MapNodes[upNodeIndex] = nodeUp;

                if ((nodeUp.GetNodeType == NodeState.NodeType.Empty || nodeParent.CheckUpNeighBorToThisPathGround(nodeUp, PresiceVerticalDetection))
                    && CheckNodeConditionAvailableForPath(upNodeIndex, nodeParentIndex, diagonalMove))
                {
                    return true;
                }
            }
                
            return false;
        }
        
        //todo check raycast between two node (node up against parent down), to check if it's connected

        /// <summary>
        /// Check node empty collide under node parent index
        /// </summary>
        /// <param name="nodeParent">node parent index</param>
        /// <returns></returns>
        private bool CheckCharacteFall(int nodeParent)
        {
            NodeState node = MapNodes[nodeParent];
            NodeState.NodeType nodeType = node.GetNodeType;
            MapNodes[nodeParent] = node;

            if (nodeType != NodeState.NodeType.Empty && node.GetMapIndex == 0)
            {
                return false;
            }
            
            int indexCheck = 0;
            int nodeParentIndex = node.GetNodeParent;
            while(nodeParentIndex != -1 && MapNodes[nodeParentIndex].GetNodeType != NodeState.NodeType.Walkable)
            {
                if (nodeParent + MapLengthNodes != nodeParentIndex)
                {
                    return false;
                }
                nodeParentIndex = MapNodes[nodeParentIndex].GetNodeParent;
                indexCheck++;
                if (indexCheck >= NumberOfCaseCharacterCanFall)
                {
                    return false;
                }
            }
            
            if (nodeType == NodeState.NodeType.Empty && node.GetMapIndex > 0)
            {
                int underNode = nodeParent - MapLengthNodes;
                NodeState underNodeState = MapNodes[underNode];
                
                bool isNodeEmpty = underNodeState.GetNodeType == NodeState.NodeType.Empty;
                if (isNodeEmpty && underNodeState.CheckNodeParent(nodeParent, node.GetCost, false))
                {
                    MapNodes[underNode] = underNodeState; 
                    return AddNodeToPath(nodeParent - MapLengthNodes);
                }
                
                MapNodes[underNode] = underNodeState; 
                if (CheckNodeConditionAvailableForPath(underNode, nodeParent, false))
                {
                    return true;
                }
            }

            return false;
        }

        // TODO next update
       /* private bool CheckOtherNodeCharacterAbilities(int nodeParentNeighBor, int nodeParent, bool diagonalMove)
        {
            NodeState node = mapNodes[nodeParentNeighBor];
            NodeState.NodeType nodeType = node.GetNodeType;
            mapNodes[nodeParentNeighBor] = node;

            if (nodeType != NodeState.NodeType.Empty)
            {
                return false;
            }
            
            return false;
        }*/
        
       /// <summary>
       /// Check current neighbor node
       /// </summary>
       /// <param name="nodeParent">neighbor node</param>
       /// <param name="targetIndexNeighbor">neighbor node index</param>
       /// <param name="diagonalMove">is a diagonal move or not</param>
       /// <param name="nodeWalkable">ref current node type if walkable or not</param>
       /// <returns></returns>
        private bool IsNeighBorNodeAvailable(int nodeParent, Vector2Int targetIndexNeighbor, bool diagonalMove, ref bool nodeWalkable)
        {
            int indexNodexList = MapNodes[nodeParent].NodeIndexList(targetIndexNeighbor, BorderSize);

            if (indexNodexList < 0)
            {
                return false;
            }
            
            indexNodexList += MapLengthNodes * MapNodes[nodeParent].GetMapIndex;
            if (CheckNodeConditionAvailableForPath(indexNodexList, nodeParent, diagonalMove, ref nodeWalkable))
            {
                return true;
            }

            return CheckNodeWithCharacterFly(indexNodexList, nodeParent, diagonalMove);
        }

        /// <summary>
        /// Check if neighbor node is walkable
        /// </summary>
        /// <param name="nodeNeighborIndex">neighbor node index</param>
        /// <param name="nodeParentIndex">node parent index</param>
        /// <param name="diagonalMove">is diagonal move or not</param>
        /// <returns></returns>
        private bool CheckNodeConditionAvailableForPath(int nodeNeighborIndex, int nodeParentIndex, bool diagonalMove)
        {
            bool nodeWalkable = false;
            return CheckNodeConditionAvailableForPath(nodeNeighborIndex, nodeParentIndex, diagonalMove, ref nodeWalkable);
        }

        /// <summary>
        /// Check if neighbor node is walkable
        /// </summary>
        /// <param name="nodeNeighborIndex">neighbor node index</param>
        /// <param name="nodeParentIndex">node parent index</param>
        /// <param name="diagonalMove">is diagonal move or not</param>
        /// <param name="isTypeNodeWalkable">ref if node type is walkable</param>
        /// <returns></returns>
        private bool CheckNodeConditionAvailableForPath(int nodeNeighborIndex, int nodeParentIndex, bool diagonalMove, ref bool isTypeNodeWalkable)
        {
            PathFindingController.currentCalculeIndex++;
            
            NodeState parentNode = MapNodes[nodeParentIndex];
            NodeState neighborNode = MapNodes[nodeNeighborIndex];
            
            NodeState.NodeType nodeType = neighborNode.GetNodeType;
            bool nodeNeighborAvailable = neighborNode.CheckNodeParent(nodeParentIndex, parentNode.GetCost, diagonalMove);

            isTypeNodeWalkable = nodeType == NodeState.NodeType.Walkable || (CharacterCanFly || parentNode.GetNodeType == NodeState.NodeType.Walkable && parentNode.GetMapIndex >= neighborNode.GetMapIndex) && nodeType == NodeState.NodeType.Empty;
        
            MapNodes[nodeNeighborIndex] = neighborNode;

            if (!nodeNeighborAvailable || !isTypeNodeWalkable || !neighborNode.manualNodeAvailable)
            {
#if UNITY_EDITOR
                if (PathFindingController.displayDebug)
                {
                    Debug.DrawLine(parentNode.GetPosition, neighborNode.GetPosition, PathFindingController.instance.pathNotWalkable, 5);
                }
#endif
                return false;
            }

#if UNITY_EDITOR
            if (PathFindingController.displayDebug)
            {
                Debug.DrawLine(parentNode.GetPosition, neighborNode.GetPosition, PathFindingController.instance.pathWalkable, 5);
            }
#endif

            if (!CheckValideNodeGroundCondition(nodeNeighborIndex, nodeParentIndex))
            {
                return false;
            }

            return AddNodeToPath(nodeNeighborIndex);
        }

        /// <summary>
        /// Check neighbor node ground available or not
        /// </summary>
        /// <param name="nodeNeighbor">neighbor node index</param>
        /// <param name="nodeParent">node parent index</param>
        /// <returns></returns>
        private bool CheckValideNodeGroundCondition(int nodeNeighbor, int nodeParent)
        {
            NodeState neighborNode = MapNodes[nodeNeighbor];
            NodeState parentNode = MapNodes[nodeParent];

            if (parentNode.GetNodeType == NodeState.NodeType.Empty &&
                parentNode.GetPosition.y <= neighborNode.GetPosition.y && neighborNode.GetNodeType != NodeState.NodeType.Empty)
            {
                return false;
            }

            if (CharacterCanFly)
            {
                return true;
            }

            if (neighborNode.GetNodeType == NodeState.NodeType.Empty)
            {
                return true;
            }

            // vertical heigh check
            if (neighborNode.GetGroundPosition.y - parentNode.GetGroundPosition.y > MAXVerticalSizeGroundAutoClimb)
            {
                if (!parentNode.CheckVerticalAutoClimb(neighborNode.GetGroundPosition, MAXVerticalSizeGroundAutoClimb,
                    PresiceVerticalDetection))
                {
                    #if UNITY_EDITOR
                    neighborNode.walkStatus = NodeState.NodeWalkStatus.VerticalNotGood;
                    MapNodes[nodeNeighbor] = neighborNode;
                    #endif
                   
                    return false;
                }
            }

            if (!parentNode.CheckAngleWithNode(neighborNode, MAXAngleAvailableToWalkDown, MAXAngleAvailableToWalkUp))
            {
                #if UNITY_EDITOR
                neighborNode.walkStatus = NodeState.NodeWalkStatus.AngleNotGood;
                MapNodes[nodeNeighbor] = neighborNode;
                #endif

                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Add neighbor on path node to continue checking
        /// </summary>
        /// <param name="nodeNeighbor">neighbor node index</param>
        /// <returns></returns>
        private bool AddNodeToPath(int nodeNeighbor)
        {
            NodeState neighborNode = MapNodes[nodeNeighbor];
            
#if UNITY_EDITOR
            neighborNode.walkStatus = NodeState.NodeWalkStatus.CanWalk;
            MapNodes[nodeNeighbor] = neighborNode;
#endif
            int nodeNeighborIndex = neighborNode.GetNodeData.nodeIndexOnList;

            if (neighborNode.IsTargetPathNode && neighborNode.GetCost < _costTargetToPath)
            {
                FindPathToTarget = true;

                _costTargetToPath = neighborNode.GetCost;
                _currentNodeIndexNearestTarget = nodeNeighborIndex;
                        
                NodesPath.Clear();
                return true;
            }

            float distance = Vector3.Distance(PositionTarget, neighborNode.GetPosition);
            if (distance < _currentDistanceToTarget)
            {
                _currentDistanceToTarget = distance;
                _currentNodeIndexNearestTarget = nodeNeighborIndex;
            }
            else if (MapDiffLimit && distance - _currentDistanceToTarget > neighborNode.GetNodeData.nodeSize.x)
            {
                return false;
            }
            
            NodesPath.Add(nodeNeighborIndex);

            return false;
        }

        /// <summary>
        /// Get next node
        /// </summary>
        /// <returns></returns>
        private int NextNode()
        {
            JobGetNextNodeOnPath jobPath = new JobGetNextNodeOnPath()
            {   
                mapNodes = MapNodes,
                nodesPath = NodesPath,
               // minCostValue = int.MaxValue,
                minCostTravel = int.MaxValue,
                indexNodeTarget = 0
            };

            for (int i = 0; i < NodesPath.Length; i++)
            {
                jobPath.Execute(i);   
            }
           // JobHandle sheduleParralelJobHandle = jobPath.ScheduleParallel(nodesPath.Length, 64, new JobHandle()); 
            //sheduleParralelJobHandle.Complete();

            
            return jobPath.indexNodeTarget;
        }
            
        /// <summary>
        /// Continue Checking node
        /// </summary>
        /// <returns></returns>
        private bool LoopCurrentNodePath( )
        {
            while (NodesPath.Length > 0 && !FindPathToTarget)
            {
#if UNITY_EDITOR
                if (PathFindingController.debugWithClick && !PathFindingController.canCheckNextNode)
                {
                    return false;
                }

                PathFindingController.canCheckNextNode = false;
#endif
                
                int nodePathIndex = NextNode();
                int nodeIndex = NodesPath[nodePathIndex];
                NodesPath.RemoveAt(nodePathIndex);
                CheckNeighborNode(nodeIndex);

                if (PathFindingController.currentCalculeIndex > PathFindingController.maxCalculOnFrame)
                {
                    return FindPathToTarget || NodesPath.Length == 0;
                }
            }

            return true;
        }

        #region AfterReachTarget
        /// <summary>
        /// re create target path to target node to start node 
        /// </summary>
        private void PathFromParentNode()
        {
            if(!FindPathToTarget)
            {
                if (_currentNodeIndexNearestTarget == -1)
                {
                    return;
                }
            }

            NodesPath.Clear();
            NodeState node = MapNodes[_currentNodeIndexNearestTarget];
            
            if (node.GetNodeType != NodeState.NodeType.Walkable)
            {
                node = MapNodes[node.GetNodeParent]; 
            }

            while (NodeParent(node) && node.GetNodeParent != node.GetNodeData.nodeIndexOnList)
            {
                node = MapNodes[node.GetNodeParent];
            }
        }

        /// <summary>
        /// Check if node parent is the same with the start node
        /// </summary>
        /// <param name="currentNode">current node</param>
        /// <returns></returns>
        private bool NodeParent (NodeState currentNode)
        {
#if UNITY_EDITOR
            currentNode.isValideForPath = true;
            MapNodes[currentNode.GetNodeData.nodeIndexOnList] = currentNode;
#endif
            
            NodesPath.Add(currentNode.GetNodeData.nodeIndexOnList);
                
            int indexOnList = currentNode.GetNodeData.nodeIndexOnList;
            if (indexOnList == -1 || currentNode.GetNodeData.nodeIndexOnList == IndexOfFirstNode)
            {
                return false;
            }

            return true;
        }
        #endregion

        
        //[BurstCompile]
        private struct JobGetNextNodeOnPath //: IJobFor
        {
            [ReadOnly] public NativeArray<NodeState> mapNodes;
            [ReadOnly] public NativeList<int> nodesPath;
   
            [WriteOnly] public int indexNodeTarget;
             
            public float minCostTravel;
        
            public void Execute(int index)
            {
                NodeState nodes = mapNodes[nodesPath[index]];
                float nodeDir = nodes.GetCaseSpaceWithTravelCost;
                
                if (minCostTravel > nodeDir)
                {
                    minCostTravel = nodeDir;
                    indexNodeTarget = index;
                }
            }
        }
    }

}
