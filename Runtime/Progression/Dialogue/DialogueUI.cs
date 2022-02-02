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
    public GameObject choicesParent;
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

    public void DisableChoices() {
        choiceDivider.enabled = false;

        for(int i = 0; i < choicesTxt.Count; ++i) {
            if(choicesTxt[i])
                choicesTxt[i].gameObject.SetActive(false);
        }
        choicesParent?.SetActive(false);
    }

    public void ProcessChoices(List<Choice> choices) {
        choiceDivider.enabled = true;

        for(int i = 0; i < choicesTxt.Count; ++i) {
            if(i < choices.Count && choices[i] != null) {
                choicesTxt[i].gameObject.SetActive(true);
                choicesTxt[i].text = choices[i].text;
            }
        }
        EventSystem.current.SetSelectedGameObject(choiceDivider.gameObject);
        choicesParent?.SetActive(true);
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