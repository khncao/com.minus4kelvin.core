using System;
using UnityEngine;

namespace m4k.Progression {
// Record goal within one record period interval(hour/day/etc). Resets at next record period interval
[Serializable]
public class ConditionRecordSession : Condition {
    public string key;
    public ComparisonType op;
    public long val;

    string _lastCheckStatus = "";

    public override bool CheckConditionMet() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogWarning($"Key empty: {key}");
        }
        Record rec = RecordManager.I.GetOrCreateRecord(key);
        
        return Comparisons.Compare(op, rec.sessionVal, val);
    }

    public override string ToString() {
        if(string.IsNullOrEmpty(key))
            return "";

        Record rec = RecordManager.I.GetOrCreateRecord(key);
        
        string col = rec.sessionVal < val ? "white" : "green";
        _lastCheckStatus = $"<color={col}>- {rec.id}: {rec.sessionVal}/{val}</color>";
        return _lastCheckStatus;
    }
}
}