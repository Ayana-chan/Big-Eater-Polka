using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodLogic : MonoBehaviour
{
    [Header("State")]
    public int maxBlood = 50;
    public int currentBlood = 50;//[0,100]

    [Space]

    [Tooltip("If true, currentBlood won't change")]
    public bool isBloodFixed=false;
    public int unDeadLeastBlood = 1;

    [Space]

    public DieLogic dieLogic;

    public void addBlood(int cureBlood) {
        if (isBloodFixed) {
            return;
        }
        currentBlood += cureBlood;
        if (currentBlood > maxBlood) {
            currentBlood = maxBlood;
        }
    }

    public void reduceBlood(int hurtBlood,bool undead=false) {
        if (isBloodFixed) {
            return;
        }
        currentBlood -= hurtBlood;
        if (!undead) {
            if (currentBlood <= 0) {
                currentBlood = 0;
                dieLogic.dieImmediately();
            }
        } else {
            if (currentBlood < unDeadLeastBlood) {
                currentBlood = unDeadLeastBlood;
            }
        }
    }

    public void completeCure() {
        currentBlood = maxBlood;
    }

    public void setBloodZero() {
        currentBlood = 0;
    }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    //get
    public int getCurrentBlood() {
        return currentBlood;
    }
    public int getMaxBlood() {
        return maxBlood;
    }

    //set
    public void setIsBloodFixed(bool b) {
        isBloodFixed = b;
    }
}
