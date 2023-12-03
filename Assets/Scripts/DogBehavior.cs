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

    private Animator animator;


    bool hasBall = false;
    NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        balls = GameObject.FindObjectsOfType<Ball>();
        //pathfinder = new Pathfinder<Vector3>(GetDistance, GetNeighbourNodes, 100);

        StartCoroutine(RandomIdle());
        IEnumerator RandomIdle()
        {
            animator = GetComponent<Animator>();
            while (true)
            {
                yield return new WaitForSeconds(8);

                animator.SetInteger("IdleIndex", Random.Range(0, 3));
                animator.SetTrigger("Idle");
            }
        }
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
                if (ball.isReady2PickUp)
                {
                    targetBall = ball;
                    agent.destination = ball.transform.position;
                    animator.SetTrigger("Ismove");
                }
            }
        }

        if (targetBall != null && targetBall.readyFrames > 250 && targetBall.isReady2PickUp)
        {

            if (Vector3.Distance(this.transform.position, targetBall.transform.position) < .2f)
            {

                //Play pickup/eat animation.
                animator.SetTrigger("PickupReady");

                targetBall.myRB.useGravity = false;
                targetBall.isReady2PickUp = false;
                if (targetBall.CompareTag("Treat"))
                {
                    targetBall.Reset();

                    //Trigger celebration animation when Dog picks up the treat
                    animator.SetTrigger("GaveTreat");
                }
                else if (targetBall.CompareTag("Ball"))
                {
                    //Wait for animation to finish before proceeding
                    StartCoroutine(PickupWait());
                    IEnumerator PickupWait()
                    {
                        yield return new WaitForSeconds(3);
                        targetBall.transform.position = lowerJawSpot.transform.position;
                        targetBall.transform.SetParent(lowerJawSpot);
                        hasBall = true;
                        yield break;
                    }
                    
                }
            }
            else
            {                
                //Play running animation
                animator.SetTrigger("Ismove");
                agent.destination = targetBall.transform.position;
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
                animator.SetTrigger("PickupReady");
                hasBall = false;
                targetBall = null;
                animator.ResetTrigger("Ismove");
                animator.SetTrigger("GoToIdle");
            }
            else
            {
                //Play running animation
                animator.SetTrigger("Ismove");
                agent.destination = destPortal.transform.position;
            }
        }
    }

    bool PathFind(Transform target)
    {
        
        return true;
    }
}
