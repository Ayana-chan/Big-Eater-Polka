using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestLogic : MonoBehaviour
{
    public float activeSaveCDLength = 2;
    public float activeSaveCDRest = 0;

    [Space]

    public MainLogic mainLogic;
    public SaveDataManager saveDataManager;
    public BlockDestroyManager blockDestroyManager;
    public RebornBlockParticleLogic rebornBlockParticleLogic;
    public UI_TopMessageLogic ui_topMessageLogic;
    public BuffLogic buffLogic;
    public BloodLogic bloodLogic;

    public void rest() {
        //in cd
        if (activeSaveCDRest > 0) {
            return;
        }
        GameObject ball = mainLogic.getBall();
        BallLogic ballLogic = ball.GetComponent<BallLogic>();
        if (ball == null) {
            return;
        }
        float checkDuration = mainLogic.getBallDiameter() + mainLogic.getBlockLength() / 3;
        RaycastHit rh;
        //Debug.DrawRay(ball.transform.position, Vector3.down, Color.red, checkDuration, false);
        if (Physics.Raycast(ball.transform.position, Vector3.down, out rh, checkDuration)) {
            GameObject block = rh.collider.gameObject;
            BlockLogic blockLogic = block.GetComponent<BlockLogic>();
            BlockTypeEnum blockType = blockLogic.getBlockType();
            if (blockType == BlockTypeEnum.rebornType) {//reborn block, to rest
                //reset cd
                activeSaveCDRest = activeSaveCDLength;
                //have touched before, rest and load
                if (saveDataManager.getCurrentSave().rebornBlockTouchedSet.Contains(blockLogic.getBlockIndexMessage())) {
                    //move to rest place
                    ballLogic.ballRest(block);
                    //clear buff
                    buffLogic.clearAllBuff();
                    //cure
                    bloodLogic.completeCure();
                    //reset map
                    mainLogic.loadAfterRest();
                    //change par
                    rebornBlockParticleLogic.restAtRebornBlock(block);
                    //save
                    saveDataManager.getCurrentSave().setRebornBlockPos(block.transform.localPosition);
                    saveDataManager.saveBySerialization();
                } else {    //first touch, won't rest
                    //move to rest place
                    ballLogic.ballRest(block, true);
                    //mark touched
                    saveDataManager.getCurrentSave().rebornBlockTouchedSet.Add(blockLogic.getBlockIndexMessage());
                    //change par
                    rebornBlockParticleLogic.firstTouchRebornBlock(block);
                    ui_topMessageLogic.addTopMessage("又一块土地被点燃了");
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeSaveCDRest > 0) {
            activeSaveCDRest -= Time.deltaTime;
        }

    }
}
