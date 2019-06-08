using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ArcCameraController : MonoBehaviour
{
    bool isLeftDown_ = false;
    float rotationSpeed_ = 1.0f;
    [Range(0.1f, 50.0f)]
    public float minSpeed_ = 1.0f;
    [Range(0.1f, 50.0f)]
    public float maxSpeed_ = 30.0f;
    [Range(0.1f, 50.0f)]
    public float acceleration_ = 8.0f;
    [Range(0.1f, 500.0f)]
    public float etaLow = 90;
    [Range(0.1f, 500.0f)]
    public float etaHigh = 230;

    //Vector3 globeRotationAxis_;
    float rotationTargetAngle_;
    public GameObject globe;

    // Start is called before the first frame update
    void Start()
    {
        destAngle = globe.transform.localEulerAngles.y;
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
        rotationTargetAngle_ = globe.transform.localEulerAngles.y;
        rotationTargetQtn_ = globe.transform.rotation;
    }


    float rotationStartMousePos_;
    Quaternion rotationStartQtn_;
    Quaternion rotationTargetQtn_;
    float rotationStartLastMousePos_;
    // Update is called once per frame
    void Update()
    {
        GlobeRotation();

        Zoom();

        VerticalMovement();

    }

    float zoomParameter = 0.5f;
    float zoomSensitivity = 1.0f;
    float minRadius = 0.8f;
    float maxRadiusPolar = 0.9f;
    float maxRadiusHorizon = 1.5f;
    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            zoomParameter = Mathf.Max(0.0f, zoomParameter - zoomSensitivity * Time.deltaTime);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {

            zoomParameter = Mathf.Min(1.0f, zoomParameter + zoomSensitivity * Time.deltaTime);
        }

        float maxRadius = maxRadiusHorizon;

        if (polarAngleDeg >= 90.0f)
        {
            maxRadius = Mathf.Lerp(maxRadiusHorizon, maxRadiusPolar, (polarAngleDeg - 90) / (maxPolarAngle - 90));
        }
        else
        {
            maxRadius = Mathf.Lerp(maxRadiusHorizon, maxRadiusPolar, (90 - polarAngleDeg) / (90 - minPolarAngle));
        }

        radius = Mathf.Lerp(minRadius, maxRadius, zoomParameter);

        //Debug.Log(scroll);
    }

    readonly Vector3 centralPoint = new Vector3(0, 0, 0);
    float radius = 1.2f;
    float polarAngleDeg = 90.0f;
    readonly float minPolarAngle = 30;
    readonly float maxPolarAngle = 150;
    float polarMovementSpeed = 25.0f;
    void VerticalMovement()
    {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        if (!screenRect.Contains(Input.mousePosition))
            return;

        //float lastPolarAngleDeg = polarAngleDeg;
        float mousePos = Input.mousePosition.y / (float) Screen.height;
        //Debug.Log(mousePos);
        if (mousePos > 0.9f)
        {
            float speedMod = (mousePos - 0.9f) * 10;
            polarAngleDeg = Mathf.Max(minPolarAngle, polarAngleDeg - polarMovementSpeed * Time.deltaTime * speedMod);
        }
        else if (mousePos < 0.1f)
        {
            float speedMod = (0.1f - mousePos) * 10;
            polarAngleDeg = Mathf.Min(maxPolarAngle, polarAngleDeg + polarMovementSpeed * Time.deltaTime * speedMod);
        }

        //Debug.Log(polarAngleDeg);

        GetComponent<Transform>().position = new Vector3(radius * Mathf.Sin(polarAngleDeg * Mathf.Deg2Rad), radius * Mathf.Cos(polarAngleDeg * Mathf.Deg2Rad), 0.0f);
        GetComponent<Transform>().LookAt(centralPoint);
    }

    float destAngle;
    public float smoothTime = 1.0f;
    float rotationVelocity = 0.0f;
    float rotationStartMousePos;
    float startingAngle;
    void GlobeRotation()
    {
        float currAngle = globe.transform.localEulerAngles.y;
        float newAngle = Mathf.SmoothDampAngle(currAngle, destAngle, ref rotationVelocity, smoothTime);
        globe.transform.Rotate(Vector3.up, newAngle - currAngle, Space.Self);

        if (Input.GetMouseButtonDown(0))
        {
            rotationStartMousePos = Input.mousePosition.x;
            startingAngle = globe.transform.localEulerAngles.y;
            //Debug.Log(rotationStartMousePos);
        }

        if (Input.GetButton("Fire1"))
        {
            float rotationCurrentMousePos = Input.mousePosition.x;
            float rotationAngle = ((rotationStartMousePos - rotationCurrentMousePos) / (float)Screen.width) * 180.0f;
            this.destAngle = Mathf.Clamp(rotationAngle, -180.0f, 180.0f) + startingAngle;
        }
    }

    void GlobeRotation2()
    {

        // Check for left button changes
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Left pressed.");
            isLeftDown_ = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            rotationStartMousePos_ = Input.mousePosition.x;
            rotationStartQtn_ = globe.transform.rotation;
            rotationStartLastMousePos_ = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Left released.");
            isLeftDown_ = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        float currentRotationAngle_ = globe.transform.localEulerAngles.y;
        if (isLeftDown_ && IsClickPosValid())
        {
            float rotationCurrentMousePos = Input.mousePosition.x;
            float rotationAngle = ((rotationStartMousePos_ - rotationCurrentMousePos) / (float)Screen.width) * 180.0f;
            rotationAngle = Mathf.Clamp(rotationAngle, -180.0f, 180.0f);
            rotationTargetQtn_ = rotationStartQtn_ * Quaternion.AngleAxis(rotationAngle, globe.transform.InverseTransformVector(globe.transform.up));

        }

        if (Quaternion.Dot(rotationTargetQtn_, globe.transform.rotation) < 0.99)
        {



            float deltaAngle = rotationSpeed_ * Time.deltaTime;
            float diffAngle = Mathf.Abs(Mathf.Acos(Quaternion.Dot(rotationTargetQtn_, globe.transform.rotation))) * Mathf.Rad2Deg;





            float para = deltaAngle / diffAngle;

            globe.transform.rotation = Quaternion.Slerp(globe.transform.rotation, rotationTargetQtn_, para);



            float eta = diffAngle / deltaAngle;
            if (eta > etaHigh)
            {
                rotationSpeed_ = Mathf.Min(rotationSpeed_ + acceleration_, maxSpeed_);
            }
            else if (eta < etaLow)
            {
                rotationSpeed_ = Mathf.Max(rotationSpeed_ - acceleration_, minSpeed_);
            }

        }


    }

    // Return true if the mouse if at the empty background
    // TODO: Implement it
    bool IsClickPosValid()
    {
        return true;
    }
}
