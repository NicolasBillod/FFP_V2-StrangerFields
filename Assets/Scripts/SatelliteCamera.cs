using UnityEngine;
using UnityEngine;
using System.Collections;
 
public class SatelliteCamera : MonoBehaviour
{
	public static SatelliteCamera Instance;

    public float minDist;
    public float maxDist;

    public float minElevation;
    public float maxElevation;

    public float elevation;
    float targetElevation;

    public float azimutSpeed;
    public float distanceSpeed;
    public float elevationSpeed;

    public Transform target;

    public float startAzimut = 45f;

    float azimut;
    float targetAzimut;

    float distance;
    float targetDistance;

    public float kLerpPos;

    Vector3 targetPos;

	public static float Ycamera;

	public void Awake () {
		Instance = this;
	}


	public static float GetYcamera() {
		return Ycamera;
	}

    // Use this for initialization
    void Start()
    {
        distance = (minDist + maxDist) / 2f;
        targetDistance = distance;

        azimut = startAzimut;
        targetAzimut = azimut;

        elevation *= Mathf.PI / 180f;
        targetElevation = elevation;

        minElevation *= Mathf.PI / 180f;
        maxElevation *= Mathf.PI / 180f;
    }

    // Update is called once per frame
    void Update()
    {
		Ycamera = transform.eulerAngles.y;
        //if (Input.touchSupported) {

#if UNITY_ANDROID

        if (Input.touchCount > 0)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                // Move object across XY plane
                targetAzimut -= azimutSpeed * Time.deltaTime * touchDeltaPosition.x * 0.2f;
                targetElevation -= elevationSpeed * Time.deltaTime * touchDeltaPosition.y * 0.2f;
            }
        }

        /*} else {
            if (Input.GetMouseButton (0) && (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0)) {
                targetAzimut += azimutSpeed * Time.deltaTime * Input.GetAxis ("Mouse X") * 0.5f;
                targetElevation += elevationSpeed * Time.deltaTime * Input.GetAxis ("Mouse Y") * 0.5f;
            }
        }*/

#endif

        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            targetDistance = Mathf.Clamp(targetDistance - azimutSpeed * deltaMagnitudeDiff, minDist, maxDist);
        }


#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR || UNITY_WEBPLAYER

        if (Input.GetMouseButton(0) && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
        {
            targetAzimut += azimutSpeed * Time.deltaTime * Input.GetAxis("Mouse X") * 0.5f;
            targetElevation += elevationSpeed * Time.deltaTime * Input.GetAxis("Mouse Y") * 0.5f;
        }

#endif


        if (Input.GetKey(KeyCode.Q))
            targetAzimut -= azimutSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.D))
            targetAzimut += azimutSpeed * Time.deltaTime;
        azimut = Mathf.Lerp(azimut, targetAzimut, Time.deltaTime * kLerpPos);

        if (Input.GetKey(KeyCode.Z))
            targetElevation += elevationSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            targetElevation -= elevationSpeed * Time.deltaTime;
        targetElevation = Mathf.Clamp(targetElevation, minElevation, maxElevation);
        elevation = Mathf.Lerp(elevation, targetElevation, Time.deltaTime * kLerpPos);

        targetDistance = Mathf.Clamp(targetDistance - distanceSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime, minDist, maxDist);
        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * kLerpPos);

        Vector3 dirH = new Vector3(Mathf.Cos(azimut), 0, Mathf.Sin(azimut));

        Vector3 newPos = target.position + dirH * distance * Mathf.Cos(elevation) + Vector3.up * distance * Mathf.Sin(elevation);
        transform.position = newPos;//Vector3.Lerp(transform.position,newPos,Time.deltaTime*kLerpPos);
        transform.LookAt(target);
    }

}