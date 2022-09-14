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

    public bool isMulBlockCoverUp=false;

    [Space]

    public MainLogic ml;
    public SaveDataManager saveDataManager;
    public BlockTypeManager blockTypeManager;

    //one layer,one txt
    public void mapDatasInitialization_2() {
        int level;
        int layer;
        int row;
        int col;
        string[] layerDatas;
        string[] linesDatas;//divided by \n
        string[] singleDatas;
        string[][][][] stringDatas;//divided by \t
        int rowSize;//max row number(number of lines)
        int colSize;//max col number(depand on first line£©
        StreamReader sr;
        FileInfo[][] layerFiles;
        DirectoryInfo mapDatasPathInfo = new DirectoryInfo(mapDatasPath);
        DirectoryInfo[] mapsOfLevelsPathInfos = mapDatasPathInfo.GetDirectories();

        maxNumberOfLevels = mapsOfLevelsPathInfos.Length;
        //1st index
        stringDatas = new string[maxNumberOfLevels][][][];

        layerFiles = new FileInfo[maxNumberOfLevels][];
        firstBornIndex = new Vector3[maxNumberOfLevels];

        //Read And Parse
        //read every level
        //read file
        foreach (DirectoryInfo mapFolder in mapsOfLevelsPathInfos) {
            if (!int.TryParse(mapFolder.Name, out level)) {
                Debug.LogError("ERROR: Unexpected Level File Name.");
                continue;
            }
            //Level assigned
            layerFiles[level] = mapFolder.GetFiles("*.txt");//no meta file
            //2nd index
            stringDatas[level] = new string[layerFiles[level].Length][][];
        }
        //read from low to high
        for (level = 0; level < maxNumberOfLevels; level++) {
            //read every layer
            layerDatas = new string[layerFiles[level].Length];
            //read file
            foreach (FileInfo layerFile in layerFiles[level]) {
                string realLayerName = layerFile.Name.Substring(0, layerFile.Name.Length - 4);
                int.TryParse(realLayerName, out layer);//Layer assigned
                //read text
                sr = layerFile.OpenText();
                layerDatas[layer] = sr.ReadToEnd();
                sr.Close();
            }
            //read from low to high
            for (layer = 0; layer < layerFiles[level].Length; layer++) {
                //analysis text
                linesDatas = layerDatas[layer].Split(new char[] { '\n' });
                rowSize = linesDatas.Length - 1;//there would be an empty line at the end
                //3rd index
                stringDatas[level][layer] = new string[rowSize][];
                //read every line
                for (row = 0; row < rowSize; row++) {
                    singleDatas = linesDatas[row].Split(new char[] { '\t', '\n', '\r' });
                    colSize = singleDatas.Length - 1;//last must be empty
                    //4th index
                    stringDatas[level][layer][row] = singleDatas;
                }
            }
        }

        //Analyse strings
        for (level = 0; level < stringDatas.Length; level++) {
            for (layer = 0; layer < stringDatas[level].Length; layer++) {
                for (row = 0; row < stringDatas[level][layer].Length; row++) {
                    for (col = 0; col < stringDatas[level][layer][row].Length; col++) {
                        //special handle
                        //set first born place
                        if (stringDatas[level][layer][row][col] == "-") {
                            firstBornIndex[level] = new Vector3(layer, col, row);//exchange y&z
                            stringDatas[level][layer][row][col] = "reborn";
                        }
                        //multiple blocks
                        if (judgeMultiple(stringDatas[level][layer][row][col])) {
                            handleMul(stringDatas, level, layer, row, col);
                        }
                    }
                }
            }
        }

        //Trans to type
        mapDatasArray = new BlockTypeEnum[stringDatas.Length][][][];
        for (level = 0; level < stringDatas.Length; level++) {
            mapDatasArray[level] = new BlockTypeEnum[stringDatas[level].Length][][];
            for (layer = 0; layer < stringDatas[level].Length; layer++) {
                mapDatasArray[level][layer] = new BlockTypeEnum[stringDatas[level][layer].Length][];
                for (row = 0; row < stringDatas[level][layer].Length; row++) {
                    mapDatasArray[level][layer][row] = new BlockTypeEnum[stringDatas[level][layer][row].Length];
                    for (col = 0; col < stringDatas[level][layer][row].Length; col++) {
                        mapDatasArray[level][layer][row][col] = blockTypeManager.transStrToType(stringDatas[level][layer][row][col]);
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

    /// <summary>
    /// means some the same block on it
    /// </summary>
    /// <returns></returns>
    private bool judgeMultiple(string s) {
        if (s.Length > 1 && s[0]=='m' && judgeDigit(s[1])) {
            return true;
        }
        return false;
    }

    private bool judgeDigit(char c) {
        if(c >= '0' && c <= '9') {
            return true;
        }
        return false;
    }

    private void handleMul(string[][][][] stringDatas,int level,int layer,int row,int col) {
        int startMul = 1;
        string mulNumStr = "";
        int mulNum;//if 1, just like no mul
        while (startMul < stringDatas[level][layer][row][col].Length && judgeDigit(stringDatas[level][layer][row][col][startMul])) {
            mulNumStr += stringDatas[level][layer][row][col][startMul];
            startMul++;
        }
        string mulS = stringDatas[level][layer][row][col].Substring(startMul);
        int.TryParse(mulNumStr, out mulNum);
        //change stringDatas
        stringDatas[level][layer][row][col] = "";
        //need more layer
        if (layer + mulNum > stringDatas[level].Length) {
            string[][][] temp = new string[layer + mulNum][][];
            string[][] t0 = new string[0][];//empty basic array
            for (int l = 0; l < stringDatas[level].Length; l++) {
                temp[l] = stringDatas[level][l];
            }
            for (int l = stringDatas[level].Length; l < layer + mulNum; l++) {
                temp[l] = t0;//default empty
            }
            stringDatas[level] = temp;
        }
        for (int aimLayer = layer; aimLayer < layer + mulNum; aimLayer++) {
            //need more row;
            if (stringDatas[level][aimLayer].Length <= row) {
                string[][] temp = new string[row + 1][];
                for (int r = 0; r < stringDatas[level][aimLayer].Length; r++) {
                    temp[r] = stringDatas[level][aimLayer][r];
                }
                for (int r = stringDatas[level][aimLayer].Length; r <= row; r++) {
                    temp[r] = new string[0];
                }
                stringDatas[level][aimLayer] = temp;
            }
            //need more col
            if (stringDatas[level][aimLayer][row].Length <= col) {
                string[] temp = new string[col + 1];
                for (int c = 0; c < stringDatas[level][aimLayer][row].Length; c++) {
                    temp[c] = stringDatas[level][aimLayer][row][c];
                }
                for (int c = stringDatas[level][aimLayer][row].Length; c <= col; c++) {
                    temp[c] = "";
                }
                stringDatas[level][aimLayer][row] = temp;
            }
            if (isMulBlockCoverUp || stringDatas[level][aimLayer][row][col] == "") {
                //assign
                stringDatas[level][aimLayer][row][col] = mulS;
            }
        }
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
