using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    [SerializeField] private Transform HoopLookAt;
    [SerializeField] private Transform Hoop;
    [SerializeField] private GameObject HoopProtector;


    public int shotpercent;
    private int shotresultnum;
    public bool shotresult;
    public int shotscore;
    public float shotdistance;
    private bool in3ptline;
    private float shotdistancechanger;
    public TMP_Text shotscoretext;
    public TMP_Text shotdistancetext;
    [SerializeField] public EventScriptSystem ess;

    private bool inthreeptzone;
    public bool threeptcontest;

    [SerializeField] private GameObject ShotUI;
    [SerializeField] private GameObject PauseUI;

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
    public void pauseimp(InputAction.CallbackContext value)
    {
        PauseUI.SetActive(true);
        ShotUI.SetActive(false);
        Time.timeScale = 0;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("3ptline"))
        {
            in3ptline = true;
        }
        if (other.gameObject.CompareTag("3ptzoneforcontest"))
        {
            inthreeptzone = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("3ptline"))
        {
            in3ptline = false;
        }
        if (other.gameObject.CompareTag("3ptzoneforcontest"))
        {
            inthreeptzone = false;
        }
    }



    public void shooter(int shootingskill, int smeter)
    {
        //in 00.0 ft
        shotdistance = Mathf.Round(Vector3.Distance(new Vector3(Hoop.position.x, 0, Hoop.position.z), new Vector3(transform.position.x, 0, transform.position.z)) * 2.1872266f * 10) / 10;
        shotdistancetext.text = shotdistance + " ft";


        bbrb.detectCollisions = false;
        bballscript.playerholding = false;
        if (smeter == 0)
        {
            shotpercent = 100 - (shootingskill); //green
        }
        else if (smeter == 1)
        {
            shotpercent = 100 - (shootingskill); //green

            if (shotdistance >= 45)
            {
                shotpercent -= Mathf.RoundToInt(((shotdistance - 45) * 0.78f)) + 40;
            } //90 feet should be 20% 45 should be 60%
            else if (shotdistance >= 30)
            {
                shotpercent -= Mathf.RoundToInt((shotdistance - 30) * 2.667f) ;
            }
        }
        else if(smeter == 20)
        {
            shotpercent = 1;
        }
        else
        {
            
            if (in3ptline)
            {
                shotpercent = 100 - (shootingskill) - (smeter * 4); //60 late
            }
            else
            {
                shotpercent = 100 - ((shootingskill) * 2) - (smeter * 6); //40 late

                if (shotdistance >= 45)
                {
                    shotpercent -= Mathf.RoundToInt(((shotdistance - 45) * 0.78f)) + 40;
                } //90 feet should be 20% 45 should be 60%
                else if (shotdistance >= 30)
                {
                    shotpercent -= Mathf.RoundToInt(((shotdistance - 30) * 2.667f));
                }
            }
        }


        //out of 100, green - 99, slight early - 80, slight late - 70, early - 50, late - 40, very early/late - 10, nah - 0


        shotresultnum = (shotpercent - Random.Range(0, 100));
        if (shotresultnum >= 0)
        {
            if(threeptcontest) //3ptcontest
            {
                if(inthreeptzone)
                {
                    shotscore += 1;
                }
            }
            else //normal shooting
            {
                if (in3ptline)
                {
                    shotscore += 2;
                }
                else
                {
                    shotscore += 3;
                }
            }

            //shots good or special 0 wet like water
            HoopProtector.SetActive(false);
            shotresult = true;
        }
        else if (shotresultnum < 0)
        {
            //shots bad
            HoopProtector.SetActive(true);
            shotresult = false;
        }
        Debug.Log("Shotresult: " + shotresult + " | Shotresultnum: " + shotresultnum + " | Shotpercentage: " + shotpercent);
        Debug.Log(shotresultnum);
        bballscript.shoot = true;

    }


    void FixedUpdate()
    {
        if ((movementVector.x > 0 || movementVector.x < 0) || (movementVector.y > 0 || movementVector.y < 0))
        {
            if(0.5f > Mathf.Abs(HoopLookAt.position.x - transform.position.x) + Mathf.Abs(HoopLookAt.position.z + 0.5f - transform.position.z))
            {
                //Debug.Log("TRIPPING");
                transform.LookAt(new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f));
            }
            else
            {
                //Debug.Log(Mathf.Abs(HoopLookAt.position.x - transform.position.x) + Mathf.Abs(HoopLookAt.position.z - transform.position.z));
                transform.LookAt(new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f));
            }
            rb.AddRelativeForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
        }

    }

}
