using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EventScriptSystem : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject basketball;
    private Transform[] tpmarkers = new Transform[5];
    private GameObject tpzone;

    private playermovement ps;
    private bassetball bb;


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

    public int CameraVer;

    void Awake()
    {
        ps = player.GetComponent<playermovement>();
        bb = basketball.GetComponent<bassetball>();
        tpmarkers[0] = transform.GetChild(0);
        tpmarkers[1] = transform.GetChild(1);
        tpmarkers[2] = transform.GetChild(2);
        tpmarkers[3] = transform.GetChild(3);
        tpmarkers[4] = transform.GetChild(4);
        tpzone = transform.GetChild(5).gameObject;
        kcamtracker = transform.GetChild(6);
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
            ps.shotscore = 0;
            ps.shotscoretext.text = ps.shotscore.ToString();
            PauseUI.SetActive(false);
            ShotUI.SetActive(true);
            tpzone.SetActive(true);
            tmptimer = Time.time;
            timeron = true;
            Time.timeScale = 1;
            ps.threeptcontest = true;
            player.transform.position = tpmarkers[0].position + new Vector3(0,1.25f,0);
            basketball.transform.position = tpmarkers[0].position + new Vector3(0, 1.25f, 0);
        }
        if(tpmarkerno == 5)
        {
            timeron = false;
            ps.threeptcontest = false;
            tpzone.SetActive(false);
        }
        else
        {
            tpzone.transform.position = tpmarkers[tpmarkerno].position;
        }


    }
    public void oneonone()
    {
        timeron = false;
        ps.shotscore = 0;
        ps.shotscoretext.text = ps.shotscore.ToString();
        ps.threeptcontest = false;
        PauseUI.SetActive(false);
        ShotUI.SetActive(true);
        Time.timeScale = 1;
        player.transform.position = Vector3.zero;
        basketball.transform.position = Vector3.zero;
    }
    public void quitgame()
    {
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        if(CameraVer == 1)
        {
            if(player.transform.position.z > 7) //if at -21, then should be -3, if at 0 should be -1, if at 4 should be 0
            {
                kcamtracker.position = (new Vector3(0, 0, 7));
            }
            else
            {
                //kcamtracker.position = (new Vector3(0, 0, player.transform.position.z - (3/((player.transform.position.z + 21.4884f)/ 21.4884f) + 1))); //3/1, 3/2 then 3/3 at full.
                kcamtracker.position = (new Vector3(0, 0, player.transform.position.z - (3 - (player.transform.position.z + 21.2884f)/9))); //3/1, 3/2 then 3/3 at full.
                Debug.Log((player.transform.position.z + 21.2884f)/9);
            }

            //kcamtracker.position = (new Vector3(0,0,player.transform.position.z - 8));
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
