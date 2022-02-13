using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using m4k.Progression;

namespace m4k.Interaction {
[System.Serializable]
public class ToggleUnityEvent : UnityEvent<bool> { }

public enum InteractableType {
    Dialogue = 0, Menu = 2,
    Use = 10,
    PickUp = 15,
    InteractOnSpawn = 100,
    ActiveConditionListener = 101,
}

public class Interactable : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class InteractableUnityEvents {
        [Header("OnInteractToggle is always called on load.\nEnable onInteractOnLoad to also call\nOnInteract events on load(interactCount>0)")]
        public ToggleUnityEvent onInteractToggle;
        public UnityEvent onInteractable, onInteract, onNonInteractable;
        public bool onInteractOnLoad;
    }
    [Header("If id not empty, save state and register as key state")]
    [SerializeField]
    string id;

    public string description;
    public InteractableType interactableType;

    [Header("Will destroy gameObject if root, else will destroy parent")]
    public bool destroyOnInteract;
    public InteractableUnityEvents events;
    
    public Conditions conditions;

    public float interactCooldown;
    public float holdTimeToInteract;

    [Header("Trigger parameter or state name")]
    public string interactorAnimation;
    public bool enforcePlayerNeutral; 

    [HideInInspector]
    public int interactCount;

    public bool IsInteractable { get { return _isInteractable; }}
    public bool HasInteracted { get { return interactCount > 0; } }
    public bool IsToggled { get { return interactCount % 2 != 0; }}
    public string Key { get; private set; }

    bool _isInteractable, _isRoot;
    float _lastInteractTime;

    private void Start() {
        if(interactableType != InteractableType.ActiveConditionListener 
        && interactableType != InteractableType.InteractOnSpawn
        && !TryGetComponent<Collider>(out var col)) {
            Debug.LogWarning("Non-self-triggering interactable does not have collider");
        }
        if(transform.root == transform) {
            _isRoot = true;
        }
        if(string.IsNullOrEmpty(description)) {
            description = _isRoot ? name : transform.parent.name;
        }
        if(!string.IsNullOrEmpty(id))
            Key = id;
        // else if(TryGetComponent<GuidComponent>(out var guidComponent))
        //     Key = guidComponent.GetGuid().ToString();

        if(!string.IsNullOrEmpty(Key))
            ProgressionManager.I.RegisterInteractable(this);

        if(HasInteracted) {
            events.onInteractToggle?.Invoke(!IsToggled);
            if(events.onInteractOnLoad)
                events.onInteract?.Invoke();
            if(destroyOnInteract)
                Destroy();
        }
        else {
            _lastInteractTime = -interactCooldown;

            if(interactableType == InteractableType.InteractOnSpawn)
                Interact();
            else if(interactableType == InteractableType.ActiveConditionListener) {
                conditions.RegisterChangeListener();
                conditions.onComplete += OnConditionsMet;
            }
        }
    }

    private void OnDisable() {
        if(InteractionManager.I)
            InteractionManager.I.UnregisterInteractable(this);
        OnNonInteractable();
        conditions.UnregisterChangeListener();
        conditions.onComplete -= OnConditionsMet;
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
        _isInteractable = true;
    }
    public void OnNonInteractable() {
        events.onNonInteractable?.Invoke();
        _isInteractable = false;
        InteractInput(false); // force hold input release on noninteractable
    }

    public bool Interact(GameObject go = null) {
        float timeSinceLastInteract = Time.time - _lastInteractTime;
        if(timeSinceLastInteract < interactCooldown) {
            Feedback.I.SendLine($"{description} cooldown: {(interactCooldown - timeSinceLastInteract).ToString("F1")} seconds remaining");
            return false;
        }
        if(!conditions.CheckCompleteReqs()) {
            Feedback.I.SendLine("Requirements not met");
            return false;
        }
        if(holdTimeToInteract > 0f) {
            if(holdInteractCR != null) {
                StopCoroutine(holdInteractCR);
                Debug.LogWarning("Unexpected nonnull coroutine");
            }
            holdInteractCR = StartCoroutine(HoldInteract(holdTimeToInteract, go));
            return false;
        }

        ProcessInteract(go);
        return true;
    }

    public void InteractInput(bool buttonDown) {
        if(buttonDown) {
            Interact();
        }
        else {
            if(holdInteractCR != null) {
                StopCoroutine(holdInteractCR);
                holdInteractCR = null;
                Feedback.I?.DisableHoldProgress();
            }
        }
    }

    void ProcessInteract(GameObject go) {
        interactCount++;
        events.onInteract?.Invoke();
        events.onInteractToggle?.Invoke(!IsToggled);
        _lastInteractTime = Time.time;
        
        if(interactCount == 1 && !string.IsNullOrEmpty(id)) {
            ProgressionManager.I.RegisterKeyState(id);
        }
        
        if(destroyOnInteract) {
            Destroy();
        }
    }

    Coroutine holdInteractCR;
    IEnumerator HoldInteract(float timer, GameObject go) {
        Feedback.I.EnableHoldProgress(timer);

        while(timer > 0f) {
            timer -= Time.deltaTime;
            // Debug.Log($"Hold left: {timer}");
            Feedback.I.UpdateHoldProgressSliderValue(timer);
            yield return null;
        }
        ProcessInteract(go);
        Feedback.I.DisableHoldProgress();
        holdInteractCR = null;
    }

    void Destroy() {
        if(_isRoot)
            Destroy(gameObject);
        else
            Destroy(transform.parent.gameObject);
    }
}
}