using UnityEngine;

namespace m4k.Progression {
public class LineSO : ScriptableObject {
    public Line line;


    private void OnValidate() {
        name = line.text;
    }
    // [TextArea(1, 3)]
    // public string text;
    // public Character character;
    // public AudioClip audio;
    // public List<Choice> choices;
}
}