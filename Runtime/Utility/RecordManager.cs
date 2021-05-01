
using UnityEngine;
using System.Collections.Generic;

namespace m4k {
public class RecordManager : Singleton<RecordManager> {
    public List<Record> records;
    public System.Action<string, long> onChange;
    
    Dictionary<string, Record> statsDict = new Dictionary<string, Record>();

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
    }

    public Record GetRecord(string statName) {
        Record rec;
        statsDict.TryGetValue(statName, out rec);
        if(rec == null) {
            rec = new Record(statName);
            statsDict.Add(statName, rec);
            records.Add(rec);
        }
        return rec;
    }

    public void UpdateValue(string statName, long change) {
        Record rec = GetRecord(statName);

        rec.ModifyRuntimeValue(change);
        onChange?.Invoke(statName, change);
    }

    public void Log(string statName, long val) {
        Record rec = GetRecord(statName);
        if(rec != null)
            rec.Log();
    }

    public void LogAll() {
        foreach(var r in records) {
            r.Log();
        }
    }

    public Record Peek() {
        if(records.Count < 1) {
            Debug.LogError("No records");
            return null;
        }
        return records[records.Count - 1];
    }
}

}