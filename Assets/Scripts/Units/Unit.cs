using UnityEngine;
using System.Collections.Generic;
using TMPro;


public abstract class Unit : MonoBehaviour {

    #region Variables & Properties

    //FOR DEBUG 
    //[SerializeField]
    //protected TextMeshProUGUI _rotationText;

    [SerializeField]
    protected string _name = " ";
    public string Name { get { return _name; } set { _name = value; } }
    [SerializeField]
    protected float _moveSpeed = 30f;
    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    [SerializeField]
    protected float _rotationSpeed = 80f;
    public float RotationSpeed { get { return _rotationSpeed; } set { _rotationSpeed = value; } }
    
    [SerializeField]
    protected State _currentState = State.Idle;
    public State CurrentState { get { return _currentState; } set { _currentState = value; } }
    protected delegate void currentState();
    protected currentState _currentStateDelegate;
    protected Hex _destinationHex;
    protected Quaternion _destinationRotation;
    [SerializeField]
    protected HexDirection _currentRotation = HexDirection.N;
    public HexDirection CurrentRotation { get { return _currentRotation; } set { _currentRotation = value; } }
    public HexGrid Grid { get; set; }
    protected bool _achievedFirstHex = false;
    
    protected GameObject _marker;
    public SpriteRenderer MarkerRenderer { get; set; }
    [SerializeField]
    protected Sprite[] _markers;
    public Sprite[] Markers { get { return _markers; } }
    protected GameObject _unitBar;
    protected TextMeshProUGUI _unitBarText;
    public GameObject CanvasInfo { get; set; }
    public TextMeshProUGUI InfoText { get; set; }
    public GameObject InfoAndIcons { get; set; }
    public Vector3 OriginalInfoSize { get; set; }
    [SerializeField]
    protected bool _side; // true - North, false - English
    public bool Side { get { return _side; } }
    
    [SerializeField]
    protected int _power;
    public int Power { get { return _power; } set { _power = value; } }
    [SerializeField]
    private int _tPower;
    public int TPower { get { return _tPower; } }
    protected bool _shouldDie = false;
    public bool ShouldDie { get { return _shouldDie; } set { _shouldDie = value; } }
    [SerializeField]
    protected bool _condition = true; // true - fresh, false - tired
    public bool Condition { get { return _condition; } set { _condition = value; } } // true - fresh, false - tired
    protected AudioSource _attackSound;
    public AudioSource AttackSound { get { return _attackSound; } }
    protected AudioSource _click;
    public Animator Animator { get; set; }
    [SerializeField]
    protected bool _isBlockedByEnemyControlZone = false;
    [SerializeField]
    protected GameObject _canvasUI;
    protected GameObject _model;
    public List<Unit> FightList { get; set; }
    public int IndexInFightList { get; set; }
    public List<Unit> AttackingEnemies { get; private set; }
    protected bool _attackedFromBack = false;
    public bool AttackedFromBack { get { return _attackedFromBack; } set { _attackedFromBack = value; } }
    public int AttackingEnemiesCounter { get { return AttackingEnemies.Count; }  }
    protected bool _attackAssigned = false;
    public bool AttackAssigned { get { return _attackAssigned; } set { _attackAssigned = value; } }
    public List<Unit> AvailableEnemies { get; private set; }
    public List<Unit> AttackedEnemies { get; private set; }
    [SerializeField]
    private List<GameObject> _shieldsIcons;
    public List<GameObject> ShieldsIcons { get { return _shieldsIcons; } set { _shieldsIcons = value; } }
    public bool HasEnemies()
    {
        return AvailableEnemies.Count > 0 ? true : false;
    }
   
    public virtual bool ShowTurnIcon { get; set; }

    public int Armor { get { return _shieldsIcons.Count; } }

    [SerializeField]
    protected float _mobility;
    public float Mobility
    {
        get { return _mobility; }
        set
        {
            _mobility = value;
            SetUnitInfoText();
            SetUnitBarText();
        }
    }

    [SerializeField]
    protected float _maxMobility;
    public float MaxMobility { get { return _maxMobility; } }

    public abstract int Unlocked { get; }

    public enum State { Rotating, Moving, Fighting, Idle }

    [SerializeField]
    protected bool _isChecked;
    public bool IsChecked
    {
        get { return _isChecked; }
        set
        {
            _isChecked = value;
            if (value) MarkerRenderer.sprite = Markers[0];
            else
            {
                if (Grid.CurrentPhase == 1 || Grid.CurrentPhase == 3)
                {
                    if (AttackedEnemies.Count > 0) MarkerRenderer.sprite = Markers[2];
                    else MarkerRenderer.sprite = Markers[3];
                }
                else MarkerRenderer.sprite = null;
            }
            _unitBar.SetActive(value);
        }
    }

    [SerializeField]
    protected Hex _currentHex;
    public Hex CurrentHex
    {
        get { return _currentHex; }
        set
        {
            _currentHex.Unit = null;
            _currentHex = value;
            if (value)
            {
                _currentHex.Unit = this;
                transform.position = new Vector3(_currentHex.transform.position.x, _currentHex.transform.position.y, _currentHex.transform.position.z);
            }
        }
    }
    #endregion

    #region UnityFuncs - Start, Update...
    protected virtual void Start()
    {
        _attackSound = GetComponent<AudioSource>();
        AttackingEnemies = new List<Unit>();
        AttackedEnemies = new List<Unit>();
        AvailableEnemies = new List<Unit>();
        Grid = GameObject.Find("Game").GetComponentInChildren<HexGrid>();
        Grid.Units.Add(this);
        if (Side) Grid.UnitsFirstSide.Add(this);
        else Grid.UnitsSecondSide.Add(this);
        _currentStateDelegate = Idle;
        Animator = GetComponentInChildren<Animator>();
        _model = transform.GetChild(2).gameObject;
        _canvasUI = GameObject.Find("Canvas UI");
        _unitBar = _canvasUI.transform.GetChild(1).gameObject;
        _unitBarText = _unitBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _currentHex.Unit = this;
        _click = GameObject.Find("Click Source").GetComponent<AudioSource>();
        CanvasInfo = transform.GetChild(0).gameObject;
        InfoAndIcons = CanvasInfo.transform.GetChild(0).gameObject;
        InfoText = InfoAndIcons.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        OriginalInfoSize = InfoAndIcons.transform.localScale;
        SetUnitInfoText();
        if (Side) InfoText.color = new Color(253f / 255, 194f / 255, 194f / 255);
        else InfoText.color = new Color(115f / 255, 231f/ 255, 241f / 255);
        _marker = transform.GetChild(1).gameObject;
        MarkerRenderer = _marker.GetComponent<SpriteRenderer>();
        MarkerRenderer.sortingOrder = 1;
        transform.position = new Vector3(_currentHex.transform.position.x, _currentHex.transform.position.y, _currentHex.transform.position.z);
        _model.transform.rotation = Quaternion.Euler(0f , (int)CurrentRotation * 60f , 0f);
    }

    void Update()
    {
        _currentStateDelegate();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tree")
        {
            Transform lodGroup = other.transform.GetChild(0);
            lodGroup.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            lodGroup.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
            lodGroup.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
            lodGroup.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Transform lodGroup = other.transform.GetChild(0);
        lodGroup.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        lodGroup.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        lodGroup.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
        lodGroup.GetChild(3).GetComponent<MeshRenderer>().enabled = true;
    }

    #endregion

    #region Processing Input
    public abstract void OnLeftMouseDownMovement();
    public abstract void OnLeftMouseDownFight();
    public abstract void OnRightMouseDownMovement();
    public abstract void OnRightMouseDownFight();
    public abstract void OnLeftMouseDownEnemyFightCheck();
    public abstract void OnLeftMouseDownEnemyFightUncheck();
    public abstract void OnRightMouseDownEnemyFight();

    public virtual void HandleLeftClick(Hex hex)
    {
        if (CurrentState == State.Idle)
        {
            _click.Play();
            if (Grid.ShowPath && hex == Grid.Path[Grid.Path.Count - 1]) GoToTarget();
            else FindPath(hex);
        }
        else if (CurrentState == State.Moving || CurrentState == State.Rotating) RewindMovement();
    }

    public void HandleRightClick(Hex hex)
    {
        Debug.Log("Held!");
    }
    #endregion

    #region StateFuncs

    protected void Rotating()
    {
        _model.transform.rotation = Quaternion.RotateTowards(_model.transform.rotation, _destinationRotation, RotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(_model.transform.rotation, _destinationRotation) <= 1)
        {
            CurrentRotation = (HexDirection)(_destinationRotation.eulerAngles.y / 60);
            if (Grid.Path.Count != 0)
            {
                _currentStateDelegate = Moving;
                _currentState = State.Moving;
            }
            else
            {
                foreach (Hex neighbor in CurrentHex.GetNeighbors())
                    if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                _currentStateDelegate = Idle;
                Grid.ShowRotationFields = false;
                Animator.SetBool("Walking", false);
                _currentHex.TurnOffArrowsRenderers();
                _currentState = State.Idle;
                Grid.HideApproachables();
                Grid.ApproachableHexes.Clear();
                FindApproachableHexes();
            }
        }
    }

    protected void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, Grid.Path[0].transform.position, MoveSpeed * Time.deltaTime);
        if (transform.position == Grid.Path[0].transform.position) OnArrivalToHex(Grid.Path[0]);
    }


    protected void Idle() { }

    
    #endregion

    #region Pathfinding & Movement

    protected void GoToTarget()
    {
        Animator.SetBool("Walking", true);
        _currentHex.TurnOffArrowsRenderers();
        Grid.HideApproachablesWithoutPath();
        OnArrivalToHex(CurrentHex);
    }

    protected void OnArrivalToHex(Hex arrivalHex)
    {
        if (_achievedFirstHex)
        {
            CurrentHex = arrivalHex;
            PathRemoveFirst();
        }
        else _achievedFirstHex = true;
        
        if (Grid.Path.Count == 0)
        {
            Animator.SetBool("Walking", false);
            if (CurrentHex.IsInEnemyZone) Mobility = 0;
            _currentState = State.Idle;
            _currentStateDelegate = Idle;
            Grid.ShowPath = false;
            Grid.ResetAproachablesCosts();
            FindApproachableHexes();
        }
        else
        {
            if (_currentHex.GetNeighbor(CurrentRotation) == Grid.Path[0])
            {
                _currentState = State.Moving;
                _currentStateDelegate = Moving;
            }
            else Rotate(Grid.Path[0]);
        }
    }

    protected void FindApproachableHexes()
    {
        Grid.ResetAproachablesEnemyZone();
        Grid.ApproachableHexes.Clear();
        if (_isBlockedByEnemyControlZone) return;
        Grid.Frontier.Clear();
        Grid.Frontier.Enqueue(CurrentHex);
        Grid.CameFrom.Clear();
        Grid.CameFrom.Add(CurrentHex, null);
        Grid.CostSoFar.Clear();
        Grid.CostSoFar.Add(CurrentHex, 0f);
        Grid.FromDirection.Clear();
        Grid.FromDirection.Add(CurrentHex, (int)CurrentRotation);

        if (Mobility == 0) return;


        bool isAlreadyInEnemyControlZone = false;
        foreach (Hex hex in CurrentHex.GetNeighbors())
        {
            if (hex && hex.Unit && hex.Unit.Side != Side)
            {
                if (hex.GetDirection(CurrentHex) == hex.Unit.CurrentRotation.Previous() ||
                         hex.GetDirection(CurrentHex) == hex.Unit.CurrentRotation ||
                         hex.GetDirection(CurrentHex) == hex.Unit.CurrentRotation.Next())
                    isAlreadyInEnemyControlZone = true;
            }
        }

        if (isAlreadyInEnemyControlZone)
        {
            for (HexDirection direction = HexDirection.N; direction <= HexDirection.NW; direction++)
            {
                Hex hex = CurrentHex.GetNeighbor(direction);
                if (hex && !hex.IsUnderWater && !hex.Unit)
                {
                    bool hexAvailable = true;
                    foreach (Hex neighbor in hex.GetNeighbors())
                    {
                        if (neighbor && neighbor.Unit && neighbor.Unit.Side != Side)
                        {
                            if (neighbor.GetDirection(hex) == neighbor.Unit.CurrentRotation.Previous() ||
                                     neighbor.GetDirection(hex) == neighbor.Unit.CurrentRotation ||
                                     neighbor.GetDirection(hex) == neighbor.Unit.CurrentRotation.Next())
                            {
                                hexAvailable = false;
                                break;
                            }
                        }
                    }
                    if (hexAvailable)
                    {
                        Grid.CameFrom[hex] = CurrentHex;
                        Grid.ApproachableHexes.Add(hex);
                        Grid.CostSoFar[hex] = Mobility;
                        hex.CostText.text = Mobility.ToString();
                        hex.SetColorOfCost(Mobility);
                        hex.CostWithTurnings = Mobility;
                        Grid.FromDirection[hex] = (int)direction;
                    }
                }
                
            }
            foreach (Hex hex in Grid.ApproachableHexes)
            {
                hex.Renderer.sprite = hex.Sprites[0];
                if (Grid.FromDirection[hex] != (int)CurrentRotation) CurrentHex.ArrowRenderers[Grid.FromDirection[hex]].sprite = hex.Sprites[4];
                hex.CostText.gameObject.SetActive(true);
            }
        }
        else
        {
            while (Grid.Frontier.Count != 0)
            {
                Hex current = Grid.Frontier.Dequeue();
                for (HexDirection direction = HexDirection.N; direction <= HexDirection.NW; direction++)
                {
                    Hex neighbor = current.GetNeighbor(direction);
                    if (neighbor != null && !neighbor.IsUnderWater && !neighbor.Unit)
                    {
                        bool HasRoadOrBridgeConnection = (current.HasRoadOrBridge() && neighbor.HasRoadOrBridge());
                        bool heightDiff = false;
                        float newCost = Grid.CostSoFar[current];
                        if (HasRoadOrBridgeConnection) newCost += 1f;
                        else
                        {
                            newCost += neighbor.AbsoluteCost;
                            
                            if (neighbor.transform.position.y - current.transform.position.y > 2f)
                            {
                                newCost++;
                                heightDiff = true;
                            }
                        }
                        bool isTurning = false;
                        if ((int)direction != Grid.FromDirection[current])
                        {
                            newCost++;
                            isTurning = true;
                        }

                        if ((!Grid.CostSoFar.ContainsKey(neighbor) && newCost <= Mobility) || (Grid.CostSoFar.ContainsKey(neighbor) && newCost < Grid.CostSoFar[neighbor]))
                        {
                            if (isTurning)
                            {
                                Grid.FromDirection[neighbor] = (int)direction;
                                if (HasRoadOrBridgeConnection) neighbor.CostWithTurnings = 2f;
                                else
                                {
                                    neighbor.CostWithTurnings = neighbor.AbsoluteCost + 1;
                                    if (heightDiff) neighbor.CostWithTurnings++;
                                }
                            }
                            else
                            {
                                if (HasRoadOrBridgeConnection) neighbor.CostWithTurnings = 1f;
                                else
                                {
                                    neighbor.CostWithTurnings = neighbor.AbsoluteCost;
                                    if (heightDiff) neighbor.CostWithTurnings++;
                                }
                                Grid.FromDirection[neighbor] = Grid.FromDirection[current];
                            }

                            if (!neighbor.IsInEnemyZone) foreach (Hex hex in neighbor.GetNeighbors())
                                {
                                    if (hex && hex.Unit && hex.Unit.Side != Side)
                                    {
                                        if (hex.GetDirection(neighbor) == hex.Unit.CurrentRotation.Previous() ||
                                                 hex.GetDirection(neighbor) == hex.Unit.CurrentRotation ||
                                                 hex.GetDirection(neighbor) == hex.Unit.CurrentRotation.Next())
                                        {
                                            neighbor.IsInEnemyZone = true;
                                            break;
                                        }
                                    }
                                }

                            Grid.CostSoFar[neighbor] = newCost;
                            neighbor.CostText.text = newCost.ToString();
                            neighbor.SetColorOfCost(newCost);
                            Grid.CameFrom[neighbor] = current;
                            Grid.ApproachableHexes.Add(neighbor);
                            if (!neighbor.IsInEnemyZone) Grid.Frontier.Enqueue(neighbor);
                        }
                    }
                }
            }
            foreach (Hex hex in Grid.ApproachableHexes)
            {
                if (hex.IsInEnemyZone) hex.Renderer.sprite = hex.Sprites[6];
                else hex.Renderer.sprite = hex.Sprites[0];
                Hex fromHex = Grid.CameFrom[hex];
                if (Grid.FromDirection[hex] != Grid.FromDirection[fromHex]) fromHex.ArrowRenderers[Grid.FromDirection[hex]].sprite = hex.Sprites[4];
                hex.CostText.gameObject.SetActive(true);
            }
        }
    }
   
    protected void FindPath(Hex goal)
    {
        if (Grid.Path.Count != 0) // when the path is not empty 
        {
            foreach (Hex hex in Grid.Path) if (Grid.ApproachableHexes.Contains(hex))
            {
                if (!hex.IsInEnemyZone) hex.Renderer.sprite = hex.Sprites[0];
                else hex.Renderer.sprite = hex.Sprites[6];
                if (Grid.ShowPath) // if the path is displayed
                {
                   Grid.Path[0].SetColorOfCost(Grid.CostSoFar[Grid.Path[0]]);  // resetting a color of the first field in path
                   if (Grid.Path.Count > 1) Grid.Path[1].SetColorOfCost(Grid.CostSoFar[Grid.Path[1]]); // resetting a color of the second field in path
                }
            }
            Grid.Path.Clear(); // clearing the path
        }

        Hex current = goal;
        while (current != CurrentHex)
        {
            
            if (Grid.CostSoFar[current] == 1) current.CostText.color = HexDims.AdditionalCostColors[0]; // we change a color of displayed cost to light green
            else if (Grid.CostSoFar[current] == 2) current.CostText.color = HexDims.AdditionalCostColors[1]; // we change a color of displayed cost to green
            current.Renderer.sprite = current.Sprites[3];
            Grid.Path.Add(current);
            current = Grid.CameFrom[current];
        }
        Grid.Path.Reverse();
        Grid.Path[Grid.Path.Count - 1].Renderer.sprite = Grid.Path[Grid.Path.Count - 1].Sprites[2];
        Grid.ShowPath = true;
        _achievedFirstHex = false;
    }

    

    protected void PathRemoveFirst()
    {
        Grid.Path[0].TurnOffAllRenderers();
        Mobility -= Grid.Path[0].CostWithTurnings;
        Grid.Path[0].ResetCostWithTurnings();
        Grid.Path.RemoveAt(0);
    }

    protected Quaternion UnitRotation(HexDirection currentDirection, HexDirection nextDirection)
    {
        return Quaternion.Euler(0, (int)CurrentRotation * 60 + currentDirection.SmallestDifference(nextDirection) * 60, 0);
    }

    protected Quaternion UnitRotation(HexDirection currentDirection, Hex currentHex, Hex neighbor)
    {
        HexDirection nextDirection = currentHex.GetDirection(neighbor);
        return UnitRotation(currentDirection, nextDirection);
    }

    public void Rotate(Hex neighbor)
    {
        _currentState = State.Rotating;
        _currentStateDelegate = Rotating;
        _destinationRotation = UnitRotation(CurrentRotation, _currentHex, neighbor);
    }

    protected Quaternion ArrowRotation(HexDirection currentDirection, HexDirection nextDirection)
    {
        return Quaternion.Euler(90, 0, (int)CurrentRotation * 60 + currentDirection.SmallestDifference(nextDirection) * 60);
    }

    protected Quaternion ArrowRotation(HexDirection currentDirection, Hex currentHex, Hex neighbor)
    {
        HexDirection nextDirection = currentHex.GetDirection(neighbor);
        return ArrowRotation(currentDirection, nextDirection);
    }

    public void RewindMovement()
    {
        if (!Grid.ShowRotationFields)
        {
            CurrentHex = Grid.Path[Grid.Path.Count - 1];
            CurrentRotation = (HexDirection)Grid.FromDirection[_currentHex];
            _model.transform.rotation = Quaternion.Euler(0f, (int)CurrentRotation * 60f, 0f);
            while (Grid.Path.Count > 0) PathRemoveFirst();
            if (CurrentHex.IsInEnemyZone) Mobility = 0;
            Animator.SetBool("Walking", false);
            _currentState = State.Idle;
            _currentStateDelegate = Idle;
            Grid.ShowPath = false;
            FindApproachableHexes();
        }
        else
        {
            CurrentRotation = (HexDirection)(_destinationRotation.eulerAngles.y / 60);
            _model.transform.rotation = _destinationRotation;
        }
    }


    #endregion

    #region Additional Funcs
    public void Deactivate()
    {
        gameObject.SetActive(false);
        CurrentHex = null;
    }

    public void SwitchState()
    {
        _condition = false;
        _power = _tPower;
    }

    public void SetUnitInfoText()
    {
        InfoText.text = Name[0].ToString() + (Side ? ".N" : ".E") + "\n" + Power + "-" + Mobility;
    }

    public void SetUnitBarText()
    {
        _unitBarText.text = Name + "\nPower: " + Power + "\nMobility: " + Mobility + "\nArmor: " + ShieldsIcons.Count + "\nState: " + (Condition ? "Fresh" : "Tired");
    }

    #endregion
}

