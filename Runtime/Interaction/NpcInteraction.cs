using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Progression;
using m4k.Characters;

namespace m4k.Interaction {
[RequireComponent(typeof(Interactable))]
public class NpcInteraction : MonoBehaviour
{
    // public Character character;
    public Transform ikParent;
    public Dialogue dialogue;
    public Character character;
    CharacterIK characterIK;
    Interactable interactable;
    private void Start() {
        characterIK = ikParent ? ikParent.GetComponentInChildren<CharacterIK>() : transform.parent.GetComponentInChildren<CharacterIK>();
        interactable = GetComponentInChildren<Interactable>();
    }

    void SetLookAt() {
        if(!characterIK || !interactable.otherCol)
            return;
        // characterIK.EnableIk();
        // characterIK.SetLook(interactable.otherCol.GetComponentInChildren<PlayerController>().headTarget);
        characterIK.SetLook(CharacterManager.I.Player.charAnim.headHold);
    }
    void RemoveLookAt() {
        if(!characterIK)
            return;
        characterIK.SetLook(null);
    }
    public void OnInteractable() {
        SetLookAt();
        // Game.I.SetFocusTarget(characterIK.headTarget, true);
    }
    public void OnNonInteractable() {
        RemoveLookAt();
        // if(Game.I)
        //     Game.I.SetFocusTarget(null, false);
        EndDialogue();
    }

    public void StartDialogue() {
        // DialogueManager.I.AssignDialogue("1 Test coded line\n2 line 2");
        if(dialogue) {
            dialogue.AssignDialogue();
        }
    }
    void EndDialogue() {
        if(DialogueManager.I && dialogue) {
            dialogue.EndDialogue();
        }
    }
}
}