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
    public GameObject Hoop; //hoop
    private Transform target; //hooptransform

    private GameObject player;
    private playermovement ps;
    [SerializeField] private EventScriptSystem ess;
    public bool shoot;
    public bool playerholding;
    private Rigidbody bbrb;

    private GameObject HoopEffObj;
    private ParticleSystem HoopEff;
    private ParticleSystemRenderer HoopEffrnr;
    private GameObject firehoop;
    private EmissionModule HoopEffEmission;

    [SerializeField] private ParticleSystem bballeff;

    [SerializeField] Material[] ParticleMaterials;
    [SerializeField] private AudioSource[] asc;

    private Vector3 startpoint;
    private Vector3 archpoint;
    private Vector3 targetpoint;
    private float riselength;
    private float shotinairtime;
    private Vector3 offset;
    private bool setarch;
    private bool setcount;
    private bool shotdone;
    private int rotat;

    public bool playerchange;
    private Vector3 pastballpos;
    private float progress;
    private bool playerswitching;

    public bool dunkedtheball;

    private float progressshoot;
    private GameObject bballRend;

    private float bballrigtime = 0.5f;
    private float bballrigtimecount;
    private bool bballrigset;

    [HideInInspector] public Transform bballholdref;
    [HideInInspector] public bballrelease bbrelease;

    private bool readytoswitchsides;

    void Awake()
    {
        bballRend = transform.GetChild(0).gameObject;
        bbrb = GetComponent<Rigidbody>();

        HoopEffObj = Hoop.transform.GetChild(3).gameObject;
        firehoop = HoopEffObj.transform.GetChild(0).gameObject;
        HoopEff = HoopEffObj.GetComponent<ParticleSystem>();
        HoopEffrnr = HoopEffObj.GetComponent<ParticleSystemRenderer>();
        target = Hoop.transform;
        HoopEffEmission = HoopEff.emission;


        //player script gets bballholdref and bbrelease
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player") && !shoot && !playerswitching && !dunkedtheball)
        {
            player = other.gameObject;
            ps = player.GetComponent<playermovement>();
            //need to stop playback or go back to idle animation
            bbrb.isKinematic = true;
            bbrb.detectCollisions = false;
            bbrb.useGravity = false;
            playerholding = true;
            Debug.Log("KONNECT: " + other.gameObject);

            if(readytoswitchsides)
            {
                ess.SwitchSides();

                HoopEffObj = Hoop.transform.GetChild(3).gameObject;
                firehoop = HoopEffObj.transform.GetChild(0).gameObject;
                HoopEff = HoopEffObj.GetComponent<ParticleSystem>();
                HoopEffrnr = HoopEffObj.GetComponent<ParticleSystemRenderer>();
                target = Hoop.transform;
                HoopEffEmission = HoopEff.emission;
                readytoswitchsides = false;
            }
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
            //Debug.Log("transformin 3");
        }
        if (progress >= 1)
        {
            playerholding = true;
            playerswitching = false;
            progress = 0;
        }

        if ((playerholding && !shoot) || (shoot && !bbrelease.shotreleasenow)) //rigon
        {
            //Debug.Log("transformin 2");
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
                if(dunkedtheball)
                {
                    bballrigtimecount = Mathf.Clamp01(bballrigtimecount - 1f / 0.2f * Time.deltaTime);
                }
                else
                {
                    bballrigtimecount = Mathf.Clamp01(bballrigtimecount - 1f / bballrigtime * Time.deltaTime);
                }
                ps.characterrigs[1].weight = bballrigtimecount;
            }
        }



        if (progressshoot <= 1 && !shotdone && shoot && bbrelease.shotreleasenow && !dunkedtheball) //jumpshottimeneeded to movebball ref up in animation
        {
            //Debug.Log("transformin 1 | pos: " + transform.position + " pshoot: " + progressshoot);
            if (progressshoot == 1 && transform.position == targetpoint)
            {
                Debug.Log("pos: " + transform.position + " | target: " + targetpoint);
                shotdone = true;
            }

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
                    offset.x = Random.Range(-0.6f, -0.3f);
                    offset.z = Random.Range(-0.6f, -0.3f);

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


            Vector3 m1 = Vector3.Lerp(startpoint, archpoint, progressshoot);
            Vector3 m2 = Vector3.Lerp(archpoint, targetpoint, progressshoot);
            transform.position = Vector3.Lerp(m1, m2, progressshoot);
            //Debug.Log("transformin 2 | pos: " + transform.position + " pshoot: " + progressshoot);

            progressshoot = Mathf.Clamp01(shotinairtime * Time.deltaTime + progressshoot); //shotinairtime should = 1 at around 3pt line | shotinairtime changes how fast the count adds (how fast timer times)

        }
        else if (shotdone) //after shot
        {
            Debug.Log("setarchturnoff");
            //Debug.Log("setarchoff | pos: " + transform.position + " pshoot: " + progressshoot);

            rotat = 0;
            //transform.position = targetpoint;
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
                }
                else if (ps.smeter == 1)
                {
                    HoopEffrnr.material = ParticleMaterials[1];
                }
                else if (40 < ps.shotdistance)
                {
                    HoopEffEmission.rateOverTime = 0;
                }

                HoopEff.Play();
            }

            ShotHits();

            ps.shotmeterscript.shotmeterslider.value = 0;
            setcount = false;
            setarch = false;
            progressshoot = 0f;
            shotdone = false;
        }
    }

    public void ShotHits()
    {
        if (ps.shotresult)
        {
            asc[0].Play(); //swishsound
            //practice = 0, 1on1 = 1, threept = 2, threewii = 3, 3on3 = 4
            if (ess.gamemode == 1 || ess.gamemode == 4)
            {
                readytoswitchsides = true;
            }
            
        }

        shoot = false;
        bbrelease.shotreleasenow = false;

        bbrb.useGravity = true;
        bbrb.isKinematic = false;
        bbrb.detectCollisions = true;

        bbrb.AddForce(0, -3, 0, ForceMode.Impulse);

        ess.shotscoretext.text = ess.shotscore.ToString();
    }

}
