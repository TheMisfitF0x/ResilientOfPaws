using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class Ball : MonoBehaviour
{
    //delay in seconds before a ball is teleported back when still.
    const int stillTimer = 500;

    public Rigidbody myRB;
    public Vector3 initialPosition;
    public ActiveStateGroup grabDetection;

    public bool beenGrabbed = false;
    public bool isReady2PickUp = false;
    public int readyFrames;

    int stillFrames = 0;

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
        if(grabDetection.Active && !beenGrabbed)
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
    public void Reset(Portal resetPoint)
    {
        myRB.useGravity = false;
        beenGrabbed = false;
        readyFrames = 0;
        stillFrames = 0;
        this.transform.position = resetPoint.portalPoint.position;
    }
}
