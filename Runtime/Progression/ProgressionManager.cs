// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using m4k.Interaction;
using m4k.Items;

namespace m4k.Progression {
[System.Serializable]
public class ProgressionData {
    public string[] keyStates;
    public string[] objectivesInProgress;
    public SerializableDictionary<string, int> interactableStates;
}

public class ProgressionManager : Singleton<ProgressionManager>
{
    public ProgressionUI UI;
    public List<KeyAction> globalChoiceActions;

    public System.Action onRegisterCompletionState;
    public System.Action<Interactable> onRegisterInteractable;

    HashSet<string> keyStates = new HashSet<string>();
    HashSet<string> objectivesInProgress = new HashSet<string>();

    Dictionary<string, Interactable> interactablesDict = new Dictionary<string, Interactable>();
    
    Inventory _achieveInv;
    List<Item> _achieves;
    Dictionary<string, UnityEvent> _globalChoiceActionsDict;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;

        _globalChoiceActionsDict = new Dictionary<string, UnityEvent>();
        foreach(var i in globalChoiceActions) {
            _globalChoiceActionsDict.Add(i.key, i.action);
        }
    }
    
    private void Start() {
        UI.Init(this);
        _achieves = AssetRegistry.I.GetItemListByType(typeof(ItemConditional));
        _achieveInv = new Inventory(16);
        _achieveInv.keepZeroItems = true;

        CheckAchievements();
        UI.achieveSlotManager.AssignInventory(_achieveInv);
    }

    public void CheckAchievements() {
        if(_achieves == null || _achieves.Count < 1) 
            return;
        foreach(var i in _achieves) {
            if(_achieveInv.GetItemTotalAmount(i) > 0)
                continue;

            if(i is ItemConditional itemCond 
            && itemCond.CheckConditions()) {
                _achieveInv.AddItemAmount(i, 1);
            }
            else {
                _achieveInv.AddItemAmount(i, 0);
            }
        }
    }

    public void RegisterInteractable(Interactable interactable) {
        if(interactablesDict.ContainsKey(interactable.Key)) {
            Debug.LogError($"Duplicate interactableid: {interactable.gameObject} - {interactable.Key}");
        }
        else {
            // if previously serialized, load state
            if(_interactableStates.ContainsKey(interactable.Key)) {
                int val;
                _interactableStates.TryGetValue(interactable.Key, out val);
                interactable.interactCount = val;
            }

            interactablesDict.Add(interactable.Key, interactable);
            onRegisterInteractable?.Invoke(interactable);
        }
    }

    public void StartObjective(Objective objective) {
        if(CheckKeyState(objective.ObjectiveId)) 
            return;
        objectivesInProgress.Add(objective.ObjectiveId);
    }
    public void FinishObjective(Objective objective) {
        objectivesInProgress.Remove(objective.ObjectiveId);
    }
    public bool CheckIfObjectiveInProgress(string objectiveId) {
        return objectivesInProgress.Contains(objectiveId);
    }

    public void RegisterKeyState(string stateName) {
        if(keyStates.Contains(stateName)) {
            Debug.LogWarning($"Tried to register existing key state: {stateName}");
            return;
        }
        Debug.Log("Completed state: " + stateName);
        keyStates.Add(stateName);
        onRegisterCompletionState?.Invoke();
    }
    public bool CheckKeyState(string stateName) {
        return keyStates.Contains(stateName);
    }

    public void InvokeKeyActions(List<string> keys) {
        foreach(var k in keys) {
            InvokeKeyActions(k);
        }
    }

    public void InvokeKeyActions(string key) {
        int invokeCount = 0;
        if(_globalChoiceActionsDict.ContainsKey(key)) {
            _globalChoiceActionsDict[key].Invoke();
            invokeCount++;
        }

        foreach(var i in SceneController.SceneControllers) {
            if(i.Value.InvokeKeyEvent(key))
                invokeCount++;
        }

        if(interactablesDict.ContainsKey(key)) {
            interactablesDict[key].Interact();
            invokeCount++;
        }

        if(invokeCount > 1) {
            Debug.LogWarning($"KeyAction {key} invoked more than once");
        }
    }

    SerializableDictionary<string, int> _interactableStates = new SerializableDictionary<string, int>();

    public void Serialize(ref ProgressionData data) {
        foreach(var i in interactablesDict) {
            if(_interactableStates.ContainsKey(i.Key))
                _interactableStates[i.Key] = i.Value.interactCount;
            else
                _interactableStates.Add(i.Key, i.Value.interactCount);
        }

        data.interactableStates = _interactableStates;
        data.keyStates = keyStates.ToArray();
        data.objectivesInProgress = objectivesInProgress.ToArray();
    }
    
    public void Deserialize(ProgressionData data) {
        _interactableStates = data.interactableStates;

        keyStates = new HashSet<string>();
        foreach(var i in data.keyStates) 
            keyStates.Add(i);
        
        objectivesInProgress = new HashSet<string>();
        foreach(var i in data.objectivesInProgress)
            objectivesInProgress.Add(i);
    }
}
}