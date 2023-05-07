using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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


    [SerializeField] private Transform bballholdref;
    [SerializeField] private float jumpshottime = 0.5f;
    private float progressjs;
    private float progressshoot;
    private Vector3 bballholdreftemppos;
    private GameObject bballRend;


    void Awake()
    {
        bballRend = transform.GetChild(0).gameObject;
        bbrb = GetComponent<Rigidbody>();
        HoopEffEmission = HoopEff.emission;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player") && !shoot && !playerswitching)
        {
            player = other.gameObject;
            ps = player.GetComponent<playermovement>();
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
        if (progress == 1)
        {
            playerholding = true;
            playerswitching = false;
            progress = 0;
        }
        if (playerholding && !shoot)
        {
            transform.position = bballholdref.position;
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z + 0.5f));

        }


        if (count > jumpshottime && count < (1.0f + jumpshottime) && shoot)
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
        }
        else if (rotat != 0)
        {
            rotat = 0;
        }

        if (count > jumpshottime && count < (1.0f + jumpshottime) && shoot) //jumpshottimeneeded to movebball ref up in animation
        {
            if (!setarch)
            {

                startpoint = bballholdref.position;
                if (!ps.shotresult)
                {
                    offset.x = Random.Range(-0.8f, 0.8f);
                    offset.z = Random.Range(-0.8f, -0.5f);
                    if (Mathf.Abs(offset.x) < 0.5f && Mathf.Abs(offset.z) < 0.5f)
                    {
                        offset.x = Random.Range(-0.8f, 0.8f);
                        offset.z = Random.Range(-0.8f, -0.5f);
                    }
                    targetpoint = (target.position + offset);
                }
                else
                {
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

            //if (count < jumpshottime)
            //{
            //    if (bballholdreftemppos == Vector3.zero)
            //    {
            //        bballholdreftemppos = bballholdref.position;
            //    }
            //    progressjs = Mathf.Clamp01(count / jumpshottime);
            //   bballholdref.position = Vector3.Lerp(bballholdreftemppos, (new Vector3(bballholdreftemppos.x, bballholdreftemppos.y + 0.5f, bballholdreftemppos.z)), progressjs);
            //    Debug.Log(count);
            //}
            //else 
            if (jumpshottime < count)
            {
                progressshoot = Mathf.Clamp01((count - jumpshottime));
                Vector3 m1 = Vector3.Lerp(startpoint, archpoint, progressshoot);
                Vector3 m2 = Vector3.Lerp(archpoint, targetpoint, progressshoot);
                transform.position = Vector3.Lerp(m1, m2, progressshoot);
            }
            //countjs += shotinairtime/jumpshottime * Time.deltaTime; I was thinking it could be seperate timer so more smooth lerping by frame
            count += shotinairtime * Time.deltaTime; //shotinairtime should = 1 at around 3pt line | shotinairtime changes how fast the count adds (how fast timer times)

            //Vector3 archpoint = new Vector3((target.position.x - transform.position.x)/2, (target.position.y - transform.position.y)/2 + 3, (target.position.z - transform.position.z)/2);

        }
        else if (setarch) //after shot
        {
            Debug.Log("setarchturnoff");
            bbrb.useGravity = true;
            bbrb.isKinematic = false;
            bbrb.detectCollisions = true;
            ps.shotscoretext.text = ps.shotscore.ToString();
            if (ess.gamemode == 2 && ps.shotscore != 0)
            {
                ess.threeptcontest(ps.shotscore); //move to bassetball shot
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
            ps.shotmeterscript.shotmeterslider.value = 0;
            shoot = false;
            progressjs = 0f;
            progressshoot = 0f;
            //bballholdref.position = bballholdreftemppos;
            //bballholdreftemppos = Vector3.zero;
            count = 0.0f;
            setcount = false;
            bbrb.AddForce(0,-3,0, ForceMode.Impulse);
            setarch = false;
        }
    }

}
