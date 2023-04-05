using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class playermovement : MonoBehaviour
{

    private Rigidbody rb;
    private Vector2 movementVector;
    private bool jumpon;
    public float mscale = 5f;
    public float jscale = 5f;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void moveinp(InputAction.CallbackContext movementValue)
    {
        movementVector = movementValue.ReadValue<Vector2>();

    }
    public void jumpinp(InputAction.CallbackContext movementValue)
    {
        rb.AddForce(0, jscale * mscale, 0, ForceMode.Impulse);
        Debug.Log("Jump");
    }
    void FixedUpdate()
    {
        // Add a forward force
        rb.AddForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
        //Debug.Log(movementVector);

    }
}
