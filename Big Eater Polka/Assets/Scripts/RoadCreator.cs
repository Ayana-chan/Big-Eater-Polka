using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCreator : MonoBehaviour {
    [Tooltip("If false, no edge blocks")]
    public bool isEdgeEnabled=true;
    public int edgeHeight = 10;

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
    public RebornBlockParticleLogic rebornBlockParticleLogic;

    public void createRoad() {
        if (mapData == null) {
            Debug.LogError("ERROR: Map Data Null.");
            return;
        }
        calCreateRoad();

        StartCoroutine(createMid());//edge creation is call by mid
    }

    /// <summary>
    /// BlockPos = BlockIndexMessage * blockLength
    /// could also used when reset destroyed block
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="blockType"></param>
    /// <param name="layer"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public GameObject createSingleMidBlock(float x, float y, float z, BlockTypeEnum blockType, int layer = 0, int row = 0, int col = 0) {
        if (blockType == BlockTypeEnum.empty) {
            return null;//no create
        }
        GameObject nBlock;
        nBlock = Instantiate(roadMidBlockPrefab, midBlocks);//put into midBlocks
        //assign arguments
        BlockLogic bl = nBlock.GetComponent<BlockLogic>();
        BlockIndexMessage aim = new BlockIndexMessage(level, layer, row, col);
        bl.initBlock(blockType, aim);
        nBlock.transform.localScale = new Vector3(blockLength, blockLength, blockLength);
        nBlock.transform.localPosition = new Vector3(x, y, z);
        //reborn block
        if (blockType == BlockTypeEnum.rebornType) {
            rebornBlockParticleLogic.createRebornBlockParticle(nBlock);
        }
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
        return nBlock;
    }
    public GameObject createSingleMidBlock(Vector3 pos, BlockTypeEnum blockType, BlockIndexMessage bim) {
        return createSingleMidBlock(pos.x, pos.y, pos.z, blockType, bim.layer, bim.row, bim.col);
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

    private int totalOfCreation;
    private int cntOfCreation;

    // Start is called before the first frame update
    void Start() {
        blockLength = ml.blockLength;

        level = ml.level;
        mapData = mapDataManager.getMapData(level);//

        Road = GameObject.Find("/GamePlace/Road");
        midBlocks = Road.transform.Find("MidBlocks");
        edgeBlocks = Road.transform.Find("EdgeBlocks");
    }

    // Update is called once per frame
    void Update() {

    }

    //calculate basic argument
    private void calCreateRoad() {
        mapLayerSize=mapData.Length;
        if (isEdgeEnabled) {
            //cal mapRowSize
            mapRowSize = 0;
            int temp = 0;
            for (int i = 0; i < mapLayerSize; i++) {
                temp = mapData[i].Length;
                if (temp > mapRowSize) {
                    mapRowSize = temp;
                }
            }
            //cal mapColSize
            mapColSize = 0;
            temp = 0;
            for (int i = 0; i < mapLayerSize; i++) {
                for (int j = 0; j < mapData[i].Length; j++) {
                    temp = mapData[i][j].Length;
                    if (temp > mapColSize) {
                        mapColSize = temp;
                    }
                }
            }
        }
    }

    //
    //
    //mid
    private IEnumerator createMid() {
        totalOfCreation = 0;
        //let totalOfCreation be the number of rows
        for (int layer = 0; layer < mapLayerSize; layer++) {
            totalOfCreation+=mapData[layer].Length;
        }
        //edge
        if (isEdgeEnabled) {
            totalOfCreation += 4 * edgeHeight;
            yield return StartCoroutine(createEdge());//create edge first
        }
        //mid
        float x, y, z;
        BlockTypeEnum blockType;
        for (int layer = 0; layer < mapLayerSize; layer++) {
            for (int row = 0; row < mapData[layer].Length; row++) {
                for (int col = 0; col < mapData[layer][row].Length; col++) {
                    blockType = mapData[layer][row][col];
                    if (blockType == BlockTypeEnum.empty) {
                        continue;
                    }
                    x = col * blockLength;
                    y = layer * blockLength;
                    z = -row * blockLength;
                    //create all layer
                    createSingleMidBlock(x, y, z, blockType, layer, row, col);
                    cntOfCreation++;
                    //wait for next frame
                    if (cntOfCreation >= blockCreatedPerFrame) {
                        cntOfCreation = 0;
                        yield return null;
                    }
                }
                //add to progress
                progressBlockCreation += 1f / totalOfCreation;
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
            progressBlockCreation += 2f / (mapColSize + 2) * edgeHeight / totalOfCreation;
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
            progressBlockCreation += 2f / mapRowSize * edgeHeight / totalOfCreation;
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
        for (int i = 0; i < edgeHeight; i++) {
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
