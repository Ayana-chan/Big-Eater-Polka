using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class BallLogic : MonoBehaviour
{
    [Tooltip("The material of each ball type.")]
    public Material[] materialOfBallTypes;

    [Space]

    [Header("Movement set")]
    [Tooltip("Movement's base force")]
    public float baseForce = 5;
    [Tooltip("Movement's max speed in XZ")]
    public float maxMoveSpeed = 6;
    [Tooltip("Max speed in Y, avoiding too high jump")]
    public float maxYSpeed = 3;

    [Space]

    [Header("Rest set")]
    [Tooltip("The move speed when go to rest")]
    public float restMoveSpeed = 1;

    [Space]

    [Header("Jump set")]
    [Tooltip("Jump height when initial speed y = 0")]
    public float jumpHight = 1.2f;
    private float jumpSpeed;//calculate by jumpHight

    public float jumpCDLength = 2;
    private float jumpCDRest = 0;

    public void changeBallType(int flag) {
        if (flag >= 0 && flag <= materialOfBallTypes.Length) {
            ballType = flag;
            GetComponent<MeshRenderer>().material = materialOfBallTypes[ballType];
        } else {
            Debug.LogError("ERROR: Unexpected ball type.");
        }
    }

    public void ballJump() {
        if (jumpCDRest <= 0.1f) {
            Debug.Log("Jump.");
            jumpCDRest = jumpCDLength;//update cd
            rb.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.VelocityChange);
        }
    }

    public void ballRest(GameObject rebornBlock) {
        Vector3 rebornBlockPos = rebornBlock.transform.position;
        Vector3 aimPos = rebornBlockPos + Vector3.up * (0.01f + ml.getBlockLength() / 2 + ml.getBallDiameter() / 2);
        float moveTime = restMoveSpeed * Vector3.Distance(transform.position, aimPos) / ml.getBlockLength();
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.DOMove(aimPos, moveTime).OnComplete(() => {
            transform.position = aimPos;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
        });
    }

    MainLogic ml;

    Rigidbody rb;

    //associated with buff
    int ballType = 0;

    //input
    float forward = 0;//z
    float right = 0;//x

    private void Awake() {
        ml = GameObject.Find("MainController").GetComponent<MainLogic>();

        rb = GetComponent<Rigidbody>();
        jumpSpeed = Mathf.Pow(19.62f * jumpHight, 0.5f);

        rb.useGravity = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (!rb.useGravity && !ml.getIsLoading()) {
            rb.useGravity = true;//start fall
            Debug.Log("Ball useGravity.");
        }
        //input
        if (Input.GetKey(KeyCode.W)) {
            forward = 1;
        } else if (Input.GetKey(KeyCode.S)) {
            forward = -1;
        } else {
            forward = 0;
        }
        if (Input.GetKey(KeyCode.A)) {
            right = -1;
        } else if (Input.GetKey(KeyCode.D)) {
            right = 1;
        } else {
            right = 0;
        }
    }

    private void FixedUpdate() {
        //input to move
        if (ml.getIsInControl()) {
            //rb.AddTorque(new Vector3(forward * torque, 0, 0),ForceMode.Impulse);
            //rb.AddTorque(new Vector3(0, 0, -right * torque),ForceMode.Impulse);
            rb.AddForce(new Vector3(forceFunction(right, rb.velocity.x), 0, 0));
            rb.AddForce(new Vector3(0, 0, forceFunction(forward, rb.velocity.z)));
        }
        //jump cd rest decrease
        if (jumpCDRest > 0) {
            jumpCDRest -= Time.fixedDeltaTime;
        }
        //limit Y speed
        if (rb.velocity.y > maxYSpeed) {
            rb.velocity=new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.x);
        }
    }

    //calculate force
    private float forceFunction(float direction, float v) {
        float f = (direction * ml.getBlockLength() * maxMoveSpeed - v) * baseForce;
        return f;
    }
}
