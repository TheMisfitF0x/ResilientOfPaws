using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Oculus.Interaction
{
public class kinematciControl : MonoBehaviour
{
    [SerializeField]
    private InteractorActiveState interactorActiveState; // Reference to the InteractorActiveState script.

    private Rigidbody rb; // Reference to the Rigidbody component.

    private void Awake()
    {
        // Get a reference to the Rigidbody component attached to this GameObject.
        rb = GetComponent<Rigidbody>();

        // Check if the InteractorActiveState script is assigned.
        if (interactorActiveState == null)
        {
            Debug.LogError("InteractorActiveState is not assigned. Please assign it in the Inspector.");
        }
    }

    private void Update()
    {
        // Check if the InteractorActiveState's Property includes the "IsSelecting" flag.
        if (interactorActiveState != null && (interactorActiveState.Property & InteractorActiveState.InteractorProperty.IsSelecting) != 0)
        {
            // Set the kinematic property to false if "IsSelecting" flag is included.
            rb.isKinematic = false;
        }
        else
        {
            // Set the kinematic property to true for other cases.
            rb.isKinematic = true;
        }
    }
}

}