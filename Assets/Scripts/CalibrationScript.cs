using UnityEngine;
using System.Collections;

public class CalibrationScript : MonoBehaviour
{
    public SteamVR_Controller.Device controller1;
    public SteamVR_Controller.Device controller2;

    public Transform V1; //Virtual
    public Transform V2; //Virtual
    public Transform V3; //Virtual

    public Transform P1; //Physic
    public Transform P2; //Physic
    public Transform P3; //Physic

	// Use this for initialization
	void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObject.index);

        StartCoroutine(Calibrate());
	}

    void Update()
    {
        if (controller1.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) ||
            controller2.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Transform controllerPosition = null;
            if (controller1.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                controllerPosition = controller1.transform.position;
            else if (controller2.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                controllerPosition = controller2.transform.position;

            if (P1 == null)
                P1 = controllerPosition;
            if (P2 == null)
                P2 = controllerPosition;
            if (P3 == null)
                P3 = controllerPosition;
        }
    }

    IEnumerator Calibrate()
    {
        // 1) Remove all rotation (we will add it manually later)
        yield return new WaitForSeconds(1);
        transform.rotation = Quaternion.identity;

        // 2) translate the Chaperon in a vector that begin at the virtual point to the physical point
        yield return new WaitForSeconds(0.5f);
        CalibratePoint1();        

        // 3) Rotate the Chaperon so v2 and p2 touch each other (p1 et v1 already touch so we use this position as the center of rotation)
        yield return new WaitForSeconds(0.5f);
        CalibratePoint2();

        // 4) Rotate around the vector of the 2 first point that now collide so the third virtual point is close to the third physical point 
        yield return new WaitForSeconds(0.5f);
        CalibratePoint3();
    }

    private void CalibratePoint1()
    {
        var p1 = P1.position;
        var p2 = P2.position;
        var p3 = P3.position;

        var v1 = V1.position;
        var v2 = V2.position;
        var v3 = V3.position;

        var translationVector = new Vector3(p1.x - v1.x, p1.y - v1.y, p1.z - v1.z);
        transform.Translate(translationVector, Space.World);
    }

    private void CalibratePoint2()
    {
        var p1 = P1.position;
        var p2 = P2.position;
        var p3 = P3.position;

        var v1 = V1.position;
        var v2 = V2.position;
        var v3 = V3.position;

        var rotationVectorVirtual = new Vector3(p1.x - v2.x, p1.y - v2.y, p1.z - v2.z);
        var rotationVectorPhysic = new Vector3(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);

        var parentCenter = createParent(p1);
        parentCenter.transform.rotation = Quaternion.FromToRotation(rotationVectorVirtual, rotationVectorPhysic);
        removeParent();
    }

    private void CalibratePoint3()
    {
        var p1 = P1.position;
        var p2 = P2.position;
        var p3 = P3.position;

        var v1 = V1.position;
        var v2 = V2.position;
        var v3 = V3.position;

        var rotationVectorVirtual = new Vector3(p1.x - v3.x, p1.y - v3.y, p1.z - v3.z);
        var rotationVectorPhysic1 = new Vector3(p1.x - p3.x, p1.y - p3.y, p1.z - p3.z);
        //var rotationVectorPhysic2 = new Vector3(p2.x - p3.x, p2.y - p3.y, p2.z - p3.z);

        var parentCenter = createParent(p1);
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


    //IEnumerator TestSomething(Vector3 center, Vector3 cross, Vector3 angle)
    //{
    //    var parentCenter = createParent(center);
    //    while (true)
    //    {
    //        parentCenter.transform.RotateAround(cross, angle.magnitude);

    //        //parentCenter.transform.rotation = Quaternion.FromToRotation(rotationVector1, rotationVector2);
    //        //transform.RotateAround(parentCenter.transform.position, cross, magnitude);
    //        yield return new WaitForSeconds(0.5f);
    //    }
    //}
}
