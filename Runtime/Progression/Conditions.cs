
using UnityEngine;
using System.Collections.Generic;
using m4k.InventorySystem;
using m4k.Progression;

namespace m4k {

[System.Serializable]
public class Conditions  
{
    public bool removeRequiredItems;
    public SerializableDictionary<Item, int> requiredItems;
    public SerializableDictionary<string, long> requiredRecordTemp; // record goal for record period interval(hour/day/)
    public SerializableDictionary<string, long> requiredRecordTotal;
    public List<string> requiredStates;

    public System.Action<Conditions> onChange;
    public System.Action onComplete;
    
    public bool HasCompleted { get { return completed; }}
    [System.NonSerialized]
    bool completed = false;

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
        if(completed) {
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

        foreach(var i in requiredItems) {
            if(InventoryManager.I.mainInventory.GetItemTotalAmount(i.Key) < i.Value) {
                return false;
            }
        }
        if(removeRequiredItems)
            RemoveRequiredItems();

        completed = true;
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