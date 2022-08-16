using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveDataManager : MonoBehaviour
{
    public bool isAutoSave;
    public float timeAutoSave=30;

    public string savePath;

    public SaveStructure currentSave;

    [Space]

    public MainLogic ml;
    public MapDataManager mapDataManager;
    public BlockDestroyManager blockDestroyManager;

    public void activeSave() {
        GameObject ball=ml.getBall();
        BallLogic ballLogic=ball.GetComponent<BallLogic>();
        if (ball == null) {
            return;
        }
        float checkDuration=ml.getBallDiameter()+ml.getBlockLength()/3;
        RaycastHit rh;
        //Debug.DrawRay(ball.transform.position, Vector3.down, Color.red, checkDuration, false);
        if (Physics.Raycast(ball.transform.position, Vector3.down,out rh, checkDuration)) {
            GameObject block = rh.collider.gameObject;
            BlockTypeEnum blockType = block.GetComponent<BlockLogic>().getBlockType();
            if (blockType == BlockTypeEnum.rebornType) {//reborn block
                //move to rest place
                ballLogic.ballRest(block);
                //reset map
                ml.loadAfterRest();
                //save
                currentSave.changeRebornBlockPos(block.transform.localPosition);
                saveBySerialization();
            }
        }
    }
    //
    //save data functions
    //save
    public void saveBySerialization() {
        Debug.Log("Saving...");
        BinaryFormatter bf=new BinaryFormatter();
        FileStream fs = File.Create(savePath);
        bf.Serialize(fs, currentSave);
        fs.Close();
    }
    //load
    public void loadByDeserialization() {
        Debug.Log("Loading...");
        if (File.Exists(savePath)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(savePath, FileMode.Open);
            currentSave = bf.Deserialize(fs) as SaveStructure;
            fs.Close();
            buildSave();
        } else {//if no save
            rebuildSave();
        }
    }

    private BallLogic ballLogic;

    //after load
    private void buildSave() {
        blockDestroyManager.blockDestroyedForeverList = currentSave.blockDestroyedForeverList;
        blockDestroyManager.blockDestroyedOneWayDoorList = currentSave.blockDestroyedOneWayDoorList;
        mapDataManager.handleForeverChangedBlockInMap();
    }
    //restart
    private void rebuildSave() {
        currentSave = new SaveStructure();
        currentSave.changeRebornBlockPos(mapDataManager.getFirstBornPlace(0));
        currentSave.blockDestroyedForeverList = new List<BlockIndexMessage>();
        currentSave.blockDestroyedOneWayDoorList = new List<BlockIndexMessage>();
        buildSave();
    }

    private IEnumerator autoSave() {
        while (true) {
            yield return new WaitForSeconds(timeAutoSave);
            if (isAutoSave) {
                saveBySerialization();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(autoSave());

    }

    // Update is called once per frame
    void Update()
    {

    }
}

//
//
//choose what to save
[System.Serializable]
public class SaveStructure {
    public int level;

    //reborn position
    public float rebornBlockPos_X;
    public float rebornBlockPos_Y;
    public float rebornBlockPos_Z;

    //block change forever message
    public List<BlockIndexMessage> blockDestroyedForeverList;
    public List<BlockIndexMessage> blockDestroyedOneWayDoorList;

    public SaveStructure() { }

    public void changeRebornBlockPos(Vector3 rebornBlockPos) {
        rebornBlockPos_X=rebornBlockPos.x;
        rebornBlockPos_Y = rebornBlockPos.y;
        rebornBlockPos_Z = rebornBlockPos.z;
    }
}
