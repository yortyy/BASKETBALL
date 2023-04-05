using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class bassetball : MonoBehaviour
{
    public Transform target;
    private GameObject player;
    private bool shoot;
    private bool playerholding;
    private Rigidbody bbrb;
    float count = 0.0f;
    void Awake()
    {
        bbrb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player"))
        {
            player = other.gameObject;
            bbrb.isKinematic = true;
            bbrb.detectCollisions = false;
            bbrb.useGravity = false;
            playerholding = true;
            Debug.Log("KONNECT");
        }
    }
    public void shooter(InputAction.CallbackContext value)
    {
        if(playerholding)
        {
            bbrb.detectCollisions = true;
            playerholding = false;
            shoot = true;
        }

    }

    Vector3 startpoint;
    Vector3 archpoint;
    bool setarch;
    void FixedUpdate()
    {
        if (count < 1.0f && shoot)
        {

            if (!setarch)
            {
                archpoint = transform.position + (target.position - transform.position) / 2 + Vector3.up * 8.0f;
                startpoint = transform.position;
                setarch = true;
            }
            count += 1f * Time.deltaTime;

            //Vector3 archpoint = new Vector3((target.position.x - transform.position.x)/2, (target.position.y - transform.position.y)/2 + 3, (target.position.z - transform.position.z)/2);
            Vector3 m1 = Vector3.Lerp(startpoint, archpoint, count);
            Vector3 m2 = Vector3.Lerp(archpoint, target.position, count);
            transform.position = Vector3.Lerp(m1, m2, count);
        }
        else if (setarch)
        {
            Debug.Log("setarchturnoff");
            bbrb.useGravity = true;
            bbrb.isKinematic = false;
            bbrb.detectCollisions = true;
            shoot = false;
            count = 0.0f;
            setarch = false;
        }
        else if (playerholding)
        {
            transform.position = player.gameObject.transform.position;
        }
    }
}
