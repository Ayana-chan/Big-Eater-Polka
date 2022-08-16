using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public UI_LoadLogic ui_loadLogic;
    public GameObject mainCamera;
    public GameObject gamePlace;
    public RoadCreator roadCreator;
    public BlockTypeManager blockTypeManager;
    public BlockDestroyManager blockDestroyManager;

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

    public void loadAfterNewMap() {
        isInControl = false;
        setIsLoading(true);
        ui_loadLogic.setLoadingValueMode(0);
        StartCoroutine(roadCreationIEnumerator(reallyLoadObjectDelayTime));
    }

    public void loadAfterRest() {
        isInControl = false;
        setIsLoading(true);
        ui_loadLogic.setLoadingValueMode(1);
        StartCoroutine(blockResetIEnumerator(reallyLoadObjectDelayTime));
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

        blockTypeManager.initTransMaterialDic();

        StartCoroutine(afterLoadingIEnumerator());//read progress and deside how to take control

        level = 0;

        createBall();

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
            saveDataManager.activeSave();
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
            quitGame();
        }
        //press F5 to quit without save
        if (Input.GetKeyDown(KeyCode.F5)) {
            Debug.Log("Quit without Save.");
            quitGame();
        }
    }

    private void createBall() {
        ball = Instantiate(ballPrefab, gamePlace.transform);
        ball.transform.localScale = new Vector3(ballDiameter, ballDiameter, ballDiameter);
        float ballBornHeight = 0.01f + saveDataManager.currentSave.rebornBlockPos_Y + blockLength / 2 + ballDiameter / 2;
        ball.transform.localPosition = new Vector3(saveDataManager.currentSave.rebornBlockPos_X,ballBornHeight, saveDataManager.currentSave.rebornBlockPos_Z);
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
    //read progress and deside how to take control. Used any time.
    private IEnumerator afterLoadingIEnumerator() {
        while (true) {
            yield return null;
            yield return new WaitUntil(() => isLoading && isStart && ui_loadLogic.judgeLoadCompelete());//待优化
            //ready
            switch (ui_loadLogic.loadingValueMode) {
                case 0:
                    StartCoroutine(leaveLoadingDelayIEnumerator(1.5f));
                    StartCoroutine(giveControlDelayIEnumerator(2.5f));
                    break;
                case 1:
                    StartCoroutine(leaveLoadingDelayIEnumerator(0.8f));
                    StartCoroutine(giveControlDelayIEnumerator(1.8f));
                    break;
                default:
                    Debug.LogError("ERROR: Unexpected LoadingValueMode.");
                    break;
            }
            yield return new WaitUntil(() => !isLoading);
        }
    }
    private IEnumerator leaveLoadingDelayIEnumerator(float delayTime) {
        yield return new WaitForSecondsRealtime(delayTime);
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
    private IEnumerator giveControlDelayIEnumerator(float delayTime) {
        yield return new WaitForSecondsRealtime(delayTime);
        isInControl = true;
    }

    private IEnumerator roadCreationIEnumerator(float delayTime) {
        yield return new WaitForSecondsRealtime(delayTime);
        roadCreator.createRoad();//create road
    }

    private IEnumerator blockResetIEnumerator(float delayTime) {
        yield return new WaitForSecondsRealtime(delayTime);
        blockDestroyManager.handleDestoryWhenRest();//reset block
    }



    //get
    public GameObject getBall() {
        return ball;
    }
    public BallLogic getBallLogic() {
        return ballLogic;
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
}
