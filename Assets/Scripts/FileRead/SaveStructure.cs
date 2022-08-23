using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//
//choose what to save
[System.Serializable]
public class SaveStructure {
    public int level;

    //reborn local position. Maybe should be int
    public float rebornBlockPos_X;
    public float rebornBlockPos_Y;
    public float rebornBlockPos_Z;

    //block change forever message
    public List<BlockIndexMessage> blockDestroyedForeverList;
    public List<BlockIndexMessage> blockDestroyedOneWayDoorList;

    //reborn ball touched message
    public HashSet<BlockIndexMessage> rebornBlockTouchedSet;

    public SaveStructure() { }



    //get
    public Vector3 getRebornBlockPos() {
        return new Vector3(rebornBlockPos_X, rebornBlockPos_Y, rebornBlockPos_Z);
    }
    public HashSet<BlockIndexMessage> getRebornBlockTouchedSet() {
        return rebornBlockTouchedSet;
    }

    //set
    public void setRebornBlockPos(Vector3 rebornBlockPos) {
        rebornBlockPos_X = rebornBlockPos.x;
        rebornBlockPos_Y = rebornBlockPos.y;
        rebornBlockPos_Z = rebornBlockPos.z;
    }
}
