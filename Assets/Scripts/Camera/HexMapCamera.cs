using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using TMPro;



public class HexMapCamera : MonoBehaviour
{
    #region Variables & Properties
    [SerializeField]
    private float _scrollBorderThickness = 0.005f;  // percentage of screen height

    // FOR DEBUG
    /*//####################

    [SerializeField]
    private TextMeshProUGUI _fpsCounter;

    private float _updateInterval = 0.5f;
    private float _accum = 0.0f; // FPS accumulated over the interval
    private int _frames = 0; // Frames drawn over the interval
    private float _timeLeft; // Left time for current interval

    //###################*/

    [SerializeField]
    private float _stickMinZoom = -250f;
    [SerializeField]
    private float _stickMaxZoom = -45f;
    [SerializeField]
    private float _swivelMinZoom = 90f;
    [SerializeField]
    private float _swivelMaxZoom = 60f;
    [SerializeField]
    private float _moveSpeedMinZoom = 400f;
    [SerializeField]
    private float _moveSpeedMaxZoom = 100f;
    [SerializeField]
    private float _moveSpeedMinZoomTouch = 2.25f;
    [SerializeField]
    private float _moveSpeedMaxZoomTouch = 0.25f;
    [SerializeField]
    private float _rotationSpeedKeyboard = 150f;
    [SerializeField]
    private float _rotationSpeedTouch = 1.5f;
    private Transform _swivel, _stick;
    private static float _zoom = 0f;
    private HexGrid _grid;
    public HexGrid Grid { get { return _grid; } }
    private FightMechanics _fightMechanics;
    private IngameUI _ingameUI;
    private Hex[,] _hexes;
    [SerializeField]
    private PostProcessingProfile _lowProfile;
    [SerializeField]
    private PostProcessingProfile _midProfile;
    [SerializeField]
    private PostProcessingProfile _highProfile;
    private PostProcessingBehaviour _postProcessingBehaviour;
    private GameObject _cameraObject;
    private GameObject _light;
    public static bool TouchedOnContext = false;
    public static int CouroutineCounter = 0;
    private List<Unit> _units = new List<Unit>();
    public List<Unit> Units { get { return _units; } }
    private bool _centeredOnEnemy = false;
    public bool CenteredOnEnemy { get { return _centeredOnEnemy; } }
    private int _index = 0;
    public int Index { get { return _index; } }
    private int _zoomState;
    [SerializeField]
    private GameObject _fightStatement;
    [SerializeField]
    private GameObject _fightStatement2;
    private bool _slidingThroughUnits = false;
    public bool SlidingThroughUnits { get { return _slidingThroughUnits; } }
    private float _angleToRotate;
    private bool _sideAngle;
    private Hex targetCamp;
    private delegate void currentCameraState();
    private delegate void angleSwitchingParam();
    private currentCameraState _currentCameraStateDelegate;
    public enum CameraState { ShowingUnitsObligedToFight, ShowingFights, ShowingWin, Free };
    private CameraState _currentCameraState = CameraState.Free;
    public CameraState CurrentCameraState
    {
        get { return _currentCameraState; }
        set
        {
            CameraState previousState = _currentCameraState;
            _currentCameraState = value;
            switch (value)
            {
                case CameraState.Free:
                    SetCameraFree(previousState);
                    break;
                case CameraState.ShowingFights:
                    SetCameraShowingFights();
                    break;

                case CameraState.ShowingUnitsObligedToFight:
                    SetCameraShowingUnitsObligedToFight();
                    break;

                case CameraState.ShowingWin:
                    SetCameraShowingWin();
                    break;
            }
        }
    }
    #endregion

    #region CameraStateFuncs
    private void SetCameraFree(CameraState previousState)
    {
        _currentCameraStateDelegate = CameraFree;
        switch(previousState)
        {
            case CameraState.ShowingFights:

                _fightStatement2.SetActive(false);
                _units.Clear();
                _zoomState = 0;
                _index = 0;
                _slidingThroughUnits = false;
                foreach (Unit enemy in _grid.EnemiesInControlZone)
                {
                    enemy.AttackedFromBack = false;
                    enemy.AttackingEnemies.Clear();
                }
                _grid.EnemiesInControlZone.Clear();
                foreach (Unit unit in _grid.UnitsWithEnemies)
                {
                    unit.AvailableEnemies.Clear();
                    unit.AttackedEnemies.Clear();
                }
                _grid.UnitsWithEnemies.Clear();
                _grid.UnitsAttackingManyOrOne.Clear();
                _grid.EnemyUnitsAttackedByMany.Clear();
                if (_grid.AngloSaxonCounter == 0 && _grid.VikingCounter == 0)
                {
                    _ingameUI.ShowWin(false, false);
                    return;
                }
                if (_grid.CurrentPhase == 3)
                {
                    if (_grid.CurrentTurn == 30)
                    {
                        _ingameUI.ShowWin(false, false);
                        return;
                    }
                    _grid.CurrentTurnText.text = "Turn:  " + (++_grid.CurrentTurn).ToString() + "/30";
                    foreach (Unit unit in _grid.Units)
                    {
                        unit.Mobility = unit.MaxMobility;
                        unit.SetUnitInfoText();
                    }
                    _grid.CurrentPhase = 0;
                    foreach (InfantryUnit unit in _grid.InfantryUnits)
                        if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                }
                else _grid.CurrentPhase = 2;
                _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                break;

            case CameraState.ShowingUnitsObligedToFight:

                _fightStatement.SetActive(false);
                _units.Clear();
                _zoomState = 0;
                _index = 0;
                _slidingThroughUnits = false;
                _ingameUI.TurnButton.enabled = true;
                break;
        }
    }

    private void SetCameraShowingFights()
    {
        _fightStatement2.SetActive(true);
        _currentCameraStateDelegate = ShowingFights;
        _angleToRotate = transform.localRotation.eulerAngles.y;
        if (_angleToRotate <= 90f)
            _sideAngle = true;
        else if (_angleToRotate >= 270f)
        {
            _angleToRotate = -360 + _angleToRotate;
            _sideAngle = true;
        }
        else if (_angleToRotate > 90f)
        {
            _angleToRotate = -180f + _angleToRotate;
            _sideAngle = false;
        }
        _slidingThroughUnits = true;
        foreach (Unit enemy in _grid.EnemyUnitsAttackedByMany) _units.Add(enemy);
        foreach (Unit unit in _grid.UnitsAttackingManyOrOne) _units.Add(unit);
        _units.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
        if (_zoom > 0.75f) _zoomState = 1;
        else if (_zoom < 0.75f) _zoomState = 2;
        else _zoomState = 0;
        SlideToNextFight();
    }

    private void SetCameraShowingUnitsObligedToFight()
    {
        _fightStatement.SetActive(true);
        _currentCameraStateDelegate = ShowingUnitsObligedToFight;
        _angleToRotate = transform.localRotation.eulerAngles.y;
        if (_angleToRotate <= 90f)
            _sideAngle = true;
        else if (_angleToRotate >= 270f)
        {
            _angleToRotate = -360 + _angleToRotate;
            _sideAngle = true;
        }
        else if (_angleToRotate > 90f)
        {
            _angleToRotate = -180f + _angleToRotate;
            _sideAngle = false;
        }
        _slidingThroughUnits = true;
        foreach (Unit unit in _grid.UnitsWithEnemies)
        {
            if (unit.AttackedEnemies.Count == 0)
            {
                _units.Add(unit);
                unit.MarkerRenderer.sprite = unit.Markers[3];
            }
            else unit.MarkerRenderer.sprite = unit.Markers[2];
        }
        foreach (Unit enemy in _grid.EnemiesInControlZone)
        {
            if (enemy.AttackingEnemies.Count == 0)
            {
                _units.Add(enemy);
                enemy.MarkerRenderer.sprite = enemy.Markers[5];
            }
            else enemy.MarkerRenderer.sprite = null;
        }
        _units.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
        if (_zoom > 0.75f) _zoomState = 1;
        else if (_zoom < 0.75f) _zoomState = 2;
        else _zoomState = 0;
    }

    
    private void SetCameraShowingWin()
    {
        _currentCameraStateDelegate = ShowingWin;
        if (transform.localRotation.eulerAngles.y > 90f && transform.localRotation.eulerAngles.y <= 270f)
            _angleToRotate = transform.localRotation.eulerAngles.y - 90f;
        else if (transform.localRotation.eulerAngles.y >= 0f && transform.localRotation.eulerAngles.y < 90f)
            _angleToRotate = -90 + transform.localRotation.eulerAngles.y;
        else
            _angleToRotate = -450 + transform.localRotation.eulerAngles.y;

        if (_ingameUI.WinCameraTarget) targetCamp = _grid.VikingsCamp;
        else targetCamp = _grid.AngloSaxonsCamp;

        if (_zoom > 0.75f) _zoomState = 1;
        else if (_zoom < 0.75f) _zoomState = 2;
        else _zoomState = 0;

        _slidingThroughUnits = true;
    }


    private void CameraFree()
    {
        if (Input.touchCount > 0)
        {
            if (Input.touchCount > 1)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float turnAngle = HexDims.Angle(touchZero.position, touchOne.position);
                float prevTurn = HexDims.Angle(touchZero.position - touchZero.deltaPosition, touchOne.position - touchOne.deltaPosition);
                float turnAngleDelta = Mathf.DeltaAngle(prevTurn, turnAngle);

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                if (Mathf.Abs(deltaMagnitudeDiff) > 0f) AdjustZoom(-deltaMagnitudeDiff / 300);

                if (Mathf.Abs(turnAngleDelta) > 0f) AdjustRotationTouch(turnAngleDelta);
            }
            else if (!TouchedOnContext)
            {
                float xDelta = Input.GetTouch(0).deltaPosition.x;
                float zDelta = Input.GetTouch(0).deltaPosition.y;

                if (xDelta != 0f || zDelta != 0f) AdjustPositionTouch(-xDelta, -zDelta);
            }
        }
        //FOR DEBUGGING
        else
        {
            TouchedOnContext = false;

            float xDelta = 0f, zDelta = 0f, rotationDelta = 0f;

            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f) AdjustZoom(zoomDelta);

            //if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - _scrollBorderThickness) zDelta++;
            //if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.mousePosition.y <= _scrollBorderThickness) zDelta--;
            //if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - _scrollBorderThickness) xDelta++;
            //if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.mousePosition.x <= _scrollBorderThickness) xDelta--;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Comma)) rotationDelta++;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Period)) rotationDelta--;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) zDelta++;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) zDelta--;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) xDelta++;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) xDelta--;

            if (xDelta != 0f || zDelta != 0f) AdjustPositionMouse(xDelta, zDelta);
            if (rotationDelta != 0f) AdjustRotationKeyboard(rotationDelta);
        }

    }


    private void ZoomSwitching()
    {
        if (_zoomState == 2)
        {
            AdjustZoom(Time.deltaTime * 0.65f);
            if (_zoom >= 0.9f) _zoomState = 0;
        }
        else if (_zoomState == 1)
        {
            AdjustZoom(Time.deltaTime * -0.65f);
            if (_zoom <= 0.9f) _zoomState = 0;
        }
    }


    private void AngleSwitchingTarget()
    {
        if (_sideAngle)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            foreach (Hex hex in _hexes) hex.CostText.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            foreach (Unit unit in _grid.Units) unit.CanvasInfo.transform.rotation = Quaternion.Euler(unit.CanvasInfo.transform.rotation.eulerAngles.x, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            foreach (Hex hex in _hexes) hex.CostText.transform.rotation = Quaternion.Euler(90f, 180f, 0f);
            foreach (Unit unit in _grid.Units) unit.CanvasInfo.transform.rotation = Quaternion.Euler(unit.CanvasInfo.transform.rotation.eulerAngles.x, 180f, 0f);
        }
    }

    private void AngleSwitchingTargetShowingWin()
    {
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        foreach (Hex hex in _hexes) hex.CostText.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
        foreach (Unit unit in _grid.Units) unit.CanvasInfo.transform.rotation = Quaternion.Euler(unit.CanvasInfo.transform.rotation.eulerAngles.x, 90f, 0f);
    }

    private void AngleSwitching(angleSwitchingParam angleSwitchingTarget)
    {
        if (_angleToRotate > 0f)
        {
            if (_angleToRotate - _rotationSpeedKeyboard * Time.deltaTime > 0)
            {
                _angleToRotate -= _rotationSpeedKeyboard * Time.deltaTime;
                AdjustRotationKeyboard(-1f);
            }
            else
            {
                _angleToRotate = 0f;
                angleSwitchingTarget();
            }
        }
        else if (_angleToRotate < 0f)
        {

            if (_angleToRotate + _rotationSpeedKeyboard * Time.deltaTime < 0)
            {
                AdjustRotationKeyboard(1f);
                _angleToRotate += _rotationSpeedKeyboard * Time.deltaTime;
            }
            else
            {
                _angleToRotate = 0f;
                angleSwitchingTarget();
            }
        }
    }


    

    private void ShowingFights()
    {
        ZoomSwitching();
        AngleSwitching(AngleSwitchingTarget);
        if (_slidingThroughUnits)
        {
            Vector3 newPosition = new Vector3(_units[_index].transform.position.x, transform.position.y, _sideAngle ? _units[_index].transform.position.z + 50f : _units[_index].transform.position.z - 50f);
            if (_index == 0) transform.position = Vector3.MoveTowards(transform.position, newPosition, 130f * Time.deltaTime);
            else transform.position = Vector3.MoveTowards(transform.position, newPosition, 45f * Time.deltaTime);
            if (transform.position == newPosition)
            {
                _fightMechanics.HandleVisualAspectOfFight(_centeredOnEnemy, _units[_index]);
                _index++;
                _slidingThroughUnits = false;
            }
        }
    }


    private void ShowingUnitsObligedToFight()
    {
        ZoomSwitching();
        AngleSwitching(AngleSwitchingTarget);
        if (_slidingThroughUnits)
        {
            Vector3 newPosition = new Vector3(_units[_index].transform.position.x, transform.position.y, _sideAngle ? _units[_index].transform.position.z + 50f : _units[_index].transform.position.z - 50f);
            if (_index == 0) transform.position = Vector3.MoveTowards(transform.position, newPosition, 130f * Time.deltaTime);
            else transform.position = Vector3.MoveTowards(transform.position, newPosition, 45f * Time.deltaTime);
            if (transform.position == newPosition)
            {
                _index++;
                if (_index == _units.Count)
                {
                    _index = 0;
                    _slidingThroughUnits = false;
                }
            }
        }
    }

    private void ShowingWin()
    {
        ZoomSwitching();
        AngleSwitching(AngleSwitchingTargetShowingWin);
        if (_slidingThroughUnits)
        {
            Vector3 newPosition = new Vector3(targetCamp.transform.position.x + 35f, transform.position.y, targetCamp.transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, newPosition, 115f * Time.deltaTime);
            if (transform.position == newPosition) _slidingThroughUnits = false;
        }
    }
    #endregion

    #region UnityFuncs - Start, Update...
    void Awake()
    {
        //_timeLeft = _updateInterval;
        _currentCameraStateDelegate = CameraFree;
        _zoom = 0f;
        _light = GameObject.Find("Directional Light");
        _cameraObject = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        _postProcessingBehaviour = _cameraObject.GetComponent<PostProcessingBehaviour>();
        _swivel = transform.GetChild(0);
        _stick = _swivel.GetChild(0);
        _scrollBorderThickness *= Screen.height; // because ScrollBorderThickness should be percentage of screen height 
        TouchedOnContext = false;
        _grid = GameObject.Find("Game").GetComponentInChildren<HexGrid>();
        _fightMechanics = gameObject.AddComponent<FightMechanics>();
        _fightMechanics.Grid = _grid;
    }

    void OnEnable()
    {
       if (QualitySettings.GetQualityLevel() == 0)
       {
           _postProcessingBehaviour.profile = _midProfile;
            _light.GetComponent<Light>().intensity = 1.65f;
       }
       else if (QualitySettings.GetQualityLevel() == 1)
       {
          _postProcessingBehaviour.profile = _midProfile;
         _light.GetComponent<Light>().intensity = 1.65f;
       }
       else
       {
            _postProcessingBehaviour.profile = _highProfile;
            _light.GetComponent<Light>().intensity = 1.65f;
       }
    }

    void Start()
    {
        _hexes = _grid.Hexes;
        _ingameUI = GameObject.Find("Ingame UI Logic").GetComponent<IngameUI>();
    }

    void LateUpdate()
    {
        _currentCameraStateDelegate();

        
        
        //FOR DEBUG
        /*//######################

        _timeLeft -= Time.deltaTime;
        _accum += 1 / Time.deltaTime;
        _frames++;

        // Interval ended - update GUI text and start new interval
        if (_timeLeft <= 0f)
        {
            
            _timeLeft = _updateInterval;
            _fpsCounter.text = Mathf.RoundToInt(_accum / _frames).ToString() + "  " + _timeLeft.ToString();
            _accum = 0f;
            _frames = 0;
        }
        
        //#######################*/

    }
    #endregion

    #region Additional Funcs
    void AdjustZoom(float delta)
    {
        _zoom = Mathf.Clamp01(_zoom + delta);

        float distance = Mathf.Lerp(_stickMinZoom, _stickMaxZoom, _zoom);
        _stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(_swivelMinZoom, _swivelMaxZoom, _zoom);
        _swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);

        foreach (Unit unit in _grid.Units)
        {
            unit.CanvasInfo.transform.localRotation = Quaternion.Euler(angle, unit.CanvasInfo.transform.localRotation.eulerAngles.y, 0f);
            float textScale = Mathf.Lerp(unit.OriginalInfoSize.x, 0.25f, _zoom);
            unit.InfoAndIcons.transform.localScale = new Vector3(textScale, textScale, unit.OriginalInfoSize.z);
        }
        foreach (Hex hex in _hexes) foreach (SpriteRenderer renderer in hex.ArrowRenderers) renderer.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.85f, 0.85f, 0.85f), _zoom);
    }

    void AdjustPositionMouse(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float distance = Mathf.Lerp(_moveSpeedMinZoom, _moveSpeedMaxZoom, _zoom) * Time.deltaTime;
        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    void AdjustPositionTouch(float xDelta, float zDelta)
    {
        Vector3 delta = transform.localRotation * new Vector3(xDelta, 0f, zDelta) * Mathf.Lerp(_moveSpeedMinZoomTouch, _moveSpeedMaxZoomTouch, _zoom);
        Vector3 position = transform.localPosition;
        position += delta;
        transform.localPosition = ClampPosition(position);
    }

    void AdjustRotationKeyboard(float angle)
    {
        angle *= _rotationSpeedKeyboard * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y + angle, 0f);
        foreach (Hex hex in _hexes) hex.CostText.transform.rotation = Quaternion.Euler(90f, hex.CostText.transform.rotation.eulerAngles.y + angle, 0f);
        foreach (Unit unit in _grid.Units) unit.CanvasInfo.transform.rotation = Quaternion.Euler(unit.CanvasInfo.transform.rotation.eulerAngles.x, unit.CanvasInfo.transform.rotation.eulerAngles.y + angle, 0f);
    }

    void AdjustRotationTouch(float angle)
    {
        angle *= _rotationSpeedTouch;
        transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y  + angle, 0f);
        foreach (Hex hex in _hexes) hex.CostText.transform.rotation = Quaternion.Euler(90f, hex.CostText.transform.rotation.eulerAngles.y + angle, 0f);
        foreach (Unit unit in _grid.Units) unit.CanvasInfo.transform.rotation = Quaternion.Euler(unit.CanvasInfo.transform.rotation.eulerAngles.x, unit.CanvasInfo.transform.rotation.eulerAngles.y + angle, 0f);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (_grid.HexCountX - 1f) * (1.5f * HexDims.OuterRadius) + 60f;
        position.x = Mathf.Clamp(position.x, -60f, xMax);

        float zMax = (_grid.HexCountZ - 0.5f)* (2f * HexDims.InnerRadius) + 60f;
        position.z = Mathf.Clamp(position.z, -60f, zMax);

        return position;
    }

    public void ShowWinInstantly()
    {
        transform.position =  new Vector3(targetCamp.transform.position.x + 35f, transform.position.y, targetCamp.transform.position.z);
        _slidingThroughUnits = false;
        SetZoomAndAngleInstantly(false);
    }

    public void ShowNextFight()
    {
        transform.position = new Vector3(_units[_index].transform.position.x, transform.position.y, _sideAngle ? _units[_index].transform.position.z + 50f : _units[_index].transform.position.z - 50f);
        if (_index == 0) SetZoomAndAngleInstantly(true);
        _fightMechanics.HandleVisualAspectOfFight(_centeredOnEnemy, _units[_index]);
        _slidingThroughUnits = false;
        _index++;
    }

    public void HideMarkersFromPreviousFight()
    {
        _units[_index - 1].MarkerRenderer.sprite = null;
        if (_centeredOnEnemy)
            foreach (Unit unit in _units[_index - 1].AttackingEnemies)
                unit.MarkerRenderer.sprite = null;
        else
            foreach (Unit enemy in _units[_index - 1].AttackedEnemies)
                enemy.MarkerRenderer.sprite = null;
        
    }

    public void SlideToNextFight()
    {
        if (_index > 0) HideMarkersFromPreviousFight();
        _slidingThroughUnits = true;
        if (_grid.EnemyUnitsAttackedByMany.Contains(_units[_index]))
        {
            _centeredOnEnemy = true;
            _fightMechanics.ResolveFightCenteredOnEnemy(_units[_index]);
        }
        else // _grid.UnitsAttackingManyOrOne.Contains(_units[_index])
        {
            _centeredOnEnemy = false;
            _fightMechanics.ResolveFightCenteredOnUnit(_units[_index]);
        }
    }


    private void SetZoomAndAngleInstantly(bool type) // true - showingFights, false - showingWin
    {
        _zoom = 0.9f;
        _zoomState = 0;
        _angleToRotate = 0f;

        if (type)
            AngleSwitchingTarget();
        else
            AngleSwitchingTargetShowingWin();

        float distance = Mathf.Lerp(_stickMinZoom, _stickMaxZoom, _zoom);
        _stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(_swivelMinZoom, _swivelMaxZoom, _zoom);
        _swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);

        foreach (Unit unit in _grid.Units)
        {
            unit.CanvasInfo.transform.localRotation = Quaternion.Euler(angle, unit.CanvasInfo.transform.localRotation.eulerAngles.y, 0f);
            float textScale = Mathf.Lerp(unit.OriginalInfoSize.x, 0.25f, _zoom);
            unit.InfoAndIcons.transform.localScale = new Vector3(textScale, textScale, unit.OriginalInfoSize.z);
        }
        foreach (Hex hex in _hexes) foreach (SpriteRenderer renderer in hex.ArrowRenderers) renderer.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.85f, 0.85f, 0.85f), _zoom);
    }

   public static IEnumerator TouchDelay()
   {
       CouroutineCounter++;
       TouchedOnContext = true;
       yield return new WaitForSeconds(0.15f);
       CouroutineCounter--;
       if (CouroutineCounter == 0)
            TouchedOnContext = false;
    }
    #endregion
}
