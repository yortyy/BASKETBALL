using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Animations.Rigging;
using Unity.VisualScripting;

public class playermovement : MonoBehaviour
{

    private Rigidbody rb;
    private Transform ts;
    private ParticleSystem ps;
    private ParticleSystemRenderer psr;
    public SHOTMETER shotmeterscript;
    public GameObject basketballobj;
    private bassetball bballscript;
    public bballrelease bbrel;
    private Rigidbody bbrb;
    private Transform bbt;
    private Vector2 movementVector;
    private Quaternion qTo;
    public float mscale = 5f;
    public float jscale = 5f;

    private int shootingSkill = 10;
    private int heatLevel = 0;
    private int shotsInARow = 0;
    public float shotDistance;

    public int shotMode;
    public float defenceRadius = 2.5f;
    public bool shootbuttonon;
    public bool shootbuttonbuffer;
    public bool shootingcurrently;
    public bool smcalcstarted;

    [SerializeField] private float leagueAverageUncovered3ptPercent = 0.45f;


    public Transform HoopLookAt;
    public Transform Hoop;
    public GameObject HoopProtector;
    public MultiAimConstraint[] HeadTrackers = new MultiAimConstraint[2]; // 0 is N, 1 is S

    [SerializeField] private bool rootMotionMovement;

    public int shotpercent;
    public bool shotResult;
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

    [HideInInspector] public Vector3 DunkLocationOffset;
    private Vector3 StartDunkLocation;
    private float dunkcount;
    [HideInInspector] public int dunk;

    private bool resetrot;
    private float hoopdistance;

    public bool hasball;
    public bool otherhasball;
    private bool defenceon;
    public bool needcheckballoutofthreept; //true cant shoot, need to check ball

    public bool blockjumpon;
    public bool blockjumprbnow;

    void Awake()
    {
        charactermodel = transform.GetChild(0).gameObject;
        characteranimator = charactermodel.GetComponent<Animator>();
        characterrigs[0] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 2).GetComponent<Rig>();
        characterrigs[1] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 1).GetComponent<Rig>();
        HeadTrackers[0] = charactermodel.transform.GetChild(charactermodel.transform.childCount - 1).GetChild(2).gameObject.GetComponent<MultiAimConstraint>();
        rb = GetComponent<Rigidbody>();
        ts = GetComponent<Transform>();
        ps = GetComponent<ParticleSystem>();
        psr = GetComponent<ParticleSystemRenderer>();
        bballscript = basketballobj.GetComponent<bassetball>();
        bbrel = charactermodel.transform.GetChild(charactermodel.transform.childCount - 3).GetChild(0).GetComponent<bballrelease>();
        bbrb = basketballobj.GetComponent<Rigidbody>();
        bbt = basketballobj.transform;
        DunkLocationOffset = new Vector3(0f, -1.63f, -0.52f);

    }
    void OnAnimatorMove()
    {
        ts.position += characteranimator.deltaPosition;
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
        if(value.started && !hasball)
        {
            characteranimator.SetBool("dstance", true);
            defenceon = true;
        }
        else if(value.canceled && !hasball)
        {
            characteranimator.SetBool("dstance", false);
            defenceon = false;
        }
    }
    public void shootinp(InputAction.CallbackContext value)
    {
        float shotMeterTimer;
        float shotReleasePercent;
        float coveredPercent;

        if (hasball && !ess.checkball)
        {
            //dunking
            if (value.started && movementVector.y > 0.1f && 14 >= Mathf.Round(Vector3.Distance(new Vector3(Hoop.position.x, 0, Hoop.position.z), new Vector3(transform.position.x, 0, transform.position.z)) * 2.1872266f * 10) / 10)
            {
                bbrel.shotreleasenow = false;
                dunkcount = 0f;
                StartDunkLocation = transform.position;
                bballscript.dunkedtheball = true;
                shooter(0, -1, -1);
                rb.useGravity = false;
                dunk = 1;
                characteranimator.SetInteger("DunkNow", 1);
            }


            if (value.started && dunk == 0 && (shootboolwii || ess.gamemode != 3))
            {
                shootbuttonon = true;
                shootbuttonbuffer = true;
                //Debug.Log("shootbuttonbuffer on");
                if (bballscript.playerholding)
                {
                    characteranimator.SetInteger("EmoteNum", 0);
                    characteranimator.SetBool("ShootNow", true);
                    shotmeterscript.shotmetercalc(false);
                }
            }
            else if (value.started && dunk == 0 && !shootboolwii && ess.gamemode == 3)
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

                    shotMeterTimer = shotmeterscript.shotmetercalc(true);

                    bool shotEarly = (shotMeterTimer < 0);
                    shotReleasePercent = 1 - Mathf.Abs(shotMeterTimer);
                    coveredPercent = GuardPercent(transform);

                    //Debug.Log("CoveredPercent: " + coveredPercent);
                    //Debug.Log("shotReleasePercent: " + shotReleasePercent);

                    shotmeterscript.SetShotDescription(shotReleasePercent, coveredPercent, shotEarly);
                    shooter(shootingSkill, shotReleasePercent, coveredPercent);
                    if (ess.gamemode == 3)
                    {
                        shootboolwii = false;
                    }
                }
            }
        }

    }

    private void Update()
    {
        if(blockjumpon && bbrel.blockjumpnow)
        {
            rb.AddForce(0, jscale, 0, ForceMode.Impulse);
            blockjumprbnow = true;
            blockjumpon = false;
        }
        else if(!bbrel.blockjumpnow && blockjumprbnow)
        {
            blockjumprbnow = false; //turns off blockjumprbnow when anim is done
            characteranimator.SetBool("BlockJump", false);
        }

        if(dunk == 1 && dunkcount < 1 && bbrel.jumpdunknow)
        {
            //Debug.Log(DunkLocationOffset);
            //Debug.Log(dunkcount);
            rb.MovePosition(Vector3.Lerp(StartDunkLocation, DunkLocationOffset + Hoop.transform.position, dunkcount));
            dunkcount = Mathf.Clamp01(3.5f * Time.deltaTime + dunkcount);
        }
        else if(dunk == 1 && dunkcount >= 1 && bbrel.shotreleasenow)
        {
            //Debug.Log("dunkinrn");
            //Debug.Log(dunkcount);
            //fencepost, lerp at one
            rb.MovePosition(Vector3.Lerp(StartDunkLocation, DunkLocationOffset + Hoop.transform.position, dunkcount));

            rb.isKinematic = true; //need the shotreleasenow to rigoff
            shotmeterscript.shotRelease.text = "Shot Release: " + "<color=red>Dunk</color>";
            bballscript.ShotHits(true);
            characteranimator.SetInteger("DunkNow", 2);
            dunk = 2;
        }
        else if(dunk == 3 && bbrel.dunkfallnow)
        {
            bbrel.shotreleasenow = false;
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
        if (shootbuttonon && bballscript.playerholding && bbrel.pauseanimenow && characteranimator.speed == 1)
        {
            //Debug.Log("FREEZE");
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
            //Debug.Log(tempangle);
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
        if(!hasball && !blockjumpon && !blockjumprbnow && value.started)
        {
            characteranimator.SetBool("BlockJump", true);
            blockjumpon = true;
        }

        float negchange = 1;

        if (ess.gamemode == 3 || ess.gamemode == 4)
        {
            if (hasball && bballscript.playerholding && value.started && !shootboolwii)
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
                    //Debug.Log("TeamMate 1 BRUH");
                    characteranimator.SetBool("Moving", false);
                    ess.PlayerChange(TeamMates[0], false);
                }
                else if (Mathf.Abs(passANGLE3 - passANGLE) < Mathf.Abs(passANGLE2 - passANGLE))
                {
                    //Debug.Log("TeamMate 2 BRUH");
                    characteranimator.SetBool("Moving", false);
                    ess.PlayerChange(TeamMates[1], false);
                }
                //Debug.Log("TeamMates | PassAngle1: " + passANGLE + " | PassAngle2: " + passANGLE2 + " | PassAngle3: " + passANGLE3);
                //Debug.Log("TeamMates | Player: " + gameObject + "Teammate1: " + TeamMates[0] + " | Teammate2: " + TeamMates[1]);
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
            ess.checkball = false;
            in3ptline = false;
        }
        if (other.gameObject.CompareTag("3ptzoneforcontest"))
        {
            inthreeptzone = false;
        }
    }

    public float GuardPercent(Transform currentPlayer)
    {
        GameObject nearestEnemy = ess.NearestEnemy(currentPlayer);
        Transform nEnemyTF = nearestEnemy.transform;
        float GuardPercent = (1 - Mathf.Clamp01((Vector3.Distance(new Vector3(nEnemyTF.position.x, 0, nEnemyTF.position.z), new Vector3(currentPlayer.position.x, 0, currentPlayer.position.z)) - 0.8f) / (defenceRadius)));
        float GuardPercentSquared = GuardPercent * GuardPercent;
        float GuardPercentSquaredRounded = Mathf.Round(GuardPercent * 100) / 100;
        float GuardDistance = Vector3.Distance(new Vector3(nEnemyTF.position.x, 0, nEnemyTF.position.z), new Vector3(currentPlayer.position.x, 0, currentPlayer.position.z));
        float GuardDistanceFT = Mathf.Round(Vector3.Distance(new Vector3(nEnemyTF.position.x, 0, nEnemyTF.position.z), new Vector3(currentPlayer.position.x, 0, currentPlayer.position.z)) * 240 / 13) / 10;
        return GuardPercentSquaredRounded;
    }

    public void shooter(int shootingSkill, float shotReleasePercent, float coveredPercent)
    {
        float weightedShotReleasePercent;
        float shotDistancePercent = 0.0f;
        float shotProbability = 0.0f;
        float weightedCoveredPercent = (1 - coveredPercent) * -1;

        shotDistance = Mathf.Round(Vector3.Distance(new Vector3(Hoop.position.x, 0, Hoop.position.z), new Vector3(transform.position.x, 0, transform.position.z)) * 240 / 13) / 10;
        ess.shotDistanceText.text = shotDistance + " ft";

        //in 00.0 ft
        bbrb.detectCollisions = true;
        bballscript.physicalballcollider.enabled = false;
        bballscript.playerholding = false;
        ess.currentballhaver = null;
        if (shotReleasePercent == -1 && coveredPercent == -1)
        {
            psr.material = ParticleMaterials[2];
            ps.Play();
            shotProbability = 1;
        }
        else if (shotReleasePercent == 0)
        {
            psr.material = ParticleMaterials[0];
            ps.Play();
            shotProbability = 1;
            shotMode = 3;
        }
        else if (shotReleasePercent >= 0.98f)
        {
            psr.material = ParticleMaterials[1];
            ps.Play();
            shotProbability = 0.99f;
            shotMode = 2;
        }
        else if (shotReleasePercent < 0.5f)
        {
            shotProbability = 0.1f;
        }
        else
        {
            shotMode = 0;
            //irl: FG% AVG = 45%, 3PT% AVG = 35%, 3PT% Unguarded = 45%, Steph AVG  = 45%
            if (in3ptline)
            {
                shotDistancePercent = 45 / 35;
            }
            else
            {
                shotDistancePercent = 1;

                if (shotDistance > 30)
                {
                    shotDistancePercent = (30 - shotDistance + 25) / 35;
                }
            }

            if (coveredPercent == 0)
            {
                weightedCoveredPercent = 1;
            }
            else if (coveredPercent == 1)
            {
                weightedCoveredPercent = 0;
            }
            else
            {
                weightedCoveredPercent = 1 - coveredPercent;
            }

            weightedShotReleasePercent = shotReleasePercent / 0.97f;

            //Example: (leagueAVGuncovered 45% + skill 10% + heat 10%) * (slate release 95% / weighted 95%) * 3 point distance 100% * covered 0% = 65%
            shotProbability = (leagueAverageUncovered3ptPercent + (0.01f * shootingSkill) + (0.05f * heatLevel)) * weightedShotReleasePercent * shotDistancePercent * weightedCoveredPercent;
            //Debug.Log("LeagueAVGU: " + leagueAverageUncovered3ptPercent + " | Skill: " + shootingSkill + " | Heat: " + heatLevel + " | Release %: " + weightedShotReleasePercent + " | Distance: " + shotDistance + " | DistancePercent: " + shotDistancePercent + " | Coverage: " + weightedCoveredPercent);
        }
        float shotRandomRange = Random.Range(0.0f, 1.0f);
        shotResult = (shotRandomRange <= shotProbability);
        if (shotResult)
        {
            shotsInARow += 1;
            if(shotsInARow == 3)
            {
                heatLevel = 1;
            }
            else if (shotsInARow == 5)
            {
                heatLevel = 2;
            }
            else if (shotsInARow >= 8)
            {
                heatLevel = Mathf.FloorToInt((shotsInARow - (5.0f + (heatLevel - 2 * 3))) / 3);
            }

            if (ess.gamemode == 2)
            {
                if (inthreeptzone)
                {
                    ess.shotscore[0] += 1;
                }
            }
            else
            {
                if (in3ptline)
                {
                    if(ess.sballpossesion)
                    {
                        ess.shotscore[0] += 2;
                    }
                    else if(!ess.sballpossesion)
                    {
                        ess.shotscore[1] += 2;
                    }
                }
                else
                {
                    if (ess.sballpossesion)
                    {
                        ess.shotscore[0] += 3;
                    }
                    else if (!ess.sballpossesion)
                    {
                        ess.shotscore[1] += 3;
                    }
                }
            }

            //shots good or special 0 wet like water
            HoopProtector.SetActive(false);
            shotResult = true;
        }
        else
        {
            //shots bad
            shotsInARow = 0;
            heatLevel = 0;
            HoopProtector.SetActive(true);
        }
        //Debug.Log("shotResult: " + shotResult + " | shotResultRange: " + shotRandomRange + " | ShotProbability: " + shotProbability);
        shootingcurrently = true; //shootcurrently is same as shoot but needs to be seperate when disabling moving input
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
        else if (ess.CameraVer != 0 && !hasball && otherhasball)
        {
            //look oposite from hoop
            //qTo = Quaternion.LookRotation(transform.position - new Vector3(HoopLookAt.position.x, transform.position.y, HoopLookAt.position.z + 0.5f));

            //look at ball
            qTo = Quaternion.LookRotation(new Vector3(bbt.position.x, transform.position.y, bbt.position.z + 0.5f) - transform.position);
            qTo = Quaternion.Slerp(transform.rotation, qTo, 10 * Time.deltaTime);
            rb.MoveRotation(qTo);
            if (resetrot == true)
            {
                resetrot = false;
            }
        }
        else if (ess.CameraVer != 0 && hasball)
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
            if (dunk == 0 && !shootbuttonon && !shootingcurrently && bbrel.rigoffnow && ((Mathf.Abs(movementVector.x) >= 0.1f || Mathf.Abs(movementVector.y) >= 0.1f)))
            {
                if (!characteranimator.GetBool("Moving"))
                {
                    characteranimator.SetBool("Moving", true);
                }
                characteranimator.SetFloat("ForwardAngleX", Mathf.Clamp(movementVector.x, -1f, 1f), 0.05f, Time.deltaTime);
                characteranimator.SetFloat("ForwardAngleY", Mathf.Clamp(movementVector.y, -1f, 1f), 0.05f, Time.deltaTime);
                if(rootMotionMovement) {

                }
                else if (ess.CameraVer == 0)
                {
                    rb.AddRelativeForce(movementVector.x * mscale, 0, movementVector.y * mscale, ForceMode.Impulse);
                }
                else if (ess.HoopNum == 1)
                {
                    rb.AddForce(movementVector.x * -mscale, 0, movementVector.y * -mscale, ForceMode.Impulse);
                }
                else if (blockjumpon || bbrel.blockjumpnow)
                {
                    rb.AddForce(movementVector.x * mscale * 0.2f, 0, movementVector.y * mscale * 0.75f, ForceMode.Impulse);
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
