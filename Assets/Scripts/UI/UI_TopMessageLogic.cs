using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_TopMessageLogic : MonoBehaviour
{
    public Queue<string> messagesQueue = new Queue<string>();

    [Header("Time set")]
    public float textChangeTime = 1.2f;
    public float disappearDelayTime = 2f;
    public float showTimeUse = 0.5f;
    public float disapearTimeUse = 3;

    [Header("Color set")]
    public Color panelDefaultColor = new Color(1, 1, 1, 0.25f);
    public Color textDefaultColor = new Color(1, 1, 1, 1);

    public void addTopMessage(string content) {
        //judge repeat
        if (messagesQueue.Contains(content)) {  //repeat, not add, but reset timer to keep last message
            alphaStayTimer = disappearDelayTime;
        } else {
            messagesQueue.Enqueue(content);
        }
    }

    private float alphaStayTimer;//if 0, start disappear
    private Text textC;
    private Image panelImage;

    private void Awake() {
        textC=transform.Find("TopMessageText").GetComponent<Text>();
        panelImage=GetComponent<Image>();

        alphaStayTimer = 0;

        StartCoroutine(topMessageTextIEnumerator());
        StartCoroutine(topMessageAlphaIEnumerator());

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator topMessageTextIEnumerator() {
        textC.text = "";
        //Tween tweenFadeDown = textC.DOFade(0f, 5f).SetAutoKill(false).Pause();
        //Tween tweenFadeUp = textC.DOFade(1f, 3f).SetAutoKill(false).Pause();
        while (true) {
            yield return null;
            //no message
            if (messagesQueue.Count == 0) {
                yield return new WaitUntil(()=> messagesQueue.Count != 0);
                Debug.Log("messagesQueue.Count: " + messagesQueue.Count);
            }
            //not empty, reset timer to keep alpha (must together with Dequeue)
            alphaStayTimer = disappearDelayTime;
            //tweenFadeDown.Restart();
            //yield return tweenFadeDown.WaitForCompletion();
            textC.text = messagesQueue.Dequeue();
            //tweenFadeUp.Restart();
            //yield return tweenFadeUp.WaitForCompletion();
            yield return new WaitForSeconds(textChangeTime);
        }
    }

    private IEnumerator topMessageAlphaIEnumerator() {
        Color newColor;
        while (true) {
            yield return null;
            alphaStayTimer -= Time.deltaTime;
            //timer not use up, show
            if (alphaStayTimer > 0) {
                //text
                newColor = textC.color;
                newColor.a += textDefaultColor.a / showTimeUse * Time.deltaTime;
                if (newColor.a >= textDefaultColor.a) {
                    newColor.a = textDefaultColor.a;
                }
                textC.color = newColor;
                //panel
                newColor = panelImage.color;
                newColor.a += panelDefaultColor.a / showTimeUse * Time.deltaTime;
                if (newColor.a >= panelDefaultColor.a) {
                    newColor.a = panelDefaultColor.a;
                }
                panelImage.color = newColor;
            }
            //timer use up, disappear
            if (alphaStayTimer < 0) {
                //text
                newColor = textC.color;
                newColor.a -= textDefaultColor.a / disapearTimeUse * Time.deltaTime;
                if (newColor.a <= 0) {
                    newColor.a = 0;
                }
                textC.color = newColor;
                //panel
                newColor = panelImage.color;
                newColor.a -= panelDefaultColor.a / disapearTimeUse * Time.deltaTime;
                if (newColor.a <= 0) {
                    newColor.a = 0;
                }
                panelImage.color = newColor;
            }
        }
    }
}
