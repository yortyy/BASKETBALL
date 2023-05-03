using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SHOTMETER : MonoBehaviour
{
    public GameObject player;
    public GameObject target;
    public GameObject ShotMeterUI;
    private playermovement playermov;
    public float shotmetertimer;
    public Slider shotmeterslider;
    private bool shoottimeron;
    public TMP_Text shottext;
    Vector3 targ;
    [SerializeField] float smSpeed = 1.5f;
    [SerializeField] int greencenterval = 100;

    private void Start()
    {
        playermov = player.GetComponent<playermovement>();
    }
    void LateUpdate()
    {
        targ = (Camera.main.WorldToScreenPoint(target.transform.position) + new Vector3(50, 0, 0));
        ShotMeterUI.transform.position = targ;
        if (shoottimeron)
        {
            if(1 < (Time.time - shotmetertimer) * smSpeed)
            {
                shotmeterslider.value = 1 - (((Time.time - shotmetertimer) * smSpeed) - 1);
                Debug.Log("suck: " + shotmeterslider.value);
            }
            else
            {
                shotmeterslider.value = (Time.time - shotmetertimer) * smSpeed;
            }
        }
    }
    public void shotmetercalc(bool shoot)
    {
        if(!shoot)
        {
            shotmetertimer = Time.time;
            shoottimeron = true;
            Debug.Log("shotmetertimer: " + shotmetertimer);
        }
        else if(shoot)
        {
            shoottimeron = false;
            shotmetertimer = (Mathf.RoundToInt((Time.time - shotmetertimer) * smSpeed * 100) - greencenterval);
            if(shotmetertimer == 0)
            {
                playermov.smeter = 0;
                shottext.color = Color.blue;
                shottext.text = "WET LIKE WATER";
            }
            else if (shotmetertimer <= -33 || 33 <= shotmetertimer) //bigger than 32
            {
                playermov.smeter = 20;
                shottext.color = Color.black;
                shottext.text = "Nah";
            }
            else if (-5 <= shotmetertimer && shotmetertimer <= 5) //not 0, between -2 and 2
            {
                playermov.smeter = 1;
                shottext.color = Color.green;
                shottext.text = "IRISH SPRING GREEN GREEN";
            } //0-3 is ex, 4 - 8 is sl/se, 9-
            else if(0 <= shotmetertimer && shotmetertimer <= 10) //not -2 to 2, 3 to 7
            {
                playermov.smeter = 4;
                shottext.color = Color.yellow;
                shottext.text = "Slightly Late";
            }
            else if (0 <= shotmetertimer && shotmetertimer <= 20) //not -2 to 7, 8 to 18
            {
                playermov.smeter = 10;
                shottext.color = Color.yellow;
                shottext.text = "Late";
            }
            else if (0 <= shotmetertimer && shotmetertimer <= 32) //not -2 to 18, 19 to 32
            {
                playermov.smeter = 18;
                shottext.color = Color.red;
                shottext.text = "Very Late";
            }
            else if(-10 <= shotmetertimer) //not -2 to 32, -7 to -2
            {
                playermov.smeter = 4;
                shottext.color = Color.yellow;
                shottext.text = "Slightly Early";
            }
            else if (-20 <= shotmetertimer) //not -2 to 7, 8 to 18
            {
                playermov.smeter = 10;
                shottext.color = Color.yellow;
                shottext.text = "Early";
            }
            else if (-32 <= shotmetertimer) //not -2 to 18, 19 to 32
            {
                playermov.smeter = 18;
                shottext.color = Color.red;
                shottext.text = "Very Early";
            }
            Debug.Log("slider: " + shotmeterslider.value + " | smeter: " + Mathf.RoundToInt(shotmeterslider.value * 100) + " | smeter: " + playermov.smeter);
        }
    }
}
