using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviour
    {

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
        private List<UnitManager> _units = new List<UnitManager>();
        public static List<UnitManager> Units { get { return Instance._units; } }
        private List<InfantryUnitManager> _infantryUnits = new List<InfantryUnitManager>();
        public static List<InfantryUnitManager> InfantryUnits { get { return Instance._infantryUnits; } }
        private List<UnitManager> _unitsFirstSide = new List<UnitManager>();
        public static List<UnitManager> UnitsFirstSide { get { return Instance._unitsFirstSide; } }
        private List<UnitManager> _unitsSecondSide = new List<UnitManager>();
        public static List<UnitManager> UnitsSecondSide { get { return Instance._unitsSecondSide; } }
        public GameObject[] Trees = new GameObject[3];

        private int _currentPhase = 0;
        public static int CurrentPhase { get { return Instance._currentPhase; } set { Instance._currentPhase = value; } }
        private int _currentTurn = 1;
        public static int CurrentTurn { get { return Instance._currentTurn; } set { Instance._currentTurn = value; } }
        private List<UnitManager> _enemiesInControlZone = new List<UnitManager>();
        public static List<UnitManager> EnemiesInControlZone { get { return Instance._enemiesInControlZone; } }
        private List<UnitManager> _unitsWithEnemies = new List<UnitManager>();
        public static List<UnitManager> UnitsWithEnemies { get { return Instance._unitsWithEnemies; } }
        private List<UnitManager> _enemyUnitsAttackedByMany = new List<UnitManager>();
        public static List<UnitManager> EnemyUnitsAttackedByMany { get { return Instance._enemyUnitsAttackedByMany; } }
        private List<UnitManager> _unitsAttackingManyOrOne = new List<UnitManager>();
        public static List<UnitManager> UnitsAttackingManyOrOne { get { return Instance._unitsAttackingManyOrOne; } }
        public static TextMeshProUGUI CurrentPhaseText { get; set; }
        public static TextMeshProUGUI CurrentTurnText { get; set; }

        public static UnitManager CurrentlyChecked { get; set; }

        public static string[] Phases =
        {
        "Phase:  Movement Phase 1   1/4", "Phase:  Attack Phase 1   2/4",
        "Phase:  Movement Phase 2   3/4", "Phase:  Attack Phase 2   4/4"
    };

        void Awake()
        {

            if (Instance != null) Debug.LogError("More than one GameManager in scene!");
            else
            {
                Instance = this;
                Grid = GetComponent<HexGrid>();
                Camera = GameObject.Find("Hex Map Camera").GetComponent<HexMapCamera>();
                CurrentPhaseText = GameObject.Find("Current Phase Text").GetComponent<TextMeshProUGUI>();
                CurrentTurnText = GameObject.Find("Current Turn Text").GetComponent<TextMeshProUGUI>();
                CurrentPhaseText.text = Phases[0];
                CurrentTurnText.text = "Turn:  1/30";
            }
        }
    }
}
