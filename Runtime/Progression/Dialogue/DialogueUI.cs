using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using m4k.Characters;

namespace m4k.Progression {
public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueUIObj;//, choicesObj;
    public TMP_Text speakerNameTxt, dialogueTxt;
    public Image speakerImgUI, choiceDivider;
    public List<TMP_Text> choicesTxt;
    public AudioClip nextLineSfx;

    DialogueManager dialogueManager;
    int _highlightedChoiceInd;

    public void Init(DialogueManager dm) {
        dialogueManager = dm;
    }

    public void InputNextLine() {
        dialogueManager.InputNextLine();
    }

    public void UpdateDialogueUI(Character character, string line, bool usePortrait) {
        if(character) {
            UpdateDialogueSpeaker(character.name);

            if(usePortrait && character.itemIcon) {
                UpdateDialogueSprite(character.itemIcon);
            }
        }
        else {
            UpdateDialogueSprite(null);
            UpdateDialogueSpeaker("");
        }
        UpdateDialogueText(line);
    }

    void UpdateDialogueText(string text) => dialogueTxt.text = text;
    void UpdateDialogueSpeaker(string charName) => speakerNameTxt.text = charName;

    void UpdateDialogueSprite(Sprite sprite) {
        speakerImgUI.sprite = sprite;
        speakerImgUI.color = sprite != null ? Color.white : Color.clear;
    }
    public void ToggleDialogueUI(bool enabled) {
        dialogueUIObj?.SetActive(enabled);
    }
    public void EnableChoices() {
        choiceDivider.enabled = true;

        for(int i = 0; i < choicesTxt.Count; ++i) {
            choicesTxt[i].enabled = true;
        }
        EventSystem.current.SetSelectedGameObject(choiceDivider.gameObject);
    }
    public void DisableChoices() {
        choiceDivider.enabled = false;

        for(int i = 0; i < choicesTxt.Count; ++i) {
            if(choicesTxt[i])
                choicesTxt[i].enabled = false;
        }
    }

    public void ProcessChoices(List<Choice> choices) {
        EnableChoices();
        
        for(int i = 0; i < choicesTxt.Count; ++i) {
            choicesTxt[i].text = i < choices.Count && choices[i] != null ? choices[i].text : "";
        }
    }

    public void SelectChoice() {
        var choiceInd = choicesTxt.FindIndex(x=>x.gameObject == EventSystem.current.currentSelectedGameObject);
        if(choiceInd != -1) {
            dialogueManager.SelectChoice(choiceInd);
            DisableChoices();
        }
    }
    
    public void SelectChoice(int val) {
        DisableChoices();
        dialogueManager.SelectChoice(val);
    }
}
}