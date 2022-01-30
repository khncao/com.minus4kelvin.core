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
    public string[] completedStates;
    public string[] objectivesInProgress;
    public SerializableDictionary<string, int> interactableStates;
}

public class ProgressionManager : Singleton<ProgressionManager>
{
    public ProgressionUI UI;
    public List<KeyAction> globalChoiceActions;
    public List<SceneController> sceneControllers;

    public System.Action onRegisterCompletionState;
    public System.Action<Interactable> onRegisterInteractable;

    HashSet<string> completedStates = new HashSet<string>();
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
        _achieveInv = new Inventory(16, 0, true);

        CheckAchievements();
        UI.achieveSlotManager.AssignInventory(_achieveInv);
    }

    public void CheckAchievements() {
        if(_achieves == null || _achieves.Count < 1) 
            return;
        foreach(var i in _achieves) {
            int unlocked = 0;
            if(i.Primary(null)) {;
                unlocked = 1;
                if(_achieveInv.GetItemTotalAmount(i) < 1)
                    _achieveInv.AddItemAmount(i, unlocked);
            }
        }
    }

    public void RegisterInteractable(Interactable interactable) {
        if(interactablesDict.ContainsKey(interactable.id)) {
            Debug.LogError($"Duplicate interactableid: {interactable.gameObject} - {interactable.id}");
        }
        else {
            // if previously serialized, load state
            if(_interactableStates.ContainsKey(interactable.id)) {
                int val;
                _interactableStates.TryGetValue(interactable.id, out val);
                interactable.interactCount = val;
            }

            interactablesDict.Add(interactable.id, interactable);
            onRegisterInteractable?.Invoke(interactable);
        }
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
            Debug.LogWarning($"Tried to register existing completed state: {stateName}");
            return;
        }
        Debug.Log("Completed state: " + stateName);
        completedStates.Add(stateName);
        onRegisterCompletionState?.Invoke();
    }
    public bool CheckCompletionState(string stateName) {
        return completedStates.Contains(stateName);
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

        foreach(var i in sceneControllers) {
            if(i.InvokeKeyEvent(key))
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

    public void RegisterSceneController(SceneController se) {
        if(!sceneControllers.Contains(se)) {
            sceneControllers.Add(se);
        }
    }

    public void UnregisterSceneController(SceneController se) {
        if(sceneControllers.Contains(se)) {
            sceneControllers.Remove(se);
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
        data.completedStates = completedStates.ToArray();
        data.objectivesInProgress = objectivesInProgress.ToArray();
    }
    
    public void Deserialize(ProgressionData data) {
        _interactableStates = data.interactableStates;

        completedStates = new HashSet<string>();
        foreach(var i in data.completedStates) 
            completedStates.Add(i);
        
        objectivesInProgress = new HashSet<string>();
        foreach(var i in data.objectivesInProgress)
            objectivesInProgress.Add(i);
    }
}
}