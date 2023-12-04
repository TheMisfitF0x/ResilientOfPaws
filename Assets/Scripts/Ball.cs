using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class Ball : MonoBehaviour
{
    //delay in seconds before a ball is teleported back when still.
    const int stillTimer = 250;
    public Material myMat;
    public Rigidbody myRB;
    public Vector3 initialPosition;
    public ActiveStateGroup grabDetection;

    public bool beenGrabbed = false;
    public bool isReady2PickUp = false;
    public bool fading = false;
    public bool fadingOut = false;
    public int readyFrames;

    protected int stillFrames = 0;

    public Transform parentDebug;
    public Light debug;
    // Start is called before the first frame update
    void Start()
    {
        
        myRB = this.gameObject.GetComponent<Rigidbody>();
        initialPosition = this.transform.position;
        grabDetection = this.gameObject.GetComponent<ActiveStateGroup>();
        readyFrames = 0;
        stillFrames = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (fading && beScored() == true)
        {
            fading = false;
            fadingOut = false;
        }

        if (grabDetection.Active && !beenGrabbed && Vector3.Distance(this.transform.position, initialPosition) > 3)
        {
            myRB.useGravity = true;
            beenGrabbed = true;
        }

        if (myRB.velocity.magnitude < Vector3.one.magnitude && beenGrabbed)
        {
            stillFrames++;
        }

        if (stillFrames == stillTimer && !isReady2PickUp)
        {
            isReady2PickUp = true;
        }
        else if(isReady2PickUp)
        {
            readyFrames++;
        }
    }

    //Called when ball travels through portals, or falls out of bounds.
    public void Reset()
    {
        beenGrabbed = false;
        readyFrames = 0;
        stillFrames = 0;
        this.transform.position = initialPosition;
        myRB.useGravity = false;
    }

    bool beScored()
    {
        if(fadingOut)
        {
            fadeOut();
            return false;
        }
        else
        {
            return fadeIn();
        }
    }

    void fadeOut()
    {
        float transparency = myMat.color.a;
        transparency -= .01f;
        myMat.color = new Vector4(myMat.color.r, myMat.color.g, myMat.color.b, transparency);
        if(transparency <=0)
        {
            Reset();
            fadingOut = false;
        }
    }

    bool fadeIn()
    {
        float transparency = myMat.color.a;
        transparency += .01f;
        myMat.color = new Vector4(myMat.color.r, myMat.color.g, myMat.color.b, transparency);
        if(transparency >= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Despawn()
    {
        Destroy(this.gameObject);
    }
}
