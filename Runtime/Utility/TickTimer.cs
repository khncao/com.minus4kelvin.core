using UnityEngine;
using UnityEngine.Events;

namespace m4k {
[System.Serializable]
public class TickTimer: ISerializationCallbackReceiver {
    public int time;
    public System.Action onStart, onCancel;
    [System.NonSerialized]
    public UnityEvent onComplete = new UnityEvent();
    public System.Action<int> onChange;

    public TickTimer() { }
    public TickTimer(int initTime) {
        SetTimer(initTime);
    }
    public bool Running { get { return time > 0; }}
    
    public bool SetTimer(int amount) {
        if(time > 0) 
            return false;
        time = amount;
        RegisterTick();
        onStart?.Invoke();
        return true;
    }
    public void Tick(int ticks) {
        time--;
        onChange?.Invoke(time);
        if(time <= 0)
            CompleteTimer();
    }
    public void CancelTimer() {
        UnregisterTick();
        time = 0;
        onCancel?.Invoke();
        onChange = null;
    }
    public void CompleteTimer() {
        onComplete?.Invoke();
        UnregisterTick();
        time = 0;
    }

    public void OnLoad() {
        if(time <= 0)
            return;
        RegisterTick();
    }

    void RegisterTick() {
        GameTime.I.onTickTime -= Tick;
        GameTime.I.onTickTime += Tick;
    }
    void UnregisterTick() {
        GameTime.I.onTickTime -= Tick;
    }

    public void OnBeforeSerialize() {

    }

    public void OnAfterDeserialize() {
        OnLoad();
    }
}}