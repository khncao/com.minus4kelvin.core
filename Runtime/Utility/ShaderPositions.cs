using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public class ShaderPositions : MonoBehaviour {
    Vector4[] positions;
    List<Transform> targets;

    public ShaderPositions(int size) {
        positions = new Vector4[size];
        targets = new List<Transform>();
    }

    public void AddPosTarget(Transform transform) {
        if(targets.Count == positions.Length) 
            return;
        targets.Add(transform);
    }
	
	void Update() 
    {
        for(int i = 0; i < targets.Count; ++i) {
            positions[i] = targets[i].position;
        }

        Shader.SetGlobalFloat("_PositionArray", targets.Count);
        Shader.SetGlobalVectorArray("_Positions", positions);
	}
}
}