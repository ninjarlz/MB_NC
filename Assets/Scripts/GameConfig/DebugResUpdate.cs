using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugResUpdate : MonoBehaviour {

    public class Res
    {
        public int ResX, ResY;
        public Res(int ResX, int ResY)
        {
            this.ResX = ResX;
            this.ResY = ResY;
        }
    }

    private TextMeshProUGUI _textMesh;
   

    void Awake()
    {
         _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        DontDestroyOnLoad(gameObject);
    }

    void Update ()
    {
        _textMesh.text = Screen.currentResolution.width + " x " + Screen.currentResolution.height;
      
    }
}
