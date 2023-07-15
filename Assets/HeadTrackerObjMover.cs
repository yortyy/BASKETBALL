using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hooplerpobjMover : MonoBehaviour
{
    private Transform hooplerpobj;
    private Transform hooplerpt;
    private Transform behindhoopobj;
    private Transform behindhoopt;
    private Transform belowhoopobj;
    private Transform belowhoopt;
    private Vector3 pasttargetvector;
    private Vector3 pastbvector;
    private Vector3 pastcvector;
    private float lerptime = 1;

    private void Start()
    {
        hooplerpobj = transform.GetChild(transform.childCount - 3);
        belowhoopobj = transform.GetChild(transform.childCount - 2);
        behindhoopobj = transform.GetChild(transform.childCount - 1);
    }

    public void trackObjs(Transform target, Transform targetb, Transform targetc)
    {
        pasttargetvector = hooplerpobj.position;
        pastbvector = behindhoopobj.position;
        pastcvector = belowhoopobj.position;
        hooplerpt = target;
        behindhoopt = targetb;
        belowhoopt = targetc;
        lerptime = 0.0f;
    }

    // Update is called once per frame
    private void Update()
    {
        if(lerptime != 1f)
        {
            hooplerpobj.position = Vector3.Lerp(pasttargetvector, hooplerpt.position, lerptime);
            behindhoopobj.position = Vector3.Lerp(pastbvector, behindhoopt.position, lerptime);
            belowhoopobj.position = Vector3.Lerp(pastcvector, belowhoopt.position, lerptime);

            lerptime = Mathf.Clamp01(0.5f * Time.deltaTime + lerptime);
        }
    }
}
