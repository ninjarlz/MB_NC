using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class IngameUI : UIModule {

    public bool WinCameraTarget { get; set; } // true - viking camp, false - anglo-saxons camp
    [SerializeField]
    private GameObject _winStatement;
    [SerializeField]
    private TextMeshProUGUI _winDescription;
    public GameObject WinStatement { get { return _winStatement; } set { _winStatement = value; } }
    [SerializeField]
    private GameObject _inGameUI;
    [SerializeField]
    private GameObject _optionsMenu;
    public bool _inGameUIActive = false;
    public bool InGameUIActive { get { return _inGameUIActive; } set { _inGameUIActive = value; } }
    private bool _winStatementActive = false;
    public bool WinStatementActive
    {
        get { return _winStatementActive; }
        set
        {
            _winStatementActive = value;
            if (value)
                _winStatement.SetActive(true);
        }
    }
    private Color _clickedColor = new Color(130f / 255, 130f / 255, 130f / 255);
    [SerializeField]
    private Button _menuButton;
    private HexGrid _grid;
    [SerializeField]
    private Button _turnButton;
    public Button TurnButton
    {
        get { return _turnButton; }
    }
    [SerializeField]
    private HexMapCamera _hexCameraScript;
    

    public void ShowWin(bool winType, bool player) // true - camp, false - time delay
    {
        if (!winType)
        {
            WinCameraTarget = false;
            _winDescription.text = "Player 2 has won\nby defending a river.";
        }
        else
        {
            WinCameraTarget = player ? false : true;
            _winDescription.text = "Player " + (player ? "1" : "2") + " has won\nby capturing enemy camp.";
        }
        WinStatementActive = true;
        _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingWin;
    }

    public override void Awake()
    {
        base.Awake();
        _grid = GameObject.Find("Game").GetComponentInChildren<HexGrid>();
    }

    public void OnNextPhaseButton()
    {
        _source.Play();
        Unit tempCurrentlyChecked;
        if (!InGameUIActive && !WinStatementActive)
        {
            switch (_grid.CurrentPhase)
            {
                case 0:

                    if (_grid.CurrentlyChecked)
                    {
                        _grid.CurrentlyChecked.IsChecked = false;
                        _grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                        _grid.HideApproachables();
                        if (_grid.CurrentlyChecked.CurrentState != Unit.State.Idle) _grid.CurrentlyChecked.RewindMovement();
                        _grid.CurrentlyChecked = null;
                        
                    }

                    if (_grid.AngloSaxonsCamp.Unit && _grid.AngloSaxonsCamp.Unit.Side == true)
                    {
                        ShowWin(true, true);
                        return;
                    }

                    foreach (Unit unit in _grid.UnitsFirstSide)
                    {
                        if (unit.CurrentHex)
                        {
                            foreach (Hex neighbor in unit.CurrentHex.GetNeighbors())
                            {
                                if (neighbor && neighbor.Unit && neighbor.Unit.Side != unit.Side)
                                {
                                    if (unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Previous() ||
                                            unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation ||
                                            unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Next())
                                    {
                                        if (!unit.HasEnemies())
                                        {
                                            unit.MarkerRenderer.sprite = unit.Markers[3];
                                            _grid.UnitsWithEnemies.Add(unit);
                                        }
                                        if (!_grid.EnemiesInControlZone.Contains(neighbor.Unit))
                                            _grid.EnemiesInControlZone.Add(neighbor.Unit);
                                        unit.AvailableEnemies.Add(neighbor.Unit);
                                        if (neighbor.Unit.MarkerRenderer.sprite == null)
                                            neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                    }
                                }
                            }
                        }
                    }
                    
                    _grid.CurrentPhase = 1;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    
                    break;

                case 1:

                    tempCurrentlyChecked = _grid.CurrentlyChecked;
                    if (_grid.CurrentlyChecked)
                    {
                        _grid.CurrentlyChecked.IsChecked = false;
                        _grid.CurrentlyChecked = null;
                    }
                    if (_grid.UnitsWithEnemies.Count != 0)
                    {
                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            if (unit.AttackedEnemies.Count == 0)
                            {
                                _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                TurnButton.enabled = false;
                                return;
                            }
                        }
                        foreach (Unit enemy in _grid.EnemiesInControlZone)
                        {
                            if (enemy.AttackingEnemies.Count == 0)
                            {
                                _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;//ShowingUnitsObligedToFight = true;
                                TurnButton.enabled = false;
                                return;
                            }
                        }

                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            unit.MarkerRenderer.sprite = null;
                            if (tempCurrentlyChecked) foreach (Unit enemy in tempCurrentlyChecked.AttackedEnemies)
                                    enemy.MarkerRenderer.sprite = null;
                            if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                            {
                                if (!_grid.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                    _grid.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                            }
                            else
                                _grid.UnitsAttackingManyOrOne.Add(unit);
                        }
                        _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingFights;
                        TurnButton.enabled = false;
                    }
                    else
                    {
                        foreach (InfantryUnit unit in _grid.InfantryUnits)
                            if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                        _grid.CurrentPhase = 2;
                        _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    }

                    break;

               case 2:

                    if (_grid.CurrentlyChecked)
                    {
                        _grid.CurrentlyChecked.IsChecked = false;
                        _grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                        _grid.HideApproachables();
                        if (_grid.CurrentlyChecked.CurrentState != Unit.State.Idle) _grid.CurrentlyChecked.RewindMovement();
                        _grid.CurrentlyChecked = null;
                    }

                    if (_grid.VikingsCamp.Unit && _grid.VikingsCamp.Unit.Side == false)
                    {
                        ShowWin(true, false);
                        return;
                    }

                    foreach (Unit unit in _grid.UnitsSecondSide)
                    {
                        if (unit.CurrentHex)
                        {
                            foreach (Hex neighbor in unit.CurrentHex.GetNeighbors())
                            {
                                if (neighbor && neighbor.Unit && neighbor.Unit.Side != unit.Side)
                                {
                                    if (unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Previous() ||
                                        unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation ||
                                        unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Next())
                                    {
                                        if (!unit.HasEnemies())
                                        {
                                            unit.MarkerRenderer.sprite = unit.Markers[3];
                                            _grid.UnitsWithEnemies.Add(unit);
                                        }
                                        if (!_grid.EnemiesInControlZone.Contains(neighbor.Unit))
                                            _grid.EnemiesInControlZone.Add(neighbor.Unit);
                                        unit.AvailableEnemies.Add(neighbor.Unit);
                                        if (neighbor.Unit.MarkerRenderer.sprite == null)
                                            neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                    }
                                }
                            }
                        }
                    }
                    
                    _grid.CurrentPhase = 3;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];

                    break;

                case 3:

                    tempCurrentlyChecked = _grid.CurrentlyChecked;
                    if (_grid.CurrentlyChecked)
                        if (_grid.CurrentlyChecked)
                        {
                            _grid.CurrentlyChecked.IsChecked = false;
                            _grid.CurrentlyChecked = null;
                        }
                    if (_grid.UnitsWithEnemies.Count != 0)
                    {
                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            if (unit.AttackedEnemies.Count == 0)
                            {
                                _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                TurnButton.enabled = false;
                                return;
                            }
                        }
                        foreach (Unit enemy in _grid.EnemiesInControlZone)
                        {
                            if (enemy.AttackingEnemies.Count == 0)
                            {
                                _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                TurnButton.enabled = false;
                                return;
                            }
                        }

                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            unit.MarkerRenderer.sprite = null;
                            if (tempCurrentlyChecked) foreach (Unit enemy in tempCurrentlyChecked.AttackedEnemies)
                                    enemy.MarkerRenderer.sprite = null;
                            if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                            {
                                if (!_grid.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                    _grid.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                            }
                            else
                                _grid.UnitsAttackingManyOrOne.Add(unit);
                        }
                        _grid.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingFights;
                        TurnButton.enabled = false;
                    }
                    else
                    {
                        if (_grid.CurrentTurn == 30)
                        {
                            ShowWin(false, false);
                            return;
                        }
                        _grid.CurrentTurnText.text = "Turn:  " + (++_grid.CurrentTurn).ToString() + "/30";
                        foreach (Unit unit in _grid.Units)
                        {
                            unit.Mobility = unit.MaxMobility;
                            unit.SetUnitInfoText();
                            unit.SetUnitBarText();
                            unit.AvailableEnemies.Clear();
                        }
                        foreach (InfantryUnit unit in _grid.InfantryUnits)
                            if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                        _grid.CurrentPhase = 0;
                        _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    }

                    break;
            }
        }
        
        #region SystemImplementation
        /*
        _source.Play();
        Unit tempCurrentlyChecked;
        if (!InGameUIActive)
        {
            switch (_grid.CurrentPhase)
            {
                case 0:
                    _grid.CurrentPhase = 1;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    break;

                case 1:
                    _grid.CurrentPhase = 2;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    break;

                case 2:

                    if (_grid.CurrentlyChecked)
                    {
                        _grid.CurrentlyChecked.IsChecked = false;
                        _grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                        _grid.HideApproachables();
                        _grid.CurrentlyChecked = null;
                    }

                    foreach (Unit unit in _grid.UnitsFirstSide)
                    {
                        foreach (Hex neighbor in unit.CurrentHex.GetNeighbors())
                        {
                            if (neighbor && neighbor.Unit && neighbor.Unit.Side != unit.Side)
                            {
                                if (unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Previous() ||
                                        unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation ||
                                        unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Next())
                                {
                                    if (!unit.HasEnemies())
                                    {
                                        unit.MarkerRenderer.sprite = unit.Markers[3];
                                        _grid.UnitsWithEnemies.Add(unit);
                                    }
                                    if (!_grid.EnemiesInControlZone.Contains(neighbor.Unit))
                                        _grid.EnemiesInControlZone.Add(neighbor.Unit);
                                    unit.AvailableEnemies.Add(neighbor.Unit);
                                    if (neighbor.Unit.MarkerRenderer.sprite == null)
                                        neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                }
                            }
                        }
                    }
                    //_grid.CurrentPhase = ++_grid.CurrentPhase % 8;
                    _grid.CurrentPhase = 3;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];

                    break;

                case 3:

                    tempCurrentlyChecked = _grid.CurrentlyChecked;
                    if (_grid.CurrentlyChecked)
                    {
                        _grid.CurrentlyChecked.IsChecked = false;
                        _grid.CurrentlyChecked = null;
                    }
                    if (_grid.UnitsWithEnemies.Count != 0)
                    {
                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            if (unit.AttackedEnemies.Count == 0)
                            {
                                _grid.Camera.ShowingUnitsObligedToFight = true;
                                TurnButton.enabled = false;
                                return;
                            }
                        }
                        foreach (Unit enemy in _grid.EnemiesInControlZone)
                        {
                            if (enemy.AttackingEnemies.Count == 0)
                            {
                                _grid.Camera.ShowingUnitsObligedToFight = true;
                                TurnButton.enabled = false;
                                return;
                            }
                        }

                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            unit.MarkerRenderer.sprite = null;
                            if (tempCurrentlyChecked) foreach (Unit enemy in tempCurrentlyChecked.AttackedEnemies)
                                    enemy.MarkerRenderer.sprite = null;
                            if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                            {
                                if (!_grid.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                    _grid.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                            }
                            else
                                _grid.UnitsAttackingManyOrOne.Add(unit);
                        }
                        _grid.Camera.ShowingFights = true;
                        TurnButton.enabled = false;
                    }
                    else
                    {
                        foreach (InfantryUnit unit in _grid.InfantryUnits)
                            if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                        //_grid.CurrentPhase = ++_grid.CurrentPhase % 8;
                        _grid.CurrentPhase = 4;
                        _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    }

                    break;

                case 4:
                    _grid.CurrentPhase = 5;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    break;

                case 5:
                    _grid.CurrentPhase = 6;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    break;

                case 6:

                    if (_grid.CurrentlyChecked)
                    {
                        _grid.CurrentlyChecked.IsChecked = false;
                        _grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                        _grid.HideApproachables();
                        _grid.CurrentlyChecked = null;
                    }

                    foreach (Unit unit in _grid.UnitsSecondSide)
                    {
                        foreach (Hex neighbor in unit.CurrentHex.GetNeighbors())
                        {
                            if (neighbor && neighbor.Unit && neighbor.Unit.Side != unit.Side)
                            {
                                if (unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Previous() ||
                                    unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation ||
                                    unit.CurrentHex.GetDirection(neighbor) == unit.CurrentRotation.Next())
                                {
                                    if (!unit.HasEnemies())
                                    {
                                        unit.MarkerRenderer.sprite = unit.Markers[3];
                                        _grid.UnitsWithEnemies.Add(unit);
                                    }
                                    if (!_grid.EnemiesInControlZone.Contains(neighbor.Unit))
                                        _grid.EnemiesInControlZone.Add(neighbor.Unit);
                                    unit.AvailableEnemies.Add(neighbor.Unit);
                                    if (neighbor.Unit.MarkerRenderer.sprite == null)
                                        neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                }
                            }
                        }
                    }
                    _grid.CurrentPhase = 7;
                    _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];

                    break;

                case 7:

                    tempCurrentlyChecked = _grid.CurrentlyChecked;
                    if (_grid.CurrentlyChecked)
                        if (_grid.CurrentlyChecked)
                        {
                            _grid.CurrentlyChecked.IsChecked = false;
                            _grid.CurrentlyChecked = null;
                        }
                    if (_grid.UnitsWithEnemies.Count != 0)
                    {
                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            if (unit.AttackedEnemies.Count == 0)
                            {
                                _grid.Camera.ShowingUnitsObligedToFight = true;
                                TurnButton.enabled = false;
                                return;
                            }
                        }
                        foreach (Unit enemy in _grid.EnemiesInControlZone)
                        {
                            if (enemy.AttackingEnemies.Count == 0)
                            {
                                _grid.Camera.ShowingUnitsObligedToFight = true;
                                TurnButton.enabled = false;
                                return;
                            }
                        }

                        foreach (Unit unit in _grid.UnitsWithEnemies)
                        {
                            unit.MarkerRenderer.sprite = null;
                            if (tempCurrentlyChecked) foreach (Unit enemy in tempCurrentlyChecked.AttackedEnemies)
                                    enemy.MarkerRenderer.sprite = null;
                            if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                            {
                                if (!_grid.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                    _grid.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                            }
                            else
                                _grid.UnitsAttackingManyOrOne.Add(unit);
                        }
                        _grid.Camera.ShowingFights = true;
                        TurnButton.enabled = false;
                    }
                    else
                    {
                        _grid.CurrentTurnText.text = "Turn:  " + (++_grid.CurrentTurn).ToString() + "/20";
                        foreach (Unit unit in _grid.Units)
                        {
                            unit.Mobility = unit.MaxMobility;
                            unit.SetUnitInfoText();
                            unit.SetUnitBarText();
                            unit.AvailableEnemies.Clear();
                        }
                        foreach (InfantryUnit unit in _grid.InfantryUnits)
                            if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                        _grid.CurrentPhase = 0;
                        _grid.CurrentPhaseText.text = HexGrid.Phases[_grid.CurrentPhase];
                    }

                    break;
            }
        }*/
        #endregion
    }

    public override void OnQuitGameButton()
    {
        base.OnQuitGameButton();
        SceneManager.LoadScene("Main Menu");
    }

    public void OnMenuButton()
    {
        if (InGameUIActive)
        {
            _menuButton.GetComponent<Image>().color = Color.white;
            InGameUIActive = false;
        }
        else
        {
            InGameUIActive = true;
            _menuButton.GetComponent<Image>().color = _clickedColor;
        }
        _inGameUI.SetActive(InGameUIActive);
        _source.Play();
    }

    public void OnSaveGameButton()
    {
        _source.Play();
    }

    public override void OnOptionsButton()
    {
        base.OnOptionsButton();
        _hexCameraScript.enabled = false;
    }

    public override void OnBackButton()
    {
        base.OnBackButton();
        _hexCameraScript.enabled = true;
    }

    public IEnumerator TurnButtonUnlock()
    {
        yield return new WaitForEndOfFrame();
        TurnButton.enabled = true;
    }
}
