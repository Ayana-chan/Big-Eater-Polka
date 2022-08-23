using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebornBlockParticleLogic : MonoBehaviour
{
    //[Tooltip("Manage all particle of reborn block")]
    //public List<GameObject> rebornBlockParticleList = new List<GameObject>();
    //[Tooltip("Saved in save")]
    //public HashSet<BlockIndexMessage> rebornBlockTouchedSet;
    public GameObject currentRestPar;

    [Space]

    public GameObject rebornBlockParticlePrefab;

    [Space]

    [Header("Default")]
    //public ParticleSystem.MinMaxGradient defaultColor;
    public Color defaultColor;
    public ParticleSystem.MinMaxCurve defaultStartSpeed;
    [Header("RestedBefore")]
    //public ParticleSystem.MinMaxGradient touchedColor;
    public Color touchedColor;
    public ParticleSystem.MinMaxCurve touchedStartSpeed;
    [Header("CurrentRest")]
    //public ParticleSystem.MinMaxGradient currentRestColor;
    public Color currentRestColor;
    public ParticleSystem.MinMaxCurve currentRestStartSpeed;

    [Space]

    public MainLogic mainLogic;
    public SaveDataManager saveDataManager;

    //called when create road
    public void createRebornBlockParticle(GameObject rebornBlock) {
        var currentSave = saveDataManager.getCurrentSave();
        var rebornBlockTouchedSet = currentSave.getRebornBlockTouchedSet();
        //create par
        GameObject nPar= Instantiate(rebornBlockParticlePrefab,rebornBlock.transform);
        nPar.name = "RebornBlockParticle";
        //rebornBlockParticleList.Add(nPar);//add to list
        nPar.transform.localPosition = (mainLogic.getBlockLength() / 2 - 0.1f) * Vector3.up;
        Vector3 rebornBlockPos = currentSave.getRebornBlockPos();
        var rebornBlockIndexMessage = rebornBlock.GetComponent<BlockLogic>().getBlockIndexMessage();
        //judge and change style
        if (rebornBlock.transform.localPosition == rebornBlockPos) {    //is CurrentRest
            changeStyle_CurrentRest(nPar);
            currentRestPar = nPar;
            //avoid first block not touched
            rebornBlockTouchedSet.Add(rebornBlockIndexMessage);
        } else if (rebornBlockTouchedSet.Contains(rebornBlockIndexMessage)) {  //touched
            changeStyle_Touched(nPar);
        } else {    //never touched
            changeStyle_Default(nPar);
        }
    }

    public void firstTouchRebornBlock(GameObject block) {
        GameObject par = block.transform.Find("RebornBlockParticle").gameObject;
        changeStyle_Touched(par);
    }

    public void restAtRebornBlock(GameObject block) {
        GameObject par = block.transform.Find("RebornBlockParticle").gameObject;
        //rest in the same place
        if (currentRestPar == par) {
            return;
        }
        //remove last
        if (currentRestPar != null) {
            changeStyle_Touched(currentRestPar);
        }
        currentRestPar = par;
        changeStyle_CurrentRest(par);
    }

    //public void changeParticleStyle(GameObject block, RebornBlockParticleStyleEnum style) {
    //    GameObject par = transform.Find("RebornBlockParticle").gameObject;
    //    switch (style) {
    //        case RebornBlockParticleStyleEnum.defaultStyle:
    //            changeStyle_Default(par);
    //            break;
    //        case RebornBlockParticleStyleEnum.touchedStyle:
    //            changeStyle_Touched(par);
    //            break;
    //        case RebornBlockParticleStyleEnum.CurrentRestStyle:
    //            changeStyle_CurrentRest(par);
    //            break;
    //    }

    //}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //never rested
    private void changeStyle_Default(GameObject par) {
        ParticleSystem parSystem = par.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = parSystem.main;
        mainModule.startColor=defaultColor;
        mainModule.startSpeed = defaultStartSpeed;
    }

    //have touched
    private void changeStyle_Touched(GameObject par) {
        ParticleSystem parSystem=par.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = parSystem.main;
        mainModule.startColor = touchedColor;
        mainModule.startSpeed = touchedStartSpeed;
    }

    //last rest place
    private void changeStyle_CurrentRest(GameObject par) {
        ParticleSystem parSystem = par.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = parSystem.main;
        mainModule.startColor = currentRestColor;
        mainModule.startSpeed = currentRestStartSpeed;
    }
}

public enum RebornBlockParticleStyleEnum {
    defaultStyle,
    touchedStyle,
    CurrentRestStyle,
}
