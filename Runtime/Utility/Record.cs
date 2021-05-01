
using UnityEngine;
using System.Collections.Generic;

namespace m4k {
[System.Serializable]
public enum RecordTags {
    Business = 10,
    Combat = 20,
    Craft = 30,
    Gather = 40,
}

[System.Serializable]
public class Record {
    public string id;
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