using LetMeGo.Scripts.PathFindingAlgo;
using Plugins.RuntimePathFinding.Scripts;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.Controller
{
    [System.Serializable]
    public struct NodeState
    {
        #region Variables
        public bool manualNodeAvailable;

        #if UNITY_EDITOR
        public NodeWalkStatus walkStatus;
        public bool isValideForPath;
        #endif
        
        private Vector3 _nodeGroundPosition;
        private Vector3 _nodeGroundAngle;
        private Vector3 _nodePosition;
        private Vector3 _currentNodeDirection;
        private Vector3 _upVectorDirection;
        
        private NodeType _nodeType;
        private NodeData _nodeData;

        //private NodeCharacterState _nodeCharacterState;

        private float _caseSpaceWithTravelCost;
        private float _percentageGroundSize;
        private float _pourcNodeCostBonus;
        
        private int _nodeParentIndex;
        private int _mapParentIndex;
        private int _currentCost;
        
        private bool _isTargetPathNode;
        private bool _is2DRaycast;

        
        public NodeData GetNodeData => _nodeData;
        public NodeType GetNodeType => CheckNodeType();
        
       /* public NodeCharacterState GetNodeCharaState
        {
            get => _nodeCharacterState;
            set => _nodeCharacterState = value;
        }*/

        public Vector3 GetNodeDirection => _currentNodeDirection;
        public Vector3 GetPosition => _nodePosition;
        
        public Vector3 GetGroundPosition => _nodeGroundPosition;
        public Vector3 GetGroundAngle => _nodeGroundAngle;
        
        public float GetCaseSpaceWithTravelCost => _caseSpaceWithTravelCost;
        
        public int GetCost => _currentCost;
        
        public int GetNodeParent => _nodeParentIndex;
        public int GetMapIndex => _mapParentIndex;

        public bool IsTargetPathNode => _isTargetPathNode;

       /* public enum NodeCharacterState
        {
            Nothing,
            Fly,
            Fall,
            Walk,
            Climb,
            Jump
        }*/
        
#if UNITY_EDITOR
       public enum NodeWalkStatus
       {
            CanWalk,
            AngleNotGood,
            VerticalNotGood
       }
#endif
        
        public enum NodeType
        {
            NoLoaded,
            Walkable,
            Obstacle,
            Empty
        }
        
        public struct NodeData
        {
            public Vector3 centerPosition;
            public Vector2 nodeSize;
            public Vector2Int nodeIndex;

            public int nodeIndexOnList;
            public LayerMask layerWalkable;
            public LayerMask layerObstacle;
            
            public NodeData(Vector3 position, int index, int borderSize, Vector2 nodeSize, LayerMask layerWalkable, LayerMask layerObstacle)
            {
                centerPosition = position;
                nodeIndexOnList = index;

                float percentageWithLarger = (float) index / borderSize;
                int yIndex = Mathf.FloorToInt(percentageWithLarger);
                int xIndex = Mathf.RoundToInt((percentageWithLarger - yIndex) * borderSize);

                nodeIndex = new Vector2Int(xIndex, yIndex);

                this.nodeSize = nodeSize;
                
                this.layerWalkable = layerWalkable;
                this.layerObstacle = layerObstacle;
            }
        }
        #endregion

        #region Public
        /// <summary>
        /// Initialise node value
        /// </summary>
        /// <param name="data">position and layer value</param>
        /// <param name="positionTarget">target position</param>
        /// <param name="positionStart">player position</param>
        /// <param name="percentageSizeGround">size ground percentage with node size</param>
        /// <param name="travelCost">cost travel</param>
        /// <param name="mapIndex">vertical index on map</param>
        /// <param name="raycast2D">2D raycast or not</param>
        /// <param name="axeToUse">axe to use</param>
        public void InitNodeState(NodeData data, Vector3 positionTarget, Vector3 positionStart, float percentageSizeGround, int travelCost, int mapIndex, bool raycast2D, AxeType axeToUse)
        {
            _percentageGroundSize = percentageSizeGround;
            _nodeType = NodeType.NoLoaded;
            _nodeData = data;

            _is2DRaycast = raycast2D;
            _upVectorDirection = Vector3.up;
            _nodePosition = data.centerPosition + new Vector3(data.nodeIndex.x, 0, data.nodeIndex.y) * data.nodeSize.x;
            
            /*switch (axeToUse)
            {
                case AxeType.XYZ: //case AxeType.XZ:
                    _upVectorDirection = Vector3.up;
                    _nodePosition = data.centerPosition + new Vector3(data.nodeIndex.x, 0, data.nodeIndex.y) * data.nodeSize.x;
                    break;
                case AxeType.XY:
                    _upVectorDirection = new Vector3(0,0, 1);
                    _nodePosition = data.centerPosition + new Vector3(data.nodeIndex.x, data.nodeIndex.y, 0) * data.nodeSize.x;
                    break;
                case AxeType.YZ:
                    _upVectorDirection = new Vector3(1,0, 0);
                    _nodePosition = data.centerPosition + new Vector3(0, data.nodeIndex.x, data.nodeIndex.y) * data.nodeSize.x;
                    break;
            }*/

            _nodeGroundPosition = _nodePosition;
            
            float horizontalDistance = Vector3.Distance(new Vector3(_nodePosition.x, 0, _nodePosition.z), new Vector3(positionTarget.x, 0, positionTarget.z));
            _caseSpaceWithTravelCost = (horizontalDistance / _nodeData.nodeSize.x) * travelCost;
            _caseSpaceWithTravelCost += Mathf.Abs(_nodePosition.y - positionTarget.y) / _nodeData.nodeSize.y * travelCost;

            Vector3 direction = (positionTarget - positionStart).normalized;
            _pourcNodeCostBonus = Mathf.Abs(Vector3.Dot(direction, (_nodePosition - positionStart).normalized) - 1);
            
            float distToTargetHorizontale = Mathf.Max(Mathf.Abs(positionTarget.x - _nodePosition.x),Mathf.Abs(positionTarget.z - _nodePosition.z));
            _isTargetPathNode = distToTargetHorizontale <= _nodeData.nodeSize.x * 0.5f && Mathf.Abs(positionTarget.y - _nodePosition.y) <= _nodeData.nodeSize.y * 0.5f;
          
            _nodeParentIndex = -1;
            _mapParentIndex = mapIndex;
            _currentCost = int.MaxValue;

            manualNodeAvailable = true;

            #if UNITY_EDITOR
            isValideForPath = false;
            #endif
        }

        /// <summary>
        /// Setup node on player position
        /// </summary>
        public void SetUpFirstNode()
        {
            CheckNodeType();
            _nodeType = NodeType.Walkable;

            _currentCost = 0;
            manualNodeAvailable = false;
   
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Debug.DrawLine(_nodePosition, _nodePosition - Vector3.up * 2, Color.blue, 10);
                Debug.DrawLine(_nodePosition, _nodePosition + Vector3.right * 2, Color.blue, 10);
            }
#endif
        }

        /// <summary>
        /// Node index on total node list
        /// </summary>
        /// <param name="indexList"></param>
        public void UpdateNodeDataIndexList(int indexList)
        {
            _nodeData.nodeIndexOnList = indexList;
        }

        /// <summary>
        ///  check if node has new value
        /// </summary>
        /// <returns>true if node has new value</returns>
        public bool CheckIfNodeTypeHasNewValue()
        {
            NodeType saveNodeType = _nodeType;
            _nodeType = NodeType.NoLoaded;
            
            CheckNodeType();

            return saveNodeType != _nodeType;
        }

        /// <summary>
        /// Check if node is valide to go depending of the parent and his data
        /// </summary>
        /// <param name="nodeParent">node parent index</param>
        /// <param name="parentCost">parent current cost value</param>
        /// <param name="diagonalMove">is diagonal move or not</param>
        /// <returns></returns>
        public bool CheckNodeParent(int nodeParent, int parentCost, bool diagonalMove)
        {
            if (!manualNodeAvailable || nodeParent == _nodeParentIndex)
            {
                return false;
            }
            
            float bonusMoveCost = 1;
            if (diagonalMove)
            {
                bonusMoveCost = 1.5f;
            }

            float caseMove = NodeCost() * bonusMoveCost;
            int otherCost = Mathf.CeilToInt(parentCost + caseMove);
            if (_currentCost <= otherCost)
            {
                return false;
            }

            _nodeParentIndex = nodeParent;
            _currentCost = otherCost;
            return true;
        }

        /// <summary>
        /// Check if neighbor node on up is valide to go with the ground
        /// </summary>
        /// <param name="upNeighbor">node to compare</param>
        /// <param name="precizeDist">number of point to test between two nodes</param>
        /// <returns></returns>
        public bool CheckUpNeighBorToThisPathGround(NodeState upNeighbor, float precizeDist)
        {
            float distToCheck = upNeighbor.GetGroundPosition.y - _nodeGroundPosition.y;
            Vector3 targetPosThis = new Vector3(_nodeGroundPosition.x, upNeighbor.GetGroundPosition.y + 0.01f, _nodeGroundPosition.z);

            float distMagnitudeToCheck = (upNeighbor.GetGroundPosition - GetGroundPosition).magnitude * 0.01f;
            for (int i = 1; i < precizeDist; i++)
            {
                Vector3 currentPointA = Vector3.Lerp(upNeighbor.GetGroundPosition, targetPosThis, i / precizeDist);
                Vector3 pointA = GetCustomPointOnPos(currentPointA, Vector3.down, _nodeData.layerWalkable, distToCheck);

                Vector3 currentPointB = Vector3.Lerp(upNeighbor.GetGroundPosition, targetPosThis, i / precizeDist);
                currentPointB.y = _nodeGroundPosition.y;
                
                Vector3 pointB = GetCustomPointOnPos(currentPointB, Vector3.down, _nodeData.layerWalkable, distToCheck);
                
                if (Physics.Raycast(currentPointB, Vector3.down, out RaycastHit hitB, distToCheck, _nodeData.layerWalkable))
                {
                    pointB = hitB.point;
                }

                if ((pointA - pointB).magnitude < distMagnitudeToCheck)
                {
                    return true;
                }
            }
        
            return false;
        }
        
        /// <summary>
        /// Check the angle ground of nodes
        /// </summary>
        /// <param name="neighborNode">node to check</param>
        /// <param name="maxAngleAvailableToWalkDown">max up angle</param>
        /// <param name="maxAngleAvailableToWalkUp">max down angle</param>
        /// <returns>nodes is walkable</returns>
        public bool CheckAngleWithNode(NodeState neighborNode, float maxAngleAvailableToWalkDown, float maxAngleAvailableToWalkUp)
        {
            float angleValue = Mathf.Max(0, Vector3.Angle(Vector3.up, neighborNode.GetGroundAngle));

            float angleTarget = maxAngleAvailableToWalkDown;
            if (neighborNode.GetGroundPosition.y > GetGroundPosition.y)
            {
                angleTarget = maxAngleAvailableToWalkUp;
            }
            
            if (angleValue > angleTarget)
            {
#if UNITY_EDITOR
                if (PathFindingController.displayDebug)
                {
                    Debug.DrawLine(GetGroundPosition, neighborNode.GetGroundPosition, PathFindingController.instance.caseNodeWalkNotGoodAngle, 5);
                }
#endif
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Check if node can be auto climb depending of vertical distance
        /// </summary>
        /// <param name="neighborNodeGroundPosition">neighbor node ground position</param>
        /// <param name="maxVerticalSizeGroundAutoClimb">max vertical distance available</param>
        /// <param name="presiceVerticalDetection">number of point to test</param>
        /// <returns>node is walkable</returns>
        public bool CheckVerticalAutoClimb(Vector3 neighborNodeGroundPosition, float maxVerticalSizeGroundAutoClimb, int presiceVerticalDetection)
        {
            Vector3 lastValue = GetCustomPointOnPos(GetGroundPosition, Vector3.down, _nodeData.layerWalkable, maxVerticalSizeGroundAutoClimb);
            for (int i = 1; i < presiceVerticalDetection; i++)
            {
                Vector3 point = Vector3.Lerp(GetGroundPosition, neighborNodeGroundPosition, (float) i / (presiceVerticalDetection - 1));
                Vector3 newValue = GetCustomPointOnPos(point, Vector3.down, _nodeData.layerWalkable, maxVerticalSizeGroundAutoClimb);

                if (Mathf.Abs(newValue.y - lastValue.y) > maxVerticalSizeGroundAutoClimb)
                {
#if UNITY_EDITOR
                    if (PathFindingController.displayDebug)
                    {
                        Debug.DrawLine(GetPosition, newValue, PathFindingController.instance.caseNodeWalkNotGoodVertical, 5);
                    }
#endif
                    return false;
                }
                    
#if UNITY_EDITOR
                if (PathFindingController.displayDebug)
                {
                    Debug.DrawLine(lastValue, newValue, Color.gray, 5);
                }
#endif

                lastValue = newValue;
            }

            return true;
        }

        /// <summary>
        /// Get the node index on total node list
        /// </summary>
        /// <param name="nodeIndex">node map position index</param>
        /// <param name="horizontalBorder">horizontal size of map</param>
        /// <returns>node index</returns>
        public int NodeIndexList(Vector2Int nodeIndex, int horizontalBorder)
        {
            if (nodeIndex.x < 0 || nodeIndex.y < 0 || nodeIndex.x >= horizontalBorder || nodeIndex.y >= horizontalBorder)
            {
                return -1;
            }
            
            if (horizontalBorder - nodeIndex.x < 0 || horizontalBorder - nodeIndex.y < 0)
            {
                return -1; 
            }

            int index = nodeIndex.y * horizontalBorder + nodeIndex.x;
            return index;
        }
        
        /// <summary>
        /// Get the node cost
        /// </summary>
        /// <returns>node cost</returns>
        public int NodeCost()
        {
            /*switch (GetNodeType)
            {
                
            }*/
            return Mathf.CeilToInt(10 * (1 + _pourcNodeCostBonus));
        }
        
        /// <summary>
        /// Get ground position depending on a custom point and direction
        /// </summary>
        /// <param name="pointStart">start point for raycast</param>
        /// <param name="direction">direction of raycast</param>
        /// <param name="layerTarget">layer target for raycast</param>
        /// <param name="minDistanceToCheck">min raycast distance</param>
        /// <returns>point raycast</returns>
        public Vector3 GetCustomPointOnPos(Vector3 pointStart, Vector3 direction, LayerMask layerTarget, float minDistanceToCheck)
        {
            float maxDist = Mathf.Max(minDistanceToCheck, _nodeData.nodeSize.y);
            if (!_is2DRaycast)
            {
                if (Physics.Raycast(pointStart, direction, out RaycastHit raycastHit, maxDist,
                    layerTarget))
                {
                    return raycastHit.point;
                }
            }
            
            return pointStart - Vector3.up * maxDist;
        }
        #endregion

        #region Private
        /// <summary>
        /// Get node type
        /// </summary>
        /// <returns>node type</returns>
        private NodeType CheckNodeType()
        {
            if (_nodeType != NodeType.NoLoaded)
            {
                return _nodeType;
            }
            
            _nodeGroundPosition = GetPosition;
            _nodeGroundAngle = Vector3.zero;
            _nodeType = NodeType.Empty;
            if (CheckObstacleCollider(_nodeData.layerObstacle))
            {
                _nodeType = NodeType.Obstacle;
            }
            else if (CheckWalkableCollider(_nodeData.layerWalkable))
            {
                _nodeType = NodeType.Walkable;
            }

            return _nodeType;
        }

        /// <summary>
        /// Check if node is an obstacle collider
        /// </summary>
        /// <param name="layerTarget">layer obstacle</param>
        /// <returns>true or not</returns>
        private bool CheckObstacleCollider(LayerMask layerTarget)
        {
            Vector2 nodeSize = _nodeData.nodeSize * 0.5f;

            if (!_is2DRaycast)
            {
                if (Physics.CheckBox(_nodePosition, new Vector3(nodeSize.x, nodeSize.y, nodeSize.x), Quaternion.identity, layerTarget.value))
                {
                    return true;
                }   
            }
           /* else
            { 
                if (Physics2D.BoxCast(_nodePosition + _upVectorDirection, new Vector2(nodeSize.x, nodeSize.y), 0,  Vector2.up, layerTarget.value))
                {
                    return true;
                }
            }*/

            return false;
        }

        /// <summary>
        /// Check if node is a walkable collider
        /// </summary>
        /// <param name="layerTarget">layer walkable</param>
        /// <returns>true or not</returns>
        private bool CheckWalkableCollider(LayerMask layerTarget)
        {
            Vector2 nodeSize = new Vector2( _nodeData.nodeSize.x * _percentageGroundSize, _nodeData.nodeSize.y + 0.011f) * 0.5f;

            bool isCast;
            //if (!_is2DRaycast)
            {
                Vector3 positionNodeRaycast = _nodePosition + _upVectorDirection * nodeSize.y;
                isCast = Physics.BoxCast(positionNodeRaycast, new Vector3(nodeSize.x, 0.01f, nodeSize.x), Vector3.down,
                    out RaycastHit raycastHit, Quaternion.identity, _nodeData.nodeSize.y, layerTarget.value);
                
                if (isCast)
                {
                    float distancePointDiff = (_nodeData.nodeSize.y - raycastHit.distance) * 0.5f;

                    isCast = !Physics.CheckBox(positionNodeRaycast + Vector3.up * distancePointDiff,
                        new Vector3(nodeSize.x, distancePointDiff, nodeSize.x), Quaternion.identity, layerTarget.value);

                    manualNodeAvailable = manualNodeAvailable && isCast;
                    
                    _nodeGroundPosition = new Vector3(_nodeGroundPosition.x, raycastHit.point.y + 0.01f, _nodeGroundPosition.z);
                    _nodeGroundAngle = raycastHit.normal;
#if UNITY_EDITOR
                    if (!isCast)
                    {
                        Debug.DrawLine(_nodePosition, positionNodeRaycast + Vector3.up * distancePointDiff, Color.red, 1);
                    }
#endif
                }
            }
           /* else
            {
                // todo check dir and pos with axe type
                RaycastHit2D raycastHit2D = Physics2D.BoxCast(_nodePosition + _upVectorDirection, new Vector2(nodeSize.x, nodeSize.y), 0, Vector2.down,
                    layerTarget.value);
                isCast = raycastHit2D.collider != null;
                
                if (isCast)
                {
                    _nodeGroundPosition = new Vector3(_nodeGroundPosition.x, raycastHit2D.point.y, _nodeGroundPosition.z);
                    _nodeGroundAngle = raycastHit2D.normal;
                }
            }*/
            
            return isCast;
        }
        #endregion
    }
}
