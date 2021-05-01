using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using m4k.Characters;

namespace m4k.Progression {
public class Dialogue : MonoBehaviour
{
    [System.Serializable]
    public class Convo {
        public string id;
        public List<Line> lines;
        public string nextConvoId;
    }
    [System.Serializable]
    public class Line {
        [TextArea]
        public string text;
        public Character character;
        public List<Choice> choices;
    }
    [System.Serializable]
    public class Choice {
        public string text;
        public string key;
        public string nextConvoId;
    }

    public DialogueSO dialogueSO;
    public string persistConvoId;
    public Choice exitChoice;
    public List<Choice> choices;
    public List<Objective> objectives;
    public List<KeyAction> choiceActions;
    public string dialogueId { get { return dialogueSO.name; }}
    public List<Convo> convos { get { return dialogueSO.convos; }}

    Line mainChoiceLine;

    private void Start() {
        if(dialogueSO && string.IsNullOrEmpty(dialogueId))
            Debug.LogWarning("Dialogue missing id");

        // mainChoiceLine = GetLine("choice");
        // if(mainChoiceLine != null) {
        //     for(int i = 0; i < mainChoiceLine.choices.Count; ++i) {
        //         choices.Add(mainChoiceLine.choices[i]);
        //     }
        // }
        if(!string.IsNullOrEmpty(exitChoice.text))
            choices.Add(exitChoice);
            
        GetObjectivesInChildren();
        ProgressionManager.I.RegisterDialogue(this);
    }
    void OnConvoComplete(Convo convo) {
        if(!string.IsNullOrEmpty(convo.id) && convo.id != "choice")
            ProgressionManager.I.RegisterCompletedState(GetConvoIdPath(convo));
    }
    void GetObjectivesInChildren() {
        var o = GetComponentsInChildren<Objective>();
        foreach(var i in o) 
            if(!objectives.Contains(i))
                objectives.Add(i);
    }
    public void RegisterObjective(Objective objective) {
        if(!objectives.Contains(objective))
            objectives.Add(objective);
    }

    public string GetConvoIdPath(Convo convo) {
        if(!dialogueSO)
            return null;
        return string.Format("{0}/{1}", dialogueId, convo.id);
    }

    public void AssignDialogue(DialogueSO dialogue) {
        dialogueSO = dialogue;
        AssignDialogue();
    }
    public void AssignDialogue() {
        if(string.IsNullOrEmpty(persistConvoId))
            persistConvoId = convos[0].id;
        AssignDialogue(persistConvoId);
    }

    public void AssignDialogue(string convoId) {
        if(!dialogueSO || convos == null) {
            Debug.LogWarning(gameObject.name + " has no dialogue");
            return;
        }
        if(dialogueId.ToLower().Contains("random")) {
            DialogueManager.I.AssignDialogue(this, GetRandomLines(dialogueSO.convos), 0);
            return;
        }
        // GetObjectivesInChildren();
        for(int i = objectives.Count-1; i >= 0; --i) {
            if(objectives[i].state != Objective.ObjectiveState.Completed) 
            {
                if(objectives[i].state == Objective.ObjectiveState.NotStarted && !objectives[i].MetStartConds())
                    continue;
                if(RegisterObjectiveChoice(objectives[i])) {
                    objectives[i].onComplete += RemoveChoice;
                }
            }
        }
        Convo c = string.IsNullOrEmpty(convoId) ? convos[0] : GetConvo(convoId);

        DialogueManager.I.AssignDialogue(this, c, 0);
        DialogueManager.I.onCompleteConvo = OnConvoComplete;
    }

    public void NextLine() {
        DialogueManager.I.InputNextLine();
    }

    Convo GetRandomLines(List<Convo> convos) {
        return convos[Random.Range(0, convos.Count)];
    }

    public Line GetLine(string convoId) {
        var convo = GetConvo(convoId);
        if(convo == null)
            return null;
        return convo.lines[0];
    }

    public Convo GetConvo(string id) {
        var ind = convos.FindIndex(x=>x.id == id);
        if(ind == -1) {
            return null;
        }
        return convos[ind];
    }

    public void EndDialogue() {
        DialogueManager.I.StopDialogue();
    }

    public bool RegisterObjectiveChoice(Objective objective) {
        if(!choices.Contains(objective.objectiveChoice)) {
            choices.Insert(0, objective.objectiveChoice);

            choiceActions.Add(objective.choiceAction);
            return true;
        }
        return false;
    }
    public bool RegisterInteractionChoice(Choice choice, KeyAction action) {
        if(!choices.Contains(choice)) {
            choices.Insert(0, choice);
            choiceActions.Add(action);
            return true;
        }
        return false;
    }
    public void RemoveChoice(Choice choice) {
        if(choices.Remove(choice))
            Debug.Log("remove success: " + choice.text);
    }

    public void InvokeChoiceAction(Choice choice) {
        InvokeChoiceAction(choice.key);
    }
    public void InvokeChoiceAction(string key) {
        if(string.IsNullOrEmpty(key))
            return;
        DialogueManager.I.InvokeChoiceAction(key);
        
        int ind = choiceActions.FindIndex(x=>x.key == key);
        if(ind != -1) 
            choiceActions[ind].action?.Invoke();
    }
}
}