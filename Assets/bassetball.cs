using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;

public class bassetball : MonoBehaviour
{
    public Transform target;
    private GameObject player;
    private playermovement ps;
    [SerializeField] private EventScriptSystem ess;
    public bool shoot;
    public bool playerholding;
    private Rigidbody bbrb;
    float count = 0.0f;
    [SerializeField] private ParticleSystem HoopEff;
    [SerializeField] private AudioSource[] asc;
    private EmissionModule HoopEffEmission;
    [SerializeField] private ParticleSystemRenderer HoopEffrnr;
    [SerializeField] private GameObject firehoop;
    [SerializeField] private ParticleSystem bballeff;
    [SerializeField] Material[] ParticleMaterials;

    Vector3 startpoint;
    Vector3 archpoint;
    Vector3 targetpoint;
    float riselength;
    float shotinairtime;
    Vector3 offset;
    bool setarch;
    bool setcount;
    int rotat;

    public bool playerchange;
    Vector3 pastballpos;
    float progress;
    bool playerswitching;


    private float jumpshottime = 0f;
    //private float progressjs;
    private float progressshoot;
    //private Vector3 bballholdreftemppos;
    private GameObject bballRend;

    private float bballrigtime = 0.5f;
    private float bballrigtimecount;
    private bool bballrigset;

    [HideInInspector] public Transform bballholdref;
    [HideInInspector] public bballrelease bbrelease;

    void Awake()
    {
        bballRend = transform.GetChild(0).gameObject;
        bbrb = GetComponent<Rigidbody>();
        HoopEffEmission = HoopEff.emission;
        //player script gets bballholdref and bbrelease
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player") && !shoot && !playerswitching)
        {
            player = other.gameObject;
            ps = player.GetComponent<playermovement>();
            //need to stop playback or go back to idle animation
            bbrb.isKinematic = true;
            bbrb.detectCollisions = false;
            bbrb.useGravity = false;
            playerholding = true;
            Debug.Log("KONNECT: " + other.gameObject);
        }
    }
    public void PlayerChangeBBALL(GameObject newplayer)
    {
        pastballpos = transform.position;
        player = newplayer;
        ps = newplayer.GetComponent<playermovement>();
        bballholdref = ps.charactermodel.transform.GetChild(ps.charactermodel.transform.childCount - 3);
        bbrelease = ps.charactermodel.transform.GetChild(ps.charactermodel.transform.childCount - 3).GetComponent<bballrelease>();
        playerholding = false;
        playerswitching = true;
    }

    private void Update()
    {
        if(playerswitching)
        {
            progress = Mathf.Clamp01(progress + 2f * Time.deltaTime);
            transform.position = Vector3.Lerp(pastballpos, player.transform.position, progress);
        }
        if (progress >= 1)
        {
            playerholding = true;
            playerswitching = false;
            progress = 0;
        }
        if ((playerholding && !shoot) || (shoot && !bbrelease.shotreleasenow)) //rigon
        {
            transform.position = bballholdref.position;
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z + 0.5f));
            if (ps.characterrigs[1].weight != 1) //take existing weight, add by timecount then stop at 1
            {
                if(!bballrigset)
                {
                    bballrigtimecount = ps.characterrigs[1].weight;
                    bballrigset = true;
                }
                if(bbrelease.rigoffnow)
                {
                    bbrelease.rigoffnow = false;
                }
                bballrigtimecount = Mathf.Clamp01(bballrigtimecount + 1f/bballrigtime * Time.deltaTime);
                ps.characterrigs[1].weight = bballrigtimecount;
            }
        }
        else if((playerswitching || bbrelease.rigoffnow) && bballrigset) //rigoff
        {
            if (bbrelease.rigoffnow)
            {
                ps.characteranimator.SetBool("ShootNow", false);
            }
            if (ps.characterrigs[1].weight != 0f)
            {
                bballrigtimecount = Mathf.Clamp01(bballrigtimecount - 1f / bballrigtime * Time.deltaTime);
                ps.characterrigs[1].weight = bballrigtimecount;
            }
        }

        if (count <= (1.0f + jumpshottime) && shoot && bbrelease.shotreleasenow) //jumpshottimeneeded to movebball ref up in animation
        {
            if (rotat == 0)
            {
                transform.LookAt(new Vector3(target.position.x, bballRend.transform.position.y, target.position.z + 0.5f));
                rotat = 5;
            }
            else
            {
                rotat += 5;
                Quaternion rotation = Quaternion.Euler(rotat, bballRend.transform.rotation.y, bballRend.transform.rotation.z);
                bballRend.transform.rotation = rotation;
            }

            if (!setarch)
            {

                startpoint = bballholdref.position;
                if (!ps.shotresult)
                {
                    offset.x = Random.Range(-0.6f, 0.6f);
                    offset.z = Random.Range(-0.6f, -0.2f);
                    if (Mathf.Abs(offset.x) < 0.3f && Mathf.Abs(offset.z) < 0.3f)
                    {
                        offset.x = Random.Range(-0.6f, 0.6f);
                        offset.z = Random.Range(-0.6f, -0.2f);
                    }


                    targetpoint = (target.position + offset);
                }
                else
                {
                    offset = Vector3.zero;
                    targetpoint = target.position;
                    if(40 < ps.shotdistance)
                    {
                        asc[1].Play();
                        bballeff.Play();
                    }
                }

                if (ps.shotdistance <= 12 && !setcount)
                {

                    Debug.Log("shottype: 1");
                    //tangent of 45 deg, times adj (length between player and archpoint)
                    riselength = Mathf.Tan(Mathf.PI / 4) * Vector3.Distance((startpoint + (targetpoint - startpoint) / 2), startpoint);
                    shotinairtime = 27f / 12f;
                    setcount = true;
                }
                else if (ps.shotdistance <= 30 && !setcount)
                {
                    Debug.Log("shottype: 2");
                    //tangent of 45 deg, times adj (length between player and archpoint)
                    riselength = Mathf.Tan(Mathf.PI / 4) * Vector3.Distance((startpoint + (targetpoint - startpoint) / 2), startpoint);
                    shotinairtime = 30 / ps.shotdistance;
                    setcount = true;
                }
                else if (ps.shotdistance <= 50 && !setcount)
                {
                    Debug.Log("shottype: 3");
                    riselength = Mathf.Tan(2 * Mathf.PI / 9) * Vector3.Distance((startpoint + (targetpoint - startpoint) / 2), startpoint);
                    shotinairtime = 30f/30f;
                    setcount = true;
                }
                else if (ps.shotdistance <= 70 && !setcount)
                {
                    Debug.Log("shottype: 3");
                    riselength = Mathf.Tan(7* Mathf.PI / 36) * Vector3.Distance((startpoint + (targetpoint - startpoint) / 2), startpoint);
                    shotinairtime = 30f / 30f;
                    setcount = true;
                }
                else if (ps.shotdistance > 70 && !setcount)
                {
                    Debug.Log("shottype: 3");
                    riselength = Mathf.Tan(Mathf.PI / 6) * Vector3.Distance((startpoint + (targetpoint - startpoint) / 2), startpoint);
                    shotinairtime = 30f / 30f;
                    setcount = true;
                }

                archpoint = startpoint + (targetpoint - startpoint) / 2 + Vector3.up * riselength;
                setarch = true;
            }

            if (jumpshottime <= count)
            {
                progressshoot = Mathf.Clamp01((count - jumpshottime));
                Vector3 m1 = Vector3.Lerp(startpoint, archpoint, progressshoot);
                Vector3 m2 = Vector3.Lerp(archpoint, targetpoint, progressshoot);
                transform.position = Vector3.Lerp(m1, m2, progressshoot);
                Debug.Log("YO MAMAMAMAMAM");
            }
            count += Mathf.Clamp01(shotinairtime * Time.deltaTime); //shotinairtime should = 1 at around 3pt line | shotinairtime changes how fast the count adds (how fast timer times)

        }
        else if (setarch) //after shot
        {
            Debug.Log(transform.position);
            Debug.Log("setarchturnoff");
            rotat = 0;
            //transform.position = targetpoint;
            bbrb.useGravity = true;
            bbrb.isKinematic = false;
            bbrb.detectCollisions = true;
            ess.shotscoretext.text = ess.shotscore.ToString();
            if (ess.gamemode == 2 && ess.shotscore != 0)
            {
                ess.threeptcontest(ess.shotscore); //move to bassetball shot
            }
            if(ps.shotresult)
            {

                if (40 < ps.shotdistance)
                {
                    firehoop.SetActive(true);
                    bballeff.Stop();
                    HoopEffEmission.rateOverTime = 6;
                }
                else
                {
                    firehoop.SetActive(false);
                    HoopEffEmission.rateOverTime = 12;
                }

                if (ps.smeter == 0)
                {
                    HoopEffrnr.material = ParticleMaterials[0];
                    HoopEff.Play();
                }
                else if (ps.smeter == 1)
                {
                    HoopEffrnr.material = ParticleMaterials[1];
                    HoopEff.Play();
                }
                else if (40 < ps.shotdistance)
                {
                    HoopEffEmission.rateOverTime = 0;
                    HoopEff.Play();
                }

                asc[0].Play(); //swishsound
            }

            Debug.Log("OFF");
            ps.shotmeterscript.shotmeterslider.value = 0;
            bbrelease.shotreleasenow = false;
            shoot = false;
            //progressjs = 0f;
            progressshoot = 0f;
            //bballholdref.position = bballholdreftemppos;
            //bballholdreftemppos = Vector3.zero;
            count = 0.0f;
            setcount = false;
            Debug.Log(transform.position);
            bbrb.AddForce(0,-3,0, ForceMode.Impulse);
            setarch = false;
        }
    }


}
