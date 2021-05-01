
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace m4k.Characters {
public class CharacterUI : MonoBehaviour {
    public Button changeNameBtn, changePortraitBtn, customizeCharacterBtn, customizeSelfBtn;
    public Image portraitImg;
    public TMP_Text characterNameTxt;
    public Character currentCharacter;

    private void OnEnable() {
        Reset();
    }
    private void OnDisable() {
        Reset();
    }

    public void SetCharacter(Character character) {
        currentCharacter = character;
        if(character.itemIcon) {
            portraitImg.enabled = true;
            portraitImg.sprite = character.itemIcon;
        }
        characterNameTxt.text = character.itemName;
    }
    public void Reset() {
        currentCharacter = null;
        portraitImg.sprite = null;
        portraitImg.enabled = false;
        characterNameTxt.text = "";
    }
}
}