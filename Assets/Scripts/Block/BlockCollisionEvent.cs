using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollisionEvent : MonoBehaviour
{
    [Header("Cure Block")]
    public GameObject cureBulletPrefab;
    [Tooltip("Range of mid point")]
    public float cureBulletMoveRange=1.5f;
    public int cureBulletCreationPerSecond = 2;
    public int cureBulletNum = 30;

    [Space]

    public Transform tempThings;
    public MainLogic ml;
    public UI_TopMessageLogic uI_topMessageLogic;

    public void handleCollisionEvent(GameObject block) {
        BlockLogic bl = block.GetComponent<BlockLogic>();
        switch (bl.getBlockType()) {
            case BlockTypeEnum.cureType:
                if (bl.getIsFormalDestoryed()) {
                    uI_topMessageLogic.addTopMessage("¿ÝÎ®...");
                    return;
                }
                StartCoroutine(cureBlockCoEventIEnumerator(block));
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator cureBlockCoEventIEnumerator(GameObject block) {
        int cnt = cureBulletNum;
        float waitToCreate=0;
        while (true) {
            yield return new WaitForFixedUpdate();
            waitToCreate += cureBulletCreationPerSecond * Time.fixedDeltaTime;
            while (waitToCreate >= 1) {
                waitToCreate -= 1;
                cnt--;
                GameObject cureBullet = Instantiate(cureBulletPrefab, block.transform.position, Quaternion.identity, tempThings);
                cureBullet.name = "Cure Bullet";
                BezierMoveLogic bezierMoveLogic = cureBullet.GetComponent<BezierMoveLogic>();
                bezierMoveLogic.shoot(block.transform.position, block.transform.position + getRandomVector(cureBulletMoveRange), ml.getBall().transform);
                if (cnt <= 0) {
                    yield break;
                }
            }
        }
    }

    private Vector3 getRandomVector(float r) {
        return new Vector3(Random.Range(-r, r), Random.Range(-r, r), Random.Range(-r, r));
    }
}
