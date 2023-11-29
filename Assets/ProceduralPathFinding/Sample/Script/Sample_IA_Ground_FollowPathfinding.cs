using UnityEngine;

public class Sample_IA_Ground_FollowPathfinding : MonoBehaviour
{
    #region Variables
    [SerializeField] private float moveSpeed;
    #endregion

    #region Private
    public void FollowPath(Vector3 pathToMove)
    {
        float distMove = pathToMove.magnitude;
        transform.position += pathToMove.normalized * Mathf.Min(moveSpeed * Time.fixedDeltaTime, distMove);
    }
    #endregion
}
