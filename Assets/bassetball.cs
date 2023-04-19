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
    int rotat;
    private void Update()
    {
        if (playerholding && !shoot)
        {
            transform.position = player.gameObject.transform.position;
        }
        if(count < 1.0f && shoot)
        {
            if(rotat == 0)
            {
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z + 0.5f));
                rotat = 5;
            }
            else
            {
                rotat += 5;
                Quaternion rotation = Quaternion.Euler(rotat, transform.rotation.y, transform.rotation.z);
                transform.rotation = rotation;
            }
        }
        else if(rotat != 0)
        {
            rotat = 0;
        }
    }
    void FixedUpdate()
    {

        if (count < 1.0f && shoot)
        {

            if (!setarch)
            {
                archpoint = transform.position + (target.position - transform.position) / 2 + Vector3.up * 10.0f;
                startpoint = transform.position;
                if (!ps.shotresult)
                {
                    offset.x = Random.Range(-0.8f, 0.8f);
                    offset.z = Random.Range(-0.8f, 0.1f);
                    if (Mathf.Abs(offset.x) < 0.4f && Mathf.Abs(offset.z) < 0.4f)
                    {
                        offset.x = Random.Range(-0.5f, 0.5f);
                        offset.z = Random.Range(-0.5f, 0.5f);
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
