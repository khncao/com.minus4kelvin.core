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
    public GameObject genConfirmPrompt;
    public GameObject genInputPrompt;
    // public GameObject genQuantityPrompt;
    // public Button[] genQuantityButtons;

    // int[] quantityButtonVals = { 0, -10, -1, 1, 10, 1000 };
    TMP_Text genConfirmLabel, genInputLabel;
    // TMP_Text genQuantityLabel;
    TMP_InputField genInputField;
    // TMP_InputField genQuantityField;
    Button genInputConfirm;
    // Button genQuantityConfirm;

    private void Start() {
        genConfirmLabel = genConfirmPrompt.GetComponentInChildren<TMP_Text>();
        
        genInputLabel = genInputPrompt.GetComponentInChildren<TMP_Text>();
        genInputField = genInputPrompt.GetComponentInChildren<TMP_InputField>();
        genInputConfirm = genInputPrompt.GetComponentInChildren<Button>();
        genInputConfirm.onClick.AddListener(ConfirmInput);

        // genQuantityLabel = genQuantityPrompt.GetComponentInChildren<TMP_Text>();
        // genQuantityField = genQuantityPrompt.GetComponentInChildren<TMP_InputField>();
        // genQuantityConfirm = genQuantityPrompt.GetComponentInChildren<Button>();
        // genQuantityField.onValueChanged.AddListener(OnQuantityChange);
        // genQuantityConfirm.onClick.AddListener(ConfirmQuantity);
        // for(int i = 0; i < genQuantityButtons.Length; ++i) {
        //     genQuantityButtons[i].onClick
        // }
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

    System.Func<int> confirmFunc;
    public void RegisterConfirmRequest(System.Func<int> func, string promptText) {
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
    System.Func<string, int> inputFunc;
    public void RegisterInputRequest(System.Func<string, int> func, string promptText) {
        if(inputFunc != null) {
            Debug.LogWarning("Input prompt func not null");
        }
        inputFunc = func;
        genInputPrompt.SetActive(true);
        genInputField.text = "";
        genInputLabel.text = promptText;
    }
    public void ConfirmInput() {
        inputFunc?.Invoke(genInputField.text);
        inputFunc = null;
    }
    // System.Func<int, int> quantityFunc;
    // int quantityVal;
    // public void RegisterQuantityRequest(System.Func<int, int> func, string promptText) {
    //     if(quantityFunc != null) {
    //         Debug.LogWarning("Quantity prompt func not null");
    //     }
    //     quantityFunc = func;
    //     genQuantityPrompt.SetActive(true);
    //     genInputField.text = "1";
    //     genQuantityLabel.text = promptText;
    //     quantityVal = 1;
    // }
    // public void ConfirmQuantity() {
    //     quantityFunc?.Invoke(quantityVal);
    // }
    // void OnQuantityChange(string s) {
    //     int.TryParse(s, out quantityVal);
    // }
    // void ChangeQuantity(int i) {
    //     quantityVal += i;
    // }

    Coroutine disableNotifyCr;
    public void DisplayNotification(string line) {
        if(disableNotifyCr != null)
            StopCoroutine(disableNotifyCr);
        disableNotifyCr = StartCoroutine(TimedDisableNotifier(3));
        notifyText.text = line;
    }
    IEnumerator TimedDisableNotifier(float timer) {
        notifyText.gameObject.SetActive(true);
        yield return new WaitForSeconds(timer);
        notifyText.gameObject.SetActive(false);
    }
}
}