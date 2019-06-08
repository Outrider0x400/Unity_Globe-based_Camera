using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CloseUpCameraController : MonoBehaviour
{

    [Range(0.0f, 45.0f)]
    [Tooltip("The max angle the camera can deviate from the central view at thw lowest altitude")]
    public float maxAngleDegAtLowAlt = 45.0f;
    [Range(0.0f, 45.0f)]
    [Tooltip("The max angle the camera can deviate from the central view at thw highest altitude")]
    public float maxAngleDegAtHighAlt = 15.0f;
    
    public float minAltitude = 0.53f;
    public float maxAltitude = 1.20f;
    
    void Start()
    {
        
    }

    struct TangentSpace
    {
        public TangentSpace(Vector3 directionToCenter)
        {
            this.earthCoreDir = directionToCenter.normalized;
            this.latitudeTangent = -Vector3.Cross(earthCoreDir, Vector3.up).normalized;
            this.meridianBitangent = Vector3.Cross(earthCoreDir, latitudeTangent).normalized;
            this.Tan2Earth = new Matrix4x4(latitudeTangent, meridianBitangent, earthCoreDir, new Vector4(0, 0, 0, 1));
            this.Earth2Tan = Tan2Earth.transpose;
        }
        public Vector3 earthCoreDir, latitudeTangent, meridianBitangent;
        public Matrix4x4 Tan2Earth, Earth2Tan;
    }

    public float orbitalMovementSensitivity = 1.0f;
    public float orbitalSmoothness = 1.0f;
    float verticalSpeed = 0.0f;
    float horizontalSpeed = 0.0f;
    float verticalAccelaration = 0.0f;
    float horizontalAccelaration = 0.0f;
    [Range(60.0f, 85.0f)]
    public float maxLatitude = 80.0f;
    [Range(-85.0f, -60.0f)]
    public float minLatitude = -80.0f;
    void OrbitalMovement()
    {

        Vector2 delta = Vector2.zero;

        // The view direction change also detects mouse panning
        if (!Input.GetButton("ChangeViewDirection"))
        {
            float horizontalOffset = CalculateEdgePanningIntensity(Input.mousePosition.x, Screen.width, horizontalEdgeZoneWidthEdgePanning);
            float verticalOffset = CalculateEdgePanningIntensity(Input.mousePosition.y, Screen.height, verticalEdgeZoneHeightEdgePanning);
            delta = Vector2.ClampMagnitude(new Vector2(horizontalOffset, verticalOffset), 1.0f) * orbitalMovementSensitivity * Time.deltaTime;
        }

        

        TangentSpace startingPositionTangentSpace = new TangentSpace(-(transform.localPosition).normalized);
        // These two values should stay constant after the transformation
        Vector3 tanGlazingAngle = startingPositionTangentSpace.Earth2Tan.MultiplyVector(transform.parent.InverseTransformVector(transform.forward)).normalized;
        Vector3 tanUp = startingPositionTangentSpace.Earth2Tan.MultiplyVector(transform.parent.InverseTransformVector(transform.up)).normalized;

        // Smoothly accelarates/decelerates to the target speed
        verticalSpeed = Mathf.SmoothDamp(verticalSpeed, (delta.y), ref verticalAccelaration, orbitalSmoothness);
        horizontalSpeed = Mathf.SmoothDamp(horizontalSpeed, (delta.x), ref horizontalAccelaration, orbitalSmoothness);

        // Rotate around the earth core, along the "right" and "up" of the screen/view space
        transform.RotateAround(transform.parent.position, transform.right, verticalSpeed);
        transform.RotateAround(transform.parent.position, -transform.up, horizontalSpeed);

        float polarAngle = Vector3.Angle(transform.localPosition, Vector3.up);
        float minPolarAngle = (90 - maxLatitude);
        float maxPolarAngle = (90 - minLatitude);
        // If the new position is too far in the north/south
        if (polarAngle <= minPolarAngle || polarAngle >= maxPolarAngle)
        {
            float theta = (polarAngle <= minPolarAngle ? minPolarAngle : maxPolarAngle) * Mathf.Deg2Rad;
            float orbitAltitude = transform.localPosition.magnitude;

            // The radius of the circle, intersected by the max/min latitude line
            float intersectionPlaneRadius = Mathf.Sin(theta) * orbitAltitude;

            // Project the camera position to the intersection plane
            Vector2 intersection = new Vector2(transform.localPosition.x, transform.localPosition.z);

            // Extend the intersection to find a new point along the circle
            Vector2 adjustedDestinationProjection = intersection.normalized * intersectionPlaneRadius;

            transform.localPosition = new Vector3(adjustedDestinationProjection.x, Mathf.Cos(theta) * orbitAltitude, adjustedDestinationProjection.y);
        }

        // Re-align the viewing directions
        TangentSpace destinationTangentSpace = new TangentSpace(-(transform.localPosition).normalized);
        transform.LookAt(transform.position + transform.parent.TransformVector(destinationTangentSpace.Tan2Earth.MultiplyVector(tanGlazingAngle)),
            transform.parent.TransformVector(destinationTangentSpace.Tan2Earth.MultiplyVector(tanUp)));

        
    }


    Vector3 tanViewingDir = Vector3.forward;
    Vector3 tanVelocity = Vector3.zero;
    Vector3 newTanGlainzgAngle;
    public float tanSmooth = 1.0f;
    public float tanSensitivity = 10.0f;
    void TangentMovement()
    {
        TangentSpace tangentSpace = new TangentSpace(-(transform.localPosition).normalized);
        if (Input.GetButton("ChangeViewDirection"))
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;

            // The altitude stays constant for this method.
            float currentAltitude = transform.localPosition.magnitude;


            float offsetX = CalculateEdgePanningIntensity(Input.mousePosition.x, Screen.width, horizontalEdgeZoneWidthTangentMovement);
            float offsetY = CalculateEdgePanningIntensity(Input.mousePosition.y, Screen.height, verticalEdgeZoneHeightTangentMovement);
            Vector3 offset = (new Vector3(offsetX, offsetY, 0)) * tanSensitivity * Time.deltaTime;
            newTanGlainzgAngle = (tanViewingDir + offset * Time.deltaTime).normalized;


            float angle = Vector3.Angle(Vector3.forward, newTanGlainzgAngle);
            float maxAngle = Mathf.Lerp(maxAngleDegAtLowAlt, maxAngleDegAtHighAlt, (currentAltitude - minAltitude) / (maxAltitude - minAltitude));
            if (angle > maxAngle)
            {

                Vector3 rotationalAxis = Vector3.Cross(newTanGlainzgAngle, Vector3.forward).normalized;
                newTanGlainzgAngle = Quaternion.AngleAxis(angle - maxAngle, rotationalAxis) * newTanGlainzgAngle;
            }

        }
        else
        {
            newTanGlainzgAngle = Vector3.zero;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        tanViewingDir = Vector3.SmoothDamp(tanViewingDir, newTanGlainzgAngle, ref tanVelocity, tanSmooth).normalized;

        // Aligning the up direction via the Gram–Schmidt process
        Vector3 tanUp = (Vector3.Dot(Vector3.up, tanViewingDir) * tanViewingDir + Vector3.up).normalized;
        transform.LookAt(transform.position + transform.parent.TransformVector(tangentSpace.Tan2Earth.MultiplyVector(tanViewingDir)),
            transform.parent.TransformVector(tangentSpace.Tan2Earth.MultiplyVector(tanUp)));

    }


    public float altitudeSensitivity = 1.0f;
    Vector3 altVelo = Vector3.zero;
    public float altitudeSmoothTime = 1.0f;
    void AltitudeMovement()
    {

        float delta;
        if (Input.GetButton("MouseAltitudeChange"))
        {
            float offsetY = CalculateEdgePanningIntensity(Input.mousePosition.y, Screen.height, verticalEdgeZoneHeightEdgePanning);
            delta = -offsetY * altitudeSensitivity * Time.deltaTime;
        }
        else
            delta = Input.GetAxis("ChangeAltitude") * altitudeSensitivity * Time.deltaTime;


        float currentAltitude = transform.localPosition.magnitude;

        float destinationAltitude = currentAltitude + delta;
        destinationAltitude = Mathf.Clamp(destinationAltitude, minAltitude, maxAltitude);

        Vector3 destination = transform.localPosition.normalized * destinationAltitude;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, destination, ref altVelo, altitudeSmoothTime);



    }

    float CalculateEdgePanningIntensity(float mousePosition, int totalLength, int edgeZonePercentage)
    {
        // If mouse out of window, does not move anything
        if (mousePosition < 0 || mousePosition > totalLength)
            return 0;

        float W = totalLength;
        float L = edgeZonePercentage / 100.0f * W;
        float R = (1.0f - edgeZonePercentage / 100.0f) * W;
        mousePosition = Mathf.Clamp(mousePosition, 0, totalLength);
        if (mousePosition <= L)
            return mousePosition / L - 1;
        else if (mousePosition < R)
            return 0;
        else
            return (mousePosition - R) / (W - R);
    }

    [Range(5, 20)]
    [Tooltip("The size of the trigger zone for edge panning")]
    public int horizontalEdgeZoneWidthEdgePanning = 10;
    [Range(5, 20)]
    [Tooltip("The size of the trigger zone for edge panning")]
    public int verticalEdgeZoneHeightEdgePanning = 10;
    [Range(5, 40)]
    [Tooltip("The size of the trigger zone for changing view direction")]
    public int horizontalEdgeZoneWidthTangentMovement = 10;
    [Range(5, 40)]
    [Tooltip("The size of the trigger zone for changing view direction")]
    public int verticalEdgeZoneHeightTangentMovement = 10;
    void Update()
    {
        OrbitalMovement();

        AltitudeMovement();

        TangentMovement();
    }



}
