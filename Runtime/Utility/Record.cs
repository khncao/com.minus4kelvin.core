
using UnityEngine;
using System.Collections.Generic;

namespace m4k {
// [System.Serializable]
// public class RecordAction {
//     public static readonly RecordAction Transact = new RecordAction("transaction");
//     public static readonly RecordAction Craft = new RecordAction("craft");
//     public static readonly RecordAction Damage = new RecordAction("damaged");
//     public static readonly RecordAction Kill = new RecordAction("killed");

//     string name;
//     float value;
//     public RecordAction(string name, float value = 0f) {
//         this.name = name;
//         this.value = value;
//     }
//     public override string ToString() { return name; }
//     public float GetValue() { return value; }
// }

public enum RecordTags {
    None = 0,
    Transaction = 10, Give = 11, Get = 12,
    Combat = 20, Damage = 21, Kill = 22, 
    Craft = 30, 
    Gather = 40, Chop = 41, Mine = 42, 
    Place = 50, 
}

[System.Serializable]
public class Record {
    public string id;
    public string description;
    public string fromName, toName; // from-to, from, to
    public long sessionVal;
    public List<long> records = new List<long>();
    public System.Action<Record> onChange, onLog;

    public long Sum { get { 
        long accum = 0;
        foreach(var r in records)
            accum += r;
        return accum + sessionVal;
    }}

    public long Count { get { return records.Count + 1; }}

    public Record(string n) {
        id = n;
    }

    public void ModifyRuntimeValue(long change) {
        sessionVal += change;
        onChange?.Invoke(this);
    }

    public void Log() {
        records.Add(sessionVal);
        sessionVal = 0;
        onLog?.Invoke(this);
    }
}

// [System.Serializable]
// public class Aggregate {
//     public long count;
//     public long max;
//     public long min;
//     public long sum;
//     public float avg { get { return sum / count; }}

//     public void Update(long val) {
//         count++;
//         sum += val;
//         if(val > max)
//             max = val;
//         if(val < min)
//             min = val;
//     }
// }
}