using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters.Customization;

namespace m4k.Characters {
[System.Serializable]
public class CharacterState {
    public string name; // Character.name SO filename
    public int impression;
    // status, states
    public CharacterState(string n) {
        name = n;
    }
}

[System.Serializable]
public class CharacterData {
    public SerializableDictionary<string, CharacterState> characterStates;
}

public class CharacterManager : Singleton<CharacterManager>
{
    public CharacterUI UI;
    public CharacterControl Player;
    public List<Character> activeCharacters;
    // public List<PlayerController> players;
    public Character focused;
    public GameObject playerPrefab;
    public int minImpression = -100, maxImpression = 100;

    // public System.Action<PlayerController> onPlayerRegistered;
    public System.Action<CharacterControl> onCharacterRegistered, onPlayerRegistered;

    Dictionary<Character, CharacterControl> charInstanceDict = new Dictionary<Character, CharacterControl>();
    SerializableDictionary<string, CharacterState> _characterStates = new SerializableDictionary<string, CharacterState>();

    public void SetFocused(Character character) {
        focused = character;
        UI.SetCharacter(focused);
    }

    public CharacterState TryGetCharacterState(string charName) {
        CharacterState state = null;
        _characterStates.TryGetValue(charName, out state);
        return state;
    }

    public bool AddCharacterImpression(Character character, int amount) => AddCharacterImpression(character.name, amount);

    public bool AddCharacterImpression(string charName, int amount) {
        CharacterState state = null;
        _characterStates.TryGetValue(charName, out state);
        if(state != null) {
            state.impression = Mathf.Clamp(state.impression + amount, minImpression, maxImpression);
            return true;
        }
        return false;
    }

    // public void RegisterPlayer(Character character, CharacterControl inst) {
    //     if(RegisterCharacter(character, inst)) {
    //         Player = inst;
    //     }
    // }

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
        if(!_characterStates.ContainsKey(character.name)) {
            CharacterState state = new CharacterState(character.name);
            state.impression = character.initialImpression;

            _characterStates.Add(character.name, state);
        }

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
                Cams.I.SetMainCamTarget(instance.charAnim.headHold);
                onPlayerRegistered?.Invoke(Player);
            }
                
            return true;
        }
        return false;
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

    public GameObject SpawnPlayer() {
        return Instantiate(playerPrefab);
    }

    public void Serialize(ref CharacterData characterData) {
        characterData.characterStates = _characterStates;
    }
    public void Deserialize(CharacterData characterData) {
        _characterStates = characterData.characterStates;
    }
}
}