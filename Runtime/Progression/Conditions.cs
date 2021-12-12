
using UnityEngine;
using System.Collections.Generic;
using m4k.Items;
using m4k.Progression;

namespace m4k {
// TODO: bool operators for conditions
[System.Serializable]
public class Conditions  
{
    // public bool removeRequiredItems;
    // public SerializableDictionary<Item, int> requiredItems;

    // [Tooltip("Record goal within one record period interval(hour/day/etc). Resets at next record period interval")]
    // public SerializableDictionary<string, long> requiredRecordTemp;

    // [Tooltip("Record goal for sum of archived values and instance period value")]
    // public SerializableDictionary<string, long> requiredRecordTotal;

    // public List<string> requiredStates;
    // public List<Object> requiredStateObjs;

    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<Condition> conditions;
    public bool autoFinalize;
    
    [System.NonSerialized]
    public System.Action<Conditions> onChange;
    [System.NonSerialized]
    public System.Action onComplete;

    [System.NonSerialized]
    bool _alreadyFinalized;
    
    // public bool HasCompleted { get { return _completed; }}
    // [System.NonSerialized]
    // bool _completed = false;

    public void Init() {
        _alreadyFinalized = false;
    }

    // Listens to relevant onChange events to update condition completion status
    public void RegisterChangeListener() {
        UnregisterChangeListener();
        // if(requiredRecordTemp.Count > 0 || requiredRecordTotal.Count > 0)
        //     RecordManager.I.onChange += OnChange;
        // if(requiredItems.Count > 0)
        //     InventoryManager.I.mainInventory.onChange += OnChange;
        // if(requiredStates.Count > 0) 
        //     ProgressionManager.I.onRegisterCompletionState += OnChange;

        foreach(var c in conditions) {
            if(c is ConditionRecordSession || c is ConditionRecordTotal) {
                RecordManager.I.onChange -= OnChange;
                RecordManager.I.onChange += OnChange;
            }
            else if(c is ConditionItemCount) {
                InventoryManager.I.mainInventory.onChange -= OnChange;
                InventoryManager.I.mainInventory.onChange += OnChange;
            }
            else if(c is ConditionStringState || c is ConditionObjectNameState) {
                ProgressionManager.I.onRegisterCompletionState -= OnChange;
                ProgressionManager.I.onRegisterCompletionState += OnChange;
            }
        }

        OnChange();
    }
    public void UnregisterChangeListener() {
        RecordManager.I.onChange -= OnChange;
        ProgressionManager.I.onRegisterCompletionState -= OnChange;
        InventoryManager.I.mainInventory.onChange -= OnChange;
    }
    void OnChange() {
        onChange?.Invoke(this);
    }

    public bool CheckCompleteReqs() {
        for(int i = 0; i < conditions.Count; ++i) {
            if(!conditions[i].CheckConditionMet()) {
                return false;
            }
        }
        if(autoFinalize)
            FinalizeConditions();
        
        return true;
    }

    public void FinalizeConditions() {
        if(_alreadyFinalized) return;
        _alreadyFinalized = true;
        foreach(var c in conditions) {
            c.FinalizeCondition();
        }
        UnregisterChangeListener();
        onComplete?.Invoke();
    }
    
    // public bool CheckCompleteReqs() {        
    //     foreach(var i in requiredRecordTotal) {
    //         Record rec = RecordManager.I.GetOrCreateRecord(i.Key);

    //         if(rec.Sum < i.Value)
    //             return false;
    //     }

    //     foreach(var i in requiredRecordTemp) {
    //         Record rec = RecordManager.I.GetOrCreateRecord(i.Key);

    //         if(rec.sessionVal < i.Value)
    //             return false;
    //     }

    //     for(int i = 0; i < requiredStates.Count; ++i) {
    //         if(string.IsNullOrEmpty(requiredStates[i]))
    //             continue;
    //         if(!ProgressionManager.I.CheckCompletionState(requiredStates[i])) {
    //             return false;
    //         }
    //     }
    //     for(int i = 0; i < requiredStateObjs.Count; ++i) {
    //         if(requiredStateObjs[i] == null)
    //             continue;
    //         if(!ProgressionManager.I.CheckCompletionState(requiredStateObjs[i].name)) {
    //             return false;
    //         }
    //     }

    //     foreach(var i in requiredItems) {
    //         if(InventoryManager.I.mainInventory.GetItemTotalAmount(i.Key) < i.Value) {
    //             return false;
    //         }
    //     }
    //     if(removeRequiredItems)
    //         RemoveRequiredItems();

    //     UnregisterChangeListener();
    //     onComplete?.Invoke();
    //     return true;
    // }

    // void RemoveRequiredItems() {
    //     foreach(var i in requiredItems) {
    //         InventoryManager.I.mainInventory.RemoveItemAmount(i.Key, i.Value, true);
    //     }
    // }
}}