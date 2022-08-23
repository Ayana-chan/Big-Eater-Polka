using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

public class MainLogic : MonoBehaviour {
    public int level;
    [Tooltip("The length of each blocks' edge")]
    public float blockLength = 1;
    public float ballDiameter = 0.8f;

    [Tooltip("The delay between load UI and really load objects")]
    public float reallyLoadObjectDelayTime = 1.2f;

    [Space]

    [Header("Main bools")]
    [Tooltip("Press key to start")]
    public bool isStart = false;
    [Tooltip("Can only change via set. Delay to be false")]
    public bool isLoading=true;
    public bool isInControl=false;

    [Space]

    [Tooltip("PolkaBall's prefab")]
    public GameObject ballPrefab;

    public SaveDataManager saveDataManager;
    public MapDataManager mapDataManager;
    public UI_BarLogic ui_loadLogic;
    public GameObject mainCamera;
    public GameObject gamePlace;
    public RoadCreator roadCreator;
    public BlockTypeManager blockTypeManager;
    public BlockDestroyManager blockDestroyManager;
    public RestLogic restLogic;

    //A function for many movement
    //f(x)=2S/T^2 * x^2
    public static float currencyMovementFunction_1(float t, float rightBound, float deltaY) {
        //default left bound is 0
        float mid = rightBound / 2;
        if (t <= 0) {
            return 0;
        }
        if (t <= mid) {
            float ans = 2 * deltaY / rightBound / rightBound * t * t;
            return ans;
        }
        if (t < rightBound) {
            return deltaY - currencyMovementFunction_1(rightBound - t, rightBound, deltaY);
        }
        return deltaY;//wouldn't be a unexpected value.Just judge when to escape this movement(t>=rightBound).
    }

    /// <summary>
    /// Two load functions
    /// auto set ball grivaty true when load complete
    /// </summary>
    public void loadAfterNewMap() {
        isInControl = false;
        setIsLoading(true);
        ui_loadLogic.setLoadingValueMode(0);
        roadCreationAsync(reallyLoadObjectDelayTime);
    }

    public void loadAfterRest() {
        isInControl = false;
        setIsLoading(true);
        ui_loadLogic.setLoadingValueMode(1);
        blockResetAsync(reallyLoadObjectDelayTime);
    }

    GameObject ball;
    BallLogic ballLogic;
    CameraLogic cameraLogic;

    // Start is called before the first frame update
    void Start() {
        Debug.Log("<<Big Eater Polka>> Start. Produced by DeathWind.");
        Application.targetFrameRate = 60;

        cameraLogic = mainCamera.GetComponent<CameraLogic>();

        //init data
        mapDataManager.mapDatasPath = Application.dataPath + "/Data/MapDatas";
        mapDataManager.mapDatasInitialization_2();
        saveDataManager.savePath = Application.dataPath + "/Data/save.sa";
        saveDataManager.loadByDeserialization();

        //StartCoroutine(afterLoadingIEnumerator());//read progress and deside how to take control
        afterLoading();

        level = 0;

        initBall();

    }

    // Update is called once per frame
    void Update() {
        //
        //keys
        //
        //Start functions
        //press Space to start
        if (!isStart && Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Start Playing!");
            isStart = true;
            ui_loadLogic.showBar();//init bar
            loadAfterNewMap();
        }
        //press P to restart (before start)--------------------//要归零level，重新造路，摄像头重造。待开发。
        //if (!isStart && Input.GetKeyDown(KeyCode.P)) {
        //    Debug.Log("Restart Playing!");
        //    Destroy(ball);
        //    saveDataManager.rebuildSave();
        //    createBall();
        //}
        //press Z to active save
        if (Input.GetKeyDown(KeyCode.Z) && isInControl) {
            restLogic.rest();
        }
        //
        //save quit functions
        //press F1 to save
        if (Input.GetKeyDown(KeyCode.F1)) {//也许要禁止？
            Debug.Log("Save.");
            saveDataManager.saveBySerialization();
        }
        //press Backspace to save and quit
        if (Input.GetKeyDown(KeyCode.Backspace)) {//也许要禁止？
            Debug.Log("Save and Quit.");
            saveDataManager.saveBySerialization();
            quitGame();//----------没保存完就退出了
        }
        //press F5 to quit without save
        if (Input.GetKeyDown(KeyCode.F5)) {
            Debug.Log("Quit without Save.");
            quitGame();
        }
    }

    private void initBall() {
        ball = GameObject.Find("/GamePlace/PlayerBall");
        ball.transform.localScale = new Vector3(ballDiameter, ballDiameter, ballDiameter);
        float ballBornHeight = 0.01f + saveDataManager.currentSave.rebornBlockPos_Y + blockLength / 2 + ballDiameter / 2;
        ball.transform.localPosition = new Vector3(saveDataManager.currentSave.getRebornBlockPos().x,ballBornHeight, saveDataManager.currentSave.getRebornBlockPos().z);
        //call camera to follow ball
        cameraLogic.setTarget(ball);
        ballLogic = ball.GetComponent<BallLogic>();
    }

    private void quitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    //
    //read progress and deside how to take control. Active any time.
    private async void afterLoading() {
        while (true) {
            await UniTask.Yield();
            await UniTask.WaitUntil(() => isStart && isLoading && ui_loadLogic.judgeLoadCompelete());
            //ready
            switch (ui_loadLogic.loadingValueMode) {
                case 0:
                    leaveLoadingDelayAsync(1.5f);
                    giveControlDelayAsync(2.5f);
                    break;
                case 1:
                    leaveLoadingDelayAsync(0.8f);
                    giveControlDelayAsync(1.8f);
                    break;
                default:
                    Debug.LogError("ERROR: Unexpected LoadingValueMode.");
                    break;
            }
            await UniTask.WaitUntil(() => !isLoading);
        }
    }

    private async void leaveLoadingDelayAsync(float delayTime) {
        //set ball
        ballLogic.getRigidBody().useGravity = true;
        ball.GetComponent<Collider>().enabled = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(delayTime),true);
        setIsLoading(false);
        //reset progress
        switch (ui_loadLogic.loadingValueMode) {
            case 0:
                roadCreator.setProgressBlockCreation(0);
                break;
            case 1:
                blockDestroyManager.setProcessResetBlock(0);
                break;
            default:
                Debug.LogError("ERROR: Unexpected LoadingValueMode.");
                break;
        }
    }

    private async void giveControlDelayAsync(float delayTime) {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delayTime), true);
        isInControl = true;
    }

    private async void roadCreationAsync(float delayTime) {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delayTime), true);
        roadCreator.createRoad();//create road
    }

    private async void blockResetAsync(float delayTime) {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delayTime), true);
        blockDestroyManager.handleDestoryWhenRest();//reset block
    }



    //get
    public GameObject getBall() {
        return ball;
    }
    public BallLogic getBallLogic() {
        return ballLogic;
    }
    public GameObject getBallInsideTrigger() {
        return ball.transform.Find("TiggerInside").gameObject;
    }
    public float getBlockLength() {
        return blockLength;
    }
    public float getBallDiameter() {
        return ballDiameter;
    }
    public bool getIsStart() {
        return isStart;
    }
    public bool getIsLoading() {
        return isLoading;
    }
    public bool getIsInControl() {
        return isInControl;
    }

    //set
    public void setIsLoading(bool b) {
        isLoading = b;
        ui_loadLogic.handleLoadEvent();
    }
    public void setIsInControl(bool b) {
        isInControl = b;
    }
}
