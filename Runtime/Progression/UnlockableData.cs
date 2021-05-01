
using UnityEngine;
// using UnityEngine.Events;
// using System.Collections.Generic;

namespace m4k.Progression {
[System.Serializable]
public class Unlockable {
    // [HideInInspector]
    public UnlockableData data;
    public System.Action onUnlock;
    public bool unlocked;

    public Unlockable(UnlockableData u, bool b) {
        data = u;
        unlocked = b;
    }

    public bool CheckConditions() {
        bool complete = data.conds.CheckCompleteReqs();
        if(complete) {
            onUnlock?.Invoke();
            unlocked = true;
        }
        return complete;
    }
    public void RegisterListener(UnlockableListener listener) {
        if(unlocked)
            listener.onConditionsMet?.Invoke();
        else
            onUnlock += listener.onConditionsMet.Invoke;
    }
}

[CreateAssetMenu(menuName="ScriptableObjects/UnlockableData")]
public class UnlockableData : ScriptableObject {
    public string description;
    // public string id;
    public Conditions conds;
}}