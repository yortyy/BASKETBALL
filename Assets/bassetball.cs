using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class bassetball : MonoBehaviour
{
    public Transform target;
    private GameObject player;
    private bool shoot;
    public bool playerholding;
    private Rigidbody bbrb;
    public int shotpercent;
    private int shotresultnum;
    public bool shotresult;
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
            bbrb.isKinematic = true;
            bbrb.detectCollisions = false;
            bbrb.useGravity = false;
            playerholding = true;
            Debug.Log("KONNECT: " + other.gameObject);
        }
    }
    public void shooter(int shootingskill, int smeter)
    {
        bbrb.detectCollisions = false;
        playerholding = false;
        shotpercent = 100 - (shootingskill * 2) - (smeter * 5); //out of 100, green - 95, slight early - 80, slight late - 70, early - 50, late - 40, very early/late - 10, nah - 0
        shotresultnum = (shotpercent - Random.Range(0, 100));
        if(shotresultnum > 0)
        {
            //shots good
            shotresult = true;
        }
        else if(shotresultnum < 0)
        {
            //shots bad
            shotresult = false;
        }
        else if (shotresultnum == 0)
        {
            //special shot animation
            shotresult = true;
        }
        Debug.Log("Shotresult: " + shotresult + " | Shotresultnum: " + shotresultnum + " | Shotpercentage: " + shotpercent);
        Debug.Log(shotresultnum);
        shoot = true;
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
                if (!shotresult)
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
            shoot = false;
            count = 0.0f;
            setarch = false;
        }
    }

}
