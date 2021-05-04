using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters.Customization;

namespace m4k.Characters {
[System.Serializable]
public class CharacterState {
    public string sceneName;
    public Character character;
    public GameObject instance;
}

public class CharacterManager : Singleton<CharacterManager>
{
    public CharacterUI UI;
    public CharacterControl Player;
    // { get { 
    //     if(!_Player) {
    //         GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterControl>().RegisterCharacter();
    //     }
    //     return _Player;
    // }}
    // public PlayerController Player;
    public List<Character> activeCharacters;
    // public List<PlayerController> players;
    // public System.Action<PlayerController> onPlayerRegistered;
    public System.Action<CharacterControl> onCharacterRegistered, onPlayerRegistered;
    public Character focused;

    Dictionary<Character, CharacterControl> charInstanceDict = new Dictionary<Character, CharacterControl>();
    // CharacterControl _Player;

    public void SetFocused(Character character) {
        focused = character;
        UI.SetCharacter(focused);
    }

    public void RegisterPlayer(Character character, CharacterControl inst) {
        if(RegisterCharacter(character, inst)) {
            Player = inst;
        }
    }

    // public void RegisterPlayer(PlayerController player) {
    //     if(Player != null) {
    //         Debug.LogError("Player already registered");
    //         return;
    //     }
    //     Player = player;
    //     onPlayerRegistered?.Invoke(Player);
    // }

    public CharacterControl GetCharInstance(string charName) {
        return GetCharInstance(AssetRegistry.I.GetCharacterFromName(charName));
    }
    public CharacterControl GetCharInstance(Character character) {
        CharacterControl chara = null;
        charInstanceDict.TryGetValue(character, out chara);
        if(!chara) Debug.LogWarning("character not found");
        return chara;
    }
    public bool RegisterCharacter(Character character, CharacterControl instance) {
        if(!activeCharacters.Contains(character)) {
            activeCharacters.Add(character);
            charInstanceDict.Add(character, instance);
            instance.charEquip?.Start();
            CharacterCustomize.I.ApplyCustomizationsOnCharacterSpawn(character, instance.gameObject);
            onCharacterRegistered?.Invoke(instance);

            // PlayerController player = instance.GetComponent<PlayerController>();
            // if(player && !players.Contains(player)) 
            //     players.Add(player);
            if(instance.CompareTag("Player")) {
                Player = instance;
                onPlayerRegistered?.Invoke(Player);
            }
                
            return true;
        }
        else {
            Debug.LogWarning("Active character duplicate");
            return false;
        }
    }
    public void RemoveCharacter(Character character) {
        var instance = GetCharInstance(character);
        if(!instance) return;

        // PlayerController player = instance.GetComponent<PlayerController>();
        // if(player && players.Contains(player)) 
        //     players.Remove(player);
            
        activeCharacters.Remove(character);
        charInstanceDict.Remove(character);

    }
}
}