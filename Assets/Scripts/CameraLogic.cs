using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    [Tooltip("Whether camera should go up or down")]
    public bool isCameraUp = true;
    [Tooltip("If anything is between camera and target, camera would be closer")]
    public bool isCameraAvoidOcclusion=false;

    [Tooltip("The object that the camera follows")]
    public GameObject target;

    [Space]

    [Tooltip("View's angle")]
    public float viewAngleBeforeStart = -10;
    [Tooltip("Distence between camera and ball")]
    public float viewDistenceBeforeStart = 3;

    [Tooltip("View's angle")]
    public float viewAngleInPlay = 80;
    [Tooltip("Distence between camera and ball")]
    public float viewDistenceInPlay=5;

    [Tooltip("The time camera use to change place when start")]
    public float timeCameraMovingReadyPlay = 3;

    [Space]

    public MainLogic ml;

    private float timerOfCamera;//count after ready play
    private float dAngle;
    private float dDistence;

    private void Awake() {
        timerOfCamera = timeCameraMovingReadyPlay;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (!target) {
            Debug.Log("Camera no target.");
            return;
        }
        moveCamera();
    }
    
    //fix funtions
    private void moveCamera() {
        dAngle = MainLogic.currencyMovementFunction_1(timerOfCamera, timeCameraMovingReadyPlay, viewAngleInPlay - viewAngleBeforeStart);
        dDistence = MainLogic.currencyMovementFunction_1(timerOfCamera, timeCameraMovingReadyPlay, viewDistenceInPlay - viewDistenceBeforeStart);
        Vector3 frameDeltaPos = calDeltaPos(viewAngleBeforeStart + dAngle, viewDistenceBeforeStart + dDistence);
        fixTarget(frameDeltaPos);
        if (!isCameraUp && timerOfCamera >= 0) {
            timerOfCamera -= Time.deltaTime;
        }else if (isCameraUp && timerOfCamera <= timeCameraMovingReadyPlay) {
            timerOfCamera += Time.deltaTime;
        }
    }

    void fixTarget(Vector3 deltaPos) {
        transform.position = target.transform.position + deltaPos;
        //avoid occlusion
        if (isCameraAvoidOcclusion) {
            //Debug.DrawLine(target.transform.position, transform.position);
            RaycastHit rh;
            if (Physics.Linecast(target.transform.position, transform.position, out rh)) {
                Vector3 directionVec=(target.transform.position - transform.position).normalized;
                transform.position = rh.point+0.1f*directionVec;
            }
        }
        //look at
        transform.LookAt(target.transform.position);
    }

    //calculate the relative camera position by angle and distence
    private Vector3 calDeltaPos(float viewAngle, float viewDistence) {
        float tranedAngle = viewAngle / 180 * Mathf.PI;
        float dy = viewDistence * Mathf.Sin(tranedAngle);
        float dz = -viewDistence * Mathf.Cos(tranedAngle);
        Vector3 deltaPos = new Vector3(0, dy, dz);
        return deltaPos;
    }



    //set
    public void setTarget(GameObject g) {
        target = g;
    }
    public void setIsCameraUp(bool b) {
        isCameraUp=b;
    }
}
