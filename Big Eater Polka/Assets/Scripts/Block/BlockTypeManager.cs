using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTypeManager : MonoBehaviour
{
    [Tooltip("The material of each block type.")]
    public Material[] materialOfBlockTypes;

    //here to deside which char means which type
    public BlockTypeEnum transStrToType(string blockTypeStr) {
        switch (blockTypeStr) {
            case "d":
                return BlockTypeEnum.defaultType;
            case "reborn":
                return BlockTypeEnum.rebornType;
            case "jump":
                return BlockTypeEnum.jumpType;
            case "cure":
                return BlockTypeEnum.cureType;
            case "fire":
                return BlockTypeEnum.fireType;
            case "ice":
                return BlockTypeEnum.iceType;
            case "owdE":
                return BlockTypeEnum.onewayDoor_E;
            case "owdS":
                return BlockTypeEnum.onewayDoor_S;
            case "owdW":
                return BlockTypeEnum.onewayDoor_W;
            case "owdN":
                return BlockTypeEnum.onewayDoor_N;
            case "wood":
                return BlockTypeEnum.woodBoxType;
            default:
                return BlockTypeEnum.empty;
        }
    }

    public Material transTypeToMaterial(BlockTypeEnum blockType) {
        return transMaterialDic.TryGetValue(blockType, out Material mat) ? mat : null;
    }

    //here to deside which type use which material
    private void initTransMaterialDic() {
        transMaterialDic = new Dictionary<BlockTypeEnum, Material>();
        transMaterialDic.Add(BlockTypeEnum.defaultType, materialOfBlockTypes[0]);
        transMaterialDic.Add(BlockTypeEnum.rebornType, materialOfBlockTypes[1]);
        transMaterialDic.Add(BlockTypeEnum.jumpType, materialOfBlockTypes[2]);
        transMaterialDic.Add(BlockTypeEnum.cureType, materialOfBlockTypes[3]);
        transMaterialDic.Add(BlockTypeEnum.fireType, materialOfBlockTypes[4]);
        transMaterialDic.Add(BlockTypeEnum.iceType, materialOfBlockTypes[5]);
        transMaterialDic.Add(BlockTypeEnum.onewayDoor_E, materialOfBlockTypes[6]);
        transMaterialDic.Add(BlockTypeEnum.onewayDoor_S, materialOfBlockTypes[6]);
        transMaterialDic.Add(BlockTypeEnum.onewayDoor_W, materialOfBlockTypes[6]);
        transMaterialDic.Add(BlockTypeEnum.onewayDoor_N, materialOfBlockTypes[6]);
        transMaterialDic.Add(BlockTypeEnum.woodBoxType, materialOfBlockTypes[7]);
    }

    private Dictionary<BlockTypeEnum, Material> transMaterialDic;

    private void Awake() {
        initTransMaterialDic();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
