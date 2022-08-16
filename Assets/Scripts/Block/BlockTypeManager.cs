using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTypeManager : MonoBehaviour
{
    [Tooltip("The material of each block type.")]
    public Material[] materialOfBlockTypes;

    //here to deside which char means which type
    public BlockTypeEnum transNumToType(char blockTypeNum) {
        switch (blockTypeNum) {
            case '0':
                return BlockTypeEnum.defaultType;
            case '1':
                return BlockTypeEnum.rebornType;
            case '-'://first reborn place
                return BlockTypeEnum.rebornType;
            case '2':
                return BlockTypeEnum.jumpType;
            case '3':
                return BlockTypeEnum.cureType;
            case '4':
                return BlockTypeEnum.fireType;
            case '5':
                return BlockTypeEnum.iceType;
            case 'a':
                return BlockTypeEnum.onewayDoor_E;
            case 'b':
                return BlockTypeEnum.onewayDoor_S;
            case 'c':
                return BlockTypeEnum.onewayDoor_W;
            case 'd':
                return BlockTypeEnum.onewayDoor_N;
            default:
                return BlockTypeEnum.empty;
        }
    }

    public Material transTypeToMaterial(BlockTypeEnum blockType) {
        return transMaterialDic.TryGetValue(blockType, out Material mat) ? mat : null;
    }

    //here to deside which type use which material
    public void initTransMaterialDic() {
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
    }

    private Dictionary<BlockTypeEnum, Material> transMaterialDic;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum BlockTypeEnum {
    empty,//no block
    defaultType,
    rebornType,
    jumpType,
    cureType,
    fireType,
    iceType,
    onewayDoor_E,
    onewayDoor_S,
    onewayDoor_W,
    onewayDoor_N,
}
