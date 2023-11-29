using System.Collections.Generic;
using LetMeGo.Scripts.PathFindingAlgo;
using UnityEngine;

namespace ProceduralPathFinding.PathFindingScripts.Controller
{
    [ExecuteInEditMode]
    public class PathFindingDebugController : MonoBehaviour
    {
        #region Variables
        [SerializeField] private PathSettingOnCharacter settingOnCharacterToDebug;
        [SerializeField] private List<NodeState.NodeType> cubeToDisplays = new List<NodeState.NodeType>(1) { NodeState.NodeType.Walkable };

        [SerializeField] private Color caseNodeWalk = Color.blue;
        [SerializeField] private Color caseNodeWalkNotGoodAngle = Color.magenta;
        [SerializeField] private Color caseNodeWalkNotGoodVertical = new Color(1.0f, 0.5f, 0.5f, 1);

        [SerializeField] private Color caseNodeEmpty = new Color(1,1,1, 0.3f);
        [SerializeField] private Color caseNodeObjstacle = Color.red;

        private List<NodeState> _nodeStates = new List<NodeState>();
        private Vector3 _lastPosition;
        #endregion

        #region Mono
        private void Update()
        {
            Vector3 position = CheckModuloVectorWithNodeSize(settingOnCharacterToDebug, transform.position);
            if (_lastPosition == position)
            {
                return;
            }

            _nodeStates.Clear();
            _lastPosition = position;
            for (int i = 0; i < settingOnCharacterToDebug.maxVerticalMapNodeArrayLength; i++)
            {
                DebugNodesRaycast(position);

                position.y += settingOnCharacterToDebug.nodeVerticalSize;
            }
        }
        #endregion

        #region Public
        
        #endregion

        #region Private
        /// <summary>
        /// Debug nodes arount a point
        /// </summary>
        /// <param name="nodePosition">center point</param>
        private void DebugNodesRaycast(Vector3 nodePosition)
        {
            int index = 0;
            for (int i = 0; i < settingOnCharacterToDebug.maxHorizontalMapNodeArrayLength; i++)
            {
                for (int j = 0; j < settingOnCharacterToDebug.maxHorizontalMapNodeArrayLength; j++)
                {
                    index++;
                    NodeState node = new NodeState();
                    node.InitNodeState(new NodeState.NodeData(nodePosition, index,
                        settingOnCharacterToDebug.maxHorizontalMapNodeArrayLength,
                        new Vector2(settingOnCharacterToDebug.nodeHorizontalSize,
                            settingOnCharacterToDebug.nodeVerticalSize),
                        settingOnCharacterToDebug.groundLayer, settingOnCharacterToDebug.obstacleLayer), nodePosition, nodePosition, 1, 10, i, false, AxeType.XYZ);

                    if (node.GetNodeType == NodeState.NodeType.Walkable)
                    {
                        if (!CheckVerticalNode(node))
                        {
                            #if UNITY_EDITOR
                            node.walkStatus = NodeState.NodeWalkStatus.VerticalNotGood;
                            #endif
                        }
                        
                        if (!node.CheckAngleWithNode(node, settingOnCharacterToDebug.maxAngleAvailableToWalkDown,
                            settingOnCharacterToDebug.maxAngleAvailableToWalkUp))
                        {
#if UNITY_EDITOR
                            node.walkStatus = NodeState.NodeWalkStatus.AngleNotGood;
#endif
                        }
                        else
                        {
#if UNITY_EDITOR
                            node.walkStatus = NodeState.NodeWalkStatus.CanWalk;
#endif
                        }
                    }
                    _nodeStates.Add(node);
                }
            }
        }

        /// <summary>
        /// Check node walkable depending vertical value
        /// </summary>
        /// <param name="node">node to check</param>
        /// <returns></returns>
        private bool CheckVerticalNode(NodeState node)
        {
            bool nodeWalkable = false;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    Vector3 startPosition = node.GetGroundPosition + new Vector3(settingOnCharacterToDebug.nodeHorizontalSize * i, settingOnCharacterToDebug.nodeVerticalSize * 0.5f, settingOnCharacterToDebug.nodeHorizontalSize * j);
                    if (!node.CheckVerticalAutoClimb(startPosition,
                        settingOnCharacterToDebug.maxVerticalDistancePlayerCanAutoClimb,
                        settingOnCharacterToDebug.presiceVerticalDetectionBetweenTwoCase))
                    {
                        Debug.DrawLine(startPosition, node.GetPosition, caseNodeWalkNotGoodVertical, 10);
                        continue;
                    }

                    nodeWalkable = true;
                }   
            }

            return nodeWalkable;
        }
        
        /// <summary>
        /// Module position
        /// </summary>
        /// <param name="pathSettingOnCharacter">character settings</param>
        /// <param name="position">position target</param>
        /// <returns></returns>
        private Vector3 CheckModuloVectorWithNodeSize(PathSettingOnCharacter pathSettingOnCharacter, Vector3 position)
        {
            position.x -= position.x % pathSettingOnCharacter.nodeHorizontalSize;
            position.z -= position.z % pathSettingOnCharacter.nodeHorizontalSize;
            position.y -= position.y % pathSettingOnCharacter.nodeVerticalSize;
            
            return position;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < _nodeStates.Count; i++)
            {
                DrawnNode(i);
            }
            void DrawnNode(int index)
            {
                NodeState node = _nodeStates[index];

                if (!cubeToDisplays.Contains(node.GetNodeType))
                {
                    return;
                }
                
                Color colorCube = Color.gray;
                Vector2 nodeSize = node.GetNodeData.nodeSize;
                if (node.GetNodeType == NodeState.NodeType.Walkable)
                {
                    Debug.DrawLine(node.GetGroundPosition, node.GetGroundPosition + Vector3.up * 0.25f, caseNodeWalk, 0.1f);
                    colorCube = node.walkStatus switch
                    {
                        NodeState.NodeWalkStatus.CanWalk => caseNodeWalk,
                        NodeState.NodeWalkStatus.AngleNotGood => caseNodeWalkNotGoodAngle,
                        NodeState.NodeWalkStatus.VerticalNotGood => caseNodeWalkNotGoodVertical,
                        _ => colorCube
                    };
                }
                else if (node.GetNodeType == NodeState.NodeType.Obstacle)
                {
                    colorCube = caseNodeObjstacle;
                }
                else if (node.GetNodeType == NodeState.NodeType.Empty)
                {
                    colorCube = caseNodeEmpty;
                }
                
                Gizmos.color = colorCube;
                Gizmos.DrawWireCube(node.GetPosition, new Vector3(nodeSize.x, node.GetNodeData.nodeSize.y, nodeSize.x) * 0.95f);
            }
        }
        
#endif
        #endregion
    }
}
