using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class MapDataManager : MonoBehaviour {
    /* 
     * ABOUT MAP
     * map's size depand on the floor(layer 0). Col size depand on the first line.
     * attention: remain a line after data. There would be a 'Enter' default. There comment is allowed
     */
    public string mapDatasPath;

    //level number : layer number : row number : col number : block type
    //first layer mapDatasArray[?][0] is floor
    public BlockTypeEnum[][][][] mapDatasArray;

    [Space]

    public MainLogic ml;
    public SaveDataManager saveDataManager;
    public BlockTypeManager blockTypeManager;

    //one layer,one txt
    public void mapDatasInitialization_2() {
        DirectoryInfo mapDatasPathInfo = new DirectoryInfo(mapDatasPath);
        DirectoryInfo[] mapsOfLevelsPathInfos = mapDatasPathInfo.GetDirectories();
        maxNumberOfLevels = mapsOfLevelsPathInfos.Length;
        mapDatasArray = new BlockTypeEnum[maxNumberOfLevels][][][];//1st index
        int level;
        int layer;
        string layerData;
        string[] linesDatas;//divided by \n
        string[] singleLineData;//divided by \t
        int rowSize;//max row number(number of lines)
        int colSize;//max col number(depand on first line£©
        StreamReader sr;
        FileInfo[] layerFiles;
        //read every level
        foreach (DirectoryInfo mapFolder in mapsOfLevelsPathInfos) {
            int.TryParse(mapFolder.Name, out level);//Level assigned
            layerFiles = mapFolder.GetFiles("*.txt");//no meta file
            mapDatasArray[level] = new BlockTypeEnum[layerFiles.Length][][];//2nd index
            firstBornIndex = new Vector3[layerFiles.Length];
            //read every layer
            foreach (FileInfo layerFile in layerFiles) {
                string realLayerName = layerFile.Name.Substring(0, layerFile.Name.Length - 4);
                int.TryParse(realLayerName, out layer);//Layer assigned
                //read text
                sr = layerFile.OpenText();
                layerData = sr.ReadToEnd();
                sr.Close();
                //analysis text
                linesDatas = layerData.Split(new char[] { '\n' });
                rowSize = linesDatas.Length - 1;//there would be an empty line at the end
                colSize = (linesDatas[0].Length + 1) / 2;
                mapDatasArray[level][layer] = new BlockTypeEnum[rowSize][];
                //read every line
                for (int row = 0; row < rowSize; row++) {
                    mapDatasArray[level][layer][row] = new BlockTypeEnum[colSize];//3rd index
                    singleLineData = linesDatas[row].Split(new char[] { '\t' });
                    //read every item
                    for (int col = 0; col < colSize; col++) {
                        if (col < singleLineData.Length) {
                            mapDatasArray[level][layer][row][col] = blockTypeManager.transNumToType(singleLineData[col][0]);
                            if (singleLineData[col][0] == '-') {//set first born place
                                firstBornIndex[level] = new Vector3(layer, col, row);
                            }
                        } else {//be empty default
                            mapDatasArray[level][layer][row][col] = BlockTypeEnum.empty;
                        }
                    }
                }
            }
        }
    }

    //called after save loaded
    public void handleForeverChangedBlockInMap() {
        foreach (BlockIndexMessage bIM in saveDataManager.currentSave.blockDestroyedForeverList) {
            mapDatasArray[bIM.level][bIM.layer][bIM.row][bIM.col] = BlockTypeEnum.empty;
        }
    }

    private int maxNumberOfLevels;

    private Vector3[] firstBornIndex;

    // Start is called before the first frame update

    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }



    //get
    public int getMaxNumberOfLevels() {
        return maxNumberOfLevels;
    }
    //get a level's map data
    public BlockTypeEnum[][][] getMapData(int level) {
        return mapDatasArray[level];
    }
    public Vector3 getFirstBornIndex(int level) {
        return firstBornIndex[level];
    }
    public Vector3 getFirstBornPlace(int level) {
        Vector3 fbi = getFirstBornIndex(level);
        float x = fbi.y * ml.blockLength;
        float y = fbi.x * ml.blockLength;//use layer
        float z = -fbi.z * ml.blockLength;
        return new Vector3(x, y, z);
    }
}


//one map,one txt
//private void mapDatasInitialization_1() {
//    numberOfLevels = mapDataFiles.Length;
//    mapDatasArray = new int[numberOfLevels][][][];

//    //trans text to int array
//    string text;
//    string[] linesData;
//    string certainLineData;
//    List<string[][]> singlesDataList;
//    string[][] singlesData;//save in singlesDataList
//    int mapSize;
//    int layer;
//    int lineInLayer;
//    for (int level = 0; level < numberOfLevels; level++) {
//        //get all data
//        text = mapDataFiles[level].text;
//        //Debug.Log(text);
//        //divide lines
//        linesData = text.Split(new char[] { '\n' });
//        //Debug.Log("line number: " + linesData.Length);
//        //for (int i = 0; i < linesData.Length; i++) {
//        //    Debug.Log(">>>" + linesData[i]);
//        //}
//        //calculate map size
//        mapSize = (linesData[0].Length + 1) / 2;//concern '\t'
//        if (mapSize % 2 == 0) {//change to odd number
//            mapSize++;
//        }
//        singlesData = new string[mapSize][];
//        //start trans
//        singlesDataList = new List<string[][]>();
//        layer = 0;
//        lineInLayer = 0;
//        for (int line = 0; line < linesData.Length; line++) {
//            certainLineData = linesData[line];
//            if (certainLineData[0] == '/') {    //change layer
//                Debug.Log("change Layer");
//                singlesDataList.Add(singlesData);//save data in list
//                singlesData = new string[mapSize][];//deep clean
//                layer++;
//                lineInLayer = 0;
//            } else {
//                singlesData[lineInLayer++] = certainLineData.Split(new char[] { '\t' });
//            }
//        }
//        int maxLayer = singlesDataList.Count;
//        Debug.Log("maxLayer: " + maxLayer);
//        //init&assign mapDatasArray
//        mapDatasArray[level] = new int[maxLayer][][];
//        for (layer = 0; layer < maxLayer; layer++) {
//            mapDatasArray[level][layer] = new int[mapSize][];
//            singlesData = singlesDataList[layer];//get a layer's data
//            for (lineInLayer = 0; lineInLayer < mapSize; lineInLayer++) {
//                mapDatasArray[level][layer][lineInLayer] = new int[mapSize];
//                for (int i = 0; i < mapSize; i++) {
//                    if (i < singlesData[lineInLayer].Length) {  //trans string(char) to int
//                        mapDatasArray[level][layer][lineInLayer][i] = singlesData[lineInLayer][i][0] - '0';
//                    } else {    //not set, consider as empty
//                        mapDatasArray[level][layer][lineInLayer][i] = -1;
//                    }
//                }
//            }
//        }
//    }
//}
