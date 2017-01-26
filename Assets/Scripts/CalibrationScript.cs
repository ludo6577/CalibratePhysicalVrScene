using UnityEngine;
using System.Collections;

public class CalibrationScript : MonoBehaviour
{
    // The two Vive controllers
    [Header("Vive Controllers")]
    public SteamVR_TrackedObject Controller1;
    public SteamVR_TrackedObject Controller2;
    //private SteamVR_Controller.Device Controller1;
    //private SteamVR_Controller.Device Controller2;

    //Number of points to calibrate
    [Range(1, 3)]
    [Header("Number of points to calibrate [1, 3]")]
    public int NumberOfPointToCalibrate = 3;

    // Virtual points: points present in your scene
    [Header("Virtual points (points in scene)")]
    public Transform V1; 
    public Transform V2; 
    public Transform V3;

    // Debug mode: activate the debug (physical points placed in the scene)
    [Space(20)]
    [Header("Debug Mode (click is trigger)")]
    public bool DebugActivated = false;

    // Physicals points: for debugging purpose
    [Header("Physical points (debug)")]
    public Transform P1;
    public Transform P2;
    public Transform P3;

    // Private
    private bool calibrated;
    private int calibratedPoints;
    private SteamVR_Controller.Device controller1 { get { return SteamVR_Controller.Input((int)Controller1.index); } }
    private SteamVR_Controller.Device controller2 { get { return SteamVR_Controller.Input((int)Controller2.index); } }

    // Use this for initialization
    void Start () {
        //StartCoroutine(CalibrateTest());

        // 1) Remove all rotation (we will add it manually later)
        transform.rotation = Quaternion.identity;

        calibrated = false;
	    calibratedPoints = 0;
	}

    void Update()
    {
        if (calibrated)
            return;

        var controller1Pressed = !DebugActivated && controller1.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
        var controller2Pressed = !DebugActivated && controller2.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
        var mouseClick =          DebugActivated && Input.GetMouseButtonDown(0);

        if (controller1Pressed || controller2Pressed || mouseClick)
        {
            Vector3 controllerPosition = Vector3.zero;
            if(!DebugActivated)
                controllerPosition = controller1Pressed ? controller1.transform.pos : controller2.transform.pos;

            if (calibratedPoints == 0)
            {
                if (DebugActivated)
                    controllerPosition = P1.position;

                CalibratePoint1(controllerPosition);
                calibratedPoints++;
            }
            else if (calibratedPoints == 1)
            {
                if (DebugActivated)
                    controllerPosition = P2.position;

                CalibratePoint2(controllerPosition);
                calibratedPoints++;
            }
            else if (calibratedPoints == 2)
            {
                if (DebugActivated)
                    controllerPosition = P3.position;

                CalibratePoint3(controllerPosition);
                calibratedPoints++;
            }

            if (calibratedPoints == NumberOfPointToCalibrate)
            {
                calibrated = true;
                Debug.Log(string.Format("Calibration success! {0} points founds", calibratedPoints));
            }
        }
    }

    IEnumerator CalibrateTest()
    {
        yield return new WaitForSeconds(1);
        
        // 1) translate the Chaperon in a vector that begin at the virtual point to the physical point
        yield return new WaitForSeconds(0.5f);
        CalibratePoint1(P1.position);        

        // 2) Rotate the Chaperon so v2 and p2 touch each other (p1 et v1 already touch so we use this position as the center of rotation)
        yield return new WaitForSeconds(0.5f);
        CalibratePoint2(P2.position);

        // 3) Rotate around the vector of the 2 first point that now collide so the third virtual point is close to the third physical point 
        yield return new WaitForSeconds(0.5f);
        CalibratePoint3(P3.position);
    }

    private void CalibratePoint1(Vector3 p1)
    {
        var v1 = V1.position;
        var v2 = V2.position;
        var v3 = V3.position;

        var translationVector = new Vector3(p1.x - v1.x, p1.y - v1.y, p1.z - v1.z);
        transform.Translate(translationVector, Space.World);
    }

    private void CalibratePoint2(Vector3 p2)
    {
        var v1 = V1.position;
        var v2 = V2.position;
        var v3 = V3.position;

        var rotationVectorVirtual = new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        var rotationVectorPhysic = new Vector3(v1.x - p2.x, v1.y - p2.y, v1.z - p2.z);

        var parentCenter = createParent(v1);
        parentCenter.transform.rotation = Quaternion.FromToRotation(rotationVectorVirtual, rotationVectorPhysic);
        removeParent();
    }

    private void CalibratePoint3(Vector3 p3)
    {
        var v1 = V1.position;
        var v2 = V2.position;
        var v3 = V3.position;

        var rotationVectorVirtual = new Vector3(v1.x - v3.x, v1.y - v3.y, v1.z - v3.z);
        var rotationVectorPhysic1 = new Vector3(v1.x - p3.x, v1.y - p3.y, v1.z - p3.z);
        //TODO: average the two vectors
        //var rotationVectorPhysic2 = new Vector3(p2.x - p3.x, p2.y - p3.y, p2.z - p3.z);

        var parentCenter = createParent(v1);
        parentCenter.transform.rotation = Quaternion.FromToRotation(rotationVectorVirtual, rotationVectorPhysic1);
        removeParent();
    }


    private GameObject parentGameObject;
    private GameObject createParent(Vector3 position)
    {
        parentGameObject = new GameObject("Center of rotation");
        parentGameObject.transform.position = position;
        transform.parent = parentGameObject.transform;
        return parentGameObject;
    }

    private void removeParent()
    {
        transform.parent = parentGameObject.transform.parent;
        GameObject.Destroy(parentGameObject);
    }
}
