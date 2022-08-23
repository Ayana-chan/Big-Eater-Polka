using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BlockDestroyManager : MonoBehaviour {
    [Header("Handling lists")]
    //This list would be saved in save.
    public List<BlockIndexMessage> blockDestroyedForeverList;  //get from currentSave
    //special forever. When load, set transparent and BoxCo.enabled.
    public List<BlockIndexMessage> blockDestroyedOneWayDoorList = new List<BlockIndexMessage>();
    //When rest, this list's node(back-ups) would be active, list cleared.
    public List<GameObject> blockDestroyedTemporaryList = new List<GameObject>();
    //When rest, this list's node should be destroyed.
    public List<GameObject> blockWaitForDestroyList=new List<GameObject>();

    [Space]

    [Tooltip("Load speed set")]
    public int blockResetPerFrame = 10;
    public float processResetBlock = 0;

    [Space]

    public MainLogic ml;
    public RoadCreator roadCreator;
    public UI_TopMessageLogic ui_topMessageLogic;
    public BuffLogic buffLogic;

    //called when a block touched
    public void judgeDestoryBlockByType(GameObject block) {
        BlockLogic bl = block.GetComponent<BlockLogic>();
        //avoid duplicate creation
        if (bl.isFormalDestoryed) {
            return;
        } else {
            bl.isFormalDestoryed = true;//should reset false when not destroy
        }
        DestroyDegreeEnum destroyDegree;
        DestroyWayEnum destroyWay;
        Vector3 relativePos = ml.getBall().transform.position - block.transform.position;
        float rangeLength=ml.getBlockLength()/2;//max distance to mid
        float acceptableDistance = 1.1f * rangeLength;
        bool isRightTouch;
        //judge and destroy certain child
        switch (bl.blockType) {
            case BlockTypeEnum.cureType://cure
                destroyDegree = DestroyDegreeEnum.tempDes;
                destroyWay = DestroyWayEnum.lightOff;
                break;
            //four one way door
            case BlockTypeEnum.onewayDoor_E:
                isRightTouch = relativePos.x > 0 &&
                                    Mathf.Abs(relativePos.y) < acceptableDistance &&
                                    Mathf.Abs(relativePos.z) < acceptableDistance;
                if (!isRightTouch) {
                    ui_topMessageLogic.addTopMessage("不能从这一侧打开");
                    bl.isFormalDestoryed = false;
                    return;
                }
                destroyDegree = DestroyDegreeEnum.oneWayDoor;
                destroyWay = DestroyWayEnum.graduallyTransparent;
                break;
            case BlockTypeEnum.onewayDoor_S:
                isRightTouch = relativePos.z < 0 &&
                                    Mathf.Abs(relativePos.y) < acceptableDistance &&
                                    Mathf.Abs(relativePos.x) < acceptableDistance;
                if (!isRightTouch) {
                    ui_topMessageLogic.addTopMessage("不能从这一侧打开");
                    bl.isFormalDestoryed = false;
                    return;
                }
                destroyDegree = DestroyDegreeEnum.oneWayDoor;
                destroyWay = DestroyWayEnum.graduallyTransparent;
                break;
            case BlockTypeEnum.onewayDoor_W:
                isRightTouch = relativePos.x < 0 &&
                                    Mathf.Abs(relativePos.y) < acceptableDistance &&
                                    Mathf.Abs(relativePos.z) < acceptableDistance;
                if (!isRightTouch) {
                    ui_topMessageLogic.addTopMessage("不能从这一侧打开");
                    bl.isFormalDestoryed = false;
                    return;
                }
                destroyDegree = DestroyDegreeEnum.oneWayDoor;
                destroyWay = DestroyWayEnum.graduallyTransparent;
                break;
            case BlockTypeEnum.onewayDoor_N:
                isRightTouch = relativePos.z > 0 &&
                                    Mathf.Abs(relativePos.y) < acceptableDistance &&
                                    Mathf.Abs(relativePos.x) < acceptableDistance;
                if (!isRightTouch) {
                    ui_topMessageLogic.addTopMessage("不能从这一侧打开");
                    bl.isFormalDestoryed = false;
                    return;
                }
                destroyDegree = DestroyDegreeEnum.oneWayDoor;
                destroyWay = DestroyWayEnum.graduallyTransparent;
                break;
            case BlockTypeEnum.woodBoxType:
                if (buffLogic.getCurrentBuffKind(0) != BuffKindEnum.onFireBuff) {
                    bl.isFormalDestoryed = false;
                    return;
                }
                destroyDegree = DestroyDegreeEnum.tempDes;
                destroyWay = DestroyWayEnum.miss;
                break;
            default:
                return;//can't be destroyed
        }
        destroyBlockByType(block, destroyDegree, destroyWay);
    }

    //start destroy. Can only called after judge.
    private void destroyBlockByType(GameObject block, DestroyDegreeEnum destroyDegree, DestroyWayEnum destroyWay) {
        BlockLogic blockLogic = block.GetComponent<BlockLogic>();
        //judge degree list
        switch (destroyDegree) {
            case DestroyDegreeEnum.foreverDes:
                blockDestroyedForeverList.Add(blockLogic.blockIndexMessage);
                break;
            case DestroyDegreeEnum.oneWayDoor:
                blockDestroyedOneWayDoorList.Add(blockLogic.blockIndexMessage);
                destoryOnewayDoor(block);
                return;//no need other destroy
            case DestroyDegreeEnum.tempDes:
                //recreate a back-up, and destory self
                GameObject backupBlock = roadCreator.createSingleMidBlock(block.transform.localPosition, blockLogic.getBlockType(), blockLogic.getBlockIndexMessage());
                backupBlock.SetActive(false);
                blockDestroyedTemporaryList.Add(backupBlock);
                break;
        }
        //destroy in different ways
        switch (destroyWay) {
            case DestroyWayEnum.miss:
                //起码有点动画反应？烟雾？
                Destroy(block);
                break;
            case DestroyWayEnum.graduallyTransparent:
                destroyGraduallyTransparent(block);
                break;
            case DestroyWayEnum.lightOff://no destory. Destoryed in reset. Maybe not fit forever.
                destroyLightOff(block);
                blockWaitForDestroyList.Add(block);
                break;
        }
    }

    //called when active save
    public void handleDestoryWhenRest() {
        resetBlockAsync();
    }

    private async void resetBlockAsync() {
        processResetBlock = 0;//reset progress
        int num = blockWaitForDestroyList.Count+blockDestroyedTemporaryList.Count;
        if (num == 0) { //no any block
            processResetBlock = 1;
            return;
        }
        int cnt = 0;
        //destroy
        foreach (GameObject block in blockWaitForDestroyList) {
            Destroy(block);
            processResetBlock += 1f / num;
            cnt++;
            if (cnt == blockResetPerFrame) {
                cnt = 0;
                await UniTask.Yield();//load next frame
            }
        }
        blockWaitForDestroyList.Clear();
        //recreate
        foreach (GameObject block in blockDestroyedTemporaryList) {
            block.SetActive(true);//active back up
            processResetBlock += 1f / num;
            cnt++;
            if (cnt == blockResetPerFrame) {
                cnt = 0;
                await UniTask.Yield();//load next frame
            }
        }
        blockDestroyedTemporaryList.Clear();
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void destoryOnewayDoor(GameObject block) {
        float aimARatio = 0.5f;
        float transparentDecreaseTimeUse = 1.1f;
        Material material = block.GetComponent<MeshRenderer>().material;
        material.DOFade(aimARatio, transparentDecreaseTimeUse)
            .OnComplete(()=> { block.GetComponent<BoxCollider>().enabled = false; });
    }

    private void destroyGraduallyTransparent(GameObject block) {
        float transparentDecreaseTimeUse = 1.1f;
        Material material = block.GetComponent<MeshRenderer>().material;
        material.DOFade(0, transparentDecreaseTimeUse)
            .OnComplete(() => { Destroy(block); });
    }

    private void destroyLightOff(GameObject block) {
        float lightRangeDecreaseTimeUse = 2f;
        float aimRange = 1;
        Light l = block.GetComponent<Light>();
        DOTween.To(()=>l.range,x=>l.range=x,aimRange,lightRangeDecreaseTimeUse);
    }



    //get
    public float getProcessResetBlock() {
        return processResetBlock;
    }
    //set
    public void setProcessResetBlock(float f) {
        processResetBlock = f;
    }
}

/*
 * whether block would destroyed forever or temp
*/
public enum DestroyDegreeEnum {
    foreverDes,
    oneWayDoor,
    tempDes,
}
/*
 * the way block destroyed
 */
public enum DestroyWayEnum {
    miss,//default
    graduallyTransparent,//only could use in material that support change color
    lightOff,
}



//private IEnumerator onewayDoorDesIEnumerator(GameObject block) {
//    float aimARatio = 0.5f;
//    float transparentDecreaseSpeed = 0.7f;
//    MeshRenderer mr = block.GetComponent<MeshRenderer>();
//    Color co;
//    float deltaA = transparentDecreaseSpeed * mr.material.color.a * Time.fixedDeltaTime;
//    float aimA = aimARatio * mr.material.color.a;
//    while (true) {//avoid null. But why?
//        co = mr.material.color;
//        co.a -= deltaA;
//        if (co.a >= aimA) {
//            mr.material.color = co;
//            yield return new WaitForFixedUpdate();
//        } else {
//            co.a = aimA;
//            mr.material.color = co;
//            block.GetComponent<BoxCollider>().enabled = false;
//            break;
//        }
//    }
//}

//private IEnumerator graduallyTransparentIEnumerator(GameObject block) {

//    Color co;
//    float deltaA = transparentDecreaseSpeed * mr.material.color.a * Time.fixedDeltaTime;
//    while (block) {//avoid null. But why?
//        co = mr.material.color;
//        co.a -= deltaA;
//        if (co.a >= 0) {
//            mr.material.color = co;
//            yield return new WaitForFixedUpdate();
//        } else {

//            break;
//        }
//    }
//}

//private IEnumerator lightOffIEnumerator(GameObject block) {
//    float lightRangeDecreaseSpeed = 0.5f;
//    float aimRange = 1;
//    Light l=block.GetComponent<Light>();
//    float deltaRange = lightRangeDecreaseSpeed * l.range * Time.fixedDeltaTime;
//    while (true) {
//        l.range -= deltaRange;
//        if (l.range >= aimRange) { //if >=0, won't call 'else'. Why?
//            yield return new WaitForFixedUpdate();
//        } else {
//            l.range = aimRange;
//            break;
//        }
//    }
//}
