
using UnityEngine;
using System.Collections.Generic;

namespace m4k {

[System.Serializable]
public class RecordData {
    public List<Record> records;
}

public class RecordManager : Singleton<RecordManager> {
    public TMPro.TMP_Text recordsTxt;

    public List<Record> records;

    [System.NonSerialized]
    public System.Action onChange;
    
    Dictionary<string, Record> recordsDict = new Dictionary<string, Record>();

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
    }

    public Record GetRecord(string id) {
        Record rec;
        recordsDict.TryGetValue(id, out rec);
        if(rec == null) {
            Debug.LogWarning($"Record with id {id} not found");
        }
        return rec;
    }

    public Record GetOrCreateRecord(string id, bool display = true) {
        Record rec;
        recordsDict.TryGetValue(id, out rec);
        if(rec == null) {
            rec = new Record(id);
            rec.display = display;
            recordsDict.Add(id, rec);
            records.Add(rec);
        }
        return rec;
    }

    public void UpdateValue(string id, long change) {
        Record rec = GetOrCreateRecord(id);

        rec.UpdateSessionValue(change);
        onChange?.Invoke();
        
        UpdateRecordsTxt();
    }

    // log all records; archive session values for time period tracking
    public void LogAll() {
        foreach(var r in records) {
            r.Log();
        }
    }

    public override string ToString() {
        string temp = "";
        for(int i = 0; i < records.Count; ++i) {
            if(!records[i].display) continue;
            temp += $"{records[i].id}: {records[i].Sum}\n";
        }
        return temp;
    }

    void UpdateRecordsTxt() {
        recordsTxt.text = ToString();
    }

    void InitializeDict() {
        recordsDict = new Dictionary<string, Record>();
        for(int i = 0; i < records.Count; ++i) {
            recordsDict.Add(records[i].id, records[i]);
        }
    }

    public void Serialize(RecordData data) {
        data.records = records;
    }

    public void Deserialize(RecordData data) {
        records = data.records;
        InitializeDict();
        UpdateRecordsTxt();
    }
}

}