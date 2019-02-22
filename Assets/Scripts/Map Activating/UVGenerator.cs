using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class UVGenerator : MonoBehaviour {

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform chunk = transform.GetChild(i);

            Mesh mesh = chunk.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;

           // Unwrapping.GenerateSecondaryUVSet(mesh);

        }
    }	
}
