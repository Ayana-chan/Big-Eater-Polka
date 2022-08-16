using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCreator : MonoBehaviour {
    [Tooltip("If false, no edge blocks")]
    public bool isEdgeEnabled=true;

    public float progressBlockCreation=0;

    public float blockCreatedPerFrame = 15;

    [Space]

    [Header("Prefabs")]
    [Tooltip("Mid block's prefab")]
    public GameObject roadMidBlockPrefab;
    [Tooltip("Edge block's prefab")]
    public GameObject roadEdgeBlockPrefab;

    [Space]

    [SerializeField]
    private MainLogic ml;
    [SerializeField]
    private MapDataManager mapDataManager;
    [SerializeField]
    private SaveDataManager saveDataManager;

    //
    //could also used when reset destroyed block
    public void createSingleMidBlock(float x, float y, float z, BlockTypeEnum blockType, int layer = 0, int row = 0, int col = 0) {
        if (blockType == BlockTypeEnum.empty) {
            return;//no create
        }
        GameObject nBlock;
        nBlock = Instantiate(roadMidBlockPrefab, midBlocks);//put into midBlocks
        //assign arguments
        BlockLogic bl = nBlock.GetComponent<BlockLogic>();
        BlockIndexMessage aim = new BlockIndexMessage(level, layer, row, col);
        bl.initBlock(blockType, aim);
        nBlock.transform.localScale = new Vector3(blockLength, blockLength, blockLength);
        nBlock.transform.localPosition = new Vector3(x, y, z);
        //one way door
        bool isOnewayDoor = blockType == BlockTypeEnum.onewayDoor_E || blockType == BlockTypeEnum.onewayDoor_S ||
                    blockType == BlockTypeEnum.onewayDoor_W || blockType == BlockTypeEnum.onewayDoor_N;
        if (isOnewayDoor && saveDataManager.currentSave.blockDestroyedOneWayDoorList.Contains(aim)) {
            nBlock.GetComponent<BoxCollider>().enabled = false;
            MeshRenderer mr = nBlock.GetComponent<MeshRenderer>();
            Color co = mr.material.color;
            co.a *= 0.5f;
            mr.material.color = co;
        }
    }

    int level;

    BlockTypeEnum[][][] mapData;//one level's data
    int mapLayerSize;
    int mapRowSize;
    int mapColSize;

    //block's set
    GameObject Road;
    Transform midBlocks;
    Transform edgeBlocks;

    float blockLength;

    int mapHeight;

    private int totalOfCreation;
    private int cntOfCreation;

    // Start is called before the first frame update
    void Start() {
        blockLength = ml.blockLength;

        level = ml.level;
        mapData = mapDataManager.getMapData(level);
        mapHeight = mapData.Length;

        Road = GameObject.Find("/GamePlace/Road");
        midBlocks = Road.transform.Find("MidBlocks");
        edgeBlocks = Road.transform.Find("EdgeBlocks");
    }

    // Update is called once per frame
    void Update() {

    }

    public void createRoad() {
        if (mapData == null) {
            Debug.LogError("ERROR: Map Data Null.");
            return;
        }
        calCreateRoad();
        
        StartCoroutine(createMid());//edge creation is call by mid
    }

    //calculate basic argument
    private void calCreateRoad() {
        mapLayerSize=mapData.Length;
        mapRowSize = mapData[0].Length;
        mapColSize = mapData[0][0].Length;
    }

    //
    //
    //mid
    private IEnumerator createMid() {
        //edge
        if (isEdgeEnabled) {
            totalOfCreation = (mapRowSize + 2) * (mapColSize + 2);
            yield return StartCoroutine(createEdge());//create edge first
        } else {
            totalOfCreation = mapRowSize * mapColSize;
        }
        //mid
        float x, y, z;
        BlockTypeEnum blockType;
        for (int row = 0; row < mapRowSize; row++) {
            for (int col = 0; col < mapColSize; col++) {
                x = col * blockLength;
                z = -row * blockLength;
                //create all layer
                for (int layer = 0; layer < mapLayerSize; layer++) {
                    blockType = mapData[layer][row][col];
                    y = layer * blockLength;
                    createSingleMidBlock(x, y, z, blockType, layer, row, col);
                    cntOfCreation++;
                    //wait for next frame
                    if (cntOfCreation >= blockCreatedPerFrame) {
                        cntOfCreation = 0;
                        yield return null;
                    }
                }
                //add to progress
                progressBlockCreation += 1.0f / totalOfCreation;
            }
        }
        progressBlockCreation = 1;
    }

    //
    //由于使用了不同的prefab，因此edgeBlock可以加特技
    //edge
    private IEnumerator createEdge() {
        float x, z;
        for (int i = -1; i < mapColSize + 1; i++) {
            x = i * blockLength;
            createSeriesEdgeBlock(x, blockLength);
            createSeriesEdgeBlock(x, -mapRowSize * blockLength);
            //add to progress
            progressBlockCreation += 2.0f / totalOfCreation;
            cntOfCreation += 2;
            //wait for next frame
            if (cntOfCreation >= blockCreatedPerFrame) {
                cntOfCreation = 0;
                yield return null;
            }
        }
        for (int i = 0; i < mapRowSize; i++) {
            z = -i * blockLength;
            createSeriesEdgeBlock(-blockLength, z);
            createSeriesEdgeBlock(mapColSize * blockLength, z);
            //add to progress
            progressBlockCreation += 2.0f / totalOfCreation;
            cntOfCreation += 2;
            //wait for next frame
            if (cntOfCreation >= blockCreatedPerFrame) {
                cntOfCreation = 0;
                yield return null;
            }
        }
    }
    private void createSeriesEdgeBlock(float x, float z) {
        float y = 0;
        for (int i = 0; i < mapHeight + 2; i++) {
            GameObject nNode = GameObject.Instantiate(roadEdgeBlockPrefab, edgeBlocks);//put into EdgeBlocks
            nNode.transform.localScale = new Vector3(blockLength, blockLength, blockLength);
            nNode.transform.localPosition = new Vector3(x, y, z);
            y += blockLength;//increase height
        }
    }


    private void destoryRoad() {//待开发
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);//DestroyImmediate() would change childCount immediately
        }
    }

    

    //get
    public float getProgressBlockCreation() {
        return progressBlockCreation;
    }
    //set
    public void setProgressBlockCreation(float f) {
        progressBlockCreation = f;
    }
}
