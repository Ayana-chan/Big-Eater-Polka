using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolLogic : MonoBehaviour
{
    public Stack<GameObject> bulletPoolStack=new();

    [Tooltip("The number of bullet that created when start")]
    public int baseNum = 10;
    [Tooltip("If stack count beyond maxNum, destroy instead of push")]
    public int maxNum = 20;

    [Tooltip("If the number of bullet in map is beyond objectMaxNum, no more bullet created")]
    public int objectMaxNum = 100;
    public int objectNumCnt;

    [Space]

    public GameObject bulletPrefab;

    [Space]

    public Transform bulletPool;

    public GameObject getBullet() {
        if (objectNumCnt >= maxNum) {
            Debug.LogWarning("WARNING: Bullet Num Beyond Max.");
            return null;
        }
        objectNumCnt++;
        if (bulletPoolStack.Count > 0) {
            return bulletPoolStack.Pop();
        } else {    //empty
            return Instantiate(bulletPrefab, bulletPool);
        }
    }

    public void recycleBullet(GameObject bullet) {
        objectNumCnt--;
        if (bulletPoolStack.Count <= maxNum) {
            bulletPoolStack.Push(bullet);
        } else {    //full
            Destroy(bullet);
        }
    }

    private void Awake() {
        for(int i = 0; i < baseNum; i++) {
            GameObject nBullet = Instantiate(bulletPrefab,bulletPool);
            nBullet.SetActive(false);
            bulletPoolStack.Push(nBullet);
        }
        objectNumCnt = baseNum;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
