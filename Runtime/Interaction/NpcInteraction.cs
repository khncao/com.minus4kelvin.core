using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Progression;
using m4k.Characters;

namespace m4k.Interaction {
[RequireComponent(typeof(Interactable))]
public class NpcInteraction : MonoBehaviour
{
    public CharacterControl cc;
    // public Transform ikParent;
    public Dialogue dialogue;

    Interactable interactable;
    private void Start() {
        interactable = GetComponentInChildren<Interactable>();
        if(!cc)
            cc = GetComponentInParent<CharacterControl>();
    }

    void SetLookAt() {
        if(!cc || !cc.iK)
            return;
        // cc.iK.EnableIk();
        // cc.iK.SetLook(interactable.otherCol.GetComponentInChildren<PlayerController>().headTarget);
        cc.iK.SetLook(CharacterManager.I.Player.charAnim.headHold);
    }
    void RemoveLookAt() {
        if(!cc || !cc.iK)
            return;
        cc.iK.SetLook(null);
    }
    public void OnInteractable() {
        SetLookAt();
        // Game.I.SetFocusTarget(cc.iK.headTarget, true);
    }
    public void OnNonInteractable() {
        RemoveLookAt();
        // if(Game.I)
        //     Game.I.SetFocusTarget(null, false);
        EndDialogue();
    }

    public void StartDialogue() {
        if(dialogue) {
            cc?.navChar.StopAgent();
            dialogue.AssignDialogue();
            DialogueManager.I.onEndDialogue += OnEndDialogue;
        }
    }
    void EndDialogue() {
        if(DialogueManager.I && dialogue) {
            dialogue.EndDialogue();
        }
    }

    void OnEndDialogue() {
        DialogueManager.I.onEndDialogue -= OnEndDialogue;
        cc?.navChar.ResumeLastTarget();
    }
}
}