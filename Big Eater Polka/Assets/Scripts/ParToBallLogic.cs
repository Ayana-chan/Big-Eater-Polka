using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

public class ParToBallLogic : MonoBehaviour
{
    public float moveSpeed = 3;

    [Space]

    public MainLogic mainLogic;
    public BloodLogic bloodLogic;

    private GameObject ball;
    private ParticleSystem ps;

    private ParticleSystem.Particle[] pars;
    private int pCount;

    private void Awake() {
        mainLogic = GameObject.Find("/Controllers/MainController").GetComponent<MainLogic>();
        bloodLogic= GameObject.Find("/Controllers/BloodController").GetComponent<BloodLogic>();
        ball = mainLogic.getBall();
        ps = transform.GetComponent<ParticleSystem>();

        Invoke("parPosControl", 0.5f);
        //parPosControl();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnParticleTrigger() {
    //    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    //    int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    //    if (numEnter > 0) {
    //        Debug.Log("в╟иогРак---");
    //        //bloodLogic.addBlood(numEnter);
    //    }
    //}
    
    private void parPosControl() {
        pars = new ParticleSystem.Particle[ps.particleCount];
        pCount = ps.GetParticles(pars);
        for (int i = 0; i < pCount; i++) {
            ParticleSystem.Particle par = pars[i];
            StartCoroutine(calBezierPointAsync(transform.position, transform.position + getRandomVector(2),ball.transform,i));
        }
    }

    private IEnumerator calBezierPointAsync(Vector3 startPoint, Vector3 midPoint, Transform target,int i) {
        Vector3 p1, p2, p;
        for (float insertRatio = 0; insertRatio <= 1; insertRatio += Time.fixedDeltaTime) {
            p1 = Vector3.Lerp(startPoint, midPoint, insertRatio);
            p2 = Vector3.Lerp(midPoint, target.position, insertRatio);
            p = Vector3.Lerp(p1, p2, insertRatio);
            yield return StartCoroutine(moveToPointAsync(p, i));//won't calculate next point until move to the point
        }
        StartCoroutine(moveToTargetStraightAsync(target,i));
    }

    private IEnumerator moveToPointAsync(Vector3 p, int i) {
        while (Vector3.Distance(pars[i].position, p) > 0.03f) {
            pars[i].position = Vector3.MoveTowards(pars[i].position, p, moveSpeed * Time.fixedDeltaTime);
            ps.SetParticles(pars);
            yield return null;
        }
    }
    private IEnumerator moveToTargetStraightAsync(Transform target, int i) { //just run to target at last
        while (Vector3.Distance(pars[i].position, target.position) > 0.1f) {
            pars[i].position = Vector3.MoveTowards(pars[i].position, target.position, moveSpeed * Time.fixedDeltaTime);
            ps.SetParticles(pars);
            yield return null;
        }
        //destory par
        pars[i].remainingLifetime = 0;  
        ps.SetParticles(pars);
        //add blood
        bloodLogic.addBlood(1);
    }

    private Vector3 getRandomVector(float r) {
        return new Vector3(Random.Range(-r, r), Random.Range(-r, r), Random.Range(-r, r));
    }
}
