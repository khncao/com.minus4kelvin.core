// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using m4k.Interaction;
using m4k.InventorySystem;

namespace m4k.Progression {
[System.Serializable]
public class ProgressionData {
    public string[] completedStates;
    public string[] objectivesInProgress;
    public InteractableState[] interactableStates;
    public DialogueState[] dialogueStates;
}

[System.Serializable]
public class DialogueState {
    public string dialogueId;
    public string persistConvoId;

    [System.NonSerialized]
    public Dialogue dialogue;
}
[System.Serializable]
public class InteractableState {
    public string interactableId;
    public int interactCount;

    [System.NonSerialized]
    public Interactable interactable;
}

public class ProgressionManager : Singleton<ProgressionManager>
{
    public ProgressionUI UI;
    public System.Action onRegisterCompletionState;
    public System.Action<Interactable> onRegisterInteractable;

    // List<Unlockable> unlockables = new List<Unlockable>();
    List<Objective> objectives = new List<Objective>();

    // structures for faster lookups. keys serialized as simple string arrays
    HashSet<string> completedStates = new HashSet<string>();
    HashSet<string> objectivesInProgress = new HashSet<string>();
    Dictionary<string, InteractableState> interactableStatesDict = new Dictionary<string, InteractableState>();
    Dictionary<string, DialogueState> dialogueStatesDict = new Dictionary<string, DialogueState>();
    
    string[] _completedStates;
    string[] _objectivesInProgress;
    InteractableState[] _interactableStates;
    DialogueState[] _dialogueStates;
    bool hasState;
    Inventory achieveInv;
    List<Item> achieves;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
    }
    
    private void Start() {
        UI.Init(this);
        achieves = AssetRegistry.I.GetItemListByType(ItemType.Achievement);
        achieveInv = new Inventory(16, 0, true);

        foreach(var i in achieves) {
            int unlocked = 0;
            if(i.conditions.CheckCompleteReqs()) {;
                unlocked = 1;
            }
            achieveInv.AddItemAmount(i, unlocked);
        }
        UI.achieveSlotManager.AssignInventory(achieveInv);
    }

    public void CheckAchievements() {
        foreach(var i in achieves) {
            int unlocked = 0;
            if(i.conditions.CheckCompleteReqs()) {;
                unlocked = 1;
                if(achieveInv.GetItemTotalAmount(i) < 1)
                    achieveInv.AddItemAmount(i, unlocked);
            }
        }
    }

    public void RegisterInteractable(Interactable interactable) {
        if(!interactableStatesDict.ContainsKey(interactable.id)) {
            var newState = new InteractableState();
            newState.interactable = interactable;
            newState.interactableId = interactable.id;
            newState.interactCount = interactable.interactCount;

            interactableStatesDict[interactable.id] = newState;
            // interactables.Add(interactable);
        }
        else {
            InteractableState state;
            interactableStatesDict.TryGetValue(interactable.id, out state);
            state.interactable = interactable;
            interactable.interactCount = state.interactCount;
            interactableStatesDict[interactable.id] = state;
        }
        onRegisterInteractable?.Invoke(interactable);
    }
    public InteractableState GetInteractableState(string id) {
        InteractableState state;
        interactableStatesDict.TryGetValue(id, out state);
        return state;
    }

    public void RegisterDialogue(Dialogue dialogue) {
        if(!dialogueStatesDict.ContainsKey(dialogue.dialogueId)) {
            var newState = new DialogueState();
            newState.dialogue = dialogue;
            newState.dialogueId = dialogue.dialogueId;
            newState.persistConvoId = dialogue.persistConvoId;

            dialogueStatesDict[dialogue.dialogueId] = newState;
        }
        else {
            var state = dialogueStatesDict[dialogue.dialogueId];
            state.dialogue = dialogue;
            dialogue.persistConvoId = state.persistConvoId;
            dialogueStatesDict[dialogue.dialogueId] = state;
        }
    }

    public void RegisterObjective(Objective objective) {
        if(!objectives.Contains(objective))
            objectives.Add(objective);
    }
    public void UnregisterObjective(Objective objective) {
        objectives.Remove(objective);
    }

    public void StartObjective(Objective objective) {
        if(CheckCompletionState(objective.ObjectiveId)) 
            return;
        objectivesInProgress.Add(objective.ObjectiveId);
    }
    public void FinishObjective(Objective objective) {
        objectivesInProgress.Remove(objective.ObjectiveId);
    }
    public bool CheckIfObjectiveInProgress(string objectiveId) {
        return objectivesInProgress.Contains(objectiveId);
    }

    public void RegisterCompletedState(string stateName) {
        if(completedStates.Contains(stateName)) {
            Debug.LogWarning("same string complete state register attempt");
            return;
        }
        Debug.Log("Completed state: " + stateName);
        completedStates.Add(stateName);
        // CheckUnlockConditions();
        onRegisterCompletionState?.Invoke();
    }
    public bool CheckCompletionState(string stateName) {
        return completedStates.Contains(stateName);
    }
    // public bool CheckCompletionStates(string[] stateNames) {
    //     for(int i = 0; i < stateNames.Length; ++i) {
    //         if(string.IsNullOrEmpty(stateNames[i]))
    //             continue;
    //         if(!completedStates.Contains(stateNames[i])) {
    //             return false;
    //         }
    //     }
    //     return true;
    // }

    // void CheckUnlockConditions(bool loading = false) {
    //     for(int i = 0; i < unlockables.Count; ++i) {
    //         if(string.IsNullOrEmpty(unlockables[i].name)) {
    //             Debug.LogError("Invalid id");
    //             continue;
    //         }
    //         if(unlockables[i].unlocked) 
    //             continue;

    //         // if(!completedStates.Contains(unlockables[i].name)) {
    //             if(!unlockables[i].CheckConditions()) 
    //                 continue;
    //         // }
    //         if(!loading) {
    //             completedStates.Add(unlockables[i].name);
    //             Feedback.I.SendLineQueue(string.Format("Unlocked {0}", unlockables[i].description), true);
    //         }
    //     }
    // }
    // public bool CheckUnlocked(UnlockableData unlockableData) {
    //     var existing = unlockables.FindAll(x=>x.data == unlockableData);
    //     foreach(var x in existing) {
    //         if(!x.unlocked)
    //             return false;
    //     }
    //     return true;
    // }
    // public Unlockable GetUnlockable(UnlockableData data) {
    //     return unlockables.Find(x=>x.data == data);
    // }

    public void Serialize(ref ProgressionData data) {
        hasState = true;
        _completedStates = completedStates.ToArray();

        _objectivesInProgress = objectivesInProgress.ToArray();

        _interactableStates = interactableStatesDict.Values.ToArray();
        for(int i = 0; i < _interactableStates.Length; ++i) {
            var state = _interactableStates[i];
            if(_interactableStates[i].interactable)
                state.interactCount = _interactableStates[i].interactable.interactCount;
            _interactableStates[i] = state;
        }

        _dialogueStates = dialogueStatesDict.Values.ToArray();
        for(int i = 0; i < _dialogueStates.Length; ++i) {
            if(string.IsNullOrEmpty(_dialogueStates[i].dialogueId)) continue;
            var state = _dialogueStates[i];
            if(_dialogueStates[i].dialogue)
                state.persistConvoId = _dialogueStates[i].dialogue.persistConvoId;
            _dialogueStates[i] = state;
        }

        data.completedStates = _completedStates;
        data.objectivesInProgress = _objectivesInProgress;
        data.interactableStates = _interactableStates;
        data.dialogueStates = _dialogueStates;
    }
    
    public void Deserialize(ProgressionData data) {
        _completedStates = data.completedStates;
        _objectivesInProgress = data.objectivesInProgress;
        _interactableStates = data.interactableStates;
        _dialogueStates = data.dialogueStates;

        hasState = true;
        completedStates = new HashSet<string>();
        foreach(var i in _completedStates) 
            completedStates.Add(i);
        
        objectivesInProgress = new HashSet<string>();
        foreach(var i in _objectivesInProgress)
            objectivesInProgress.Add(i);

        interactableStatesDict = new Dictionary<string, InteractableState>();
        foreach(var i in _interactableStates)
            interactableStatesDict.Add(i.interactableId, i);

        dialogueStatesDict = new Dictionary<string, DialogueState>();
        foreach(var i in _dialogueStates)
            dialogueStatesDict.Add(i.dialogueId, i);

        // CheckUnlockConditions(true);
    }
}
}