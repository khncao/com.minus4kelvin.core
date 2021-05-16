using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Characters {
public class CharacterControl : MonoBehaviour
{
    public Character character;
    public CharacterAnimation charAnim;
    public CharacterEquipment charEquip;
    public CharacterIK iK;
    public NavCharacterControl navChar;
    public RigidbodyCharacterController rbChar;
    public float moveMult = 1f;

    private void Start() {
        if(!character) {
            // Debug.Log("Character has no character item");
            return;
        }
        OnEnable();
    }
    private void OnEnable() {
        if(character && CharacterManager.I)
            CharacterManager.I.RegisterCharacter(character, this);
    }
    private void OnDisable() {
        if(character && CharacterManager.I)
            CharacterManager.I.RemoveCharacter(character);
    }
}
}