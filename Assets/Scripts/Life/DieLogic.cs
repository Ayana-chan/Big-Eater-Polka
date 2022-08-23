using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

public class DieLogic : MonoBehaviour
{
    public float ballMoveToRebornDelayTime = 3.5f;
    public float loadDelayTime = 2f;

    [Space]

    public bool isImmediatelyDie=false;

    [Space]

    public float fallToDieHeight = -15;

    [Space]

    public MainLogic mainLogic;
    public BloodLogic bloodLogic;
    public SaveDataManager saveDataManager;
    public BuffLogic buffLogic;
    public UI_DieMessageLogic ui_dieMessageLogic;

    public void dieImmediately() {
        isImmediatelyDie = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        dieManageAsync();
        fallToDieAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void dieManageAsync() {
        BallLogic ballLogic = mainLogic.getBallLogic();
        Collider ballCollider = ballLogic.gameObject.GetComponent<Collider>();
        while (true) {
            await UniTask.WaitUntil(() => isImmediatelyDie);
            //start die
            Debug.Log("Player Died");
            //fix blood
            bloodLogic.setIsBloodFixed(true);
            //out control
            mainLogic.setIsInControl(false);
            ballLogic.resetBallSpeed();
            ballLogic.getRigidBody().useGravity = false;
            ballCollider.enabled = false;
            //ËÀÍö¶¯»­------------
            //await
            ui_dieMessageLogic.showDieUI();
            //move to rest place
            rebornAsync();
            await UniTask.Delay(System.TimeSpan.FromSeconds(loadDelayTime));
            //reset map
            mainLogic.loadAfterRest();
            //save
            saveDataManager.saveBySerialization();
            await UniTask.WaitUntil(() => !isImmediatelyDie);
        }
    }

    private async void rebornAsync() {
        Vector3 rebornBlockPos = saveDataManager.getCurrentSave().getRebornBlockPos();
        Vector3 aimPos = rebornBlockPos + Vector3.up * (0.01f + mainLogic.getBlockLength() / 2 + mainLogic.getBallDiameter() / 2);
        await UniTask.Delay(System.TimeSpan.FromSeconds(ballMoveToRebornDelayTime));
        //clear buff
        buffLogic.clearAllBuff();
        //move
        mainLogic.getBallLogic().resetBallSpeed();
        mainLogic.getBall().transform.localPosition = aimPos;
        //reset blood
        bloodLogic.completeCure();
        //reset die and bloodFix
        bloodLogic.setIsBloodFixed(false);
        isImmediatelyDie = false;
    }

    private async void fallToDieAsync() {
        Transform ballTransform = mainLogic.getBall().transform;
        while (true) {
            await UniTask.WaitUntil(() => ballTransform.localPosition.y <= fallToDieHeight);
            dieImmediately();
            await UniTask.WaitUntil(() => !isImmediatelyDie && ballTransform.localPosition.y > fallToDieHeight);
        }
    }
}
