using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k {
public class ShaderVars : MonoBehaviour {
    // public Transform[] playerSet;
    Vector4[] positions = new Vector4[8];

    private void Start() {
        // playerSet = new Transform[1];
        // playerSet[0] = Characters.I.Player.transform;
        Shader.SetGlobalFloat("_PositionArray", 1);
    }
	
	void FixedUpdate () 
    {
        // for (int i = 0; i < playerSet.Length; i++) {
        //     positions[i] = playerSet[i].transform.position;
        // }
        if(CharacterManager.I.Player)
            positions[0] = CharacterManager.I.Player.transform.position;

        // Shader.SetGlobalFloat("_PositionArray", playerSet.Length);
        Shader.SetGlobalVectorArray("_Positions", positions);
	}
}
}