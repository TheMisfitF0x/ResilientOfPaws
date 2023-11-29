using UnityEngine;

namespace LetMeGo.Scripts.PathFindingAlgo
{
    [CreateAssetMenu (fileName = "PathCharacterSetting", menuName = "Scriptable/ProcePath/CharaSetting")]
    public class PathSettingOnCharacter : ScriptableObject
    {
        [Header("Map Node")]
        public float nodeHorizontalSize;
        public float nodeVerticalSize;
        
        [Space]
        [Tooltip("Size of node * this percentage is corresponding of the size of a new node to check ground")]
        [Range(0,1)] public float percentageSizeHorizontalCheckForGroundCenter = 0.1f;
        
        [Space]
        public int maxHorizontalMapNodeArrayLength = 100;
        public int maxVerticalMapNodeArrayLength = 3;

        public int bonusHorizontalMapArrayExtends;
        public int bonusVerticalMapArrayExtends;

        [Header("Path to Target")]
        [Tooltip("Check distance between current position and next node target path, if true, go to the next node")]
        public float distanceToCheckBetweenPositionAndNextNode = 0.1f;
        
        [Space]
        public float delayCheckingNodePathState = 0.5f;
        public int maxCaseDistanceToCheck = 10;
        
        [Header("Player Move")]
        [Space]
        public bool characterCanUseDiagonalMove;
        public bool characterCabUseVerticalPath;
        public bool characterCanFly;
        public bool characterCanFall;

        [Space] 
       // public bool use2DRaycast;
        [HideInInspector] public AxeType axeToUse = AxeType.XYZ;
        public TypeNodeDirection pathNodeDirectionType = TypeNodeDirection.BetweenLastNodeToNextNode;

        [Header("Layer Setting")]
        public LayerMask groundLayer;
        public LayerMask obstacleLayer;
        //public LayerMask tpOrAutoMoveWith2PointsLayer;
        
        [Header("Ground settings")]
        [Range(0,90)] public float maxAngleAvailableToWalkUp;
        [Range(0,90)] public float maxAngleAvailableToWalkDown;
        
        [Space]
        public float maxVerticalDistancePlayerCanAutoClimb;
        [Tooltip("Lerp between two case to check ground Y value for max vertical distance auto climb")]
        public int presiceVerticalDetectionBetweenTwoCase = 11;

       /* [Header("Climb settings")]
        public LayerMask climbLayer;
        public Vector2 minMaxAngleAvailableToClimb;
        public bool canClimb;

        [Header("Jump settings")] 
        public int maxNumberCanVerticalJump;
        public int maxNumberCanHorizontalJump;
        public bool canJump;*/
    
        [Header("Fall settings")]
        public int maVerticalCaseCharacterCanFall;
    }

    public enum AxeType
    {
        XYZ,
        //XY,
        //XZ,
       // YZ,
    }
    
    public enum TypeNodeDirection
    {
        BetweenCharacterAndNextNode,
        BetweenLastNodeToNextNode,
        //XY,
        //XZ,
        // YZ,
    }
}
