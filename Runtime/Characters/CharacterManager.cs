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
    public CharacterControl Player; // todo: deprecate 
    public List<CharacterControl> activeCharacterControls;
    public Character focused;
    public GameObject playerPrefab;
    public int minImpression = -100, maxImpression = 100;

    public System.Action<CharacterControl> onCharacterRegistered, onCharacterUnregistered, onPlayerRegistered, onPlayerUnregistered;

    Dictionary<string, List<CharacterControl>> _tagCharacterControlsDict = new Dictionary<string, List<CharacterControl>>();
    Dictionary<Character, CharacterControl> _charInstanceDict = new Dictionary<Character, CharacterControl>();
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

    public CharacterControl GetCharInstance(string charName) {
        return GetCharInstance(AssetRegistry.I.GetCharacterFromName(charName));
    }
    public CharacterControl GetCharInstance(Character character) {
        if(!character) return null;
        CharacterControl chara = null;
        _charInstanceDict.TryGetValue(character, out chara);
        if(!chara) Debug.LogWarning("character not found");
        return chara;
    }

    public void RegisterCharacter(CharacterControl cc) {
        if(!activeCharacterControls.Contains(cc)) {
            activeCharacterControls.Add(cc);
            onCharacterRegistered?.Invoke(cc);

            if(cc.CompareTag("Player")) {
                if(!Player) Player = cc;
                onPlayerRegistered?.Invoke(cc);
            }

            if(!_tagCharacterControlsDict.ContainsKey(cc.tag)) {
                _tagCharacterControlsDict.Add(cc.tag, new List<CharacterControl>());
            }
            _tagCharacterControlsDict[cc.tag].Add(cc);
        }

        if(!cc.character) return;

        if(!_characterStates.ContainsKey(cc.character.name)) {
            CharacterState state = new CharacterState(cc.character.name);
            state.impression = cc.character.initialImpression;

            _characterStates.Add(cc.character.name, state);
        }

        if(!_charInstanceDict.ContainsKey(cc.character)) {
            _charInstanceDict.Add(cc.character, cc);
            CharacterCustomize.I.ApplyCustomizationsOnCharacterSpawn(cc.character, cc.gameObject);
        }
    }
    
    public void RemoveCharacter(CharacterControl cc) {
        if(activeCharacterControls.Contains(cc)) {
            activeCharacterControls.Remove(cc);
            onCharacterUnregistered?.Invoke(cc);

            if(cc.CompareTag("Player")) {
                onPlayerUnregistered?.Invoke(cc);
                if(Player == cc) Player = null;
            }

            if(_tagCharacterControlsDict.ContainsKey(cc.tag)) {
                if(_tagCharacterControlsDict[cc.tag].Contains(cc))
                    _tagCharacterControlsDict[cc.tag].Remove(cc);
            }
        }

        var instance = GetCharInstance(cc.character);
        if(!instance) return;
            
        _charInstanceDict.Remove(cc.character);

    }

    public List<CharacterControl> GetCharacterControls(string tag) {
        List<CharacterControl> characterControls = null;
        _tagCharacterControlsDict.TryGetValue(tag, out characterControls);
        if(characterControls == null) {
            characterControls = new List<CharacterControl>();
            _tagCharacterControlsDict.Add(tag, characterControls);
            Debug.LogWarning($"Get created character tag list");
        }
        return characterControls;
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