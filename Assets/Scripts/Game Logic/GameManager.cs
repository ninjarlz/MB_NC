using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    public static HexGrid Grid;
    [SerializeField]
    private int _vikingCounter = 13;
    public static int VikingCounter { get { return Instance._vikingCounter; } set { Instance._vikingCounter = value; } }
    [SerializeField]
    private int _angloSaxonCounter = 12;
    public static int AngloSaxonCounter { get { return Instance._angloSaxonCounter; } set { Instance._angloSaxonCounter = value; } }
    public static HexMapCamera Camera { get; set; }
    public enum Side { Northman, Anglosaxons }

    void Awake () {

        if (Instance != null) Debug.LogError("More than one GameManager in scene!");
        else
        {
            Instance = this;
            Camera = GameObject.Find("Hex Map Camera").GetComponent<HexMapCamera>();
        }
    }
}
