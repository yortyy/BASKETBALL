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
    [SerializeField] private GameObject playerlockcam;
    [SerializeField] private GameObject skycam;
    [SerializeField] private GameObject replaycam;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private CinemachineVirtualCamera plvcam;

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
        plvcam = playerlockcam.GetComponent<CinemachineVirtualCamera>();

        teammatesrb[0] = teammates[0].GetComponent<Rigidbody>();
        teammatesrb[1] = teammates[1].GetComponent<Rigidbody>();
        enemiesrb[0] = enemies[0].GetComponent<Rigidbody>();
        enemiesrb[1] = enemies[1].GetComponent<Rigidbody>();
        enemiesrb[2] = enemies[2].GetComponent<Rigidbody>();

        CameraVer = 1;
        camchange(0);
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

    public void PlayerChange(GameObject newplayer)
    {
        player.GetComponent<PlayerInput>().actions.Disable();

        pccamtempstartvector = kcamtracker.position;
        plvcam.Follow = newplayer.transform;
        targetGroup.RemoveMember(player.transform);
        targetGroup.AddMember(newplayer.transform, 4, 1);
        ps.enabled = false;

        player = newplayer;
        ps = player.GetComponent<playermovement>();
        ps.enabled = true;
        player.GetComponent<PlayerInput>().actions.Enable();
        pccamsmooth = true;

        sm.player = player;
        sm.playermov = ps;

        bb.PlayerChangeBBALL(player);

        //change camera, everything with player to new one
    }

    public void practicemode()
    {
        gamemode = 0;
        timeron = false;
        ps.shotscore = 0;
        ps.shotscoretext.text = ps.shotscore.ToString();
        exitpause();
        PlayerChange(trueplayer);
        asc.clip = music[0];
        asc.Play();
        teammates[0].SetActive(false);
        teammates[1].SetActive(false);
        prb.MovePosition(Vector3.zero);
        brb.MovePosition(Vector3.zero);
        //player.transform.position = Vector3.zero;
        //basketball.transform.position = Vector3.zero;
    }
    public void oneonone()
    {
        gamemode = 1;
        timeron = false;
        ps.shotscore = 0;
        exitpause();
        PlayerChange(trueplayer);
        asc.clip = music[1];
        asc.Play();
        teammates[0].SetActive(false);
        teammates[1].SetActive(false);
        prb.MovePosition(Vector3.zero);
        brb.MovePosition(Vector3.zero);
        //player.transform.position = Vector3.zero;
        //basketball.transform.position = Vector3.zero;
    }

    public void threeptcontest(int tpmarkerno)
    {
        if (tpmarkerno == 0)
        {
            exitpause();
            PlayerChange(trueplayer);
            asc.clip = music[2];
            asc.Play();
            ps.shotscore = 0;
            ps.shotscoretext.text = ps.shotscore.ToString();
            tpzone.SetActive(true);
            tmptimer = Time.time;
            timeron = true;
            gamemode = 2;
            teammates[0].SetActive(false);
            teammates[1].SetActive(false);
            prb.MovePosition(tpmarkers[0].position + new Vector3(0, 1.25f, 0));
            brb.MovePosition(tpmarkers[0].position + new Vector3(0, 1.25f, 0));
            //player.transform.position = tpmarkers[0].position + new Vector3(0,1.25f,0);
            //basketball.transform.position = tpmarkers[0].position + new Vector3(0, 1.25f, 0);
        }

        if(tpmarkerno == 7)
        {
            timeron = false;
            ps.ess.gamemode = 0;
            tpzone.SetActive(false);
        }
        else
        {
            tpzone.transform.position = tpmarkers[tpmarkerno].position;
        }


    }
    public void threeonthreewii()
    {
        gamemode = 3;
        teammates[0].SetActive(true);
        teammatesrb[0].MovePosition(tpmarkers[0].position + new Vector3(0,1,0));
        teammates[1].SetActive(true);
        teammatesrb[1].MovePosition(tpmarkers[6].position + new Vector3(0, 1, 0));


        enemies[0].SetActive(true);
        enemiesrb[0].MovePosition(tpmarkers[0].position + new Vector3(-3, 1, 0));
        enemies[1].SetActive(true);
        enemiesrb[1].MovePosition(tpmarkers[6].position + new Vector3(3, 1, 0));
        enemies[2].SetActive(true);
        enemiesrb[2].MovePosition(tpmarkers[3].position + new Vector3(0, 1, 3));

        timeron = false;
        ps.shotscore = 0;
        exitpause();
        PlayerChange(trueplayer);
        CameraVer = 1;
        camchange(0);
        asc.clip = music[3];
        asc.Play();
        prb.MovePosition(tpmarkers[3].position + new Vector3(0, 1, 0));
        brb.MovePosition(tpmarkers[3].position + new Vector3(0, 1, 0));
        //player.transform.position = Vector3.zero;
        //basketball.transform.position = Vector3.zero;
    }
    public void threeonthree()
    {
        gamemode = 4;
        teammates[0].SetActive(true);
        teammatesrb[0].MovePosition(tpmarkers[1].position);
        teammates[1].SetActive(true);
        teammatesrb[1].MovePosition(tpmarkers[5].position);
        timeron = false;
        ps.shotscore = 0;
        exitpause();
        PlayerChange(trueplayer);
        asc.clip = music[3];
        asc.Play();
        prb.MovePosition(Vector3.zero);
        brb.MovePosition(Vector3.zero);
        //player.transform.position = Vector3.zero;
        //basketball.transform.position = Vector3.zero;
    }


    public void quitgame()
    {
        PlayerChange(trueplayer);
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

    private void Update()
    {

        if (CameraVer == 1)
        {
            if(player.transform.position.z > 7) //if at -21, then should be -3, if at 0 should be -1, if at 4 should be 0
            {
                //kcamtracker.position = (new Vector3(0, 0, 7));
            }
            else
            {
                //kcamtracker.position = (new Vector3(0, 0, player.transform.position.z - (3/((player.transform.position.z + 21.4884f)/ 21.4884f) + 1))); //3/1, 3/2 then 3/3 at full.
                //kcamtracker.position = (new Vector3(0, 0, player.transform.position.z - (3 - (player.transform.position.z + 21.2884f)/9))); //3/1, 3/2 then 3/3 at full.
                //Debug.Log((player.transform.position.z + 21.2884f)/9);
            }

            //kcamtracker.position = (new Vector3(0,0,player.transform.position.z - 8));
            if(Mathf.Abs(player.transform.position.x) <= 5 && player.transform.position.z >= 14)
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
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, 12)), progress);

            }
            else if (player.transform.position.z >= 6) //move kcam to 6
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
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, 9)), progress);

            }
            else if(player.transform.position.z >= 0) //move kcam to player
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
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, player.transform.position.z)), progress);


            }
            else if(player.transform.position.z <= 0) //move kcam to player -2
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
                kcamtracker.position = Vector3.Lerp(tempkcampos, (new Vector3(0, 0.5f, player.transform.position.z - 2)), progress);

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
}
