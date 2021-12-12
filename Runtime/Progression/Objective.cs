using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;

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

    [Tooltip("Auto complete with no further interaction on conditions met")]
    public bool autoCompleteOnCondsMet;

    [Tooltip("On conditions met, objective choice is registered to interactable dialogue. If auto start enabled, actively listens for conditions met")]
    public Conditions startConds;
    public Conditions completeConds, failConds;
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
    }

    // GUI hud objective tracker entry
    void RegisterObjectiveTracker() {
        var uiInstance = _progression.UI.InstantiateGetObjectiveTracker();
        _uiTxt = uiInstance.GetComponent<TMPro.TMP_Text>();

        completeConds.RegisterChangeListener();
        failConds.RegisterChangeListener();
        completeConds.onChange += UpdateObjectiveUI;
        failConds.onChange += UpdateObjectiveUI;
        failConds.onComplete += OnFail;

        UpdateObjectiveUI();
    }

    void RemoveObjectiveTracker() {
        if(_uiTxt)
            Destroy(_uiTxt.gameObject);
        completeConds.UnregisterChangeListener();
        failConds.UnregisterChangeListener();
        completeConds.onChange -= UpdateObjectiveUI;
        failConds.onChange -= UpdateObjectiveUI;
        failConds.onComplete -= OnFail;
    }

    void UpdateObjectiveUI(Conditions conds = null) {
        if(!_uiTxt)
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
            string col = _progression.CheckCompletionState(i) ? "green" : "white";
            body += $"    <color={col}>- {i}</color>\n";
        }

        foreach(var i in completeConds.requiredItems) {
            if(!i.Key) continue;
            var currVal = InventoryManager.I.mainInventory.GetItemTotalAmount(i.Key);
            string col = currVal < i.Value ? "white" : "green";
            body += $"    <color={col}>- {i.Key.itemName}: {currVal}/{i.Value}</color>\n";
        }
        _uiTxt.text = body;
    }

    void OnFail() {
        // onFail?.Invoke();
        RemoveObjectiveTracker();
        Debug.Log($"{objectiveName} failed");
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
        objectiveChoice.text = string.IsNullOrEmpty(midChoiceLine) ? choiceLine : midChoiceLine;
        _progression.StartObjective(this);
        RegisterObjectiveTracker();
        // onStart?.Invoke();
        ProgressionManager.I.InvokeKeyActions(onStartActions);
    }

    public void ProgressObjective() {
        if(state == ObjectiveState.Completed)
            return;
        if(state == ObjectiveState.NotStarted) {
            DialogueManager.I.ReplaceDialogue(begLines, 0);
            StartObjective();
            if(!autoCompleteOnCondsMet)
                return;
        }
        bool completed = completeConds.CheckCompleteReqs();
        if(completed) {
            CompleteObjective();
            DialogueManager.I.ReplaceDialogue(endLines, 0);
            RewardItems(InventoryManager.I.mainInventory);
        }
        else {
            DialogueManager.I.ReplaceDialogue(midLines, 0);
        }
    }

    public void CompleteObjective(bool loading = false) {
        state = ObjectiveState.Completed;
        // onCompleted?.Invoke();
        ProgressionManager.I.InvokeKeyActions(onCompletedActions);
        _dialogue?.RemoveChoice(objectiveChoice);
        
        // Debug.Log("Finished objective: " + objectiveName);
        if(!loading) {
            Feedback.I.SendLineQueue("Finished objective: " + objectiveName, 
        true);
            _progression.FinishObjective(this);
            RemoveObjectiveTracker();
            _progression.RegisterCompletedState(ObjectiveId);
        }

        nextObjective?.StartObjective();
    }

    void RewardItems(Inventory inventory) {
        foreach(var i in rewardItems) {
            inventory.AddItemAmount(i.item, i.amount, true);
        }
    }
}
}