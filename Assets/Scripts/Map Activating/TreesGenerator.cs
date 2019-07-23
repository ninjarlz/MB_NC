using UnityEngine;

namespace com.MKG.MB_NC
{
    [ExecuteInEditMode]
    public class TreesGenerator : MonoBehaviour {

        private HexGrid _grid;

        void Awake() {
            GameObject hexes = GameObject.Find("Hexes");
            _grid = gameObject.GetComponent<HexGrid>();
            GameObject trees = new GameObject("Trees");
            trees.transform.SetParent(transform);
            for (int i = 0; i < hexes.transform.childCount; i++)
            {
                Hex hex = hexes.transform.GetChild(i).GetComponent<Hex>();
                if (hex.HasTrees)
                {
                    GameObject forest = Instantiate(GameManager.Instance.Trees[Random.Range(0, 3)]);
                    forest.transform.position = hex.transform.position;
                    forest.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360)));
                    forest.transform.parent = trees.transform;
                    for (int j = 0; j < forest.transform.childCount; j++)
                    {
                        GameObject child = forest.transform.GetChild(j).gameObject;
                        child.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360)));
                    }
                }
            }
        }


    }
}
 
