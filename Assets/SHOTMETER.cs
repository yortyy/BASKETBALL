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
    private void Start()
    {
        playermov = player.GetComponent<playermovement>();
    }
    void Update()
    {
        targ = (Camera.main.WorldToScreenPoint(target.transform.position) + new Vector3(40, 0, 0));
        ShotMeterUI.transform.position = targ;
        if (shoottimeron)
        {
            shotmeterslider.value = (Time.time - shotmetertimer) * 1.5f;
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
            if(shotmetertimer == 0)
            {
                playermov.smeter = 0;
                shottext.color = Color.blue;
                shottext.text = "WET LIKE WATER";
            }
            else if (shotmetertimer <= 2)
            {
                playermov.smeter = 1;
                shottext.color = Color.green;
                shottext.text = "IRISH SPRING GREEN GREEN";
            } //0-3 is ex, 4 - 8 is sl/se, 9-
            else if(shotmetertimer <= 7)
            {
                playermov.smeter = 4;
                shottext.color = Color.yellow;
                shottext.text = "Slightly Late / Slightly Early";
            }
            else if (shotmetertimer <= 18)
            {
                playermov.smeter = 10;
                shottext.color = Color.yellow;
                shottext.text = "Late / Early";
            }
            else if (shotmetertimer <= 32)
            {
                playermov.smeter = 18;
                shottext.color = Color.red;
                shottext.text = "Very Late / Very Early";
            }
            else if (33 <= shotmetertimer)
            {
                playermov.smeter = 20;
                shottext.color = Color.black;
                shottext.text = "Nah";
            }
            Debug.Log("slider: " + shotmeterslider.value + " | smeter: " + Mathf.RoundToInt(shotmeterslider.value * 100) + " | smeter: " + playermov.smeter);
        }
    }
}
