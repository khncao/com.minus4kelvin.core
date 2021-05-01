using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using m4k.Progression;
using m4k.Characters;

namespace m4k.Interaction {
[System.Serializable]
public class ToggleUnityEvent : UnityEvent<bool> { }

[RequireComponent(typeof(SphereCollider))]
public class Interactable : MonoBehaviour
{
    public string id;
    public string description;
    public bool removeOnInteract, destroyOnInteract, mouseInteractable, keepProximity, isCompletionState;
    [System.Serializable]
    public class InteractableUnityEvents {
        public UnityEvent onInteractable, onInteract, onNonInteractable;
        public ToggleUnityEvent onInteractToggle;
    }
    public InteractableUnityEvents events;
    public Conditions conditions;
    public int interactCount = 0;
    public float interactCd = 0f;
    public Material highlightMat;
    public string playerAnim;
    public bool enforcePlayerNeutral;
    [System.NonSerialized]
    public Collider otherCol;
    public bool hasInteracted { get { return interactCount > 0; } }
    public bool isToggled { get { return interactCount % 2 != 0; }}
    Material[] origMats, tempMats;
    Renderer rend;
    Collider col;
    bool isInteractable, isOnCd;
    float cd;

    private void Start() {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        rend = GetComponentInChildren<Renderer>();
        if(rend && highlightMat) {
            origMats = rend.materials;
            tempMats = new Material[origMats.Length];
            for(int i = 0; i < tempMats.Length; ++i) {
                tempMats[i] = highlightMat;
            }
        }
        
        if(!string.IsNullOrEmpty(id))
            ProgressionManager.I.RegisterInteractable(this);
        if(string.IsNullOrEmpty(id))
            id = name;
        if(hasInteracted) {
            // events.onInteract?.Invoke();
            events.onInteractToggle?.Invoke(!isToggled);
        }
        if(string.IsNullOrEmpty(description)) {
            description = transform.parent.name;
        }
    }
    private void OnDisable() {
        InteractionManager.I?.UnregisterInteractable(this);
        OnNonInteractable();
    }

    // private void OnMouseEnter() {
    //     if(CharacterManager.I.Player.isPointOverObj)
    //         return;
    //     if(mouseInteractable)
    //         OnInteractable();
    // }
    // private void OnMouseUp() {
    //     // if(Characters.I.Player.playerInput.isPointOverObj)
    //     //     return;
    //     if(isInteractable) {
    //         // Interact();
    //         Characters.I.Player.navCharacterControl.SetTarget(transform);
    //     }
    //     Debug.Log("onmouseup: " + gameObject);
    // }
    // private void OnMouseExit() {
    //     if(CharacterManager.I.Player.isPointOverObj)
    //         return;
    //     if(mouseInteractable)
    //         OnNonInteractable();
    // }

    private void OnTriggerEnter(Collider other) {
        otherCol = other;
        InteractionManager.I.RegisterInteractable(this);
        OnInteractable();
    }

    private void OnTriggerExit(Collider other) {
        otherCol = null;
        InteractionManager.I.UnregisterInteractable(this);
        OnNonInteractable();
    }

    public void OnInteractable() {
        
        events.onInteractable?.Invoke();
        isInteractable = true;
        if(rend && highlightMat) {
            rend.materials = tempMats;
        }
    }
    public void OnNonInteractable() {
        
        events.onNonInteractable?.Invoke();
        isInteractable = false;
        if(rend && highlightMat) {
            rend.materials = origMats;
        }
    }

    bool CheckReqs() {
        return conditions.CheckCompleteReqs();
    }

    public bool Interact() {
        if(enforcePlayerNeutral && (!CharacterManager.I.Player.charAnim.IsMobile || !CharacterManager.I.Player.charAnim.IsNeutral)) {
            return false;
        }
        if(!CheckReqs()) {
            Feedback.I.SendLine("Requirements not met");
            return false;
        }
        if(isOnCd) {
            Feedback.I.SendLine(string.Format("{0} still in cooldown: {1} seconds", name, cd.ToString("F1")));
            return false;
        }

        interactCount++;
        events.onInteract?.Invoke();
        events.onInteractToggle?.Invoke(!isToggled);

        if(isCompletionState && interactCount == 1)
            ProgressionManager.I.RegisterCompletedState(id);

        if(interactCd > 0) {
            isOnCd = true;
            StartCoroutine(InteractCooldown());
        }

        if(!string.IsNullOrEmpty(playerAnim))
            CharacterManager.I.Player.charAnim.PlayAnimation(playerAnim);
        
        if(removeOnInteract || destroyOnInteract) {
            InteractionManager.I.UnregisterInteractable(this);
            if(!keepProximity)
                OnNonInteractable();
            else {
                isInteractable = false;
                if(rend && highlightMat) {
                    rend.materials = origMats;
                }
            }
        }
        
        if(destroyOnInteract) {
            gameObject.SetActive(false);
        }
        return true;
    }

    IEnumerator InteractCooldown() {
        cd = interactCd;
        while(cd > 0) {
            cd -= Time.deltaTime;
            yield return null;
        }
        isOnCd = false;
    }
}
}