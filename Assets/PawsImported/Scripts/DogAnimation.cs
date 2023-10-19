using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAnimation : MonoBehaviour
{
    private Animator animator; // Reference to the Animator component

    private void Start()
    {
        // Get the Animator component attached to the cube
        animator = GetComponent<Animator>();
    }

    public void PlayThumbsUpAnimation()
    {
        // Assuming "ThumbsUpAnimation" is the name of the animation trigger
       animator.SetTrigger("Ismove");
    }

    public void PlayThumbsDownAnimation()
    {
        // Assuming "ThumbsDownAnimation" is the name of the animation trigger
        animator.SetTrigger("Isrotate");
    }
}
