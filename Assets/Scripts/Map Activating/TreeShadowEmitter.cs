using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class TreeShadowEmitter : MonoBehaviour {

    public bool Active = false;

    
	void Awake ()
    {
	    for (int i = 0; i < transform.childCount; i++)
        {
            Transform forest = transform.GetChild(i);
            for (int j = 0; j < forest.childCount; j++)
            {
                Transform tree = forest.GetChild(j);
                for (int k = 0; k < tree.childCount; k++)
                {
                    tree.GetChild(k).gameObject.SetActive(Active);
                }
            }       
        }
	}
	
}
