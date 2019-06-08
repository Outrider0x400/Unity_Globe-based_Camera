using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMovementTest : MonoBehaviour
{
    public GameObject obj;
    Camera localCamera;
    // Start is called before the first frame update
    void Start()
    {
        localCamera = GetComponent<Camera>();
        disToCamera = Vector3.Distance(transform.position, obj.transform.position);
        destination = obj.transform.position;
        //destination = Vector3.zero; //world center
        baseSpeed = Vector3.zero;
    }

    float disToCamera;
    Vector3 destination;
    Vector3 baseSpeed;
    public float smoothTime = 0.3f;
    // Update is called once per frame
    void Update()
    {
        // Roughly equals
        if (destination != obj.transform.position)
        {
            obj.transform.position = Vector3.SmoothDamp(obj.transform.position, destination,
                ref baseSpeed, smoothTime);
        }

        if (Input.GetButton("Fire1"))
        {
            //float v = Mathf.Clamp01(Input.mousePosition.y / Screen.height);
            //float u = Mathf.Clamp01(Input.mousePosition.x / Screen.width);
            Ray ray = localCamera.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
            destination = ray.GetPoint(disToCamera);
        }
    }
}
