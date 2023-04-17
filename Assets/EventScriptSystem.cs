using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        ps.shotscore = 0;
        ps.shotscoretext.text = ps.shotscore.ToString();
        ps.threeptcontest = false;
        PauseUI.SetActive(false);
        ShotUI.SetActive(true);
        Time.timeScale = 1;
        player.transform.position = Vector3.zero;
        basketball.transform.position = Vector3.zero;
    }

    private void Update()
    {
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
