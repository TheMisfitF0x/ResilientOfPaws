using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogBehavior : MonoBehaviour
{   //Array of all balls in ready2PickUp State
    Ball[] balls;

    //Ball Dog has targeted
    Ball targetBall;

    //Transform to attach ball to.
    public Transform lowerJawSpot;

    public Portal destPortal;
    public float retrieveSpeed;

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

        if(targetBall.readyFrames > 500 && targetBall.isReady2PickUp)
        {
            
            if(Vector3.Distance(this.transform.position, targetBall.transform.position) < .2f)
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
                this.transform.position = Vector3.MoveTowards(this.transform.position, targetBall.transform.position, retrieveSpeed);
                this.transform.LookAt(targetBall.transform);
                //Play running animation
            }
        }
        else if(hasBall)
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
                this.transform.position = Vector3.MoveTowards(this.transform.position, destPortal.transform.position, retrieveSpeed);
                this.transform.LookAt(destPortal.transform);
                //Play running animation
            }
        }
    }
}
