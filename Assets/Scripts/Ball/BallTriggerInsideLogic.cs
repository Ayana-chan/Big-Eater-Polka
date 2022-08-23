using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTriggerInsideLogic : MonoBehaviour
{
    public string cureName = "Cure Bullet";

    [Space]

    private BloodLogic bloodLogic;

    private void Awake() {
        bloodLogic=GameObject.Find("/Controllers/BloodController").GetComponent<BloodLogic>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("SomeThing get in");
        //cure bullet
        if(other.name == cureName) {
            Destroy(other.gameObject);
            bloodLogic.addBlood(1);
        }
    }
}
