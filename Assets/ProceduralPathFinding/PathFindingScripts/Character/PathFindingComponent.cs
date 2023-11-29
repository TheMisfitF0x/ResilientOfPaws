using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using LetMeGo.Scripts.PathFindingAlgo;
using Plugins.RuntimePathFinding.Scripts;
using ProceduralPathFinding.PathFindingScripts.Controller;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
#endif

namespace ProceduralPathFinding.PathFindingScripts.Character
{
    [ExecuteInEditMode]
    public class PathFindingComponent : MonoBehaviour
    {
        #region Variables
        [SerializeField] private UnityEvent<Vector3> eventFollowingPath;
        [SerializeField] private UnityEvent<NativeArray<Vector3>> eventCurrentTotalPath;
        [SerializeField] private UnityEvent<NativeArray<Vector3>> eventTotalPath;
        [SerializeField] private Transform groundPosition;
        [Space]
        
        #if UNITY_EDITOR
        [Header("Debug settings")]
        public Vector2 debugDisplayPosition;
        
        [SerializeField] private NodeState.NodeType[] cubeToDisplays = new NodeState.NodeType[1] { NodeState.NodeType.Walkable };
        private NodeState.NodeType[] _lastCubeToDisplays = new NodeState.NodeType[1] { NodeState.NodeType.Walkable };
        private List<NodeState> _nodeStatesDebugging = new List<NodeState>();
        private List<NodeState> _totalNodeDebugging = new List<NodeState>();
        private List<NodeState> _currentNodeDebugging = new List<NodeState>();
        #endif

        private PathFindingData _pathFindingData;
        private CancellationTokenSource _cancellationTokenSource;
        private PathSettingOnCharacter _pathSettingOnCharacter;
        private float _delayTimer;
        private bool _followingPath = false; 
        
        private Action<PathFindingController.PathProcess> _callBackOnUpdate;

        //public ref PathFindingData GetPathData => ref _pathFindingData;
        public bool IsFollowingPath => _followingPath;
        #endregion

        #region Mono
        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                _pathFindingData = new PathFindingData()
                {
                    nodeStates = new NativeList<NodeState>(0, Allocator.Persistent),
                };  
            }
#else
            _pathFindingData = new PathFindingData()
            {
                nodeStates = new NativeList<NodeState>(0, Allocator.Persistent),
            };
#endif
        }

        private void Update()
        {
            if (_pathSettingOnCharacter == null || !_followingPath)
            {
                return;
            }

            Vector2 nodeSize = new Vector2(_pathSettingOnCharacter.nodeHorizontalSize, _pathSettingOnCharacter.nodeVerticalSize);
            Vector3 directionToMove = _pathFindingData.CurrentDirectionWithDistance(groundPosition.position, _pathSettingOnCharacter.pathNodeDirectionType, nodeSize, out float distance);
        
            #if UNITY_EDITOR
            Debug.DrawLine(transform.position, transform.position + directionToMove * 1.5f, Color.black, 0.1f);
            #endif
            
            if (Math.Abs(distance - (-1)) < 0.01f && _pathFindingData.pathFindingProcess == PathFindingController.PathProcess.DONE)
            {
                _followingPath = false;
            }
            
            eventFollowingPath.Invoke(directionToMove);
            
            _delayTimer += Time.deltaTime;

            if (_delayTimer > _pathFindingData.timeDelayCheckNode)
            {
                _delayTimer = 0;
                if(_pathFindingData.CheckIfPathNodesSwitchType())
                {
                    CreateNewPath(_pathSettingOnCharacter, _pathFindingData.CurrentNodeState().GetPosition,
                        _pathFindingData.targetPoint, _callBackOnUpdate);
                }
            }
        }

        private void OnDestroy()
        {
            ResetPathComponent();
        }
        #endregion

        #region Public
        /// <summary>
        /// Mthode to create a new path between point A to point B
        /// </summary>
        /// <param name="pathSetting">Character settings</param>
        /// <param name="startPosition">Point A position</param>
        /// <param name="endPosition">Point B position</param>
        /// <param name="callBackOnPathUpdate">Callback Update during pathfinding calcul</param>
        public void CreateNewPath(PathSettingOnCharacter pathSetting, Vector3 startPosition, Vector3 endPosition, Action<PathFindingController.PathProcess> callBackOnPathUpdate)
        {
            ResetPathComponent();
            _followingPath = true;
            _pathSettingOnCharacter = pathSetting;

            _pathFindingData = new PathFindingData()
            {
                nodeStates = new NativeList<NodeState>(0, Allocator.Persistent),
                pathFindingProcess = PathFindingController.PathProcess.WIP,
                targetPoint = endPosition,
                timeDelayCheckNode = _pathSettingOnCharacter.delayCheckingNodePathState,
                maxNodeToCheckOnPath = _pathSettingOnCharacter.maxCaseDistanceToCheck,
                currentIndexOnArrayPath = 0, 
                distanceToCheckWithNodePosition = _pathSettingOnCharacter.distanceToCheckBetweenPositionAndNextNode
            };
            
            _callBackOnUpdate = callBackOnPathUpdate;
            
            ContinuePathFinding(pathSetting, startPosition, endPosition);
        }

        /// <summary>
        /// Stop pathfinding calcul
        /// </summary>
        public void ResetPathComponent()
        {
            if (_cancellationTokenSource != null && _cancellationTokenSource.Token.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            
            _followingPath = false;
            
            #if UNITY_EDITOR
            _nodeStatesDebugging.Clear();
            _totalNodeDebugging.Clear();
            #endif
            _pathFindingData.DisposeData();
        }
        #endregion

        #region Private
        /// <summary>
        /// Cacule and get the point nearest EndPosition
        /// </summary>
        /// <param name="pathSetting"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        private void ContinuePathFinding(PathSettingOnCharacter pathSetting, Vector3 startPosition, Vector3 endPosition)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            #if UNITY_EDITOR
            Debug.DrawLine(startPosition, startPosition + Vector3.up * 2, Color.yellow, 5);
            Debug.DrawLine(endPosition, endPosition + Vector3.up * 2, Color.red, 5);
            #endif

            IEnumerator newPath = PathFindingController.instance.CreateNewPath(pathSetting,
                new PathFindingController.CharacterInfo(startPosition, endPosition), UpdatedPathList, _cancellationTokenSource);
            StartCoroutine(newPath);   
        }
        
        /// <summary>
        /// CallBack with value of pathfinding calcul
        /// </summary>
        /// <param name="nodes">nodes paths to follow</param>
        /// <param name="nodesDebugging">total nodes of the maps (for debugging</param>
        /// <param name="process">path status</param>
#if !UNITY_EDITOR
        private void UpdatedPathList(NativeArray<NodeState> nodes, PathFindingController.PathProcess process)
#else
        private void UpdatedPathList(NativeArray<NodeState> nodes, NativeArray<NodeState> nodesDebugging, PathFindingController.PathProcess process)
#endif
        {
            _followingPath = true;
            _pathFindingData.AddPathNode(nodes, process);
            
            #if UNITY_EDITOR
            AddNodeDebugging(nodesDebugging);
            #endif

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            
            //todo convert native list to vector3
            NativeArray<Vector3> totalNodesPosition = new NativeArray<Vector3>(_pathFindingData.nodeStates.Length, Allocator.TempJob);
            PathConvertNodeToVector convertNodeToVector = new PathConvertNodeToVector()
            {
                PathNode = _pathFindingData.nodeStates,
                NodePosition = totalNodesPosition
            };

            JobHandle jobHandle = convertNodeToVector.Schedule(_pathFindingData.nodeStates.Length, 64);
            jobHandle.Complete();
            
            if (process == PathFindingController.PathProcess.FailedDueToMapSize)
            {
                eventCurrentTotalPath.Invoke(convertNodeToVector.NodePosition);
                if (nodes.Length > 0)
                {
                    Vector3 position = nodes[0].GetPosition;
                    ContinuePathFinding(_pathSettingOnCharacter, position, _pathFindingData.targetPoint);
                }
            }
            else
            {
                eventTotalPath.Invoke(convertNodeToVector.NodePosition);
            }

            convertNodeToVector.NodePosition.Dispose();

            if (_callBackOnUpdate != null)
            {
                _callBackOnUpdate.Invoke(_pathFindingData.pathFindingProcess);
            }
        }
        #endregion
        
#if UNITY_EDITOR
        [SerializeField] private bool displayGizmo = true;

        private void AddNodeDebugging(NativeArray<NodeState> nodesDebugging)
        {
            _totalNodeDebugging.AddRange(nodesDebugging);
            
            if (CheckCubeToDisplay())
            {
                _lastCubeToDisplays = new NodeState.NodeType[cubeToDisplays.Length];
                for (int i = 0; i < cubeToDisplays.Length; i++)
                {
                    _lastCubeToDisplays[i] = cubeToDisplays[i];
                }
                
                _nodeStatesDebugging.Clear();
                CheckNodeToDisplay(_totalNodeDebugging);
            }
            else
            {
                _currentNodeDebugging.Clear();
                _currentNodeDebugging.AddRange(nodesDebugging);
                CheckNodeToDisplay(_currentNodeDebugging);
            }
            
            void CheckNodeToDisplay(List<NodeState> nodeCheckToDisplayDebug)
            {
                for (int i = 0; i < nodeCheckToDisplayDebug.Count; i++)
                {
                    NodeState node = nodeCheckToDisplayDebug[i];
                    NodeState.NodeType typeNode = node.GetNodeType;
                    if (!IsContainNodeToDisplay(typeNode) && !node.isValideForPath)
                    {
                        continue;
                    }
                    _nodeStatesDebugging.Add(node);
                }
            }

            bool CheckCubeToDisplay()
            {
                if (_lastCubeToDisplays.Length != cubeToDisplays.Length)
                {
                    return true;
                }
                
                for (int i = 0; i < cubeToDisplays.Length; i++)
                {
                    if (cubeToDisplays[i] != _lastCubeToDisplays[i])
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool IsContainNodeToDisplay(NodeState.NodeType type)
            {
                for (int i = 0; i < cubeToDisplays.Length; i++)
                {
                    if (cubeToDisplays[i] == type)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!displayGizmo)
            {
                return;
            }

            if (_pathSettingOnCharacter != null)
            {
                Gizmos.DrawWireCube(transform.position,  new Vector2(1,0) * _pathSettingOnCharacter.nodeHorizontalSize * _pathSettingOnCharacter.maxHorizontalMapNodeArrayLength);
            }
            
            try
            {
                for (int i = 0; i < _nodeStatesDebugging.Count; i++)
                {
                    float percentageValue = (float)i / _nodeStatesDebugging.Count;
                    if (percentageValue > debugDisplayPosition.x && percentageValue < debugDisplayPosition.y)
                    {
                        DrawnNode(i);
                    }
                }
            }
            catch 
            {
            }

            void DrawnNode(int index)
            {
                NodeState node = _nodeStatesDebugging[index];
                _nodeStatesDebugging[index] = node;

                displayGizmo = true;
                Color colorCube = Color.gray;
                Vector2 nodeSize = node.GetNodeData.nodeSize;

                Debug.DrawLine(node.GetGroundPosition, node.GetGroundPosition + Vector3.up * 0.25f, PathFindingController.instance.caseNodeWalk, 0.1f);
                if (node.isValideForPath)
                {
                    nodeSize *= 0.9f;
                    colorCube = Color.green;
                }
                else
                {
                    if (node.GetNodeType == NodeState.NodeType.Walkable)
                    {
                        colorCube = node.walkStatus switch
                        {
                            NodeState.NodeWalkStatus.CanWalk => PathFindingController.instance.caseNodeWalk,
                            NodeState.NodeWalkStatus.AngleNotGood => PathFindingController.instance.caseNodeWalkNotGoodAngle,
                            NodeState.NodeWalkStatus.VerticalNotGood => PathFindingController.instance.caseNodeWalkNotGoodVertical,
                            _ => colorCube
                        };
                    }
                    else if (node.GetNodeType == NodeState.NodeType.Obstacle)
                    {
                        colorCube = PathFindingController.instance.caseNodeObjstacle;
                    }
                    else if (node.GetNodeType == NodeState.NodeType.Empty)
                    {
                        colorCube = PathFindingController.instance.caseNodeEmpty;
                    }
                }
                
                Gizmos.color = colorCube;
                Gizmos.DrawWireCube(node.GetPosition, new Vector3(nodeSize.x, node.GetNodeData.nodeSize.y, nodeSize.x) * 0.95f);
            }
        }
#endif
    }
}
