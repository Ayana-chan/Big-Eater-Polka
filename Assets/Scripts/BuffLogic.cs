using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffLogic : MonoBehaviour {
    [Tooltip("Buff in every species")]
    public BuffBase[] currentBuff;

    [Tooltip("Max species.Buffs in the same species is mutually exclusive")]
    public int buffSpeciesNumber = 2;

    [Space]

    public MainLogic ml;
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
    public void clearCurrentBuff(BuffBase buff) {
        currentBuff[buff.getBuffSpecies()] = null;
    }


    // Start is called before the first frame update
    void Start() {
        ballLogic = ml.getBall().GetComponent<BallLogic>();//dangerous get

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

}

//
//
//all buff should be listed here-------------------
public enum BuffKind {
    //species 0: from block
    //priority 0
    cureBuff,
    onFireBuff,
    freezedBuff,
    //species 1
}


//
//
//base class of buffs
public abstract class BuffBase {
    //basic attributes.Given in device class's constructor,using setBasicAttributes.
    protected BuffKind buffKind;
    protected string buffName;
    protected int buffSpecies;
    protected int buffPriority;

    protected bool isTimeLimited;//if false, buff stay until be replaced
    protected float timer;

    protected float buffDuration;

    protected BuffLogic buffLogic;//buff control center

    public BuffBase(float buffDuration, BuffLogic buffLogic) {
        if (buffDuration >= 0) {    //buffDuration<0 means no limit time
            this.buffDuration = buffDuration;
            isTimeLimited = true;
        }
        this.buffLogic = buffLogic;
    }

    //important functions
    public virtual void onAdd() {//when added.Can also handle priority
        Debug.Log("Buff" + buffSpecies + ": " + this + " added.");
        buffLogic.changeCurrentBuff(this);//this function can also call onRemove
    }
    public virtual void onUpdate() {//when FixedUpdate
        if (isTimeLimited) {
            timer += Time.fixedDeltaTime;
            if (timer >= buffDuration) {
                onRemove();
            }
        }
    }
    public virtual void onRemove() {//when time out or cleared
        buffLogic.clearCurrentBuff(this);
    }

    //other override
    public override string ToString() {
        return buffName;
    }



    //get
    public BuffKind getBuffKind() {
        return buffKind;
    }
    public int getBuffSpecies() {
        return buffSpecies;
    }
    public int getBuffPriority() {
        return buffPriority;
    }
    public float getBuffDuration() {
        return buffDuration;
    }
    public float getBuffRemainingTime() {
        return buffDuration - timer;
    }
}

//
//
//species 0 buffs' base class
public class BuffSpecies_0 : BuffBase {
    public BuffSpecies_0(float buffDuration, BuffLogic buffLogic) : base(buffDuration, buffLogic) {
        buffSpecies = 0;
    }
}

//
//species 0 buffs
public class CureBuff : BuffSpecies_0 {
    public CureBuff(float buffDuration, BuffLogic buffLogic) : base(buffDuration, buffLogic) {
        buffKind = BuffKind.cureBuff;
        buffName = "Cure Buff";
        buffPriority = 0;
    }

    public override void onAdd() {
        base.onAdd();
        buffLogic.ballLogic.changeBallType(1);
        //...
    }
    public override void onRemove() {
        base.onRemove();
        buffLogic.ballLogic.changeBallType(0);
        //...
    }

}

public class OnFireBuff : BuffSpecies_0 {
    private float speedMultiplying = 1.2f;
    private float acMultiplying = 1.2f;

    public OnFireBuff(float buffDuration, BuffLogic buffLogic) : base(buffDuration, buffLogic) {
        buffKind = BuffKind.onFireBuff;
        buffName = "OnFire Buff";
        buffPriority = 0;
    }

    public override void onAdd() {
        base.onAdd();
        buffLogic.ballLogic.changeBallType(2);
        buffLogic.ballLogic.maxMoveSpeed *= speedMultiplying;
        buffLogic.ballLogic.baseForce *= acMultiplying;
    }
    public override void onRemove() {
        base.onRemove();
        buffLogic.ballLogic.changeBallType(0);
        buffLogic.ballLogic.maxMoveSpeed /= speedMultiplying;
        buffLogic.ballLogic.baseForce /= acMultiplying;
    }
}

public class FreezedBuff : BuffSpecies_0 {
    private float speedMultiplying = 0.7f;
    private float acMultiplying = 0.8f;

    public FreezedBuff(float buffDuration, BuffLogic buffLogic) : base(buffDuration, buffLogic) {
        buffKind = BuffKind.freezedBuff;
        buffName = "Freezed Buff";
        buffPriority = 0;
    }

    public override void onAdd() {
        base.onAdd();
        buffLogic.ballLogic.changeBallType(3);
        buffLogic.ballLogic.maxMoveSpeed *= speedMultiplying;
        buffLogic.ballLogic.baseForce *= acMultiplying;
    }
    public override void onRemove() {
        base.onRemove();
        buffLogic.ballLogic.changeBallType(0);
        buffLogic.ballLogic.maxMoveSpeed /= speedMultiplying;
        buffLogic.ballLogic.baseForce /= acMultiplying;
    }
}
