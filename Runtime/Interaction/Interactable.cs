using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using m4k.Progression;
// using m4k.Characters;

namespace m4k.Interaction {
[System.Serializable]
public class ToggleUnityEvent : UnityEvent<bool> { }

public enum InteractableType {
    Dialogue = 0, Menu = 2,
    Use = 10, // UseDestroy = 11,
    PickUp = 15, // PickUpDestroy = 16,
    ConditionsOnSpawn = 100,
    ActiveConditionListener = 101,
}

public class Interactable : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class InteractableUnityEvents {
        public UnityEvent onInteractable, onInteract, onNonInteractable;
        public ToggleUnityEvent onInteractToggle;
    }
    [Tooltip("If id is not empty, will register with ProgressionManager for serializing interactCount. On first interact, will register id as key state")]
    public string id;
    public string description;
    public InteractableType interactableType;
    public bool destroyOnInteract;
    public InteractableUnityEvents events;
    public Conditions conditions;
    public float interactCd;
    public Material highlightMat;
    public string playerAnim; // anim name to play on interacting actor
    public bool enforcePlayerNeutral; 

    [HideInInspector]
    public int interactCount;

    public bool HasInteracted { get { return interactCount > 0; } }
    public bool IsToggled { get { return interactCount % 2 != 0; }}

    Material[] origMats, tempMats;
    Renderer rend;
    Collider col;
    bool isInteractable, isOnCd, _isRoot;
    float cd;

    private void Start() {
        col = GetComponent<Collider>();
        rend = GetComponentInChildren<Renderer>();
        if(rend && highlightMat) {
            origMats = rend.materials;
            tempMats = new Material[origMats.Length];
            for(int i = 0; i < tempMats.Length; ++i) {
                tempMats[i] = highlightMat;
            }
        }
        if(interactableType != InteractableType.ActiveConditionListener 
        && interactableType != InteractableType.ConditionsOnSpawn
        && !col) {
            Debug.LogWarning("Non-self-triggering interactable does not have collider");
        }
        if(transform.root == transform) {
            _isRoot = true;
        }
        if(string.IsNullOrEmpty(description)) {
            description = _isRoot ? name : transform.parent.name;
        }
        if(!string.IsNullOrEmpty(id))
            ProgressionManager.I.RegisterInteractable(this);

        if(HasInteracted) {
            events.onInteractToggle?.Invoke(!IsToggled);
        }
        else {
            if(interactableType == InteractableType.ConditionsOnSpawn)
                Interact();
            else if(interactableType == InteractableType.ActiveConditionListener) {
                conditions.RegisterChangeListener();
                conditions.onComplete += OnConditionsMet;
            }
        }
    }
    private void OnDisable() {
        InteractionManager.I?.UnregisterInteractable(this);
        OnNonInteractable();
    }

    private void OnTriggerEnter(Collider other) {
        InteractionManager.I.RegisterInteractable(this);
        OnInteractable();
    }

    private void OnTriggerExit(Collider other) {
        InteractionManager.I.UnregisterInteractable(this);
        OnNonInteractable();
    }

    void OnConditionsMet() {
        Interact();
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

    public bool Interact(GameObject go = null) {
        // if(enforcePlayerNeutral && (!CharacterManager.I.Player.charAnim.IsMobile || !CharacterManager.I.Player.charAnim.IsNeutral)) {
        //     return false;
        // }
        if(!conditions.CheckCompleteReqs()) {
            Feedback.I.SendLine("Requirements not met");
            return false;
        }
        if(isOnCd) {
            Feedback.I.SendLine(string.Format("{0} still in cooldown: {1} seconds", name, cd.ToString("F1")));
            return false;
        }

        interactCount++;
        events.onInteract?.Invoke();
        events.onInteractToggle?.Invoke(!IsToggled);
        
        if(interactCount == 1 && !string.IsNullOrEmpty(id)) {
            ProgressionManager.I.RegisterCompletedState(id);
        }

        if(interactCd > 0) {
            isOnCd = true;
            StartCoroutine(InteractCooldown());
        }

        // if(!string.IsNullOrEmpty(playerAnim))
        //     CharacterManager.I.Player.charAnim.PlayAnimation(playerAnim);
        
        if(destroyOnInteract) {
            InteractionManager.I.UnregisterInteractable(this);
            OnNonInteractable();
            if(_isRoot)
                Destroy(gameObject);
            else
                Destroy(transform.parent.gameObject);
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