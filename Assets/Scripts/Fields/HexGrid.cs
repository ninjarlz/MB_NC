﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace com.MKG.MB_NC
{
    public class HexGrid : MonoBehaviour
    {

        public Queue<Hex> Frontier { get; private set; }
        public Dictionary<Hex, Hex> CameFrom { get; private set; }
        public Dictionary<Hex, float> CostSoFar { get; private set; }
        public Dictionary<Hex, int> FromDirection { get; private set; }
        public List<Hex> ApproachableHexes { get; private set; }
        public List<Hex> Path { get; private set; }

        [SerializeField]
        private int _hexCountX;
        public int HexCountX { get { return _hexCountX; } }

        [SerializeField]
        private int _hexCountZ;
        public int HexCountZ { get { return _hexCountZ; } }
        private bool _showPath = false;
        public bool ShowPath { get { return _showPath; } set { _showPath = value; } }
        private bool _showRotationFields = false;
        public bool ShowRotationFields { get { return _showRotationFields; } set { _showRotationFields = value; } }
        [SerializeField]
        private string _mapName;

        public Hex[,] Hexes { get; set; }
        [SerializeField]
        private TextMeshPro _costLabel;
        public TextMeshPro CostLabel { get { return _costLabel; } set { _costLabel = value; } }
        private MeshCollider _collider;

        [SerializeField]
        private Sprite[] _dicesSprites;
        public Sprite[] DicesSprites { get { return _dicesSprites; } }
        [SerializeField]
        private TextMeshProUGUI _result;
        public TextMeshProUGUI Result { get { return _result; } }
        [SerializeField]
        private TextMeshProUGUI[] _ratio;
        public TextMeshProUGUI[] Ratio { get { return _ratio; } }
        [SerializeField]
        private Image[] _dicesImages;
        public Image[] DicesImages { get { return _dicesImages; } }

        [SerializeField]
        private GameObject _angloSaxonsBanner;
        public GameObject AngloSaxonsBanner { get { return _angloSaxonsBanner; } }
        [SerializeField]
        public GameObject _vikingsBanner;
        public GameObject VikingsBanner { get { return _vikingsBanner; } }
        [SerializeField]
        private Hex _vikingsCamp;
        public Hex VikingsCamp { get { return _vikingsCamp; } set { _vikingsCamp = value; } }
        [SerializeField]
        private Hex _angloSaxonsCamp;
        public Hex AngloSaxonsCamp { get { return _angloSaxonsCamp; } set { _angloSaxonsCamp = value; } }


        void Awake()
        {
            Frontier = new Queue<Hex>();
            CameFrom = new Dictionary<Hex, Hex>();
            CostSoFar = new Dictionary<Hex, float>();
            FromDirection = new Dictionary<Hex, int>();
            ApproachableHexes = new List<Hex>();
            Path = new List<Hex>();
            UpdateHexes();
        }


        void UpdateHexes()
        {
            GameObject hexes = GameObject.Find("Hexes");
            Hexes = new Hex[HexCountX, HexCountZ];
            for (int x = 0, i = 0; x < HexCountX; x++) for (int z = 0; z < HexCountZ; z++, i++)
                {
                    Hexes[x, z] = hexes.transform.GetChild(i).GetComponent<Hex>();
                    Hexes[x, z].Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
                }
        }

        void OnEnable()
        {
            SetShadows();
        }

        void SetShadows()
        {
            GameObject hexChunks = GameObject.Find("Hex Meshes Chunks");
            for (int i = 0; i < hexChunks.transform.childCount; i++)
            {
                GameObject terrain = hexChunks.transform.GetChild(i).gameObject.transform.Find("Terrain").gameObject;
                if (QualitySettings.GetQualityLevel() < 3) terrain.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                else terrain.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }


        // Use this for initialization
        void Start()
        {
            for (int x = 0; x < HexCountX; x++) for (int z = 0; z < HexCountZ; z++) Hexes[x, z].CalculateNeighbors(Hexes, HexCountX, HexCountZ);

        }

        public Hex GetHex(Vector3 hitPoint)
        {
            hitPoint = transform.InverseTransformPoint(hitPoint);
            HexCoordinates coordinates = HexCoordinates.FromPosition(hitPoint);
            if (coordinates.X < 0 || coordinates.X > HexCountX - 1 || coordinates.NotShiftedZ < 0 || coordinates.NotShiftedZ > HexCountZ - 1) return null;
            return Hexes[coordinates.X, coordinates.NotShiftedZ];
        }

        public Hex GetHex(HexCoordinates coordinates)
        {
            if (coordinates.X < 0 || coordinates.X > HexCountX - 1 || coordinates.NotShiftedZ < 0 || coordinates.NotShiftedZ > HexCountZ - 1) return null;
            return Hexes[coordinates.X, coordinates.NotShiftedZ];
        }

        public void HideApproachables()
        {
            foreach (Hex hex in ApproachableHexes)
            {
                hex.TurnOffAllRenderers();
                hex.ResetCostWithTurnings();
            }
        }

        public void HideApproachablesWithoutPath()
        {
            foreach (Hex hex in ApproachableHexes)
            {
                if (!Path.Contains(hex))
                    hex.TurnOffAllRenderers();
            }

            for (int i = 1; i < Path.Count; i++)
            {
                Hex fromHex = CameFrom[Path[i]];
                for (int j = 0; j < 6; j++)
                {
                    if (j == FromDirection[Path[i]])
                    {
                        if (FromDirection[Path[i]] == FromDirection[fromHex])
                            fromHex.ArrowRenderers[j].sprite = null;
                    }
                    else fromHex.ArrowRenderers[j].sprite = null;
                }
            }
            Path[Path.Count - 1].TurnOffArrowsRenderers();
        }

        public void ResetAproachablesCosts()
        {
            foreach (Hex hex in ApproachableHexes) hex.ResetCostWithTurnings();
        }

        public void ResetAproachablesEnemyZone()
        {
            foreach (Hex hex in ApproachableHexes)
            {
                hex.IsInEnemyZone = false;
            }

        }

    }
}
