using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public VideoPlayer vp;
    private float timer;
    private bool pauseswitch;
    public GameObject faded;
    private Image fadedimg;
    public GameObject fadedtextobj;
    public GameObject fire;
    private TMP_Text fadedtext;
    private AudioSource maintheme;
    float fadedval;
    bool fadeon;
    bool fadestart;
    bool musdrop;

    public float vidspeed = 0.8f;


    void Awake()
    {
        fadedimg = faded.GetComponent<Image>();
        fadedtext = fadedtextobj.GetComponent<TMP_Text>();
        maintheme = gameObject.GetComponent<AudioSource>();
    }
    private void Update()
    {
        //Debug.Log("REEEEEEEEE: " + maintheme.time); //7.5 is beat drop
        if (maintheme.time >= (7.33f - (2.04f * (2 - vidspeed))) && !musdrop)
        {
            //Debug.Log("YURRRRRRR" + maintheme.time); //5
            faded.SetActive(false);
            fadedtextobj.SetActive(false);
            vp.playbackSpeed = vidspeed;
            timer = Time.time;
            musdrop = true;
        }
        else
        {
            fadedimg.color = new Color(0, 0, 0, 1.3f - (Time.time / 5.1f));
            fadedtext.color = new Color(1, 1, 1, 1.3f - (Time.time / 5.1f));
        }

        //Debug.Log(vp.clockTime);
        if (vp.clockTime >= 2.04f && !pauseswitch)
        {
            //Debug.Log("YURRRR 2222: " + maintheme.time); //5
            vp.Pause();
            fire.SetActive(true);
            pauseswitch = true;
        }


        if(fadeon == true)
        {
            //fadedval = (Time.time - fadedval) / 15;
            fadedimg.color = new Color(0, 0, 0, ((Time.time - fadedval) / 11) - 1);

            if((((Time.time - fadedval) / 11) - 1) >= 1)
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    public void FadeToBlack()
    {
        faded.SetActive(true);
        fadedval = Time.time;
        fadeon = true;
    }

}
