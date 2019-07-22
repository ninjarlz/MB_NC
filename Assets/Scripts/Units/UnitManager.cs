using UnityEngine;
using System.Collections.Generic;
using TMPro;


public abstract class UnitManager : MonoBehaviour
{

    #region OtherUnitComponents

    UnitMovement _unitMovement;
    UnitController _unitController;
    public UnitController UnitController { get { return _unitController; } }
    public UnitMovement UnitMovement { get { return _unitMovement; } }

    #endregion 

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
    public State CurrentState
    {
        get { return _currentState; }
        set
        {
            switch (value)
            {
                case State.Idle:
                    _currentStateDelegate = Idle;
                    break;
                case State.Moving:
                    _currentStateDelegate = Moving;
                    break;
                case State.Rotating:
                    _currentStateDelegate = Rotating;
                    break;
            }
            _currentState = value;
        }
    }
    protected delegate void currentState();
    protected currentState _currentStateDelegate;
    protected Hex _destinationHex;
    protected Quaternion _destinationRotation;
    public Quaternion DestinationRotation { get { return _destinationRotation; } set { _destinationRotation = value; } }
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
    protected GameManager.Side _side;
    public GameManager.Side Side { get { return _side; } }

    [SerializeField]
    protected int _power;
    public int Power { get { return _power; } set { _power = value; } }
    [SerializeField]
    private int _tPower;
    public int TPower { get { return _tPower; } }
    protected bool _shouldDie = false;
    public bool ShouldDie { get { return _shouldDie; } set { _shouldDie = value; } }
    public enum Condition { Fresh, Tired }
    [SerializeField]
    protected Condition _currentCondition = Condition.Fresh;
    public Condition CurrentCondition { get { return _currentCondition; } set { _currentCondition = value; } } // true - fresh, false - tired
    protected AudioSource _attackSound;
    public AudioSource AttackSound { get { return _attackSound; } }
    public Animator Animator { get; set; }
    [SerializeField]
    protected GameObject _canvasUI;
    protected GameObject _model;
    public GameObject Model { get { return _model; } }
    public List<UnitManager> FightList { get; set; }
    public int IndexInFightList { get; set; }
    public List<UnitManager> AttackingEnemies { get; private set; }
    protected bool _attackedFromBack = false;
    public bool AttackedFromBack { get { return _attackedFromBack; } set { _attackedFromBack = value; } }
    public int AttackingEnemiesCounter { get { return AttackingEnemies.Count; } }
    protected bool _attackAssigned = false;
    public bool AttackAssigned { get { return _attackAssigned; } set { _attackAssigned = value; } }
    public List<UnitManager> AvailableEnemies { get; private set; }
    public List<UnitManager> AttackedEnemies { get; private set; }
    protected List<GameObject> _shieldsIcons = new List<GameObject>();
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
                if (GameManager.CurrentPhase == 1 || GameManager.CurrentPhase == 3)
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
        AttackingEnemies = new List<UnitManager>();
        AttackedEnemies = new List<UnitManager>();
        AvailableEnemies = new List<UnitManager>();
        Grid = GameObject.Find("Game").GetComponentInChildren<HexGrid>();
        GameManager.Units.Add(this);
        if (Side == GameManager.Side.Northman) GameManager.UnitsFirstSide.Add(this);
        else GameManager.UnitsSecondSide.Add(this);
        _currentStateDelegate = Idle;
        Animator = GetComponentInChildren<Animator>();
        _model = transform.GetChild(2).gameObject;
        _canvasUI = GameObject.Find("Canvas UI");
        _unitBar = _canvasUI.transform.GetChild(1).gameObject;
        _unitBarText = _unitBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _currentHex.Unit = this;
        
        CanvasInfo = transform.GetChild(0).gameObject;
        InfoAndIcons = CanvasInfo.transform.GetChild(0).gameObject;
        InfoText = InfoAndIcons.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        OriginalInfoSize = InfoAndIcons.transform.localScale;
        SetUnitInfoText();
        if (Side == GameManager.Side.Northman) InfoText.color = new Color(253f / 255, 194f / 255, 194f / 255);
        else InfoText.color = new Color(115f / 255, 231f / 255, 241f / 255);
        _marker = transform.GetChild(1).gameObject;
        MarkerRenderer = _marker.GetComponent<SpriteRenderer>();
        MarkerRenderer.sortingOrder = 1;
        transform.position = new Vector3(_currentHex.transform.position.x, _currentHex.transform.position.y, _currentHex.transform.position.z);
        _model.transform.rotation = Quaternion.Euler(0f, (int)CurrentRotation * 60f, 0f);

        _unitMovement = GetComponent<UnitMovement>();
        _unitController = GetComponent<UnitController>();

        _unitMovement.Setup(this);
        _unitController.Setup(this, _unitMovement);
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

    
  

    #region StateFuncs

    public void Rotating()
    {
        _model.transform.rotation = Quaternion.RotateTowards(_model.transform.rotation, _destinationRotation, RotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(_model.transform.rotation, _destinationRotation) <= 1)
        {
            CurrentRotation = (HexDirection)(_destinationRotation.eulerAngles.y / 60);
            if (Grid.Path.Count != 0) CurrentState = State.Moving;
            else
            {
                foreach (Hex neighbor in CurrentHex.GetNeighbors())
                    if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                Grid.ShowRotationFields = false;
                Animator.SetBool("Walking", false);
                _currentHex.TurnOffArrowsRenderers();
                CurrentState = State.Idle;
                Grid.HideApproachables();
                Grid.ApproachableHexes.Clear();
                _unitMovement.FindApproachableHexes();
            }
        }
    }

    public void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, Grid.Path[0].transform.position, MoveSpeed * Time.deltaTime);
        if (transform.position == Grid.Path[0].transform.position) _unitMovement.OnArrivalToHex(Grid.Path[0]);
    }


    public void Idle() { }

    #endregion

    #region Additional Funcs
    public void Deactivate()
    {
        gameObject.SetActive(false);
        CurrentHex = null;
    }

    public void SwitchState()
    {
        _currentCondition = Condition.Tired;
        _power = _tPower;
    }

    public void SetUnitInfoText()
    {
        InfoText.text = Name[0].ToString() + (Side == GameManager.Side.Northman ? ".N" : ".E") + "\n" + Power + "-" + Mobility;
    }

    public void SetUnitBarText()
    {
        _unitBarText.text = Name + "\nPower: " + Power + "\nMobility: " + Mobility + "\nArmor: " + ShieldsIcons.Count + "\nState: " + (CurrentCondition == Condition.Fresh ? "Fresh" : "Tired");
    }

    #endregion
}

