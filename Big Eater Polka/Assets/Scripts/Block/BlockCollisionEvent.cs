using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollisionEvent : MonoBehaviour
{
    [Header("Cure Block")]
    public GameObject cureBulletParSysPrefab;
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
                Instantiate(cureBulletParSysPrefab,block.transform.position,block.transform.rotation,block.transform);
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

    private Vector3 getRandomVector(float r) {
        return new Vector3(Random.Range(-r, r), Random.Range(-r, r), Random.Range(-r, r));
    }
}
