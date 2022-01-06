using UnityEngine;
using System.Collections.Generic;
using m4k.Characters;

namespace m4k.Progression {
    [System.Serializable]
    public class Line {
        [TextArea(1, 3)]
        public string text;
        public Character character;
        public AudioClip audio;
        public List<Choice> choices;
    }
}