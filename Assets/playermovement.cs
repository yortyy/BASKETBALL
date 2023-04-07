using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;

public class playermovement : MonoBehaviour
{

    private Rigidbody rb;
    private ParticleSystem ps;
    public SHOTMETER shotmeterscript;
    public GameObject basketballobj;
    private bassetball bballscript;
    private Vector2 movementVector;
    private bool jumpon;
    public float mscale = 5f;
    public float jscale = 5f;
    public int shootingskill;
    public int smeter;
    public bool shootbutton;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ps = GetComponent<ParticleSystem>();
        bballscript = basketballobj.GetComponent<bassetball>();
    }

    public void moveinp(InputAction.CallbackContext movementValue)
    {
        movementVector = movementValue.ReadValue<Vector2>();

    }
    public void jumpinp(InputAction.CallbackContext value)
    {
        rb.AddForce(0, jscale * mscale, 0, ForceMode.Impulse);
        Debug.Log("Jump");
    }
    public void shootimp(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            shootbutton = false;
            shotmeterscript.shotmetercalc(false);
        }
        if (value.canceled)
        {
            shootbutton = true;
            if (bballscript.playerholding)
            {
                
                //check for skill and shotmeter then put into shooter()
                //for this example, shootingskill is 1 (excellent), smeter is green (1)
                // 1 - green, 4 - searly, 6 - slate, 10 - early, 12 - late, 18 - very late/early, 20 - nah
                // 0 - excellent skill -> 10 (worst), minus from 80 -> 60
                shotmeterscript.shotmetercalc(true);
                if (smeter == 1)
                {
                    ps.Play();
                }
                bballscript.shooter(shootingskill, smeter);
            }
        }
    }


    void FixedUpdate()
    {
        // Add a forward force
        rb.AddForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
        //Debug.Log(movementVector);

    }

}
