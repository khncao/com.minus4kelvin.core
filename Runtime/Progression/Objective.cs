using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.Progression {
[CreateAssetMenu(menuName="ScriptableObjects/Progress/Objective")]
public class Objective : ScriptableObject
{
    public enum ObjectiveState { NotStarted, Started, Completed, Failed };

    public string objectiveName;
    [TextArea(1, 3)]
    public string objectiveDescrip;
    public string choiceLine;
    [Tooltip("Leave empty to use choiceLine")]
    public string midChoiceLine;
    public Convo begLines, midLines, endLines;
    public ItemInstance[] rewardItems;
    
    [Tooltip("Actively listen to start conditions and start objective if start conditions met")]
    public bool autoStartOnCondsMet;

    // [Tooltip("Auto complete with no further interaction on conditions met")]
    // public bool autoCompleteOnCondsMet;

    [Tooltip("On conditions met, objective choice is registered to interactable dialogue. If auto start enabled, actively listens for conditions met")]
    public Conditions startConds;
    public Conditions completeConds, failConds;

    [Tooltip("KeyActions(global, scene, interactables) that are called when not loading from save")]
    public List<string> onStartActions, onCompletedActions;

    [Tooltip("Next objective to automatically start in chain")]
    public Objective nextObjective;

    [System.NonSerialized]
    public ObjectiveState state = ObjectiveState.NotStarted;
    [System.NonSerialized]
    public Choice objectiveChoice;
    [System.NonSerialized]
    public KeyAction choiceAction;

    public string ObjectiveId { get { return name; }}
    
    TMPro.TMP_Text _uiTxt;
    ProgressionManager _progression;
    Dialogue _dialogue;

    public bool MetChoiceConds {
        get {
            return (startConds.CheckCompleteReqs() && state != ObjectiveState.Completed) || state == ObjectiveState.Started;
        }
    }

    public void Init(Dialogue d) {
        _progression = ProgressionManager.I;
        _dialogue = d;

        objectiveChoice = new Choice();
        objectiveChoice.text = choiceLine;
        objectiveChoice.key = name;

        choiceAction = new KeyAction(objectiveChoice.key, ()=>ProgressObjective());

        // load state
        state = ObjectiveState.NotStarted;

        if(_progression.CheckIfObjectiveInProgress(ObjectiveId) || (
            startConds.CheckCompleteReqs() && autoStartOnCondsMet
        )) {
            Debug.Log($"Loaded inprogress objective: {ObjectiveId}");
            StartObjective(true);
        }
        else if(_progression.CheckCompletionState(ObjectiveId)) {
            Debug.Log($"Loaded complete objective: {ObjectiveId}");
            CompleteObjective(true);
        }

        // Reset private fields in ScriptableObjects
        startConds.Init();
        completeConds.Init();
        failConds.Init();
    }

    // GUI hud objective tracker entry
    void RegisterObjectiveTracker() {
        var uiInstance = _progression.UI.InstantiateGetObjectiveTracker();
        _uiTxt = uiInstance.GetComponent<TMPro.TMP_Text>();

        completeConds.RegisterChangeListener();
        failConds.RegisterChangeListener();
        completeConds.onChange -= OnConditionChange;
        completeConds.onChange += OnConditionChange;
        failConds.onChange -= OnConditionChange;
        failConds.onChange += OnConditionChange;
        failConds.onComplete -= OnFail;
        failConds.onComplete += OnFail;

        OnConditionChange();
    }

    void RemoveObjectiveTracker() {
        if(_uiTxt)
            Destroy(_uiTxt.gameObject);
        completeConds.UnregisterChangeListener();
        failConds.UnregisterChangeListener();
        completeConds.onChange -= OnConditionChange;
        failConds.onChange -= OnConditionChange;
        failConds.onComplete -= OnFail;
    }

    void OnConditionChange(Conditions conds = null) {
        if(!_uiTxt)
            return;
        string body = $" - {objectiveName}: {objectiveDescrip}.\n";

        foreach(var c in completeConds.conditions) {
            body += $"    {c.ToString()}\n";
        }

        _uiTxt.text = body;
    }

    void OnFail() {
        if(state != ObjectiveState.Started)
            return;
        state = ObjectiveState.Failed;
        RemoveObjectiveTracker();
        Debug.Log($"{objectiveName} failed");
    }

    public void FailObjective() {
        OnFail();
    }

    public void StartObjective(bool loading = false) {
        if(state != ObjectiveState.NotStarted) {
            Debug.LogWarning("Started objective in started/completed state");
        }
        state = ObjectiveState.Started;
        // Debug.Log("Started objective: " + objectiveName);
        if(!loading) {
            Feedback.I.SendLineQueue("Started objective: " + objectiveName, true);
            ProgressionManager.I.InvokeKeyActions(onStartActions);
        }
        objectiveChoice.text = string.IsNullOrEmpty(midChoiceLine) ? choiceLine : midChoiceLine;
        _progression.StartObjective(this);
        RegisterObjectiveTracker();
    }

    public void ProgressObjective() {
        if(state == ObjectiveState.Completed)
            return;
        if(state == ObjectiveState.NotStarted) {
            DialogueManager.I.ReplaceDialogue(begLines, 0);
            StartObjective();
            // if(!autoCompleteOnCondsMet)
            //     return;
        }
        if(state != ObjectiveState.Completed && completeConds.CheckCompleteReqs()) {
            CompleteObjective();
            DialogueManager.I.ReplaceDialogue(endLines, 0);
        }
        else {
            DialogueManager.I.ReplaceDialogue(midLines, 0);
        }
    }

    public void CompleteObjective(bool loading = false) {
        state = ObjectiveState.Completed;
        
        _dialogue?.RemoveChoice(objectiveChoice);
        
        // Debug.Log("Finished objective: " + objectiveName);
        if(!loading) {
            ProgressionManager.I.InvokeKeyActions(onCompletedActions);
            Feedback.I.SendLineQueue("Finished objective: " + objectiveName, 
        true);
            _progression.FinishObjective(this);
            RemoveObjectiveTracker();
            _progression.RegisterCompletedState(ObjectiveId);
            completeConds.FinalizeConditions();
            RewardItems(InventoryManager.I.mainInventory);
            nextObjective?.StartObjective();
        }
    }

    void RewardItems(Inventory inventory) {
        foreach(var i in rewardItems) {
            inventory.AddItemAmount(i.item, i.amount, true);
        }
    }
}
}