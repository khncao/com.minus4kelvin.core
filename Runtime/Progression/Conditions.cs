
using UnityEngine;
using System.Collections.Generic;
using m4k.InventorySystem;
using m4k.Progression;

namespace m4k {
// TODO: condition serializereference; ToString; bool operators
[System.Serializable]
public class Conditions  
{
    public bool removeRequiredItems;
    public SerializableDictionary<Item, int> requiredItems;

    [Tooltip("Record goal within one record period interval(hour/day/etc). Resets at next record period interval")]
    public SerializableDictionary<string, long> requiredRecordTemp;

    [Tooltip("Record goal for sum of archived values and instance period value")]
    public SerializableDictionary<string, long> requiredRecordTotal;

    public List<string> requiredStates;
    public List<Object> requiredStateObjs;

    // [SerializeReference]
    // public List<Condition> conditions;

    public System.Action<Conditions> onChange;
    public System.Action onComplete;
    
    public bool HasCompleted { get { return _completed; }}
    [System.NonSerialized]
    bool _completed = false;

    // Listens to relevant onChange events to update condition completion status
    public void RegisterChangeListener() {
        UnregisterChangeListener();
        if(requiredRecordTemp.Count > 0 || requiredRecordTotal.Count > 0)
            RecordManager.I.onChange += OnChange;
        if(requiredItems.Count > 0)
            InventoryManager.I.mainInventory.onChange += OnChange;
        if(requiredStates.Count > 0) 
            ProgressionManager.I.onRegisterCompletionState += OnChange;
    }
    public void UnregisterChangeListener() {
        RecordManager.I.onChange -= OnChange;
        ProgressionManager.I.onRegisterCompletionState -= OnChange;
        InventoryManager.I.mainInventory.onChange -= OnChange;
    }
    void OnChange() {
        onChange?.Invoke(this);
        CheckCompleteReqs();
    }

    
    public bool CheckCompleteReqs() {
        if(_completed) {
            return true;
        }
        
        foreach(var i in requiredRecordTotal) {
            Record rec = RecordManager.I.GetOrCreateRecord(i.Key);

            if(rec.Sum < i.Value)
                return false;
        }

        foreach(var i in requiredRecordTemp) {
            Record rec = RecordManager.I.GetOrCreateRecord(i.Key);

            if(rec.sessionVal < i.Value)
                return false;
        }

        for(int i = 0; i < requiredStates.Count; ++i) {
            if(string.IsNullOrEmpty(requiredStates[i]))
                continue;
            if(!ProgressionManager.I.CheckCompletionState(requiredStates[i])) {
                return false;
            }
        }
        for(int i = 0; i < requiredStateObjs.Count; ++i) {
            if(requiredStateObjs[i] == null)
                continue;
            if(!ProgressionManager.I.CheckCompletionState(requiredStateObjs[i].name)) {
                return false;
            }
        }

        foreach(var i in requiredItems) {
            if(InventoryManager.I.mainInventory.GetItemTotalAmount(i.Key) < i.Value) {
                return false;
            }
        }
        if(removeRequiredItems)
            RemoveRequiredItems();

        _completed = true;
        UnregisterChangeListener();
        onComplete?.Invoke();
        return true;
    }

    void RemoveRequiredItems() {
        foreach(var i in requiredItems) {
            InventoryManager.I.mainInventory.RemoveItemAmount(i.Key, i.Value, true);
        }
    }
}}