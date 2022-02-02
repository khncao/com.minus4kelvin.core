
using System.Collections.Generic;

namespace m4k {

[System.Serializable]
public class Record {
    public string id;
    public bool display; // allow displaying ie stat pages
    public long sessionVal;
    public List<long> records = new List<long>();

    [System.NonSerialized]
    public System.Action<Record> onChange, onLog;
    
    public long Sum { get { 
        if(accum == 0) {
            foreach(var r in records)
                accum += r;
        }
        return accum + sessionVal;
    }}

    public long LastChange { get; private set; }

    long accum = 0; // hold running sum excluding sessionVal

    public Record(string n) {
        id = n;
    }

    public void UpdateSessionValue(long change) {
        LastChange = change;
        sessionVal += change;
        onChange?.Invoke(this);
    }

    // stores session value to saved log
    public void Log() {
        records.Add(sessionVal);
        accum += sessionVal;
        sessionVal = 0;
        onLog?.Invoke(this);
    }
}
}