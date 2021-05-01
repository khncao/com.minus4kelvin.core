
using UnityEngine;
using System.Collections.Generic;
using m4k.InventorySystem;
using m4k.Progression;

namespace m4k {
[System.Serializable]
public class RecordGoal {
    public string id;
    public long goal;
    public Record record;
}

[System.Serializable]
public class Conditions  
{
    public List<string> requiredStates;
    public List<RecordGoal> requiredCounts;
    public List<ItemInstance> requiredItems;
    public bool removeRequiredItems;
    public System.Action<Conditions> onCheck, onComplete;
    
    public bool hasCompleted { get { return completed; }}
    [System.NonSerialized]
    bool completed = false;

    public void RegisterCheckConditions() {
        if(requiredItems.Count > 0)
            InventoryManager.I.mainInventory.onChange += CheckCompletion;
        if(requiredStates.Count > 0) 
            ProgressionManager.I.onRegisterCompletionState += CheckCompletion;
    }
    void CheckCompletion() {
        CheckCompleteReqs();
    }
    public bool CheckCompleteReqs() {
        if(completed) {
            return true;
        }
        onCheck?.Invoke(this);
        
        // foreach(var i in requiredCounts) {
        //     if(i.record. < i.goal)
        //         return false;
        // }

        if(!ProgressionManager.I.CheckCompletionStates(requiredStates.ToArray())) {
            return false;
        }

        foreach(var i in requiredItems) {
            if(InventoryManager.I.mainInventory.GetItemTotalAmount(i.item) < i.amount) {
                return false;
            }
        }
        if(removeRequiredItems)
            RemoveRequiredItems();

        ProgressionManager.I.onRegisterCompletionState -= CheckCompletion;
        InventoryManager.I.mainInventory.onChange -= CheckCompletion;

        completed = true;
        onComplete?.Invoke(this);
        return true;
    }

    void RemoveRequiredItems() {
        for(int i = 0; i < requiredItems.Count; ++i) {
            InventoryManager.I.mainInventory.RemoveItemAmount(requiredItems[i].item, requiredItems[i].amount, true);
        }
    }
}}