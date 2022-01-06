using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace m4k.UI {
/// <summary>
/// Automatically registers a basic following UI object
/// </summary>
public class FollowUIComponent : MonoBehaviour
{
    public string label;
    public Sprite image;
    // TODO: follow dialogue
    public Renderer followRend;
    public float vertOffset;
    GameObject uiObj;
    TMPro.TMP_Text textUI;
    Image imgUI;

    void Start()
    {
        if(!followRend)
            followRend = GetComponentInChildren<Renderer>();
        if(!followRend) {
            followRend = GetComponentInParent<Renderer>();
        }
        if(!followRend) {
            Debug.LogWarning("No follow target renderer");
        }

        // Game.Scenes.onSceneChanged += RegisterUI;
        if(!uiObj)
            RegisterUI();
    }

    void RegisterUI() {
        // if(uiObj)
        //     return;
        if(!string.IsNullOrEmpty(label)) {
            uiObj = Feedback.I.worldToScreenUIFollow.RegisterDefaultTxtUI(followRend, followRend.transform, vertOffset);
            textUI = uiObj.GetComponentInChildren<TMPro.TMP_Text>();
            textUI.text = label;
            
        }
        else if(image) {
            uiObj = Feedback.I.worldToScreenUIFollow.RegisterDefaultImgUI(followRend, followRend.transform, vertOffset);
            imgUI = uiObj.GetComponentInChildren<Image>();
            imgUI.sprite = image;
        }
    }

    private void OnDisable() {
        if(followRend)
            Feedback.I?.worldToScreenUIFollow.UnregisterFollowUI(followRend.transform);
    }
}
}