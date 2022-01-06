
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using m4k.Characters.Customization;

namespace m4k.Characters {
public class CharacterUI : MonoBehaviour {
    public Button changeNameBtn, changePortraitBtn, customizeCharacterBtn, customizeSelfBtn;
    public Image portraitImg;
    public TMP_Text characterNameTxt;
    public Character currentCharacter;

    private void OnEnable() {
        Reset();
    }
    private void Start() {
        customizeSelfBtn.onClick.AddListener(CharacterCustomize.I.CustomizePlayer);
        customizeCharacterBtn.onClick.AddListener(CharacterCustomize.I.CustomizeFocused);
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
        characterNameTxt.text = character.displayName;
    }
    public void Reset() {
        currentCharacter = null;
        portraitImg.sprite = null;
        portraitImg.enabled = false;
        characterNameTxt.text = "";
    }
}
}