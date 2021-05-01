using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using m4k.Characters;

namespace m4k.Progression {
public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueUIObj;//, choicesObj;
    public TMP_Text speakerNameTxt, dialogueTxt;
    public Image speakerImgUI, choiceDivider;
    public TMP_Text[] choicesTxt;
    public AudioClip nextLineSfx;

    DialogueManager dialogueManager;

    public void Init(DialogueManager dm) {
        dialogueManager = dm;
    }

    public void InputNextLine() {
        dialogueManager.InputNextLine();
    }

    public void UpdateDialogueUI(Character character, string line) {
        if(character) {
            UpdateDialogueSpeaker(character.name);

            if(character.itemIcon) {
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

        for(int i = 0; i < choicesTxt.Length; ++i) {
            choicesTxt[i].enabled = true;
        }
    }
    public void DisableChoices() {
        choiceDivider.enabled = false;

        for(int i = 0; i < choicesTxt.Length; ++i) {
            if(choicesTxt[i])
                choicesTxt[i].enabled = false;
        }
    }

    public void ProcessChoices(List<Dialogue.Choice> choices) {
        EnableChoices();
        
        for(int i = 0; i < choicesTxt.Length; ++i) {
            choicesTxt[i].text = i < choices.Count && choices[i] != null ? choices[i].text : "";
        }
    }
    
    public void SelectChoice(int val) {
        DisableChoices();
        dialogueManager.SelectChoice(val);
    }
}
}