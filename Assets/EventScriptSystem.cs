using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.InputSystem;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

public class EventScriptSystem : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject basketball;
    private Rigidbody prb;
    private Rigidbody brb;
    private Transform[] tpmarkers = new Transform[8];
    private GameObject tpzone;

    private playermovement ps;
    private bassetball bb;


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

    private AudioSource asc;
    [SerializeField] AudioClip[] music;

    public int gamemode;


    public int CameraVer;

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


    public void threeptcontest(int tpmarkerno)
    {
        if (tpmarkerno == 0)
        {
            exitpause();
            asc.clip = music[2];
            asc.Play();
            ps.shotscore = 0;
            ps.shotscoretext.text = ps.shotscore.ToString();
            tpzone.SetActive(true);
            tmptimer = Time.time;
            timeron = true;
            gamemode = 2;
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
    public void oneonone()
    {
        gamemode = 1;
        timeron = false;
        ps.shotscore = 0;
        exitpause();
        asc.clip = music[1];
        asc.Play();
        prb.MovePosition(Vector3.zero);
        brb.MovePosition(Vector3.zero);
        //player.transform.position = Vector3.zero;
        //basketball.transform.position = Vector3.zero;
    }
    public void practicemode()
    {

        gamemode = 0;
        timeron = false;
        ps.shotscore = 0;
        ps.shotscoretext.text = ps.shotscore.ToString();
        exitpause();
        asc.clip = music[0];
        asc.Play();
        prb.MovePosition(Vector3.zero);
        brb.MovePosition(Vector3.zero);
        //player.transform.position = Vector3.zero;
        //basketball.transform.position = Vector3.zero;
    }


    public void quitgame()
    {
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
    public float progress;
    public bool progressreset;
    bool progressresettwo;

    private void Update()
    {
        if(CameraVer == 1)
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
            if(Mathf.Abs(player.transform.position.x) <= 4 && player.transform.position.z >= 18)
            {

                if (progressreset == false && progress != 0) //reset  so that when changed the zoom can start from true kcampos
                {
                    progress = 0;
                    progressreset = true; //if progress is leftover (no reset and had value), then reset to 0
                } //how would I turn on progressreset then?

                progress = Mathf.Clamp01(progress + 0.5f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(kcamtracker.position, (new Vector3(0, 0.5f, 12)), progress);

            }
            else if (player.transform.position.z >= 6) //move kcam to 6
            {
                if (progressreset == true && progress != 0)
                {
                    progress = 0;
                    progressreset = false;
                }

                progress = Mathf.Clamp01(progress + 0.5f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(kcamtracker.position, (new Vector3(0, 0.5f, 8)), progress);

            }
            else if(player.transform.position.z >= 0) //move kcam to player
            {
                if (progressreset == false && progress != 0)
                {
                    progress = 0;
                    progressreset = true;
                }

                progress = Mathf.Clamp01(progress + 0.5f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(kcamtracker.position, (new Vector3(0, 0.5f, player.transform.position.z)), progress);


            }
            else if(player.transform.position.z <= 0) //move kcam to player -2
            {
                if (progressreset == true && progress != 0)
                {
                    progress = 0;
                    progressreset = false;
                }
                progress = Mathf.Clamp01(progress + 0.5f * Time.deltaTime);
                kcamtracker.position = Vector3.Lerp(kcamtracker.position, (new Vector3(0, 0.5f, player.transform.position.z - 2)), progress);

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
