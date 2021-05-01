
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace m4k.Progression {
public class UnlockableListener : MonoBehaviour {
    // public string unlockableId;
    public UnlockableData unlockableData;
    public UnityEvent onConditionsMet;

    private void Start() {
        Unlockable unlockable = ProgressionManager.I.GetUnlockable(unlockableData);

        if(unlockable == null) {
            Debug.LogWarning("No unlock instance found");
            return;
        }
        unlockable.RegisterListener(this);
    }


}}