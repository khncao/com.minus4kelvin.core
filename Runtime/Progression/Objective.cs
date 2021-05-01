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
    public StoryData story;
    public string targetInteractId;
    public string objectiveName, objectiveDescrip;
    public Dialogue.Choice objectiveChoice;
    [HideInInspector]
    public KeyAction choiceAction;
    public Dialogue.Convo begLines, midLines, endLines;
    public ItemInstance[] rewardItems;
    public Conditions startConds, completeConds;
    public bool autoStartOnCondsMet;
    public string objectiveId { get { return name; }}

    public UnityEvent onStart, onCompleted;
    public System.Action<Dialogue.Choice> onComplete;
    public ObjectiveState state = ObjectiveState.NotStarted;
    
    TMPro.TMP_Text uiTxt;
    ProgressionManager progression;

    public bool MetStartConds() {
        return startConds.CheckCompleteReqs();
    }
    private void Start() {
        progression = ProgressionManager.I;
        // choiceAction = new Dialogue.ChoiceAction();
        choiceAction.key = objectiveChoice.key;
        // choiceAction.action = new UnityEvent();
        choiceAction.action.AddListener(()=>ProgressObjective());
        if(string.IsNullOrEmpty(objectiveId)) {
            Debug.LogError("Objective missing id");
            return;
        }
        progression.RegisterObjective(this);
        CheckLoadState();
        
        // foreach(CountGoal i in completeConds.requiredCounts) {
        //     i.current.onChange += OnCountValueChange;
        // }
        if(state == ObjectiveState.NotStarted && startConds.CheckCompleteReqs() && autoStartOnCondsMet) {
            StartObjective();
        }
        var interactS = progression.GetInteractableState(targetInteractId);
        if(interactS != null)
            OnInteractableRegistered(interactS.interactable);
        if(state != ObjectiveState.Completed) {
            progression.onRegisterInteractable += OnInteractableRegistered;
        }
    }

    void OnInteractableRegistered(Interactable interactable) {
        if(interactable && interactable.id == targetInteractId) {
            // transform.SetParent(interactable.transform);
            Dialogue dialogue = interactable.GetComponent<Dialogue>();
            if(dialogue)
                dialogue.RegisterObjective(this);
        }
    }

    private void OnDisable() {
        progression.UnregisterObjective(this);
    }

    void CheckLoadState() {
        if(progression.CheckIfObjectiveInProgress(objectiveId)) {
            StartObjective(true);
        }
        else if(progression.CheckCompletionState(objectiveId)) {
            EndObjective(true);
        }
        if(state == ObjectiveState.Completed) onCompleted?.Invoke();
    }

    void RegisterObjectiveTracker() {
        var uiInstance = progression.UI.InstantiateGetObjectiveTracker();
        uiTxt = uiInstance.GetComponent<TMPro.TMP_Text>();

        // completeConds.RegisterCheckConditions();
        // completeConds.onCheck += UpdateObjectiveProgress;

        if(completeConds.requiredItems.Count > 0) {
            InventoryManager.I.mainInventory.onChange += UpdateObjectiveProgress;
        }
        if(completeConds.requiredStates.Count > 0) {
            progression.onRegisterCompletionState += UpdateObjectiveProgress;
        }

        // if(completeConds.requiredCounts.Length > 0)
        //     Game.Instance.testCounter = completeConds.requiredCounts[0].current;

        UpdateObjectiveProgress();
    }
    void RemoveObjectiveTracker() {
        if(uiTxt)
            Destroy(uiTxt.gameObject);
    }
    void UpdateObjectiveProgress() {
        if(!uiTxt)
            return;
        string body = $" - {objectiveName}: {objectiveDescrip}.\n";

        // move to tostring in condition class for general use
        foreach(var i in completeConds.requiredStates) {
            string col = progression.CheckCompletionState(i) ? "green" : "white";
            body += string.Format("    <color={0}>- {1}</color>\n", col, i);
        }
        // foreach(CountGoal i in completeConds.requiredCounts) {
        //     string col = i.current.GetCount() < i.goal ? "white" : "green";
        //     body += string.Format("    <color={0}>- {1}: {2}/{3}</color>\n", col, i.current.id, i.current.GetCount(), i.goal);
        // }
        foreach(var i in completeConds.requiredItems) {
            if(!i.item) continue;
            var currVal = InventoryManager.I.mainInventory.GetItemTotalAmount(i.item);
            string col = currVal < i.amount ? "white" : "green";
            body += string.Format("    <color={0}>- {1}: {2}/{3}</color>\n", col, i.item.itemName, currVal, i.amount);
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
            progression.RegisterCompletedState(objectiveId);
        }
        progression.onRegisterInteractable -= OnInteractableRegistered;
    }
}
}