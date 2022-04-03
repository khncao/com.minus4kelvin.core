using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Progression {
public class Dialogue : MonoBehaviour
{
    // public enum DialogueMode { Default, Subtitle, Cinematic }
    // public DialogueMode dialogueMode;
    public Convo convo;
    public Character character;
    public string choiceLine;
    public string exitChoiceLine = "End";
    public bool choicesOnConvoEnd = true;
    public bool useCharPortrait = true;
    public bool allowInteract = true;
    public bool blockPlayerInput = false;
    public int autopassSeconds = 0;
    public List<Objective> objectives;
    public List<ChoiceEvent> choiceEvents;
    public List<ConditionalChoice> conditionalChoices;
    
    [HideInInspector]
    public List<KeyAction> choiceActions;
    [HideInInspector]
    public List<Choice> choices;

    Choice _exitChoice;
    
    private void Start() {
        _exitChoice = new Choice();
        _exitChoice.text = exitChoiceLine;

        for(int i = 0; i < objectives.Count; ++i) {
            objectives[i].Init(this);
        }
    }

    void OnDisable() {
    }

    void OnConvoComplete(Convo convo) {
        if((convo.isKeyState || convo.autoSkipIfSeen) && !string.IsNullOrEmpty(convo.id))
            ProgressionManager.I.RegisterKeyState(convo.id);
    }

    public void AssignDialogue(Convo c) {
        convo = c;
        AssignDialogue();
    }
    public void AssignDialogue() {
        if(!convo) {
            Debug.LogWarning(gameObject.name + " has no assigned dialogue");
            return;
        }
        AssignDialogue("");
    }
    public void AssignDialogue(string newConvoId) {
        // UpdateChoices();
        for(int i = 0; i < conditionalChoices.Count; ++i) {
            if(conditionalChoices[i].replaceConvoOnConds && conditionalChoices[i].MetConditions) {
                convo = conditionalChoices[i].nextConvo;
            }
        }

        Convo c = string.IsNullOrEmpty(newConvoId) || newConvoId == convo.id ? convo : DialogueManager.I.GetConvo(newConvoId);

        DialogueManager.I.AssignDialogue(this, c, 0);
        DialogueManager.I.onCompleteConvo -= OnConvoComplete;
        DialogueManager.I.onCompleteConvo += OnConvoComplete;
    }

    public void UpdateChoices() {
        choices.Clear();
        choiceActions.Clear();

        RegisterChoice(_exitChoice);
        for(int i = 0; i < choiceEvents.Count; ++i) {
            choiceEvents[i].RegisterChoice();
        }
        for(int i = 0; i < conditionalChoices.Count; ++i) {
            // possibly skip more expensive condition check
            if(conditionalChoices[i].replaceConvoOnConds) continue;
            if(conditionalChoices[i].MetConditions) {
                RegisterChoice(conditionalChoices[i].Choice);
            }
        }
        for(int i = objectives.Count-1; i >= 0; --i) {
            if(objectives[i].MetChoiceConds) {
                RegisterChoice(objectives[i].objectiveChoice, objectives[i].choiceAction);
            }
        }
    }

    public void NextLine() {
        DialogueManager.I.InputNextLine();
    }

    public void EndDialogue() {
        DialogueManager.I.StopDialogue();
    }

    public void RegisterChoiceEvent(ChoiceEvent choiceEvent) {
        if(!choiceEvents.Contains(choiceEvent)) {
            choiceEvents.Add(choiceEvent);
        }
    }

    public bool RegisterChoice(Choice choice, KeyAction action = null) {
        if(!choices.Contains(choice)) {
            choices.Insert(0, choice);
            if(action != null)
                choiceActions.Add(action);
            return true;
        }
        return false;
    }
    public void RemoveChoice(Choice choice) {
        if(choices.Remove(choice))
            Debug.Log("removed choice: " + choice.text);
    }

    public void InvokeChoiceAction(Choice choice) {
        InvokeChoiceAction(choice.key);
    }
    public void InvokeChoiceAction(string key) {
        int ind = choiceActions.FindIndex(x=>x.key == key);
        if(ind != -1) 
            choiceActions[ind].action?.Invoke();
    }
}
}