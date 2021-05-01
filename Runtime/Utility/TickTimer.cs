// using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TickTimer {
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
    public void EndTimer() {
        UnregisterTick();
        time = 0;
        onCancel?.Invoke();
    }
    public void CompleteTimer() {
        onComplete?.Invoke();
        EndTimer();
    }

    public void OnLoad() {
        if(time <= 0)
            return;
        RegisterTick();
    }

    void RegisterTick() {
        // Game.I.tick += Tick;
    }
    void UnregisterTick() {
        // Game.I.tick -= Tick;
    }
}