using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using m4k.UI;

namespace m4k {
public class Feedback : Singleton<Feedback>
{
    public WorldToScreenUIFollow worldToScreenUIFollow;
    public AnimatedUIQueue msgQ;
    public AudioSource sfxAs;
    public AudioClip highlightAudio, selectAudio;
    public TMPro.TMP_Text notifyText;

    public Slider holdProgressSlider;
    public TMP_Text holdProgressText;

    [Header("Context Menu")]
    public GameObject contextMenu;
    public GameObject contextMenuItemParent;
    public GameObject contextMenuItemPrefab;

    [Header("Confirmation Prompt")]
    public GameObject genConfirmPrompt;
    public TMP_Text genConfirmLabel;
    public Button genConfirmButton;

    [Header("String Prompt")]
    public GameObject genInputPrompt;
    public TMP_Text genInputLabel;
    public TMP_InputField genInputField;
    public Button genInputConfirm;

    [Header("Quantity Prompt")]
    public GameObject genQuantityPrompt;
    public TMP_Text genQuantityLabel;
    public TMP_InputField genQuantityField;
    public Button genQuantityConfirm;

    List<GameObject> contextMenuItems = new List<GameObject>();

    private void Start() {
        genConfirmButton.onClick.AddListener(ConfirmRequest);
        genInputConfirm.onClick.AddListener(ConfirmStringInput);
        genQuantityField.onValueChanged.AddListener(OnQuantityChange);
        genQuantityConfirm.onClick.AddListener(ConfirmQuantity);
    }
    
    public void PlayAudio(AudioClip clip) {
        sfxAs.PlayOneShot(clip);
    }

    public void SendLineQueue(string line, bool popup = false) {
        msgQ.QueueLineWithCooldown(line, popup);
    }

    public void SendLine(string line, bool popup = false) {
        msgQ.SendLineInstant(line, popup);
    }

    int contextMenuItemIndex;
    public void StartContextMenu(Vector3 position) {
        contextMenu.SetActive(true);
        contextMenu.transform.position = position;
        contextMenuItemIndex = 0;
    }
    public void RegisterContextMenuItem(string label, System.Action func) {
        GameObject item = null;
        if(contextMenuItemIndex < contextMenuItems.Count) {
            item = contextMenuItems[contextMenuItemIndex];
        }
        else {
            item = Instantiate(contextMenuItemPrefab, contextMenuItemParent.transform, false);
            contextMenuItems.Add(item);
        }
        Button button = item.GetComponentInChildren<Button>();
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=>func?.Invoke());
        button.onClick.AddListener(DisableContextMenu);

        TMP_Text labelTxt = item.GetComponentInChildren<TMP_Text>();
        labelTxt.text = label;
    }
    public void DisableContextMenu() {
        foreach(var b in contextMenuItems) {
            b.gameObject.SetActive(false);
        }
        contextMenu.SetActive(false);
    }

    System.Action confirmFunc;
    public void RegisterConfirmRequest(System.Action func, string promptText) {
        if(confirmFunc != null) {
            Debug.LogWarning("Confirm request prompt not null");
        }
        confirmFunc = func;
        genConfirmLabel.text = promptText;
        genConfirmPrompt.SetActive(true);
    }
    public void ConfirmRequest() {
        confirmFunc?.Invoke();
        confirmFunc = null;
    }

    System.Action<string> inputFunc;
    public void RegisterStringInputRequest(System.Action<string> func, string promptText) {
        if(inputFunc != null) {
            Debug.LogWarning("Input prompt func not null");
        }
        inputFunc = func;
        genInputPrompt.SetActive(true);
        genInputField.text = "";
        genInputLabel.text = promptText;
    }
    public void ConfirmStringInput() {
        inputFunc?.Invoke(genInputField.text);
        inputFunc = null;
    }

    System.Action<int> quantityFunc;
    int _quantityVal, _maxQuantity, _minQuantity;
    public void RegisterQuantityRequest(System.Action<int> func, string promptText, int maxValue, int minValue = 0) {
        if(quantityFunc != null) {
            Debug.LogWarning("Quantity prompt func not null");
        }
        quantityFunc = func;
        genQuantityPrompt.SetActive(true);
        genQuantityLabel.text = promptText;
        _quantityVal = 1;
        genQuantityField.text = _quantityVal.ToString();
        _maxQuantity = maxValue;
        _minQuantity = minValue;
    }
    public void ConfirmQuantity() {
        quantityFunc?.Invoke(_quantityVal);
        quantityFunc = null;
    }
    public void OnQuantityChange(string s) {
        if(string.IsNullOrEmpty(s))
            return;
        int.TryParse(s, out _quantityVal);
    }
    // Can be invoked by UI triggers for concrete button inputField changes
    public void ChangeQuantityPromptValue(int i) {
        _quantityVal = i == 0 ? 0 : Mathf.Clamp(_quantityVal + i, _minQuantity, _maxQuantity);
        genQuantityField.text = _quantityVal.ToString();
    }

    Coroutine disableNotifyCr;
    public void DisplayNotification(string line, float duration = -1f) {
        if(disableNotifyCr != null)
            StopCoroutine(disableNotifyCr);
        if(duration == -1f)
            notifyText.gameObject.SetActive(true);
        else
            disableNotifyCr = StartCoroutine(TimedDisableNotifier(duration));
        notifyText.text = line;
    }
    public void DisableNotification() {
        if(disableNotifyCr != null)
            StopCoroutine(disableNotifyCr);
        notifyText.gameObject.SetActive(false);
    }
    IEnumerator TimedDisableNotifier(float timer) {
        notifyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(timer);
        notifyText.gameObject.SetActive(false);
    }

    bool _enableHoldProgressText;
    public void EnableHoldProgress(float maxValue, bool enableText = true, float startValue = 0f) {
        if(holdProgressSlider.gameObject.activeInHierarchy)
            Debug.LogWarning("Unexpected hold progress still active");
        holdProgressSlider.gameObject.SetActive(true);
        holdProgressSlider.maxValue = maxValue;
        holdProgressSlider.value = startValue;
        _enableHoldProgressText = enableText;
        holdProgressText.text = "";
    }
    public void DisableHoldProgress() {
        holdProgressSlider.gameObject.SetActive(false);
    }
    public void UpdateHoldProgressSliderValue(float value) {
        holdProgressSlider.value = holdProgressSlider.maxValue - value;
        if(_enableHoldProgressText) {
            UpdateHoldProgressText();
        }
    }
    void UpdateHoldProgressText() {
        holdProgressText.text = $"{holdProgressSlider.value.ToString("N2")} / {holdProgressSlider.maxValue}";
    }
}
}