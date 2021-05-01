using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Progression {
public class DialogueManager : Singleton<DialogueManager>
{
    public DialogueUI UI;
    public AudioClip nextLineSfx;
    public int autopassSeconds = 0;
    public bool paused, inDialogue, replaceCurr;
    public Dialogue currDialogue;
    public System.Action onStartDialogue, onNextLine, onEndDialogue;
    public System.Action<Dialogue.Convo> onCompleteConvo;
    public List<KeyAction> globalChoiceActions;

    Character currChar;
    bool skipLine;
    int currLineIndex;
    Coroutine nextLineCr;
    Dialogue.Convo currConvo;
    List<Dialogue.Choice> currChoices;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;

        if(!UI) UI = GetComponentInChildren<DialogueUI>();
        UI.Init(this);
    }

    public void InvokeChoiceAction(string key) {
        int ind = globalChoiceActions.FindIndex(x=>x.key == key);
        if(ind != -1)
            globalChoiceActions[ind].action?.Invoke();
    }

    public void StopDialogue() {
        UI.ToggleDialogueUI(false);
        UI.DisableChoices();
        paused = false;
        currDialogue = null;
        currConvo = null;
        currChar = null;
        currChoices = null;
        inDialogue = false;
        onEndDialogue?.Invoke();
    }
    public void ReplaceDialogue(string convoId) {
        Dialogue.Convo convo = currDialogue.GetConvo(convoId);
        ReplaceDialogue(convo, 0);
    }
    public void ReplaceDialogue(Dialogue.Convo convo, int startLineIndex) {
        replaceCurr = true;
        AssignDialogue(currDialogue, convo, startLineIndex);
    }

    public void AssignDialogue(Dialogue dialogue, Dialogue.Convo convo, int startLineIndex) {
        if(inDialogue && !replaceCurr)
            return;
        replaceCurr = false;
        paused = false;
        currDialogue = dialogue;
        currConvo = convo;
        currLineIndex = startLineIndex;
        UI.ToggleDialogueUI(true);
        inDialogue = true;
        onStartDialogue?.Invoke();
        NextLine();
    }

    public void InputNextLine() {
        if(paused || !inDialogue) return;
        NextLine();
    }

    void NextLine() {
        if(nextLineCr != null)
            StopCoroutine(nextLineCr);

        if(currConvo != null) 
        {
            if(currLineIndex >= currConvo.lines.Count) {
                if(!string.IsNullOrEmpty(currConvo.nextConvoId)) {
                    currDialogue.persistConvoId = currConvo.nextConvoId;
                    ReplaceDialogue(currConvo.nextConvoId);
                }
                else {
                    onCompleteConvo?.Invoke(currConvo);
                    StopDialogue();
                }
                return;
            }
            ParseDialogueLine(currConvo.lines[currLineIndex]);
            currLineIndex++;
        }

        if(nextLineSfx) {
            Feedback.I.PlayAudio(nextLineSfx);
        }
        onNextLine?.Invoke();

        if(skipLine) {
            skipLine = false;
            NextLine();
        }
        else if(autopassSeconds > 0) 
            nextLineCr = StartCoroutine(PauseAndNextLine(autopassSeconds));
    }

    void ParseDialogueLine(Dialogue.Line line) {
        if(line.character) {
            currChar = line.character;
        }
        if(currConvo.id == "choice") {
            paused = true;
            currChoices = currDialogue.choices;
            UI.ProcessChoices(currDialogue.choices);
        }
        else if(line.choices.Count > 0) {
            paused = true;
            currChoices = line.choices;
            UI.ProcessChoices(line.choices);
        }
        UI.UpdateDialogueUI(currChar, line.text);
    }

    public void SelectChoice(int val) {
        var choice = currChoices[val-1];

        currDialogue.InvokeChoiceAction(choice);

        if(!string.IsNullOrEmpty(choice.nextConvoId))
            ReplaceDialogue(choice.nextConvoId);
        else {
            paused = false;
            NextLine();
        }
    }

    public void SetPaused(bool b) => paused = b;
    public void SetSkipLine(bool b) => skipLine = b;

    IEnumerator PauseAndNextLine(int seconds) {
        while(paused) {
            yield return null;
        }
        yield return new WaitForSeconds(seconds);
        InputNextLine();
    }

    // Dictionary<string, DialogueState> dialogueStatesDict = new Dictionary<string, DialogueState>();
    // DialogueState[] _dialogueStates;
    // public void RegisterDialogue(Dialogue dialogue) {
    //     if(!dialogueStatesDict.ContainsKey(dialogue.dialogueId)) {
    //         var newState = new DialogueState();
    //         newState.dialogue = dialogue;
    //         newState.dialogueId = dialogue.dialogueId;
    //         newState.persistConvoId = dialogue.persistConvoId;

    //         dialogueStatesDict[dialogue.dialogueId] = newState;
    //     }
    //     else {
    //         var state = dialogueStatesDict[dialogue.dialogueId];
    //         state.dialogue = dialogue;
    //         dialogue.persistConvoId = state.persistConvoId;
    //         dialogueStatesDict[dialogue.dialogueId] = state;
    //     }
    // }
    
    // public void Serialize() {
    //     // _dialogueStates = dialogueStatesDict.Values.ToArray();
    //     for(int i = 0; i < _dialogueStates.Length; ++i) {
    //         if(string.IsNullOrEmpty(_dialogueStates[i].dialogueId)) continue;
    //         var state = _dialogueStates[i];
    //         state.persistConvoId = _dialogueStates[i].dialogue.persistConvoId;
    //         _dialogueStates[i] = state;
    //     }
    // }

    // public void Deserialize() {
    //     dialogueStatesDict = new Dictionary<string, DialogueState>();
    //     foreach(var i in _dialogueStates)
    //         dialogueStatesDict.Add(i.dialogueId, i);
    // }
}
}