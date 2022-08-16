using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierMoveLogic : MonoBehaviour {
    [Tooltip("Destroy self when time used up")]
    public float lifeTime = 15;

    public float moveSpeed = 5;

    public void shoot(Vector3 startPoint, Vector3 midPoint, Transform target) {
        StartCoroutine(calBezierPointIEnumerator(startPoint, midPoint, target));
    }

    // Start is called before the first frame update
    void Start() {
        Destroy(gameObject, lifeTime);//Destroy self when time used up

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    private IEnumerator calBezierPointIEnumerator(Vector3 startPoint, Vector3 midPoint, Transform target) {
        Vector3 p1, p2, p;
        for (float insertRatio = 0; insertRatio <= 1; insertRatio += Time.fixedDeltaTime) {
            p1 = Vector3.Lerp(startPoint, midPoint, insertRatio);
            p2 = Vector3.Lerp(midPoint, target.position, insertRatio);
            p = Vector3.Lerp(p1, p2, insertRatio);
            yield return StartCoroutine(moveToPointIEnumerable(p));//won't calculate next point until move to the point
        }
        StartCoroutine(moveToTargetStraightIEnumerable(target));
    }

    private IEnumerator moveToPointIEnumerable(Vector3 p) {
        Vector3 dir;
        while (Vector3.Distance(transform.position, p) > 0.1f) {
            dir = p - transform.position;
            transform.up = dir;//look at aim point
            transform.position = Vector3.MoveTowards(transform.position, p, moveSpeed * Time.fixedDeltaTime);
            yield return null;
        }
    }
    private IEnumerator moveToTargetStraightIEnumerable(Transform target) { //just run to target at last
        Vector3 dir;
        //Vector3.Distance(transform.position, target.position) > 0.1f
        while (true) {
            dir = target.position - transform.position;
            transform.up = dir;//look at aim point
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.fixedDeltaTime);
            yield return null;
        }
    }
}
