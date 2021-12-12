using UnityEngine;

namespace m4k {
[CreateAssetMenu(fileName = "StringSO", menuName = "ScriptableObjects/Primitives/StringSO", order = 0)]
public class StringSO : ScriptableObject {
    public string label;
    public string Key { get { return name; }}
}
}