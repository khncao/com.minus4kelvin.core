using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using m4k.Interaction;
using m4k.InventorySystem;

namespace m4k.Progression {
public class Objective : MonoBehaviour
{
    public enum ObjectiveState { NotStarted, Started, Completed };

    public string targetInteractId; // for serialize
    public string objectiveName, objectiveDescrip;
    public Dialogue.Choice objectiveChoice;
    public Dialogue.Convo begLines, midLines, endLines;
    public ItemInstance[] rewardItems;
    public Conditions startConds, completeConds;
    public bool autoStartOnCondsMet;

    [HideInInspector]
    public KeyAction choiceAction;

    public UnityEvent onStart, onCompleted;
    public System.Action<Dialogue.Choice> onComplete;
    public ObjectiveState state = ObjectiveState.NotStarted;

    public string ObjectiveId { get { return name; }}
    
    TMPro.TMP_Text uiTxt;
    ProgressionManager progression;

    public bool MetStartConds() {
        return startConds.CheckCompleteReqs();
    }
    private void Start() {
        progression = ProgressionManager.I;

        choiceAction.key = objectiveChoice.key;
        choiceAction.action.AddListener(()=>ProgressObjective());
        if(string.IsNullOrEmpty(ObjectiveId)) {
            Debug.LogError("Objective missing id");
            return;
        }
        progression.RegisterObjective(this);

        if(progression.CheckIfObjectiveInProgress(ObjectiveId)) {
            StartObjective(true);
        }
        else if(progression.CheckCompletionState(ObjectiveId)) {
            EndObjective(true);
        }

        if(state == ObjectiveState.NotStarted && startConds.CheckCompleteReqs() && autoStartOnCondsMet) {
            StartObjective();
        }
        if(state != ObjectiveState.Completed) {
            progression.onRegisterInteractable += OnInteractableRegistered;
        }
        else {
            onCompleted?.Invoke();
        }

        var interactS = progression.GetInteractableState(targetInteractId);
        if(interactS != null)
            OnInteractableRegistered(interactS.interactable);
    }

    void OnInteractableRegistered(Interactable interactable) {
        if(interactable && interactable.id == targetInteractId) {
            Dialogue dialogue = interactable.GetComponent<Dialogue>();
            if(dialogue)
                dialogue.RegisterObjective(this);
        }
    }

    private void OnDisable() {
        progression.UnregisterObjective(this);
    }

    // GUI hud objective tracker entry
    void RegisterObjectiveTracker() {
        var uiInstance = progression.UI.InstantiateGetObjectiveTracker();
        uiTxt = uiInstance.GetComponent<TMPro.TMP_Text>();

        completeConds.RegisterChangeListener();
        completeConds.onChange += UpdateObjectiveProgress;

        UpdateObjectiveProgress();
    }

    void RemoveObjectiveTracker() {
        if(uiTxt)
            Destroy(uiTxt.gameObject);
        completeConds.UnregisterChangeListener();
    }

    void UpdateObjectiveProgress(Conditions conds = null) {
        if(!uiTxt)
            return;
        string body = $" - {objectiveName}: {objectiveDescrip}.\n";

        foreach(var i in completeConds.requiredRecordTotal) {
            Record rec = RecordManager.I.GetOrCreateRecord(i.Key);
            
            string col = rec.Sum < i.Value ? "white" : "green";
            body += $"    <color={col}>- {rec.id}: {rec.Sum}/{i.Value}</color>\n";
        }

        foreach(var i in completeConds.requiredRecordTemp) {
            Record rec = RecordManager.I.GetOrCreateRecord(i.Key);
            
            string col = rec.sessionVal < i.Value ? "white" : "green";
            body += $"    <color={col}>- {rec.id}: {rec.sessionVal}/{i.Value}</color>\n";
        }

        foreach(var i in completeConds.requiredStates) {
            if(string.IsNullOrEmpty(i)) 
                continue;
            string col = progression.CheckCompletionState(i) ? "green" : "white";
            body += $"    <color={col}>- {i}</color>\n";
        }

        foreach(var i in completeConds.requiredItems) {
            if(!i.Key) continue;
            var currVal = InventoryManager.I.mainInventory.GetItemTotalAmount(i.Key);
            string col = currVal < i.Value ? "white" : "green";
            body += $"    <color={col}>- {i.Key.itemName}: {currVal}/{i.Value}</color>\n";
        }
        uiTxt.text = body;
    }


    public void StartObjective(bool loading = false) {
        if(state != ObjectiveState.NotStarted) {
            Debug.LogWarning("Started objective in started/completed state");
        }
        state = ObjectiveState.Started;
        // Debug.Log("Started objective: " + objectiveName);
        if(!loading) {
            Feedback.I.SendLineQueue("Started objective: " + objectiveName, true);
        }
        
        progression.StartObjective(this);
        RegisterObjectiveTracker();
        onStart?.Invoke();
    }

    public void ProgressObjective() {
        if(state == ObjectiveState.Completed)
            return;
        if(state == ObjectiveState.NotStarted) {
            DialogueManager.I.ReplaceDialogue(begLines, 0);
            StartObjective();
            return;
        }
        bool completed = completeConds.CheckCompleteReqs();
        if(completed) {
            DialogueManager.I.ReplaceDialogue(endLines, 0);
            RewardItems(InventoryManager.I.mainInventory);
            EndObjective();
        }
        else {
            DialogueManager.I.ReplaceDialogue(midLines, 0);
        }
    }

    public void RewardItems(Inventory inventory) {
        foreach(var i in rewardItems) {
            inventory.AddItemAmount(i.item, i.amount, true);
        }
    }

    public void EndObjective(bool loading = false) {
        state = ObjectiveState.Completed;
        onCompleted?.Invoke();
        onComplete?.Invoke(objectiveChoice);
        
        // Debug.Log("Finished objective: " + objectiveName);
        if(!loading) {
            Feedback.I.SendLineQueue("Finished objective: " + objectiveName, 
        true);
            progression.FinishObjective(this);
            RemoveObjectiveTracker();
            progression.RegisterCompletedState(ObjectiveId);
        }
        progression.onRegisterInteractable -= OnInteractableRegistered;
    }
}
}