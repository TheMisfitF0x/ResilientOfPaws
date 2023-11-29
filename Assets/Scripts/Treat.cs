using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treat : Ball
{
    
    

    public void Reset()
    {
        
        myRB.useGravity = false;
        beenGrabbed = false;
        readyFrames = 0;
        stillFrames = 0;
        this.transform.position = initialPosition;
    }
}
