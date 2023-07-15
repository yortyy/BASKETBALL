using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Animations.Rigging;

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
    private Quaternion qTo;
    private bool jumpon;
    public float mscale = 5f;
    public float jscale = 5f;
    public int shootingskill;
    public int smeter;
    public bool shootbuttonon;
    public bool shootbuttonbuffer;
    public bool smcalcstarted;


    public Transform HoopLookAt;
    public Transform Hoop;
    public GameObject HoopProtector;
    public MultiAimConstraint[] HeadTrackers = new MultiAimConstraint[2]; // 0 is N, 1 is S


    public int shotpercent;
    private int shotresultnum;
    public bool shotresult;
    public float shotdistance;
    public bool in3ptline;
    [SerializeField] public EventScriptSystem ess;

    private bool inthreeptzone;

    [SerializeField] private GameObject ShotUI;
    [SerializeField] private GameObject PauseUI;

    [SerializeField] Material[] ParticleMaterials;

    [SerializeField] private GameObject[] TeamMates;
    private bool shootboolwii;

    [HideInInspector] public GameObject charactermodel;
    [HideInInspector] public Animator characteranimator;
    [HideInInspector] public Rig[] characterrigs = new Rig[2]; //0 is head, 1 is bball
    private Vector2 rightstick;
    private bool emoteson;
    private int emotenum;

    [HideInInspector] public Vector3 DunkLocation = new Vector3(0, 2.3f, 20.3f);
    private Vector3 StartDunkLocation;
    private float dunkcount;
    [HideInInspector] public int dunk;

    private bool resetrot;
    private float hoopdistance;

    private bool defenceon;

    void Awake()
    {

        charactermodel = transform.GetChild(0).gameObject;
        characteranimator = charactermodel.GetComponent<Animator>();
        characterrigs[0] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 2).GetComponent<Rig>();
        characterrigs[1] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 1).GetComponent<Rig>();
        HeadTrackers[0] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 1).GetChild(2).gameObject.GetComponent<MultiAimConstraint>();
        //HeadTrackers[1] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 1).GetChild(3).gameObject.GetComponent<MultiAimConstraint>();
        rb = GetComponent<Rigidbody>();
        ps = GetComponent<ParticleSystem>();
        psr = GetComponent<ParticleSystemRenderer>();
        bballscript = basketballobj.GetComponent<bassetball>();
        bbrb = basketballobj.GetComponent<Rigidbody>();
        bballscript.bballholdref = charactermodel.transform.GetChild(charactermodel.transform.childCount - 3);
        bballscript.bbrelease = bballscript.bballholdref.GetComponent<bballrelease>();
    }

    public void moveinp(InputAction.CallbackContext movementValue)
    {
        movementVector = movementValue.ReadValue<Vector2>();
    }
    public void jumpinp(InputAction.CallbackContext value)
    {
        rb.AddForce(0, jscale * mscale, 0, ForceMode.Impulse);
    }
    public void defenceinp(InputAction.CallbackContext value)
    {
        if(value.started)
        {
            characteranimator.SetBool("dstance", true);
            defenceon = true;
        }
        else if(value.canceled)
        {
            characteranimator.SetBool("dstance", false);
            defenceon = false;
        }
    }
    public void shootinp(InputAction.CallbackContext value)
    {
        //dunking
        if (value.started && movementVector.y > 0.1f && 14 >= Mathf.Round(Vector3.Distance(new Vector3(Hoop.position.x, 0, Hoop.position.z), new Vector3(transform.position.x, 0, transform.position.z)) * 2.1872266f * 10) / 10)
        {
            bballscript.bbrelease.shotreleasenow = false;
            dunkcount = 0f;
            StartDunkLocation = transform.position;
            bballscript.dunkedtheball = true;
            shooter(0, 0);
            rb.useGravity = false;
            dunk = 1;
            characteranimator.SetInteger("DunkNow", 1);
        }


        if (value.started && dunk == 0 && (shootboolwii || ess.gamemode != 3))
        {
            shootbuttonon = true;
            shootbuttonbuffer = true;
            Debug.Log("shootbuttonbuffer on");
            if(bballscript.playerholding)
            {
                characteranimator.SetInteger("EmoteNum", 0);
                characteranimator.SetBool("ShootNow", true);
                shotmeterscript.shotmetercalc(false);
            }
        }
        else if(value.started && dunk == 0 && !shootboolwii && ess.gamemode == 3) 
        {
            ess.CameraVer = 0;
            ess.camchange(0);
            shootboolwii = true;
        }
        if (value.canceled && dunk == 0 && (shootbuttonbuffer && shootboolwii || ess.gamemode != 3))
        {
            shootbuttonon = false;
            shootbuttonbuffer = false;

            if (bballscript.playerholding)
            {
                characteranimator.speed = 1f;
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
                if (ess.gamemode == 3)
                {
                    shootboolwii = false;
                }
            }

        }
    }

    private void Update()
    {
        if(dunk == 1 && dunkcount < 1 && bballscript.bbrelease.jumpdunknow)
        {
            Debug.Log(DunkLocation);
            rb.MovePosition(Vector3.Lerp(StartDunkLocation, DunkLocation, dunkcount));
            dunkcount = Mathf.Clamp01(2 * Time.deltaTime + dunkcount);
        }
        else if(dunk == 1 && dunkcount >= 1 && bballscript.bbrelease.shotreleasenow)
        {
            Debug.Log("dunkinrn");
            rb.isKinematic = true; //need the shotreleasenow to rigoff
            shotmeterscript.shottext.color = Color.red;
            shotmeterscript.shottext.text = "!!DUNK!!";
            bballscript.ShotHits(true);
            characteranimator.SetInteger("DunkNow", 2);
            dunk = 2;
        }
        else if(dunk == 3 && bballscript.bbrelease.dunkfallnow)
        {
            characteranimator.SetInteger("DunkNow", 0);
            rb.useGravity = true;
            rb.isKinematic = false;
            bballscript.dunkedtheball = false;

            dunkcount = 0f;
            dunk = 0;
        }

        if (shootbuttonbuffer && bballscript.playerholding && ess.gamemode != 3) //shootbuttonbuffer buffering/waiting for bball hold
        {
            characteranimator.SetInteger("EmoteNum", 0);
            characteranimator.SetBool("ShootNow", true);
            shotmeterscript.shotmetercalc(false);
            shootbuttonbuffer = false;
        }
        if (shootbuttonon && bballscript.playerholding && bballscript.bbrelease.pauseanimenow && characteranimator.speed == 1)
        {
            Debug.Log("FREEZE");
            characteranimator.speed = 0.0f;
        }
    }


    public void emoteinp(InputAction.CallbackContext value)
    {
        if(value.started)
        {
            emoteson = true;
        }
        else if(value.canceled)
        {
            emoteson = false;
            characteranimator.SetInteger("EmoteNum", 0);
        }
    }
    public void rightstickinp(InputAction.CallbackContext value)
    {
        int tempangle;
        rightstick = value.ReadValue<Vector2>();
        if(emoteson)
        {
            tempangle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.right, rightstick));
            Debug.Log(tempangle);
            if (rightstick == Vector2.zero)
            {
                tempangle = -1;
                emotenum = 0;
            }
            else
            {
            characteranimator.SetInteger("EmoteNum", emotenum);
            }
            if (0 <= tempangle && tempangle < 60)
            {
                emotenum = 1;
            }
            else if (60 <= tempangle && tempangle <= 120)
            {
                emotenum = 2;
            }
            else if (120 < tempangle && tempangle <= 180)
            {
                emotenum = 3;
            }
            else if (-60 <= tempangle && tempangle < 0)
            {
                emotenum = 4;
            }
            else if (-120 <= tempangle && tempangle < -60)
            {
                emotenum = 5;
            }
            else if (-180 < tempangle && tempangle < -120)
            {
                emotenum = 6;
            }

        }

    }


    float passANGLE;
    float passANGLE2;
    float passANGLE3;

    public void passinp(InputAction.CallbackContext value)
    {
        float negchange = 1;
        if (ess.gamemode == 3 || ess.gamemode == 4)
        {
            if (bballscript.playerholding && value.started && !shootboolwii)
            {
                if(ess.HoopNum == 1 && negchange != -1)
                {
                    negchange = -1;
                }
                else if(ess.HoopNum == 0 && negchange != 1)
                {
                    negchange = 1;
                }
                passANGLE2 = Vector2.SignedAngle(new Vector2(negchange * movementVector.x / 10, negchange * movementVector.y / 10), new Vector2(TeamMates[0].transform.position.x - transform.position.x, TeamMates[0].transform.position.z - transform.position.z));
                passANGLE3 = Vector2.SignedAngle(new Vector2(negchange * movementVector.x / 10, negchange * movementVector.y / 10), new Vector2(TeamMates[1].transform.position.x - transform.position.x, TeamMates[1].transform.position.z - transform.position.z));
                passANGLE = 0;
                if (Mathf.Abs(passANGLE2 - passANGLE) <= Mathf.Abs(passANGLE3 - passANGLE)) //if the ang between tm1 and p is closer to 0
                {
                    Debug.Log("TeamMate 1 BRUH");
                    characteranimator.SetBool("Moving", false);
                    ess.PlayerChange(TeamMates[0]);
                }
                else if (Mathf.Abs(passANGLE3 - passANGLE) < Mathf.Abs(passANGLE2 - passANGLE))
                {
                    Debug.Log("TeamMate 2 BRUH");
                    characteranimator.SetBool("Moving", false);
                    ess.PlayerChange(TeamMates[1]);
                }
                Debug.Log("TeamMates | PassAngle1: " + passANGLE + " | PassAngle2: " + passANGLE2 + " | PassAngle3: " + passANGLE3);
                Debug.Log("TeamMates | Player: " + gameObject + "Teammate1: " + TeamMates[0] + " | Teammate2: " + TeamMates[1]);
            }
        }
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
        ess.shotdistancetext.text = shotdistance + " ft";

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
                shotpercent -= Mathf.RoundToInt((shotdistance - 30) * 2.667f);
            }
        }
        else if (smeter == 20)
        {
            shotpercent = 0;
        }
        else
        {

            if (in3ptline)
            {
                shotpercent = 100 - (shootingskill) - (smeter * 5); //50% late, slate is 80%
            }
            else
            {
                shotpercent = 100 - ((shootingskill) * 2) - (smeter * 7); //30% late, slightly late is 4 * 7 = 79%

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
            if (ess.gamemode == 2) //3ptcontest
            {
                if (inthreeptzone)
                {
                    ess.shotscore += 1;
                }
            }
            else //normal shooting
            {
                if (in3ptline)
                {
                    ess.shotscore += 2;
                }
                else
                {
                    ess.shotscore += 3;
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


    private void FixedUpdate()
    {
        hoopdistance = Mathf.Round(Vector3.Distance(new Vector3(Hoop.position.x, 0, Hoop.position.z), new Vector3(transform.position.x, 0, transform.position.z)) * 2.1872266f * 10) / 10;

        if (ess.CameraVer == 0)
        {
            if (resetrot == true)
            {
                resetrot = false;
            }
            if (hoopdistance <= 5f)
            {
            }
            else if (hoopdistance <= 22f)
            {
                qTo = Quaternion.LookRotation(new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f) - transform.position);
                qTo = Quaternion.Slerp(transform.rotation, qTo, 3 * Time.deltaTime);
                rb.MoveRotation(qTo);
            }
            else
            {
                qTo = Quaternion.LookRotation(new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f) - transform.position);
                qTo = Quaternion.Slerp(transform.rotation, qTo, 10 * Time.deltaTime);
                rb.MoveRotation(qTo);
            }
        }
        else if (ess.CameraVer != 0 && defenceon)
        {
            qTo = Quaternion.LookRotation(transform.position - new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f));
            qTo = Quaternion.Slerp(transform.rotation, qTo, 10 * Time.deltaTime);
            rb.MoveRotation(qTo);
            if (resetrot == true)
            {
                resetrot = false;
            }
        }
        else if (ess.CameraVer != 0)
        {
            qTo = Quaternion.LookRotation(new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f) - transform.position);
            qTo = Quaternion.Slerp(transform.rotation, qTo, 10 * Time.deltaTime);
            rb.MoveRotation(qTo);
            if (resetrot == true)
            {
                resetrot = false;
            }
        }
        else if (!resetrot)
        {
            rb.MoveRotation((Quaternion.identity));
            resetrot = true;
        }
        if (ess.gamemode != 3)
        {
            if (dunk == 0 && !shootbuttonon && !bballscript.shoot && bballscript.bbrelease.rigoffnow && ((Mathf.Abs(movementVector.x) >= 0.1f || Mathf.Abs(movementVector.y) >= 0.1f)))
            {
                if (!characteranimator.GetBool("Moving"))
                {
                    characteranimator.SetBool("Moving", true);
                }
                characteranimator.SetFloat("ForwardAngleX", Mathf.Clamp(movementVector.x, -1f, 1f), 0.1f, Time.deltaTime);
                characteranimator.SetFloat("ForwardAngleY", Mathf.Clamp(movementVector.y, -1f, 1f), 0.1f, Time.deltaTime);
                if (ess.CameraVer == 0)
                {
                    rb.AddRelativeForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
                }
                else if (ess.HoopNum == 1)
                {
                    rb.AddForce(movementVector.x * -mscale, 0, movementVector.y * -mscale, ForceMode.Impulse);
                }
                else if (defenceon)
                {
                    rb.AddForce(movementVector.x * mscale * 0.75f, 0, movementVector.y * mscale * 0.75f, ForceMode.Impulse);
                }
                else
                {
                    rb.AddForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
                }
            }
            else if (dunk == 2 && ((Mathf.Abs(movementVector.x) >= 0.1f || Mathf.Abs(movementVector.y) >= 0.1f)))
            {
                characteranimator.SetInteger("DunkNow", 3);
                dunk = 3;
            }
            else if (characteranimator.GetBool("Moving"))
            {
                characteranimator.SetBool("Moving", false);
            }
        }
    }
}
