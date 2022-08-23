using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//
//base class of buffs
public abstract class BuffBase {
    //basic attributes.Given in device class's constructor,using setBasicAttributes.
    protected BuffKindEnum buffKind;
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
        buffLogic.currentBuff[buffSpecies] = null;
    }

    //other override
    public override string ToString() {
        return buffName;
    }



    //get
    public BuffKindEnum getBuffKind() {
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
        buffKind = BuffKindEnum.cureBuff;
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
    private float speedMultiplying = 1.05f;
    private float acMultiplying = 1.1f;
    private float hurtSpeed = 12f;

    private BloodLogic bloodLogic;

    private float totalHurt=0;

    public OnFireBuff(float buffDuration, BuffLogic buffLogic) : base(buffDuration, buffLogic) {
        buffKind = BuffKindEnum.onFireBuff;
        buffName = "OnFire Buff";
        buffPriority = 0;
        //
        bloodLogic = GameObject.Find("/Controllers/BloodController").GetComponent<BloodLogic>();
    }

    public override void onAdd() {
        base.onAdd();
        buffLogic.ballLogic.changeBallType(2);
        buffLogic.ballLogic.maxMoveSpeed *= speedMultiplying;
        buffLogic.ballLogic.baseForce *= acMultiplying;
    }
    public override void onUpdate() {
        base.onUpdate();
        //hurt
        totalHurt += hurtSpeed * Time.deltaTime;
        if (totalHurt >= 1) {
            bloodLogic.reduceBlood((int)totalHurt);
            totalHurt -= (int)totalHurt;
        }
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
        buffKind = BuffKindEnum.freezedBuff;
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

