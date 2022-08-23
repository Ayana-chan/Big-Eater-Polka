using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class UI_DieMessageLogic : MonoBehaviour
{
    public float showTimeUse = 1f;
    public float stayTime = 1.5f;
    public float fadeTimeUse = 0.6f;
    public float imageAlpha=0.5f;
    public float textAlpha = 1f;

    public async void showDieUI() {
        messageImage.DOFade(imageAlpha, showTimeUse);
        textMeshProUGUI.DOFade(textAlpha, showTimeUse);
        await UniTask.Delay(System.TimeSpan.FromSeconds(stayTime));
        messageImage.DOFade(0, fadeTimeUse);
        textMeshProUGUI.DOFade(0, fadeTimeUse);
    }

    private Image messageImage;
    private TextMeshProUGUI textMeshProUGUI;

    private void Awake() {
        messageImage=GetComponent<Image>();
        textMeshProUGUI=transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
