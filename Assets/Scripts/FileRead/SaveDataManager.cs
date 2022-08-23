using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Cysharp.Threading.Tasks;

public class SaveDataManager : MonoBehaviour
{
    public string savePath;

    public SaveStructure currentSave;

    [Space]

    public bool isAutoSave;
    public float timeAutoSave=30;

    [Space]

    public float activeSaveCDLength = 2;
    public float activeSaveCDRest = 0;

    [Space]

    public MainLogic mainLogic;
    public MapDataManager mapDataManager;
    public BlockDestroyManager blockDestroyManager;

    //
    //save data functions
    //save
    public async void saveBySerialization() {
        await UniTask.SwitchToThreadPool();
        Debug.Log("Saving...");
        BinaryFormatter bf=new();
        FileStream fs = File.Create(savePath);
        bf.Serialize(fs, currentSave);
        fs.Close();
    }
    //load
    public void loadByDeserialization() {
        Debug.Log("Loading...");
        if (File.Exists(savePath)) {
            BinaryFormatter bf = new();
            FileStream fs = File.Open(savePath, FileMode.Open);
            currentSave = bf.Deserialize(fs) as SaveStructure;
            fs.Close();
            buildSave();
        } else {//if no save
            rebuildSave();
        }
    }

    private BallLogic ballLogic;

    // Start is called before the first frame update
    void Start()
    {
        autoSaveAsync();

    }

    // Update is called once per frame
    void Update()
    {
        //save cd
        if (activeSaveCDRest > 0) {
            activeSaveCDRest -= Time.deltaTime;
        }

    }

    //after load
    private void buildSave() {
        blockDestroyManager.blockDestroyedForeverList = currentSave.blockDestroyedForeverList;
        blockDestroyManager.blockDestroyedOneWayDoorList = currentSave.blockDestroyedOneWayDoorList;
        mapDataManager.handleForeverChangedBlockInMap();
    }
    //restart
    private void rebuildSave() {
        currentSave = new SaveStructure();
        currentSave.setRebornBlockPos(mapDataManager.getFirstBornPlace(0) * mainLogic.getBlockLength());
        currentSave.blockDestroyedForeverList = new List<BlockIndexMessage>();
        currentSave.blockDestroyedOneWayDoorList = new List<BlockIndexMessage>();
        currentSave.rebornBlockTouchedSet = new HashSet<BlockIndexMessage>();
        buildSave();
    }

    private async void autoSaveAsync() {
        while (true) {
            await UniTask.Delay(System.TimeSpan.FromSeconds(timeAutoSave), true);
            if (isAutoSave) {
                saveBySerialization();
            }
        }
    }



    //get
    public SaveStructure getCurrentSave() {
        return currentSave;
    }
}
