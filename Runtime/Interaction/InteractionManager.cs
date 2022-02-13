using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Interaction {
public class InteractionManager : Singleton<InteractionManager>
{
    public TMPro.TMP_Text interactTxt;
    public TMPro.TMP_Text cycleTxt;
    public bool interactBlocked = false;

    public bool CurrInteractEnforcesNeutral { get {
        if(!currInteractable) return false;
        return currInteractable.enforcePlayerNeutral;
    }}
    public Interactable CurrentInteractable { get { return currInteractable; }}

    List<Interactable> interactables = new List<Interactable>();
    Interactable currInteractable;
    int currIndex;

    public void RegisterInteractable(Interactable interactable) {
        interactables.Insert(0, interactable);
        currIndex = 0;
        if(interactables.Count > 1) {
            for(int i = 1; i < interactables.Count; ++i) {
                interactables[i].OnNonInteractable();
            }
        }
        UpdateCurrInteractable();
    }
    public void UnregisterInteractable(Interactable interactable) {
        // interactables.Remove(interactable);
        // interactables.TrimExcess();
        var temp = interactables.FindIndex(x=>x==interactable);
        if(temp != -1)
            interactables.RemoveAt(temp);
        currIndex = 0;
        UpdateCurrInteractable();
    }

    public void ClearInteractables() {
        interactables.Clear();
        UpdateCurrInteractable();
    }

    public void ToggleHideBlockInteractables(bool b) {
        interactBlocked = b;
        interactTxt.enabled = !b;
        cycleTxt.enabled = !b;
    }

    void UpdateCurrInteractable() {
        int count = interactables.Count;

        currInteractable = count > 0 ? interactables[currIndex] : null;

        interactTxt.text = count > 0 ? string.Format("F: {0}", currInteractable.description) : "";
        cycleTxt.text = count > 1 ? string.Format("X: Cycle {0} interactables", count) : "";
    }

    public void CycleInteractables() {
        if(interactables.Count < 2)
            return;

        interactables[currIndex].OnNonInteractable();
        currIndex = (currIndex + 1) % interactables.Count;
        interactables[currIndex].OnInteractable();
        // Debug.Log(currIndex);
        UpdateCurrInteractable();
    }

    public bool Interact(bool buttonDown = true) {
        if(!currInteractable || interactBlocked)
            return false;
        currInteractable.InteractInput(buttonDown);
        return true;
    }
}
}
// public interface IInteractable {
//     public void RegisterInteractable();
//     public void Interact();
// }}