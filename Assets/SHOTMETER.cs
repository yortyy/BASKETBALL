using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SHOTMETER : MonoBehaviour
{
    public GameObject player;
    public GameObject ShotMeterUI;
    private playermovement playermov;
    public float shotmetertimer;
    public Slider shotmeterslider;
    private bool shoottimeron;
    private void Start()
    {
        playermov = player.GetComponent<playermovement>();
    }
    void Update()
    {
        ShotMeterUI.transform.position = (Camera.main.WorldToScreenPoint(player.transform.position) + new Vector3(25,25,0));
        if(shoottimeron)
        {
            shotmeterslider.value = Time.time - shotmetertimer;
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
            shotmetertimer = (Mathf.Abs((Mathf.RoundToInt(shotmeterslider.value * 100) - 77)));
            if (shotmetertimer < 4)
            {
                playermov.smeter = 1;
            } //0-3 is ex, 4 - 8 is sl/se, 9-
            else if(3 < shotmetertimer && shotmetertimer < 9)
            {
                playermov.smeter = 4;
            }
            else if (8 < shotmetertimer && shotmetertimer < 15)
            {
                playermov.smeter = 10;
            }
            else if (14 < shotmetertimer && shotmetertimer < 31)
            {
                playermov.smeter = 18;
            }
            else if (30 < shotmetertimer)
            {
                playermov.smeter = 20;
            }
            Debug.Log("slider: " + shotmeterslider.value + " | smeter: " + Mathf.RoundToInt(shotmeterslider.value * 100) + " | smeter: " + playermov.smeter);
        }
    }
}
