using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aoiti.Pathfinding;

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
    public float reachedKnot = 1f;
    public int state = -1;
    public int knotIndex = 1;

    bool hasBall = false;

    Pathfinder<Vector3> pathfinder;
    List<Vector3> path;
    int curNode = 0;
    // Start is called before the first frame update
    void Start()
    {
        pathfinder = new Pathfinder<Vector3>(GetDistance, GetNeighbourNodes, 100);
        balls = GameObject.FindObjectsOfType<Ball>();
        targetBall = balls[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        
        if (targetBall == null)
        {
            balls = GameObject.FindObjectsOfType<Ball>();
            foreach (Ball ball in balls)
            {
                if (ball.isReady2PickUp && ball.readyFrames > targetBall.readyFrames)
                {
                    targetBall = ball;
                    if (pathfinder.GenerateAstarPath(transform.position, ball.transform.position, out path)) ;
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
                if (pathfinder.GenerateAstarPath(transform.position, destPortal.transform.position, out path)) ;
                curNode = 0;
                
            }
            else
            {
                this.transform.position = Vector3.MoveTowards(transform.position, path[curNode], retrieveSpeed);
                if (transform.position == path[curNode])
                {
                    curNode++;
                }
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
                curNode = 0;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, path[curNode], retrieveSpeed);
                if(transform.position == path[curNode])
                {
                    curNode++;
                }
                //Play running animation
            }
        }
    }

    float GetDistance(Vector3 A, Vector3 B)
    {
        return (A - B).sqrMagnitude; 
    }

    Dictionary<Vector3, float> GetNeighbourNodes(Vector3 pos)
    {
        Dictionary<Vector3, float> neighbours = new Dictionary<Vector3, float>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k=-1;k<2;k++)
                {

                        if (i == 0 && j == 0 && k==0) continue;

                        Vector3 dir = new Vector3(i, j,k);
                        if (!Physics2D.Linecast(pos, pos + dir))
                        {
                            neighbours.Add(pos + dir, dir.magnitude);
                        }
                    }
                }

            }
            return neighbours;
    }

    void PathFinderMethod(Transform target)
    {
        
    }

    public void foundPath()
    {

    }
}
