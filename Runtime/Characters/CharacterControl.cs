﻿using System.Collections;
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
            Debug.Log("Character has no character item");
            return;
        }
        if(character && !CharacterManager.I.RegisterCharacter(character, this))
            return;
    }
    private void OnDisable() {
        if(character && CharacterManager.I)
            CharacterManager.I.RemoveCharacter(character);
    }

    private void Update() {
        
    }
}
}