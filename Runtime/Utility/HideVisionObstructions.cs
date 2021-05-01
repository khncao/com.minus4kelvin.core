using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideVisionObstructions : MonoBehaviour
{
    public float rayLength = 20f; 
    List<Renderer> targets = new List<Renderer>();
    Shader transparentShader;
    private void Start() {
        transparentShader = Shader.Find("Transparent/Diffuse");
    }

    void Update()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, rayLength);

        for (int i = 0; i < hits.Length; i++)
        {
            if(!hits[i].transform.CompareTag("IgnoreRaycast"))
                continue;
            RaycastHit hit = hits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();

            if (rend)
            {
                rend.material.shader = transparentShader;
                Color tempColor = rend.material.color;
                tempColor.a = 0.3F;
                rend.material.color = tempColor;
                targets.Add(rend);
            }
        }
    }
}
