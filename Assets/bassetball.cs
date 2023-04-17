using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class bassetball : MonoBehaviour
{
    public Transform target;
    private GameObject player;
    private playermovement ps;
    public bool shoot;
    public bool playerholding;
    private Rigidbody bbrb;
    float count = 0.0f;
    void Awake()
    {
        bbrb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player") && !shoot)
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
    Vector3 startpoint;
    Vector3 archpoint;
    Vector3 targetpoint;
    Vector3 offset;
    bool setarch;
    private void Update()
    {
        if (playerholding && !shoot)
        {
            transform.position = player.gameObject.transform.position;
        }
    }
    void FixedUpdate()
    {

        if (count < 1.0f && shoot)
        {

            if (!setarch)
            {
                archpoint = transform.position + (target.position - transform.position) / 2 + Vector3.up * 8.0f;
                startpoint = transform.position;
                if (!ps.shotresult)
                {
                    if(1 == Random.Range(1, 2))
                    {
                        offset.x = Random.Range(0.5f, 1f);
                        offset.z = Random.Range(-1f, -0.5f);
                    }
                    else
                    {
                        offset.x = Random.Range(-1f, -0.5f);
                        offset.z = Random.Range(-1f, -0.5f);
                    }
                    targetpoint = (target.position + offset);
                }
                else
                {
                    targetpoint = target.position;
                }
                setarch = true;
            }
            count += 1f * Time.deltaTime;

            //Vector3 archpoint = new Vector3((target.position.x - transform.position.x)/2, (target.position.y - transform.position.y)/2 + 3, (target.position.z - transform.position.z)/2);
            Vector3 m1 = Vector3.Lerp(startpoint, archpoint, count);
            Vector3 m2 = Vector3.Lerp(archpoint, targetpoint, count);
            transform.position = Vector3.Lerp(m1, m2, count);
        }
        else if (setarch) //after shot
        {
            Debug.Log("setarchturnoff");
            bbrb.useGravity = true;
            bbrb.isKinematic = false;
            bbrb.detectCollisions = true;
            ps.shotscoretext.text = ps.shotscore.ToString();
            if(ps.threeptcontest)
            {
                ps.ess.threeptcontest(ps.shotscore); //move to bassetball shot
            }
            shoot = false;
            count = 0.0f;
            setarch = false;
        }
    }

}
