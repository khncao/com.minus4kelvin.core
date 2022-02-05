
namespace m4k.Progression {
[System.Serializable]
public class ConditionalChoice {
    public string text;
    public string key;
    public Convo nextConvo;
    public Conditions conditions;
    // replace dialogue convo if conditions met, skipping text
    public bool replaceConvoOnConds;

    public bool MetConditions {
        get {
            // if autoSkipIfSeen and seen, return false
            if(nextConvo.autoSkipIfSeen && ProgressionManager.I.CheckKeyState(nextConvo.id)) {
                return false;
            }
            // else return true if convo conds met
            return conditions.CheckCompleteReqs();
        }
    }

    public Choice Choice {
        get {
            if(_choice == null) {
                _choice = new Choice();
                _choice.text = text;
                _choice.key = key;
                _choice.nextConvo = nextConvo;
            }
            return _choice;
        }
    }
    [System.NonSerialized]
    Choice _choice;
}
}