using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.InputSystem;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using Cinemachine;
using System.Linq;
using UnityEngine.Animations.Rigging;
using Unity.VisualScripting;

public class EventScriptSystem : MonoBehaviour
{
    [SerializeField] private GameObject trueplayer;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject basketball;
    private Rigidbody prb;
    private Rigidbody brb;
    private Transform[] tpmarkers = new Transform[8];
    private GameObject tpzone;

    private playermovement ps;
    private bassetball bb;
    [SerializeField] private SHOTMETER sm;

    private bool paused;
    [SerializeField] private GameObject ShotUI;
    [SerializeField] private GameObject PauseUI;
    [SerializeField] private TMP_Text timertext;
    private bool timeron;
    private float tmptimer;
    private int timersec;
    private int timersecsmall;
    private Transform kcamtracker;

    [SerializeField] private TMP_Text CamChangeText;
    [SerializeField] private TMP_Text FullcourtToggleText;
    [SerializeField] private GameObject playerlockcam;
    [SerializeField] private GameObject skycam;
    [SerializeField] private GameObject replaycam;
    [SerializeField] private CinemachineTargetGroup pltargetGroup;
    [SerializeField] private CinemachineTargetGroup kcamtargetGroup;
    [SerializeField] private CinemachineVirtualCamera plcam;

    private AudioSource asc;
    [SerializeField] AudioClip[] music;

    public int gamemode;

    public int CameraVer;


    [SerializeField] private GameObject[] teammates;
    [SerializeField] private GameObject[] enemies;
    private Rigidbody[] teammatesrb = new Rigidbody[2];
    private Rigidbody[] enemiesrb = new Rigidbody[3];
    private Vector3 pccamtempstartvector;
    private bool pccamsmooth;

    private float progress;
    private bool[] progressreset = new bool[4];
    private Vector3 tempkcampos;

    public int shotscore;
    public TMP_Text shotscoretext;
    public TMP_Text shotdistancetext;

    private Vector3 formerplayerdunklocation;

    [SerializeField] private GameObject[] Hoops = new GameObject[2]; //0 is N, 1 is S
    private GameObject CurrentHoop;
    [HideInInspector] public int HoopNum;
    private float HoopNegChanger = 1f;
    public bool fullcourt;

    [SerializeField] private hooplerpobjMover hooplerpobjmover;

    void Awake()
    {
        ps = player.GetComponent<playermovement>();
        bb = basketball.GetComponent<bassetball>();
        prb = player.GetComponent<Rigidbody>();
        brb = basketball.GetComponent<Rigidbody>();
        tpmarkers[0] = transform.GetChild(0);
        tpmarkers[1] = transform.GetChild(1);
        tpmarkers[2] = transform.GetChild(2);
        tpmarkers[3] = transform.GetChild(3);
        tpmarkers[4] = transform.GetChild(4);
        tpmarkers[5] = transform.GetChild(5);
        tpmarkers[6] = transform.GetChild(6);
        tpzone = transform.GetChild(7).gameObject;
        kcamtracker = transform.GetChild(8);
        asc = GetComponent<AudioSource>();
        plcam = playerlockcam.GetComponent<CinemachineVirtualCamera>();

        teammatesrb[0] = teammates[0].GetComponent<Rigidbody>();
        teammatesrb[1] = teammates[1].GetComponent<Rigidbody>();
        enemiesrb[0] = enemies[0].GetComponent<Rigidbody>();
        enemiesrb[1] = enemies[1].GetComponent<Rigidbody>();
        enemiesrb[2] = enemies[2].GetComponent<Rigidbody>();

        CameraVer = 1;
        camchange(0);
        CurrentHoop = Hoops[0];
    }
    public void camchange(int version)
    {
        CameraVer += version;
        if (CameraVer == 3)
        {
            CameraVer = 0;
        }

        if (CameraVer == 0)
        {
            playerlockcam.SetActive(true);
            skycam.SetActive(false);
            replaycam.SetActive(false);
            CamChangeText.text = "Player<br>Lock";
            Debug.Log("CameraVersion: " + CameraVer);
        }
        else if(CameraVer == 1)
        {
            playerlockcam.SetActive(false);
            skycam.SetActive(true);
            replaycam.SetActive(false);
            CamChangeText.text = "2kCam";
            Debug.Log("CameraVersion: " + CameraVer);
        }
        else if(CameraVer == 2) 
        {
            playerlockcam.SetActive(false);
            skycam.SetActive(false);
            replaycam.SetActive(true);
            CamChangeText.text = "Replay<br>Cam";
            Debug.Log("CameraVersion: " + CameraVer);
        }

    }

    public void PlayerChange(GameObject newplayer, bool keepcontrol)
    {

        formerplayerdunklocation = ps.DunkLocation;
        pccamtempstartvector = kcamtracker.position;
        plcam.Follow = newplayer.transform;
        pltargetGroup.RemoveMember(player.transform);
        pltargetGroup.AddMember(newplayer.transform, 4, 1);
        kcamtargetGroup.RemoveMember(player.transform);
        kcamtargetGroup.AddMember(newplayer.transform, 0.3f, 1);
        ps.hasball = false;
        if (!keepcontrol)
        {
            player.GetComponent<PlayerInput>().actions.Disable();
            ps.enabled = false;
        }

        player = newplayer;
        ps = player.GetComponent<playermovement>();
        if (!keepcontrol)
        {
            ps.enabled = true;
            player.GetComponent<PlayerInput>().actions.Enable();
        }
        ps.hasball = true;

        pccamsmooth = true;

        sm.player = player;
        sm.playermov = ps;

        ps.charactermodel = player.transform.GetChild(0).gameObject;
        ps.characteranimator = ps.charactermodel.GetComponent<Animator>();
        ps.characterrigs[0] = ps.charactermodel.transform.GetChild(ps.charactermodel.transform.childCount - 2).GetComponent<Rig>();
        ps.characterrigs[1] = ps.charactermodel.transform.GetChild(ps.charactermodel.transform.childCount - 1).GetComponent<Rig>();

        if(ps.Hoop.gameObject == CurrentHoop)
        {
            Debug.Log("changecurrenthoop");
            if(HoopNum == 0)
            {
                //ps.HeadTrackers[0].weight = 1;
                //ps.HeadTrackers[1].weight = 0;
            }
            else if(HoopNum == 1)
            {
                //ps.HeadTrackers[1].weight = 1;
                //ps.HeadTrackers[0].weight = 0;
            }
            ps.DunkLocation = formerplayerdunklocation;
            ps.HoopLookAt = Hoops[HoopNum].transform;
            ps.HoopProtector = Hoops[HoopNum].transform.GetChild(0).gameObject;
            ps.Hoop = Hoops[HoopNum].transform;
        }



        bb.PlayerChangeBBALL(player);

    }

    public void changemode(int gamem)
    {
        //practice = 0, 1on1 = 1, threept = 2, threewii = 3, 3on3 = 4
        gamemode = gamem;
        shotscore = 0;
        shotscoretext.text = shotscore.ToString();
        exitpause();
        if (gamem == 4)
        {
            asc.clip = music[3];
        }
        else
        {
            asc.clip = music[gamem];
        }
        asc.Play();

        if (gamem == 0 || gamem == 1 || gamem == 2)
        {
            teammates[0].SetActive(false);
            teammates[1].SetActive(false);
        }
        else if(gamem == 3 || gamem == 4)
        {
            teammates[0].SetActive(true);
            teammates[1].SetActive(true);

        }

        if(gamem == 2 || gamem == 3) //3player
        {
            if (gamem == 2)
            {
                prb.MovePosition(tpmarkers[0].position + new Vector3(0, 1.25f, 0));
                brb.MovePosition(tpmarkers[0].position + new Vector3(0, 1.25f, 0));
            }
            else if(gamem == 3)
            {
                CameraVer = 1;
                camchange(0);
                teammatesrb[0].MovePosition(tpmarkers[0].position + new Vector3(0, 1, 0));
                teammatesrb[1].MovePosition(tpmarkers[6].position + new Vector3(0, 1, 0));
                prb.MovePosition(tpmarkers[3].position + new Vector3(0, 1, 0));
                brb.MovePosition(tpmarkers[3].position + new Vector3(0, 1, 0));
                enemies[0].SetActive(true);
                enemiesrb[0].MovePosition(tpmarkers[0].position + new Vector3(-3, 1, 0));
                enemies[1].SetActive(true);
                enemiesrb[1].MovePosition(tpmarkers[6].position + new Vector3(3, 1, 0));
                enemies[2].SetActive(true);
                enemiesrb[2].MovePosition(tpmarkers[3].position + new Vector3(0, 1, 3));
            }
            timeron = true;
        }
        else
        {
            if (gamem == 4)
            {
                teammatesrb[0].MovePosition(tpmarkers[1].position);
                teammatesrb[1].MovePosition(tpmarkers[5].position);
            }

            timeron = false;
            prb.MovePosition(Vector3.zero);
            brb.MovePosition(Vector3.zero);
        }

        PlayerChange(trueplayer, false);
        if(HoopNum == 1 && fullcourt)
        {
            SwitchSides();
            bb.ballSwitchSides();
        }
    }
    private void exitpause()
    {
        if (paused)
        {
            asc.volume = 0.5f;
            PauseUI.SetActive(false);
            ShotUI.SetActive(true);
            Time.timeScale = 1;
            paused = false;
        }
    }

    public void threeptcontest(int tpmarkerno)
    {
        if (tpmarkerno == 0)
        {
            changemode(2);
            tpzone.SetActive(true);
            tmptimer = Time.time;
        }
        if(tpmarkerno == 7)
        {
            timeron = false;
            gamemode = 0;
            tpzone.SetActive(false);
        }
        else
        {
            tpzone.transform.position = tpmarkers[tpmarkerno].position;
        }


    }

    public void fullcourttoggle()
    {
        if(fullcourt)
        {
            if(HoopNum != 0)
            {
                SwitchSides();
            }
            FullcourtToggleText.text = "Half<br>court";
            fullcourt = false;
        }
        else
        {
            FullcourtToggleText.text = "Full<br>court";
            fullcourt = true;
        }
    }

    public void quitgame()
    {
        PlayerChange(trueplayer, false);
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void pauseimp(InputAction.CallbackContext value)
    {
        if(value.started)
        {
            if (!paused)
            {
                asc.volume = 0.2f;
                PauseUI.SetActive(true);
                ShotUI.SetActive(false);
                Time.timeScale = 0;
                paused = true;
            }
            else if (paused)
            {
                exitpause();
            }
        }
    }




    private void Update()
    {

        if (CameraVer == 1)
        {

            if(Mathf.Abs(player.transform.position.x) <= 5 && player.transform.position.z * HoopNegChanger >= 14)
            {

                if ((progressreset[0] == false && progress != 0) || pccamsmooth) //reset  so that when changed the zoom can start from true kcampos
                { //how to when changing zones, reset everything once
                    progress = 0;
                    if(pccamsmooth)
                    {
                        tempkcampos = pccamtempstartvector;
                    }
                    else
                    {
                        tempkcampos = kcamtracker.transform.position;
                    }
                    for (int i = 0; i < progressreset.Length; i++)
                    {
                        progressreset[i] = false;
                    }
                    pccamsmooth = false;
                    progressreset[0] = true; //if progress is leftover (no reset and had value), then reset to 0
                } //how would I turn on progressreset then?
                if (progressreset[0] && pccamsmooth)
                {
                    pccamsmooth = false;
                }

                progress = Mathf.Clamp01(progress + 2f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, 14 * HoopNegChanger)), progress);

            }
            else if (player.transform.position.z * HoopNegChanger >= 10f) //move kcam to 6
            {
                if ((progressreset[1] == false && progress != 0) || pccamsmooth)
                {
                    progress = 0;
                    if (pccamsmooth)
                    {
                        tempkcampos = pccamtempstartvector;
                    }
                    else
                    {
                        tempkcampos = kcamtracker.transform.position;
                    }
                    for (int i = 0; i < progressreset.Length; i++)
                    {
                        progressreset[i] = false;
                    }
                    pccamsmooth = false;
                    progressreset[1] = true;
                }
                if (progressreset[1] && pccamsmooth)
                {
                    pccamsmooth = false;
                }

                progress = Mathf.Clamp01(progress + 2f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, 10 * HoopNegChanger)), progress);

            }
            else if(player.transform.position.z * HoopNegChanger >= 0) //move kcam to player
            {

                if ((progressreset[2] == false && progress != 0) || pccamsmooth)
                {
                    progress = 0;
                    if (pccamsmooth)
                    {
                        tempkcampos = pccamtempstartvector;
                    }
                    else
                    {
                        tempkcampos = kcamtracker.transform.position;
                    }
                    for (int i = 0; i < progressreset.Length; i++)
                    {
                        progressreset[i] = false;
                    }
                    pccamsmooth = false;
                    progressreset[2] = true;
                }
                if (progressreset[2] && pccamsmooth)
                {
                    pccamsmooth = false;
                }


                progress = Mathf.Clamp01(progress + 2f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, player.transform.position.z - (1f * HoopNegChanger))), progress);


            }
            else if(player.transform.position.z * HoopNegChanger <= 0) //move kcam to player -2
            {

                if ((progressreset[3] == false && progress != 0) || pccamsmooth)
                {
                    progress = 0;
                    if (pccamsmooth)
                    {
                        tempkcampos = pccamtempstartvector;
                    }
                    else
                    {
                        tempkcampos = kcamtracker.transform.position;
                    }
                    for (int i = 0; i < progressreset.Length; i++)
                    {
                        progressreset[i] = false;
                    }
                    Debug.Log("Should be just one");
                    pccamsmooth = false;
                    progressreset[3] = true;
                }


                progress = Mathf.Clamp01(progress + 2f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, player.transform.position.z - (2 * HoopNegChanger))), progress);

            }

        }
        else if(CameraVer == 2)
        {
            kcamtracker.position = (new Vector3(0, 0, player.transform.position.z - 8));
        }
        if(timeron)
        {
            timersec = Mathf.FloorToInt(Time.time - tmptimer);
            timersecsmall = Mathf.FloorToInt(100 * ((Time.time - tmptimer) - Mathf.FloorToInt((Time.time - tmptimer))));

            if (timersec < 10 && timersecsmall < 10)
            {
                timertext.text = "0" + timersec + " : 0" + timersecsmall;
            }
            else if(timersec < 10)
            {
                timertext.text = "0" + timersec + " : " + timersecsmall;

            }
            else if(timersecsmall < 10)
            {
                timertext.text = timersec + " : 0" + timersecsmall;
            }
            else
            {
                timertext.text = timersec + " : " + timersecsmall;

            }


        }
    }






    public void SwitchSides()
    {
        CurrentHoop = ps.Hoop.gameObject;
        //ps.HeadTrackers[HoopNum].weight = 0;
        HoopNum += 1;
        HoopNegChanger *= -1;
        if(HoopNum == 2)
        {
            HoopNum = 0;
        }
        hooplerpobjmover.trackObjs(Hoops[HoopNum].transform, Hoops[HoopNum].transform.GetChild(2), Hoops[HoopNum].transform.GetChild(1));
        //ps.HeadTrackers[HoopNum].weight = 1;
        ps.DunkLocation = new Vector3(0, ps.DunkLocation.y, ps.DunkLocation.z * -1);
        ps.HoopLookAt = Hoops[HoopNum].transform;
        ps.Hoop = Hoops[HoopNum].transform;
        ps.HoopProtector = Hoops[HoopNum].transform.GetChild(0).gameObject;

        bb.Hoop = Hoops[HoopNum];
        //kcamtargetGroup.RemoveMember(CurrentHoop.transform.GetChild(1));
        //kcamtargetGroup.AddMember(Hoops[HoopNum].transform.GetChild(1), 1.4f, 1);
        //pltargetGroup.RemoveMember(CurrentHoop.transform.GetChild(2));
        //pltargetGroup.AddMember(Hoops[HoopNum].transform.GetChild(2), 3, 1);
    }
}
