using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLogic : MonoBehaviour
{
    [Tooltip("Indicate the type of block")]
    public BlockTypeEnum blockType;

    public BlockIndexMessage blockIndexMessage;

    public bool isFormalDestoryed=false;

    public void initBlock(BlockTypeEnum nType,int level,int layer,int row,int col) {
        setBlockIndexMessage(level,layer, row, col);
        changeBlockType(nType);
        gameObject.name="Block_"+layer+"_"+row+"_"+col;
        addBlockLight();
    }
    public void initBlock(BlockTypeEnum nType, BlockIndexMessage bIM) {
        setBlockIndexMessage(bIM);
        changeBlockType(nType);
        gameObject.name = "Block_" + bIM.layer + "_" + bIM.row + "_" + bIM.col;
        addBlockLight();
    }

    //used in initialization or to change type
    public void changeBlockType(BlockTypeEnum nType) {
        Material mat;
        mat = blockTypeManager.transTypeToMaterial(nType);
        if (mat) {
            blockType = nType;
            GetComponent<MeshRenderer>().material = mat;
        } else {
            Debug.LogError("ERROR: Unexpected Block Type.");
        }
    }


    MainLogic ml;
    BuffLogic buffLogic;
    BlockDestroyManager blockDestroyManager;
    BlockTypeManager blockTypeManager;
    BlockCollisionEvent blockCollisionEvent;

    private void addBlockLight() {
        float range;
        Color color;
        switch (blockType) {
            case BlockTypeEnum.cureType:
                color = Color.green;
                range = 8;
                break;
            case BlockTypeEnum.fireType:
                color = Color.red;
                range = 5;
                break;
            case BlockTypeEnum.iceType:
                color = Color.blue;
                range = 5;
                break;
            //more colors...
            default:
                return;//no light
        }
        Light light = gameObject.AddComponent<Light>();
        light.color = color;
        light.range = range;
    }

    private void Awake() {
        ml = GameObject.Find("MainController").GetComponent<MainLogic>();
        buffLogic = GameObject.Find("BuffController").GetComponent<BuffLogic>();
        blockDestroyManager = GameObject.Find("BlockController").GetComponent<BlockDestroyManager>();
        blockTypeManager = GameObject.Find("BlockController").GetComponent<BlockTypeManager>();
        blockCollisionEvent= GameObject.Find("BlockController").GetComponent<BlockCollisionEvent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject == ml.getBall()) {
            //let buff manager to handle more
            buffLogic.blockCollisionEvent_index_0(gameObject);
            //trigger other events
            blockCollisionEvent.handleCollisionEvent(gameObject);
            //check whether destroy
            blockDestroyManager.judgeDestoryBlockByType(gameObject);
        }
    }

    private void OnCollisionStay(Collision collision) { //只有跳跃有用，可拓展
        switch (blockType) {
            case BlockTypeEnum.jumpType:
                ml.getBallLogic().ballJump();
                break;
        }
    }



    //get
    public BlockTypeEnum getBlockType() {
        return blockType;
    }
    public BlockIndexMessage getBlockIndexMessage() {
        return blockIndexMessage;
    }
    public bool getIsFormalDestoryed() {
        return isFormalDestoryed;
    }

    //set
    public void setBlockIndexMessage(int level,int layer, int row, int col) {
        blockIndexMessage = new BlockIndexMessage(level,layer,row,col);
    }
    public void setBlockIndexMessage(BlockIndexMessage nBIM) {
        blockIndexMessage = nBIM;
    }
}

/*
 * to save index when created
*/
[System.Serializable]
public struct BlockIndexMessage {
    public int level;//easy to judge level
    public int layer;
    public int row;
    public int col;
    public BlockIndexMessage(int level, int layer,int row,int col) {
        this.level=level;
        this.layer = layer;
        this.row = row;
        this.col = col;
    }
}
