using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform portalPoint;
    public Portal linkedPortal;
    public Ball heldBall;

    int waitFrames = 0;
    public int waitTimer = 250;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(heldBall != null && heldBall.beenGrabbed)
        {
            waitFrames++;
            if(waitFrames == waitTimer)
            {
                heldBall.fading = true;
                heldBall.fadingOut = true;
                heldBall = null;
                waitFrames = 0;
            }
        }
    }
}
