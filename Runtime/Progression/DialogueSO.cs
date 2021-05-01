using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Progression {
[System.Serializable]
[CreateAssetMenu(menuName="ScriptableObjects/DialogueSO")]
public class DialogueSO : ScriptableObject
{
    // public string dialogueName;
    // public string dialogueId;
    public List<Dialogue.Convo> convos;
    // public Dialogue.Lines lines;
}
}