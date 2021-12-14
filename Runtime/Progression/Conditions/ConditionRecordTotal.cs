using System;
using UnityEngine;

namespace m4k.Progression {
// Record goal for sum of archived values and instance period value
[Serializable]
public class ConditionRecordTotal : Condition {
    public string key;
    public ComparisonType op;
    public long val;

    string _lastCheckStatus = "";

    public override bool CheckConditionMet() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return false;
        }
        Record rec = RecordManager.I.GetOrCreateRecord(key);
        
        return Comparisons.Compare(op, rec.Sum, val);
    }

    public override string ToString() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return "";
        }

        Record rec = RecordManager.I.GetOrCreateRecord(key);
        
        string col = rec.Sum < val ? "white" : "green";
        _lastCheckStatus = $"<color={col}>- {rec.id}: {rec.Sum}/{val}</color>";
        return _lastCheckStatus;
    }
}
}