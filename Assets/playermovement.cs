using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class playermovement : MonoBehaviour
{

    private Rigidbody rb;
    private Vector2 movementVector;
    public float mscale = 5f;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void moveinp(InputAction.CallbackContext movementValue)
    {
        movementVector = movementValue.ReadValue<Vector2>();

    }
    void FixedUpdate()
    {
        // Add a forward force
        rb.AddForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
        Debug.Log(movementVector);
    }
}
