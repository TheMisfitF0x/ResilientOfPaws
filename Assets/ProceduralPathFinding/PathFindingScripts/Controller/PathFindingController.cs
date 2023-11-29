using System;
using System.Collections;
using System.Threading;
using LetMeGo.Scripts.PathFindingAlgo;
using Plugins.RuntimePathFinding.Scripts.PathfindingOperation;
using ProceduralPathFinding.PathFindingScripts.PathfindingOperation;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.Controller
{
    public class PathFindingController : MonoBehaviour
    {
        #region Variables
        public static PathFindingController instance;
        public static int currentCalculeIndex;
        public static int maxCalculOnFrame;
        
#if UNITY_EDITOR
        public static bool displayDebug;
        public static bool debugWithClick;
        public static bool canCheckNextNode;
        
        [Header("Debug Line Path Color")]
        public Color pathNotWalkable = Color.yellow;
        public Color pathWalkable = Color.cyan;

        [Header("Debug Node Color")]
        public Color caseNodeWalk = Color.blue;
        public Color caseNodeWalkNotGoodAngle = Color.magenta;
        public Color caseNodeWalkNotGoodVertical = new Color(1.0f, 0.5f, 0.5f, 1);

        public Color caseNodeEmpty = new Color(1,1,1, 0.3f);
        public Color caseNodeObjstacle = Color.red;

        [Space]
        [Space]
        [Space]
#endif

        [SerializeField] private int maxNodeTotalCalculOnFrame = 2000;
        [Space]
        
        private int travelNodeDistanceCost = 10;
        
        private readonly WaitForEndOfFrame _frame = new WaitForEndOfFrame();
        
        public enum PathProcess
        {
            WIP,
            DONE,
            Failed,
            FailedDueToMapSize
        }

        public struct CharacterInfo
        {
            public Vector3 characterPosition;
            public Vector3 characterTargetPosition;

            public CharacterInfo(Vector3 characterPosition, Vector3 characterTargetPosition)
            {
                this.characterPosition = characterPosition;
                this.characterTargetPosition = characterTargetPosition;
            }
        }
        #endregion

        #region Mono
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }

            maxCalculOnFrame = maxNodeTotalCalculOnFrame;
            instance = this;
        }
        #endregion

        #region Public
#if !UNITY_EDITOR
        public IEnumerator CreateNewPath(PathSettingOnCharacter pathSettingOnCharacter, CharacterInfo characterInfo, Action<NativeArray<NodeState>, PathProcess> callBackValuePath, CancellationTokenSource cancellation)
#else
        /// <summary>
        /// Create a new pathfinding
        /// </summary>
        /// <param name="pathSettingOnCharacter"></param>
        /// <param name="characterInfo"></param>
        /// <param name="callBackValuePath"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public IEnumerator CreateNewPath(PathSettingOnCharacter pathSettingOnCharacter, CharacterInfo characterInfo, Action<NativeArray<NodeState>, NativeArray<NodeState>, PathProcess> callBackValuePath, CancellationTokenSource cancellation)
#endif
        {
            #if UNITY_EDITOR
            if (displayDebug)
            {
                Debug.DrawLine(characterInfo.characterPosition, characterInfo.characterPosition + Vector3.up, Color.yellow, 5);
            }
            #endif
            
            int getNumberOfMapNeeded = NumberOfMapVerticalIndex(pathSettingOnCharacter, characterInfo, out bool diffVertical);
            int verticalNeed = 1 + getNumberOfMapNeeded * 2;

            PathFindingNodesInitialize withAllNodesInitialize =
                SetUpMapForPathFinding(pathSettingOnCharacter, characterInfo, -getNumberOfMapNeeded * pathSettingOnCharacter.nodeVerticalSize, 0, verticalNeed);

            NativeArray<NodeState> allMapsNodes = new NativeArray<NodeState>(withAllNodesInitialize.nodes.Length * verticalNeed, Allocator.Persistent);

            PathFindingMapNodeToCombineTogether addNode = new PathFindingMapNodeToCombineTogether()
            {
                MapsNodes = allMapsNodes,
                NodesToAdd = withAllNodesInitialize.nodes,
                StartMapNodeIndex = 0,
            };

            JobHandle jobHandle = addNode.Schedule(withAllNodesInitialize.nodes.Length, 64);
            jobHandle.Complete();

            int length = withAllNodesInitialize.nodes.Length;
            allMapsNodes = addNode.MapsNodes;
            withAllNodesInitialize.nodes.Dispose();
            
            for (int i = -getNumberOfMapNeeded + 1; i < getNumberOfMapNeeded + 1; i++)
            {
                PathFindingNodesInitialize otherVertical = SetUpMapForPathFinding(pathSettingOnCharacter, characterInfo, 
                    pathSettingOnCharacter.nodeVerticalSize * i, i + getNumberOfMapNeeded, verticalNeed);
                
                PathFindingMapNodeToCombineTogether addNodeExtends = new PathFindingMapNodeToCombineTogether()
                {
                    MapsNodes = allMapsNodes,
                    NodesToAdd = otherVertical.nodes,
                    StartMapNodeIndex = (i + getNumberOfMapNeeded) * length,
                };

                jobHandle = addNodeExtends.Schedule(otherVertical.nodes.Length, 64);
                jobHandle.Complete();
                
                allMapsNodes = addNode.MapsNodes;
                otherVertical.nodes.Dispose();
            }
            
            withAllNodesInitialize.nodes = allMapsNodes;
            
            PathfindingTravelCalcul pathInfo = CalculPathToTargetOnMap(pathSettingOnCharacter, withAllNodesInitialize, verticalNeed, length);

            while (!pathInfo.EndOfTravelCalcul && !cancellation.IsCancellationRequested)
            {
                pathInfo.Execute();
                yield return _frame;
                
                currentCalculeIndex = 0;
            }

            if (cancellation.IsCancellationRequested)
            {
                pathInfo.DisposeNativeList();
                yield break;
            }

            int maxNode = Mathf.Max(pathSettingOnCharacter.maxHorizontalMapNodeArrayLength, pathSettingOnCharacter.maxVerticalMapNodeArrayLength); 
            bool diffMapSizeWithLimit = withAllNodesInitialize.sizeDiffBetweenMapAndLimit > 0 && pathInfo.NodesPath.Length > maxNode * 0.5f -1;
            CheckPathInformationToCallBack(ref pathInfo, callBackValuePath, diffMapSizeWithLimit);
            pathInfo.DisposeNativeList();
        }
        #endregion

        #region Private
#if !UNITY_EDITOR
        private void CheckPathInformationToCallBack(ref PathfindingTravelCalcul pathInfo, Action<NativeArray<NodeState>, PathProcess> callBackValuePath, bool diffMapSize)
#else
        /// <summary>
        /// Check if the pathfinding reached the target or fail depending the size of the map or if not reachable
        /// </summary>
        /// <param name="pathInfo"></param>
        /// <param name="callBackValuePath"></param>
        /// <param name="diffMapSize"></param>
        private void CheckPathInformationToCallBack(ref PathfindingTravelCalcul pathInfo, Action<NativeArray<NodeState>, NativeArray<NodeState>, PathProcess> callBackValuePath, bool diffMapSize)
#endif
        {
            PathProcess pathInfoSuccess = PathProcess.DONE;
            
            if (!pathInfo.FindPathToTarget && diffMapSize)
            {
                pathInfoSuccess = PathProcess.Failed;
                if (diffMapSize)
                {
                    pathInfoSuccess = PathProcess.FailedDueToMapSize;
                }
            }
        
            if (callBackValuePath != null)
            {
                int length = pathInfo.NodesPath.Length;

                PathFindingConverterMapToPath pathFindingConverterMapToPath = new PathFindingConverterMapToPath()
                {
                    nodeMap = pathInfo.MapNodes,
                    nodePath = new NativeArray<NodeState>(length, Allocator.TempJob),
                    nodeIndexPath = pathInfo.NodesPath
                };

                JobHandle jobHandler = pathFindingConverterMapToPath.Schedule(length, 64);
                jobHandler.Complete();

                #if UNITY_EDITOR
                if (displayDebug)
                {
                    for (int i = 1; i < pathFindingConverterMapToPath.nodePath.Length; i++)
                    {
                        Debug.DrawLine(pathFindingConverterMapToPath.nodePath[i - 1].GetPosition, pathFindingConverterMapToPath.nodePath[i].GetPosition, Color.green, 10);
                    }
                }
                #endif
            
                #if !UNITY_EDITOR
                callBackValuePath.Invoke(pathFindingConverterMapToPath.nodePath, pathInfoSuccess);
                #else
                callBackValuePath.Invoke(pathFindingConverterMapToPath.nodePath, pathInfo.MapNodes, pathInfoSuccess);
                #endif

                pathFindingConverterMapToPath.nodePath.Dispose();
            }
        }

        /// <summary>
        /// Get the number of Vertical map for the pathfinding
        /// </summary>
        /// <param name="pathSettingOnCharacter"></param>
        /// <param name="characterInfo"></param>
        /// <param name="targetDiffVerticalWithChara"></param>
        /// <returns></returns>
        private int NumberOfMapVerticalIndex(PathSettingOnCharacter pathSettingOnCharacter, CharacterInfo characterInfo, out bool targetDiffVerticalWithChara)
        {
            Vector3 positionTarget = characterInfo.characterTargetPosition;
            Vector3 startPosition = characterInfo.characterPosition;

            //startPosition = CheckModuloVectorWithNodeSize(pathSettingOnCharacter, startPosition);
            //positionTarget = CheckModuloVectorWithNodeSize(pathSettingOnCharacter, positionTarget);

            int verticalSize = 0;
            float nodeVerticalSize = pathSettingOnCharacter.nodeVerticalSize;

            targetDiffVerticalWithChara = false;
            if (pathSettingOnCharacter.characterCabUseVerticalPath && pathSettingOnCharacter.axeToUse == AxeType.XYZ)
            {
                float distVerticalTarget = Mathf.Abs(startPosition.y - positionTarget.y);
                verticalSize = Mathf.CeilToInt(distVerticalTarget / nodeVerticalSize);

                targetDiffVerticalWithChara = verticalSize > 1;
                if (targetDiffVerticalWithChara)
                {
                    verticalSize *= 2;
                }
                
                verticalSize += pathSettingOnCharacter.bonusVerticalMapArrayExtends;
                verticalSize = Mathf.Min(pathSettingOnCharacter.maxVerticalMapNodeArrayLength, verticalSize);
            }

            return verticalSize;
        }

        /// <summary>
        /// Move the map to adjuste the position of the pathfinding depending of the node size
        /// </summary>
        /// <param name="pathSettingOnCharacter"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector3 CheckModuloVectorWithNodeSize(PathSettingOnCharacter pathSettingOnCharacter, Vector3 position)
        {
            position.x -= position.x % pathSettingOnCharacter.nodeHorizontalSize;
            position.z -= position.z % pathSettingOnCharacter.nodeHorizontalSize;
            position.y -= position.y % pathSettingOnCharacter.nodeVerticalSize;
            
            return position;
        }

        /// <summary>
        /// Set up all information for the Pathfinding map
        /// </summary>
        /// <param name="pathSettingOnCharacter"></param>
        /// <param name="characterInfo"></param>
        /// <param name="bonusVerticalPosition"></param>
        /// <param name="mapIndexParent"></param>
        /// <param name="totalVerticalIndex"></param>
        /// <returns></returns>
        private PathFindingNodesInitialize SetUpMapForPathFinding(PathSettingOnCharacter pathSettingOnCharacter, CharacterInfo characterInfo, float bonusVerticalPosition, int mapIndexParent, int totalVerticalIndex)
        {
            float nodeSize = pathSettingOnCharacter.nodeHorizontalSize;
            float nodeVerticalSizeSize = pathSettingOnCharacter.nodeVerticalSize;

            Vector3 positionTarget = characterInfo.characterTargetPosition;
            Vector3 startPosition = characterInfo.characterPosition;

            //startPosition = CheckModuloVectorWithNodeSize(pathSettingOnCharacter, startPosition);
            //positionTarget = CheckModuloVectorWithNodeSize(pathSettingOnCharacter, positionTarget);
            
            Vector3 centerPosition = (startPosition + positionTarget) * 0.5f;
            
            float distToTarget = Vector3.Distance(startPosition, positionTarget);
            int mapSize = Mathf.CeilToInt(distToTarget / nodeSize) + pathSettingOnCharacter.bonusHorizontalMapArrayExtends;
            mapSize += pathSettingOnCharacter.bonusHorizontalMapArrayExtends;
            
            int realMapSize = Mathf.Min(mapSize, pathSettingOnCharacter.maxHorizontalMapNodeArrayLength);

            centerPosition = Vector3.Lerp(startPosition, centerPosition,(float)realMapSize / mapSize);
            
            float mapIndex = (startPosition.y - centerPosition.y);
            mapIndex -= (Mathf.CeilToInt(totalVerticalIndex * 0.5f) + Mathf.CeilToInt(Mathf.Abs(mapIndex) * 0.25f)) * nodeVerticalSizeSize;
            int diffSizeBetweenMapAndLimits = Mathf.Max(mapSize - pathSettingOnCharacter.maxHorizontalMapNodeArrayLength,  (int) Mathf.Max(mapIndex, 0));

            int totalNode = realMapSize * realMapSize;
            PathFindingNodesInitialize jobSetUp = new PathFindingNodesInitialize()
            {
                nodes = new NativeArray<NodeState>(totalNode, Allocator.Persistent),
                
                centerPosition = CheckModuloVectorWithNodeSize(pathSettingOnCharacter,  centerPosition - new Vector3(1,0,1) * realMapSize * nodeSize * 0.5f) + Vector3.up * bonusVerticalPosition,
                percentageGroundCheck = pathSettingOnCharacter.percentageSizeHorizontalCheckForGroundCenter,
               
                travelCost = travelNodeDistanceCost,
                mapParentIndex = mapIndexParent,
                sizeDiffBetweenMapAndLimit = diffSizeBetweenMapAndLimits,

                layerObstacle = pathSettingOnCharacter.obstacleLayer,
                layerWalkable = pathSettingOnCharacter.groundLayer,
                
                positionTarget = positionTarget,
                positionStart = startPosition,
                
                borderSize = realMapSize,
                nodeVerticalSize = pathSettingOnCharacter.nodeVerticalSize,
                nodeHorizontaleSize = nodeSize,
                axeToUse = pathSettingOnCharacter.axeToUse,
                isRaycast2D = false // pathSettingOnCharacter.use2DRaycast
            };

            JobHandle jobHandle = jobSetUp.Schedule(totalNode, 64);
            jobHandle.Complete();
            
            return jobSetUp;
        }

        /// <summary>
        /// Calcule of the pathfinding to reach the target with map information
        /// </summary>
        /// <param name="pathSettingOnCharacter"></param>
        /// <param name="info"></param>
        /// <param name="totalMapVerticalIndex"></param>
        /// <param name="lengthNodes"></param>
        /// <returns></returns>
        private PathfindingTravelCalcul CalculPathToTargetOnMap(PathSettingOnCharacter pathSettingOnCharacter, PathFindingNodesInitialize info, int totalMapVerticalIndex, int lengthNodes)
        {
            PathFindingFindFirstIndex pathFindingFindFirstIndex = new PathFindingFindFirstIndex()
            {
                nodeStates = info.nodes,
                positionStart = info.positionStart,
            };

            int indexFirstNode = pathFindingFindFirstIndex.Execute(); 

            PathfindingTravelCalcul jCalculPath = new PathfindingTravelCalcul()
            {
                MapNodes = info.nodes,
                NodesPathSaved = new NativeList<NodeState>(Allocator.Persistent),
                NodesPath = new NativeList<int>(Allocator.Persistent),

                MapLengthNodes = lengthNodes,
                CharacterCanFly = pathSettingOnCharacter.characterCanFly,
                CharacterCanFall = pathSettingOnCharacter.characterCanFall,
                NumberOfCaseCharacterCanFall  = pathSettingOnCharacter.maVerticalCaseCharacterCanFall,
                MAXVerticalSizeGroundAutoClimb  = pathSettingOnCharacter.maxVerticalDistancePlayerCanAutoClimb,
                PresiceVerticalDetection  = pathSettingOnCharacter.presiceVerticalDetectionBetweenTwoCase,
                MAXAngleAvailableToWalkUp  = pathSettingOnCharacter.maxAngleAvailableToWalkUp,
                MAXAngleAvailableToWalkDown  = pathSettingOnCharacter.maxAngleAvailableToWalkDown,

                UseDiagonalMove = pathSettingOnCharacter.characterCanUseDiagonalMove,
                PositionTarget = info.positionTarget,
                
                IndexOfFirstNode = indexFirstNode,
                
                BorderSize = info.borderSize,
                TotalMapVerticalIndex = totalMapVerticalIndex - 1,
                
                MapDiffLimit = info.sizeDiffBetweenMapAndLimit > 0,
                FindPathToTarget = false,
            };

            return jCalculPath;
        }
        #endregion
    }
}
