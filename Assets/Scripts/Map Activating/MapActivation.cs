using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace com.MKG.MB_NC
{
    public class MapActivation : MonoBehaviour
    {

        public HexGrid Grid;

        //private GameObject[] _hexChunks;

        public int ChunkCountX;

        public int ChunkCountZ;

        private int _chunkSize;

        public string MapName;

        // Use this for initialization
        void Awake()
        {
            /* Grid = gameObject.GetComponent<HexGrid>();
             _chunkSize = ChunkCountX * ChunkCountZ;

             GameObject HexMeshesChunks = GameObject.Find("Hex Meshes Chunks");


             for (int i = 0; i < HexMeshesChunks.transform.childCount; i++)
             {
                 GameObject chunk = HexMeshesChunks.transform.GetChild(i).gameObject;

                 GameObject terrain = chunk.transform.Find("Terrain").gameObject;
                 Mesh terrainMesh = Resources.Load<Mesh>("Assets/Meshes " + MapName + "/" + MapName + " terrain of chunk " + i.ToString() + ".asset");
                 terrain.GetComponent<MeshFilter>().mesh = terrainMesh;
                 terrain.GetComponent<MeshCollider>().sharedMesh = terrainMesh;

                 GameObject rivers = chunk.transform.Find("Rivers").gameObject;
                 rivers.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Assets/Meshes " + MapName + "/" + MapName + " rivers of chunk " + i.ToString() + ".asset");

                 GameObject roads = chunk.transform.Find("Roads").gameObject;
                 roads.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Assets/Meshes " + MapName + "/" + MapName + " roads of chunk " + i.ToString() + ".asset");

                 GameObject water = chunk.transform.Find("Water").gameObject;
                 water.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Assets/Meshes " + MapName + "/" + MapName + " water of chunk " + i.ToString() + ".asset");

                 GameObject waterShore = chunk.transform.Find("Water Shore").gameObject;
                 waterShore.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Assets/Meshes " + MapName + "/" + MapName + " water shore of chunk " + i.ToString() + ".asset");

                 GameObject estuaries = chunk.transform.Find("Estuaries").gameObject;
                 estuaries.GetComponent<MeshFilter>().mesh = Resources.Load<Mesh>("Assets/Meshes " + MapName + "/" + MapName + " estauries of chunk " + i.ToString() + ".asset");
             }

             GameObject hexes = GameObject.Find("Hexes");
             Grid.Hexes = new Hex[Grid.HexCountX, Grid.HexCountZ];
             for (int x = 0, i = 0; x < Grid.HexCountX; x++) for (int z = 0; z < Grid.HexCountZ; z++, i++)
                 {
                     Grid.Hexes[x, z] = hexes.transform.GetChild(i).GetComponent<Hex>();
                     Grid.Hexes[x, z].Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
                     Grid.Hexes[x, z].SetCanvas();
                     Grid.Hexes[x, z].CalculateNeighbors(Grid.Hexes, Grid.HexCountX, Grid.HexCountZ);
                 }

             PrefabUtility.CreatePrefab("Assets/Prefabs/" + MapName + ".prefab", gameObject);
             AssetDatabase.SaveAssets();
             //_hexChunks = new GameObject[_chunkSize];*/
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.F))
            {

            }

        }
    }
}