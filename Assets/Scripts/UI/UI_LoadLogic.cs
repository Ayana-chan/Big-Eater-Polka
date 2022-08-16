using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_LoadLogic : MonoBehaviour
{
    /*
     * set loadingValueMode is necessary when get into load
     */

    [Tooltip("If false, used as blood bar")]
    public bool isLoading=true;

    [Tooltip("Decide which value to become load value.\n" +
        "0: road creation\n" +
        "1: reset blocks when rest")]
    public int loadingValueMode=0;

    [Space]

    [Tooltip("Could not change")]
    public float sliderProgress;

    [Tooltip("Show the progress of road creation")]
    public float targetRatio;
    public float loadMaxValue = 1;
    public float targetValue;
    public float targetMaxValue;

    [Space]

    [Header("Panel set")]
    [Tooltip("Default true")]
    public bool isLoadPanelShow = true;
    public float loadPanelDisappearTimeUse = 0.8f;
    public float loadPanelShowTimeUse = 0.4f;
    //public float loadPanelDisappearDelay = 1.5f;

    [Space]

    [Header("Color set")]
    public float barColorChangeTimeUse = 0.5f;
    public Color barColorChangeMidColor = new Color(236/255f,79/255f,79/255f);

    [Space]

    [Header("Bar speed set")]
    [Tooltip("speed of value movement when load")]
    public float speedOfLoad = 0.3f;
    [Tooltip("speed of value movement when load")]
    public float speedOfValueDecrease = 1;
    [Tooltip("speed of max increase movement")]
    public float speedOfMaxValueIncrease = 1.5f;
    [Tooltip("speed of max decrease movement")]
    public float speedOfMaxValueDecrease = 3f;

    [Tooltip("changingBlood would have a short pause when blood decrease")]
    public float delayOfChangingBloodDecrease = 0.7f;

    [Space]

    public GameObject loadPanel;

    public Sprite loadBarTexture;
    public Sprite bloodBarTexture;

    public GameObject bloodBar;

    public MainLogic ml;
    public RoadCreator roadCreator;
    public BloodLogic bloodLogic;
    public BlockDestroyManager blockDestroyManager;

    public void handleLoadEvent() { //called when isLoading change
        isLoading = ml.getIsLoading();
        changePanelAlpha();
        //init bar
        if (isLoading) {
            bloodImage.fillAmount = 0;
            changingBloodImage.fillAmount = 0;
            targetRatio = 0;
        }
    }

    //called when load
    public void showBar() {
        bloodBar.SetActive(true);

        //StartCoroutine(isLoadPanelShowControlIEnumerator());
        //StartCoroutine(loadPanelIEnumerator());
        StartCoroutine(loadingUIIEnumerator());
        StartCoroutine(bloodUIIEnumerator());
        StartCoroutine(valueDecreaseIEnumerator());
        StartCoroutine(maxValueIncreaseIEnumerator());
        StartCoroutine(changeColorIEnumerator());
        StartCoroutine(textOnBarIEnumerator());
    }

    public bool judgeLoadCompelete() {
        return isLoading 
            && sliderProgress >= loadMaxValue   //complete in view
            && targetRatio == 1;    //avoid bar not init
    }

    private Image loadPanelImage;

    private GameObject textOnBar;
    private GameObject backLight;//just change size, won't change scale
    private GameObject emptyBlood;
    private GameObject changingBlood;
    private GameObject blood;
    private RectTransform backLightRectTransform;
    private RectTransform emptyBloodRectTransform;
    private RectTransform changingBloodRectTransform;
    private RectTransform bloodRectTransform;
    private Image changingBloodImage;
    private Image bloodImage;
    private TextMeshProUGUI barText;

    private void Awake() {
        loadPanelImage = loadPanel.GetComponent<Image>();

        backLight = bloodBar.transform.Find("BackLight").gameObject;
        emptyBlood = bloodBar.transform.Find("EmptyBlood").gameObject;
        changingBlood = bloodBar.transform.Find("ChangingBlood").gameObject;
        blood = bloodBar.transform.Find("Blood").gameObject;
        textOnBar= bloodBar.transform.Find("TextOnBar").gameObject;
        backLightRectTransform = backLight.GetComponent<RectTransform>();
        emptyBloodRectTransform = emptyBlood.GetComponent<RectTransform>();
        changingBloodRectTransform = changingBlood.GetComponent<RectTransform>();
        bloodRectTransform = blood.GetComponent<RectTransform>();
        changingBloodImage =changingBlood.GetComponent<Image>();
        bloodImage =blood.GetComponent<Image>();
        barText = textOnBar.GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {   
        
    }

    // Update is called once per frame
    void Update()
    {
        sliderProgress = bloodImage.fillAmount;

    }

    private void changePanelAlpha() {
        if (isLoading) {
            loadPanelImage.DOFade(1, loadPanelShowTimeUse);
        } else {
            loadPanelImage.DOFade(0, loadPanelDisappearTimeUse);
        }
    }

    private IEnumerator bloodUIIEnumerator() {
        while (true) {
            yield return null;
            if (isLoading) {
                continue;
            }
            targetMaxValue = bloodLogic.maxBlood / 100f;
            targetValue = bloodLogic.currentBlood / 100f;
            targetRatio=targetValue / targetMaxValue;
            //assign slider
            bloodImage.fillAmount = targetRatio;
        }
    }

    private IEnumerator loadingUIIEnumerator() {
        while (true) {
            yield return null;
            if (!isLoading) {
                continue;
            }
            targetMaxValue = loadMaxValue;//would change maxValue immediately
            targetValue = calTargetValue() * loadMaxValue;
            targetRatio = calTargetValue();
            //assign slider
            bloodImage.fillAmount += speedOfLoad * Time.deltaTime;
            if (bloodImage.fillAmount > targetRatio) {
                bloodImage.fillAmount = targetRatio;
            }
            //load complete
            if (bloodImage.fillAmount >= 1) {
                bloodImage.fillAmount = 1;
                yield return null;
                yield return new WaitUntil(() => isLoading);
            }
        }
    }
    /// <summary>
    /// Here to decide every mode's functions
    /// </summary>
    /// <returns></returns>
    private float calTargetValue() {
        switch (loadingValueMode) {
            case 0:
                return roadCreator.getProgressBlockCreation();
            case 1:
                return blockDestroyManager.getProcessResetBlock();
            default:
                return 0;
        }
    }

    private IEnumerator valueDecreaseIEnumerator() {    //control changingBloodImage
        while (true) {
            yield return null;
            changingBloodImage.fillAmount-=speedOfValueDecrease*Time.deltaTime;
            //decrease to equal or blood increase
            if (changingBloodImage.fillAmount <= bloodImage.fillAmount) {
                changingBloodImage.fillAmount = bloodImage.fillAmount;
                yield return new WaitUntil(() => changingBloodImage.fillAmount != bloodImage.fillAmount);
                //blood decrease
                if (changingBloodImage.fillAmount > bloodImage.fillAmount) {
                    yield return new WaitForSecondsRealtime(delayOfChangingBloodDecrease);//delay to decrease
                }
            }
        }
    }

    private IEnumerator maxValueIncreaseIEnumerator() {
        float addMoreWidthOfBackLight = backLightRectTransform.rect.width - bloodRectTransform.rect.width;
        float currentMaxValue;
        float aimWidthOfbackLight;
        float aimHeightOfbackLight;
        while (true) {
            yield return null;
            currentMaxValue = emptyBloodRectTransform.localScale.x;
            if (Mathf.Abs(currentMaxValue - targetMaxValue) <= 0.03f) {
                currentMaxValue = targetMaxValue;
            } else if (currentMaxValue < targetMaxValue) { //increase
                currentMaxValue += speedOfMaxValueIncrease * Time.deltaTime;
            } else if (currentMaxValue > targetMaxValue) {   //decrease
                currentMaxValue -= speedOfMaxValueDecrease * Time.deltaTime;
            }
            emptyBloodRectTransform.localScale = new Vector3(currentMaxValue, 1, 1);
            //change other bars as emptyBlood
            changingBloodRectTransform.localScale = emptyBloodRectTransform.localScale;
            bloodRectTransform.localScale = emptyBloodRectTransform.localScale;
            //set backLight size
            aimWidthOfbackLight = emptyBloodRectTransform.rect.width * bloodRectTransform.localScale.x   //blood actual width
                                    + addMoreWidthOfBackLight;  //add more width
            aimHeightOfbackLight = backLightRectTransform.rect.height;
            backLightRectTransform.sizeDelta = new Vector2(aimWidthOfbackLight, aimHeightOfbackLight);
            yield return new WaitUntil(()=> currentMaxValue != targetMaxValue);//wait for change
        }
    }

    private IEnumerator changeColorIEnumerator() {
        bloodImage.sprite = loadBarTexture;
        while (true) {
            yield return new WaitUntil(()=> isLoading);
            bloodImage.sprite = loadBarTexture;
            yield return new WaitUntil(() => sliderProgress == 1);//load complete
            bloodImage.DOColor(barColorChangeMidColor, barColorChangeTimeUse);
            yield return new WaitUntil(() => !isLoading);
            bloodImage.color = Color.white;
            bloodImage.sprite = bloodBarTexture;
        }
    }

    private IEnumerator textOnBarIEnumerator() {
        string randomChars = "@#$%&*";
        while (true) {
            yield return null;
            if (isLoading) {
                //new map
                if (loadingValueMode == 0) {
                    barText.color = Color.white;
                    if (bloodImage.fillAmount < 1) {
                        barText.text = ((int)(bloodImage.fillAmount * 100)).ToString() + "%";
                    } else {    //load complete
                        barText.text = "100% - We beseech thee, " + makeRandomString(4, randomChars) + "....";
                    }
                } else if (loadingValueMode == 1) {//rest
                    barText.color = Color.white;
                    barText.text = "Rest in Peace...";
                }
            } else {
                barText.color = Color.red;
                barText.text = "";//can add more functions
            }
        }
    }
    private string makeRandomString(int length, string randomChars) {
        string ans = "";
        int num;
        for (int i = 0; i < length; i++) {
            num=Random.Range(0, length);
            ans += randomChars[num];
        }
        return ans;
    }



    //get
    public float getTargetValue() {
        return targetValue;
    }

    //set
    public void setLoadingValueMode(int mode) {
        loadingValueMode = mode;
    }
}


//private IEnumerator isLoadPanelShowControlIEnumerator() {
//    while (true) {
//        yield return null;
//        yield return new WaitUntil(() => ml.getIsStart() && isLoadingMode);
//        isLoadPanelShow = true;
//        yield return new WaitUntil(() => ml.getIsStart() && !isLoadingMode);
//        yield return new WaitForSecondsRealtime(loadPanelDisappearDelay);
//        isLoadPanelShow = false;
//    }
//}

//private IEnumerator loadPanelIEnumerator() {
//    Color co;
//    while (true) {
//        yield return null;
//        //show
//        while (true) {
//            yield return null;
//            co = loadPanelImage.color;
//            co.a += loadPanelShowSpeed * Time.deltaTime;
//            if (co.a < 1) {
//                loadPanelImage.color = co;
//            } else {
//                co.a = 1;
//                loadPanelImage.color = co;
//                break;
//            }
//        }
//        yield return new WaitUntil(() => !isLoadPanelShow);
//        //disappear
//        while (true) {
//            yield return null;
//            co = loadPanelImage.color;
//            co.a -= loadPanelShowSpeed * Time.deltaTime;
//            if (co.a > 0) {
//                loadPanelImage.color = co;
//            } else {
//                co.a = 0;
//                loadPanelImage.color = co;
//                break;
//            }
//        }
//        yield return new WaitUntil(() => isLoadPanelShow);
//    }
//}
