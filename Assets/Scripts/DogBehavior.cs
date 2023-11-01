using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AggregatGames.AI.Pathfinding;

public class DogBehavior : MonoBehaviour
{   //Array of all balls in ready2PickUp State
    Ball[] balls;

    //Ball Dog has targeted
    Ball targetBall;

    //Transform to attach ball to.
    public Transform lowerJawSpot;

    public Portal destPortal;
    public float retrieveSpeed;
    public float rotationSpeed;
    public Pathfinder pathfinder;
    public float reachedKnot = 1f;
    private List<PathKnot> knots = new List<PathKnot>();
    public int state = -1;
    public int knotIndex = 1;

    private Path path;

    bool hasBall = false;
    // Start is called before the first frame update
    void Start()
    {

        balls = GameObject.FindObjectsOfType<Ball>();
        targetBall = balls[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Debug.Log(this.transform.parent);
        if (targetBall == null)
        {
            balls = GameObject.FindObjectsOfType<Ball>();
            foreach (Ball ball in balls)
            {
                if (ball.isReady2PickUp && ball.readyFrames > targetBall.readyFrames)
                {
                    targetBall = ball;
                }
            }
        }

        if (targetBall.readyFrames > 500 && targetBall.isReady2PickUp)
        {

            if (Vector3.Distance(this.transform.position, targetBall.transform.position) < .2f)
            {
                targetBall.transform.position = lowerJawSpot.transform.position;
                targetBall.transform.SetParent(lowerJawSpot);
                //Play pickup animation.
                hasBall = true;
                targetBall.myRB.useGravity = false;
                targetBall.isReady2PickUp = false;
            }
            else
            {
                PathFinderMethod(targetBall.transform);
                //Play running animation
            }
        }
        else if (hasBall)
        {
            if (Vector3.Distance(this.transform.position, destPortal.transform.position) < .5f)
            {
                targetBall.transform.position = destPortal.portalPoint.position;
                targetBall.transform.SetParent(null);
                destPortal.heldBall = targetBall;
                //Play pickup animation.
                hasBall = false;
            }
            else
            {
                PathFinderMethod(destPortal.transform);
                //Play running animation
            }
        }
    }

    void PathFinderMethod(Transform target)
    {
        if (knots.Count == 0 && state != -2 || target.position != pathfinder.target)
        {
            if (knots.Count == 0) pathfinder.findPath(transform.position, target.position, foundPath);
            else pathfinder.findPath(knots[knotIndex].position, target.position, foundPath);
        }
        if (knots.Count != 0 && knotIndex < knots.Count)
        {
            Vector3 lookPos = knots[knotIndex].position - transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            Vector3 walkdir = transform.root.forward;
            walkdir.y = (knots[knotIndex].position - transform.position).y;

            if (knotIndex + 1 >= knots.Count)
            {
                return;
            }

            if (path.blockedByDynamicObstacle(knots[knotIndex], knots[knotIndex + 1]))
            {
                return;
            }

            if (Vector3.Distance(transform.position, knots[knotIndex].position) <= reachedKnot)
            {
                RaycastHit hit;
                bool isHitting = Physics.Raycast(transform.position, (knots[knotIndex].position - transform.position).normalized, out hit, Vector3.Distance(transform.position, knots[knotIndex].position));
                if (isHitting && hit.collider.gameObject.tag != pathfinder.obstacleTag)
                {
                    Obstacle obstacle = hit.collider.gameObject.GetComponent<Obstacle>();
                    if (obstacle == null)
                    {
                        if (knotIndex < knots.Count - 1) knotIndex++;
                    }
                    else if (!obstacle.isObstacle(pathfinder)) if (knotIndex < knots.Count - 1) knotIndex++;
                }
                else if (!isHitting)
                {
                    if (knotIndex < knots.Count - 1) knotIndex++;
                    else knots = new List<PathKnot>();//DONE
                }
            }
        }
    }

    public void foundPath(Pathinfo info)
    {
        if (info.foundPath)
        {
            path = info.path;
            if (knots.Count == 0)
            {
                knotIndex = 1;
                knots = info.path.getPathList();
            }
            else
            {
                knots.RemoveRange(knotIndex, knots.Count - knotIndex);
                knots.AddRange(info.path.getPathList());
            }
        }
        else
        {
            Debug.Log(info.comment);
            state = -2;
        }
    }
}
