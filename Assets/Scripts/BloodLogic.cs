using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodLogic : MonoBehaviour
{
    [Header("State")]
    public int maxBlood = 50;
    public int currentBlood = 50;//[0,100]

    public void addBlood(int cureBlood) {
        currentBlood += cureBlood;
        if (currentBlood > maxBlood) {
            cureBlood = maxBlood;
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
}
