using System;
using UnityEngine;

namespace m4k {
// Record goal within one record period interval(hour/day/etc). Resets at next record period interval
[Serializable]
public class ConditionRecordSession : Condition {
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
        
        return Comparisons.Compare(op, rec.sessionVal, val);
    }

    public override string ToString() {
        if(string.IsNullOrEmpty(key)) {
            Debug.LogError("Key empty");
            return "";
        }

        Record rec = RecordManager.I.GetOrCreateRecord(key);
        
        string col = rec.sessionVal < val ? "white" : "green";
        _lastCheckStatus = $"<color={col}>- {rec.id}: {rec.sessionVal}/{val}</color>";
        return _lastCheckStatus;
    }

    public override void RegisterListener(Conditions conditions) {
        RecordManager.I.onChange -= conditions.OnChange;
        RecordManager.I.onChange += conditions.OnChange;
    }

    public override void UnregisterListener(Conditions conditions) {
        RecordManager.I.onChange -= conditions.OnChange;
    }
}
}