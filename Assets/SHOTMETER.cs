using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SHOTMETER : MonoBehaviour
{
    public GameObject player;
    public GameObject ShotMeterUI;
    [HideInInspector] public playermovement playermov;
    public float shotmetertimer;
    public Slider shotmeterslider;
    private bool shoottimeron;
    public TMP_Text shotRelease;
    public TMP_Text shotDifficulty;
    Vector3 targ;
    [SerializeField] float smSpeed = 1.5f;
    [SerializeField] int greencenterval = 100;

    private void Start()
    {
        playermov = player.GetComponent<playermovement>();
    }
    private void Update()
    {
        if (shoottimeron)
        {
            if (1 < (Time.time - shotmetertimer) * smSpeed)
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
    void LateUpdate()
    {
        targ = (Camera.main.WorldToScreenPoint(player.transform.position) + new Vector3(50, 0, 0));
        ShotMeterUI.transform.position = targ;
    }

    public float shotmetercalc(bool shoot)
    {
        int smeter = 0;
        if(!shoot)
        {
            shotmetertimer = Time.time;
            shoottimeron = true;
            Debug.Log("shotmetertimer: " + shotmetertimer);
        }
        else if(shoot)
        {
            shoottimeron = false;
            shotmetertimer = ((Time.time - shotmetertimer) * smSpeed) - 1;

            Debug.Log("slider: " + shotmeterslider.value + " | smeter: " + Mathf.RoundToInt(shotmeterslider.value * 100) + " | smeter: " + smeter);
        }
        return shotmetertimer;
    }

    public void SetShotDescription(float shotReleasePercent, float coveredPercent, bool shotEarly)
    {
        int intShotReleasePercent = Mathf.CeilToInt(shotReleasePercent * 100);
        int intCoveredPercent = Mathf.RoundToInt(coveredPercent * 100);
        string releaseColor= "";
        string releaseText = "";
        string releaseTiming = "";
        string coveredColor = "";
        string coveredText = "";

        if(shotEarly) {
            releaseTiming = "Early";
        }
        else {
            releaseTiming = "Late";
        }

        if (shotReleasePercent > 0.99f)
        {
            releaseColor= "blue";
            releaseText = "Perfect";
        }
        else if (shotReleasePercent >= 0.98f)
        {
            releaseColor= "green";
            releaseText = "Excellent";
        }
        else if (shotReleasePercent >= 0.80)
        {
            releaseColor= "yellow";
            releaseText = "Slightly " + releaseTiming;
        }
        else if (shotReleasePercent >= 0.70)
        {
            releaseColor= "yellow";
            releaseText = releaseTiming;
        }
        else if (shotReleasePercent >= 0.50)
        {
            releaseColor= "red";
            releaseText = "Very " + releaseTiming;
        }
        else
        {
            releaseColor= "black";
            releaseText = "Horrible";
        }

        if (coveredPercent == 0)
        {
            coveredColor = "blue";
            coveredText = "Wide Open";
        }
        else if (coveredPercent < 0.2)
        {
            coveredColor = "green";
            coveredText = "Open";
        }
        else if (coveredPercent < 0.4)
        {
            coveredColor = "yellow";
            coveredText = "Slightly Covered";
        }
        else if (coveredPercent < 0.7)
        {
            coveredColor = "yellow";
            coveredText = "Covered";
        }
        else if (coveredPercent < 0.9)
        {
            coveredColor = "red";
            coveredText = "Very Covered";
        }
        else
        {
            coveredColor = "black";
            coveredText = "Smothered";
        }

        shotRelease.text = "Shot Release: " + "<color=" + releaseColor+ ">" + releaseText + " " + intShotReleasePercent + "%</color>";
        shotDifficulty.text = "Shot Coverage: " + "<color=" + coveredColor + ">" + coveredText + " " + intCoveredPercent + "%</color>";

    }
}
