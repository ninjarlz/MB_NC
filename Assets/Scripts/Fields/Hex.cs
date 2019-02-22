using UnityEngine;
using UnityEditor;
using TMPro;


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HexCoordinates coordinates = new HexCoordinates(property.FindPropertyRelative("_x").intValue, property.FindPropertyRelative("_z").intValue);
        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());
    }
}
#endif


[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int _x, _z;

    public int X { get { return _x; } }

    public int Z { get { return _z; } }

    public int Y { get { return -X - Z; } }

    public int NotShiftedZ { get { return _z + _x / 2; } }

    public HexCoordinates(int x, int z)
    {
        _x = x;
        _z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x, z - x/2);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        float z = position.z / (HexDims.InnerRadius * 2f);
        float y = -z;
        float offset = position.x / (HexDims.OuterRadius * 3f);
        y -= offset;
        z -= offset;
        int iX = Mathf.RoundToInt(-z-y);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(z);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(-z -y - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(z - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
            
        }
        return new HexCoordinates(iX, iZ);
    }

    public override string ToString()
    {
        return X.ToString() + " " + Z.ToString();
    }

    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Z.ToString();
    }
}



public class Hex : MonoBehaviour {
    
    public HexCoordinates Coordinates;
    public Unit Unit;
    public int HasCamp = 0; // 0 - no camp, 1 - viking camp, 2 - anglo-saxon camp 
    public bool IsUnderWater = false;
    public bool IsInEnemyZone = false;
    public bool HasTrees = false;
    public bool HasBridge = false;
    public bool HasMud = false;
    public bool HasRiver = false;
    public bool HasRoad = false;
    public float AbsoluteCost = 1f;
    public float CostWithTurnings = 1f;
    public Sprite[] Sprites = new Sprite[6];
    public SpriteRenderer Renderer;
    public SpriteRenderer[] ArrowRenderers = new SpriteRenderer[7];
    public Canvas CostCanvas;
    private RectTransform _uiRect;
    public TextMeshPro CostText;
    private HexGrid _grid;
    public int DefenseModificator = 0;

    [SerializeField]
    private int _elevation = 0;
    public int Elevation
    {
        get { return _elevation; }
        set
        {
           _elevation = value;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    [SerializeField]
    private Hex[] _neighbors = new Hex[6];

    public bool HasRoadOrBridge()
    {
        return (HasRoad || HasBridge);
    }

    void Awake()
    {
        _grid = transform.parent.transform.parent.gameObject.GetComponent<HexGrid>();
        GameObject spriteRenderer = new GameObject() { name = "Sprite Renderer" };
        spriteRenderer.transform.parent = transform;
        spriteRenderer.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        Renderer = spriteRenderer.AddComponent<SpriteRenderer>();
        Renderer.sortingOrder = 3;
        
        if (HasTrees)
        {
            spriteRenderer.transform.position = transform.position + new Vector3(0f, 5.9f);
            for (int i = 0; i < 6; i++)
            {
                GameObject arrowRenderer = new GameObject() { name = "Arrow Renderer " + i };
                arrowRenderer.transform.parent = transform;
                arrowRenderer.transform.position = transform.position + new Vector3(0f, 5.8f, 0f);
                arrowRenderer.transform.rotation = Quaternion.Euler(new Vector3(90f, i * 60f, 0f));
                ArrowRenderers[i] = arrowRenderer.AddComponent<SpriteRenderer>();
                ArrowRenderers[i].sortingOrder = 2;

            }
            //GameObject tree = Instantiate(_grid.Trees[Random.Range(0,2)]);
            //tree.transform.position = transform.position;
            //tree.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 359)));
            //tree.transform.parent = transform;
            //for(int i = 0; i < tree.transform.childCount; i++)
            //{
            //    GameObject child = tree.transform.GetChild(i).gameObject;
            //    child.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 359)));
            //}
            AbsoluteCost++;
            DefenseModificator++;
        }
        else
        {
            if (HasRiver)
            {
                AbsoluteCost++;
                DefenseModificator++;
                if (HasBridge)
                {
                    spriteRenderer.transform.position = transform.position + new Vector3(0f, 3.9f);
                    for (int i = 0; i < 6; i++)
                    {
                        GameObject arrowRenderer = new GameObject() { name = "Arrow Renderer " + i };
                        arrowRenderer.transform.parent = transform;
                        arrowRenderer.transform.position = transform.position + new Vector3(0f, 3.8f, 0f);
                        arrowRenderer.transform.rotation = Quaternion.Euler(new Vector3(90f, i * 60f, 0f));
                        ArrowRenderers[i] = arrowRenderer.AddComponent<SpriteRenderer>();
                        ArrowRenderers[i].sortingOrder = 2;

                    }
                }
                else
                {
                    spriteRenderer.transform.position = transform.position + new Vector3(0f, 0.01f);
                    for (int i = 0; i < 6; i++)
                    {
                        GameObject arrowRenderer = new GameObject() { name = "Arrow Renderer " + i };
                        arrowRenderer.transform.parent = transform;
                        arrowRenderer.transform.position = transform.position + new Vector3(0f, 0.5f, 0f);
                        arrowRenderer.transform.rotation = Quaternion.Euler(new Vector3(90f, i * 60f, 0f));
                        ArrowRenderers[i] = arrowRenderer.AddComponent<SpriteRenderer>();
                    }
                }
            }
            else 
            {
                if (HasMud) AbsoluteCost += 2;
                spriteRenderer.transform.position = transform.position + new Vector3(0f, 0.01f);
                for (int i = 0; i < 6; i++)
                {
                    GameObject arrowRenderer = new GameObject() { name = "Arrow Renderer " + i };
                    arrowRenderer.transform.parent = transform;
                    arrowRenderer.transform.position = transform.position + new Vector3(0f, 0.5f, 0f);
                    arrowRenderer.transform.rotation = Quaternion.Euler(new Vector3(90f, i * 60f, 0f));
                    ArrowRenderers[i] = arrowRenderer.AddComponent<SpriteRenderer>();
                }
            }
        }
    }

    void Start()
    {
        if (HasCamp == 1)
        {
            GameObject banner = Instantiate(_grid.VikingsBanner, new Vector3(transform.position.x, _grid.VikingsBanner.transform.position.y, transform.position.z), _grid.VikingsBanner.transform.rotation, transform);
            banner.GetComponentInChildren<SkinnedMeshRenderer>().sortingOrder = 15;
            _grid.VikingsCamp = this;
        }
        else if (HasCamp == 2)
        {
            GameObject banner = Instantiate(_grid.AngloSaxonsBanner, new Vector3(transform.position.x, _grid.AngloSaxonsBanner.transform.position.y, transform.position.z), _grid.AngloSaxonsBanner.transform.rotation, transform);
            banner.GetComponentInChildren<SkinnedMeshRenderer>().sortingOrder = 15;
           _grid.AngloSaxonsCamp = this;
        }
        CostCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        CostText = Instantiate(_grid.CostLabel);
        CostText.sortingOrder = 4;
        CostText.name = "Cost Text " + Coordinates.ToString();
        CostText.transform.SetParent(CostCanvas.transform, false);
        if (HasBridge) CostText.transform.position = transform.position + new Vector3(0f, 4f);
        else if (HasTrees) CostText.transform.position = transform.position + new Vector3(0f, 6.0f);
        else CostText.transform.position = transform.position + new Vector3(0f, 0.02f);
        CostText.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        CostText.gameObject.SetActive(false);
    }

    public Hex GetNeighbor(HexDirection direction)
    {
        return _neighbors[(int)direction];    
    }

    public HexDirection GetDirection(Hex neighbor)
    {
        for (int direction = 0; ; direction++) if (_neighbors[direction] == neighbor) return (HexDirection)direction;
    }

    public Hex[] GetNeighbors()
    {
        return _neighbors;
    }

    public void CalculateNeighbors(Hex[,] hexes, int width, int height)
    {
        int zInArray = Coordinates.Z - 1 + (Coordinates.X) / 2;
        if (zInArray >= 0) _neighbors[3] = hexes[Coordinates.X, zInArray];

        zInArray = Coordinates.Z - 1 + (Coordinates.X + 1) / 2;
        if (Coordinates.X + 1 <= width - 1 && zInArray >= 0) _neighbors[2] = hexes[Coordinates.X + 1, zInArray];

        zInArray = Coordinates.Z + (Coordinates.X - 1) / 2;
        if (Coordinates.X - 1 >= 0 && zInArray >= 0) _neighbors[4] = hexes[Coordinates.X - 1, zInArray];

        zInArray = Coordinates.Z + 1 + (Coordinates.X - 1) / 2;
        if (Coordinates.X - 1 >= 0 && zInArray <= height - 1) _neighbors[5] = hexes[Coordinates.X - 1, zInArray];

        zInArray = Coordinates.Z + 1 + Coordinates.X / 2;
        if (zInArray <= height - 1) _neighbors[0] = hexes[Coordinates.X, zInArray];

        zInArray = Coordinates.Z + (Coordinates.X + 1) / 2;
        if (Coordinates.X + 1 <= width - 1 && zInArray >= 0 && zInArray <= height - 1) _neighbors[1] = hexes[Coordinates.X + 1, zInArray];
    }

    public void SetColorOfCost(float cost)
    {
        if (cost > HexDims.CostColors.Length) CostText.color = HexDims.CostColors[HexDims.CostColors.Length - 1];
        else CostText.color = HexDims.CostColors[(int)cost - 1];
    }

    public void TurnOffCostRenderers()
    {
        Renderer.sprite = null;
        CostText.gameObject.SetActive(false);
    }

    public void TurnOffArrowsRenderers()
    {
        foreach (SpriteRenderer renderer in ArrowRenderers) renderer.sprite = null;
    }

    public void TurnOffAllRenderers()
    {
        Renderer.sprite = null;
        CostText.gameObject.SetActive(false);
        foreach (SpriteRenderer renderer in ArrowRenderers) renderer.sprite = null;
    }

    public void ResetCostWithTurnings()
    {
        CostWithTurnings = AbsoluteCost;
    }
}
