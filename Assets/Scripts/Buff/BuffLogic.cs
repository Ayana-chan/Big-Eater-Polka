using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffLogic : MonoBehaviour {
    [Tooltip("Buff in every species")]
    public BuffBase[] currentBuff;

    [Tooltip("Max species.Buffs in the same species is mutually exclusive")]
    public int buffSpeciesNumber = 2;

    [Space]

    public MainLogic mainLogic;
    [HideInInspector]
    public BallLogic ballLogic;

    public void blockCollisionEvent_index_0(GameObject block) {
        BlockLogic bl = block.GetComponent<BlockLogic>();
        BlockTypeEnum blockType = bl.getBlockType();
        BuffBase nBuff;
        switch (blockType) {
            case BlockTypeEnum.defaultType:
                //nothing?
                break;
            case BlockTypeEnum.rebornType:
                    //clear
                    //发个光
                    //无敌

                break;
            case BlockTypeEnum.jumpType:
                //特效

                break;
            case BlockTypeEnum.cureType:
                if (bl.isFormalDestoryed || block.GetComponent<Light>().enabled==false) {
                    return; //one rest, one cure
                }
                nBuff = new CureBuff(3, this);
                nBuff.onAdd();
                break;
            case BlockTypeEnum.fireType:
                nBuff = new OnFireBuff(-1, this);
                nBuff.onAdd();
                break;
            case BlockTypeEnum.iceType:
                nBuff = new FreezedBuff(-1, this);
                nBuff.onAdd();
                //减速？让水结冰？
                break;
        }
    }

    //current buff functions
    public void changeCurrentBuff(BuffBase nBuff) {
        int species = nBuff.getBuffSpecies();
        BuffBase cBuff = currentBuff[species];
        if (cBuff != null) {//clear previous buff
            currentBuff[species].onRemove();
        }
        currentBuff[species] = nBuff;
    }

    public void clearAllBuff() {
        for(int i=0;i<currentBuff.Length;i++) {
            if(currentBuff[i]!= null) {
                currentBuff[i].onRemove();
            }
        }
    }


    // Start is called before the first frame update
    void Start() {
        ballLogic = mainLogic.getBall().GetComponent<BallLogic>();//dangerous get

        currentBuff = new BuffBase[buffSpeciesNumber];

    }

    // Update is called once per frame
    void Update() {

    }

    private void FixedUpdate() {
        //buffs' update
        for (int i = 0; i < currentBuff.Length; i++) {
            if (currentBuff[i] != null) {
                currentBuff[i].onUpdate();
            }
        }
    }



    //get
    public BuffKindEnum getCurrentBuffKind(int species) {
        if (species >= currentBuff.Length) {
            Debug.LogError("ERROR: Unexpected Buff Species.");
            return BuffKindEnum.noBuff;
        }
        if (currentBuff[species] == null) {
            return BuffKindEnum.noBuff;
        }
        return currentBuff[species].getBuffKind();
    }
}
