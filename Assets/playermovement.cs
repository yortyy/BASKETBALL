using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using static UnityEditor.PlayerSettings;

public class playermovement : MonoBehaviour
{

    private Rigidbody rb;
    private ParticleSystem ps;
    private ParticleSystemRenderer psr;
    public SHOTMETER shotmeterscript;
    public GameObject basketballobj;
    private bassetball bballscript;
    private Rigidbody bbrb;
    private Vector2 movementVector;
    private bool jumpon;
    public float mscale = 5f;
    public float jscale = 5f;
    public int shootingskill;
    public int smeter;
    public bool shootbutton;



    public int shotpercent;
    private int shotresultnum;
    public bool shotresult;
    public int shotscore;
    private bool in3ptline;
    public TMP_Text shotscoretext;


    [SerializeField] Material[] ParticleMaterials;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ps = GetComponent<ParticleSystem>();
        psr = GetComponent<ParticleSystemRenderer>();
        bballscript = basketballobj.GetComponent<bassetball>();
        bbrb = basketballobj.GetComponent<Rigidbody>();
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
        if (bballscript.playerholding)
        {
            if (value.started)
            {
                shootbutton = false;
                shotmeterscript.shotmetercalc(false);
            }
            if (value.canceled)
            {
                shootbutton = true;

                //check for skill and shotmeter then put into shooter()
                //for this example, shootingskill is 1 (excellent), smeter is green (1)
                // 1 - green, 4 - searly, 6 - slate, 10 - early, 12 - late, 18 - very late/early, 20 - nah
                // 0 - excellent skill -> 10 (worst), minus from 80 -> 60
                shotmeterscript.shotmetercalc(true);
                if (smeter == 0)
                {
                    psr.material = ParticleMaterials[0];
                    ps.Play();
                }
                else if (smeter == 1)
                {
                    psr.material = ParticleMaterials[1];
                    ps.Play();
                }
                shooter(shootingskill, smeter);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("3ptline"))
        {
            in3ptline = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("3ptline"))
        {
            in3ptline = false;
        }
    }










    public void shooter(int shootingskill, int smeter)
    {
        bbrb.detectCollisions = false;
        bballscript.playerholding = false;
        shotpercent = 100 - (shootingskill * 2) - (smeter * 5); //out of 100, green - 95, slight early - 80, slight late - 70, early - 50, late - 40, very early/late - 10, nah - 0
        shotresultnum = (shotpercent - Random.Range(0, 100));
        if (shotresultnum > 0)
        {
            //shots good
            shotresult = true;
            if(in3ptline)
            {
                shotscore += 2;
            }
            else
            {
                shotscore += 3;
            }

        }
        else if (shotresultnum < 0)
        {
            //shots bad
            shotresult = false;
        }
        else if (shotresultnum == 0)
        {
            //special shot animation
            shotresult = true;
            if (in3ptline)
            {
                shotscore += 2;
            }
            else
            {
                shotscore += 3;
            }
        }
        Debug.Log("Shotresult: " + shotresult + " | Shotresultnum: " + shotresultnum + " | Shotpercentage: " + shotpercent);
        Debug.Log(shotresultnum);
        bballscript.shoot = true;

    }


    void FixedUpdate()
    {
        if((movementVector.x > 0 || movementVector.x < 0) || (movementVector.y > 0 || movementVector.y < 0))
        {
            rb.AddForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
        }

    }

}
