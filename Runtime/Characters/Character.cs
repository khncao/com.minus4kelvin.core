using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.Characters {
public enum Sex { None = 0, Unknown = 1, Other = 2, Male = 10, Female = 20 }

public enum Profession { None = 0, Unknown = 1, Student = 10, Farmer = 20,  }

public enum Trait { None = 0, Greedy = 1, Prideful = 2, Wrathful = 3, Glutton = 4, Lustful = 5, Envious = 6, Sloth = 7, 
Pessimist = 10, Optimist = 11, 
Cheery = 20, Talkative = 21, Gloomy = 22, Quiet = 23, 
Active = 30, ModerateActive = 31, Still = 32,
Chaotic = 40, Neutral = 41, Lawful = 42, }
// TODO: trait enum to structs with weight effects and rates

[System.Serializable]
public class CharacterDescriptors {
    public int age;
    public Sex sex;
    public string origin; 
    public Profession profession;
    public List<Trait> traits;
}

[System.Serializable]
public class CharacterGeneratedProfile {
    public string name;
    public int impression;
    public CharacterDescriptors descriptors;
}

[System.Serializable]
public struct CharExpression {
    public string name;
    public AnimationClip anim;
    public Sprite portrait;
}

[System.Serializable]
[CreateAssetMenu(menuName="Data/Items/Character")]
public class Character : Item
{
    [Header("Character")]
    public List<CharExpression> expressions;
    public int initialImpression;

    public override bool Primary(ItemSlot slot)
    {
        CharacterManager.I.SetFocused(this);
        return true;
    }
}
}