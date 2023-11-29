using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogBehavior : MonoBehaviour
{   //Array of all balls in ready2PickUp State
    Ball[] balls;

    //Ball Dog has targeted
    public Ball targetBall;

    //Transform to attach ball to.
    public Transform lowerJawSpot;

    public Portal destPortal;
    public float retrieveSpeed;
    public float rotationSpeed;
    public float reachedKnot = 1f;
    public int state = -1;
    public int knotIndex = 1;


    bool hasBall = false;
    NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        balls = GameObject.FindObjectsOfType<Ball>();
        Debug.Log(balls.Length);
        targetBall = balls[0];
        //pathfinder = new Pathfinder<Vector3>(GetDistance, GetNeighbourNodes, 100);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (targetBall == null)
        {
            balls = GameObject.FindObjectsOfType<Ball>();
            foreach (Ball ball in balls)
            {
                Debug.Log(ball);
                Debug.Log(PathFind(ball.transform));
                if (ball.isReady2PickUp && ball.readyFrames > targetBall.readyFrames)
                {
                    targetBall = ball;
                    agent.destination = ball.transform.position;
                }
            }
        }

        if (targetBall != null && targetBall.readyFrames > 250 && targetBall.isReady2PickUp)
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
                agent.destination = targetBall.transform.position;
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
                agent.destination = destPortal.transform.position;
                //Play running animation
            }
        }
    }

    bool PathFind(Transform target)
    {
        
        return true;
    }
}
