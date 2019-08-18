using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


namespace com.MKG.MB_NC
{
    public class IngameUI : UIModule
    {

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
            MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingWin;
        }

        public override void Awake()
        {
            base.Awake();
            _grid = GameObject.Find("Game").GetComponentInChildren<HexGrid>();
        }

        public void OnNextPhaseButton()
        {
            _source.Play();
            UnitManager tempCurrentlyChecked;
            if (!InGameUIActive && !WinStatementActive)
            {
                switch (MatchManager.CurrentPhase)
                {
                    case 0:

                        if (MatchManager.CurrentlyChecked)
                        {
                            MatchManager.CurrentlyChecked.IsChecked = false;
                            MatchManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            _grid.HideApproachables();
                            if (MatchManager.CurrentlyChecked.CurrentState != UnitManager.State.Idle) MatchManager.CurrentlyChecked.UnitMovement.RewindMovement();
                            MatchManager.CurrentlyChecked = null;

                        }

                        if (_grid.AngloSaxonsCamp.Unit && _grid.AngloSaxonsCamp.Unit.Side == MatchManager.Side.Northman)
                        {
                            ShowWin(true, true);
                            return;
                        }

                        foreach (UnitManager unit in MatchManager.UnitsFirstSide)
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
                                                MatchManager.UnitsWithEnemies.Add(unit);
                                            }
                                            if (!MatchManager.EnemiesInControlZone.Contains(neighbor.Unit))
                                                MatchManager.EnemiesInControlZone.Add(neighbor.Unit);
                                            unit.AvailableEnemies.Add(neighbor.Unit);
                                            if (neighbor.Unit.MarkerRenderer.sprite == null)
                                                neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                        }
                                    }
                                }
                            }
                        }

                        MatchManager.CurrentPhase = 1;
                        MatchManager.CurrentPhaseText.text = MatchManager.Phases[MatchManager.CurrentPhase];

                        break;

                    case 1:

                        tempCurrentlyChecked = MatchManager.CurrentlyChecked;
                        if (MatchManager.CurrentlyChecked)
                        {
                            MatchManager.CurrentlyChecked.IsChecked = false;
                            MatchManager.CurrentlyChecked = null;
                        }
                        if (MatchManager.UnitsWithEnemies.Count != 0)
                        {
                            foreach (UnitManager unit in MatchManager.UnitsWithEnemies)
                            {
                                if (unit.AttackedEnemies.Count == 0)
                                {
                                    MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }
                            foreach (UnitManager enemy in MatchManager.EnemiesInControlZone)
                            {
                                if (enemy.AttackingEnemies.Count == 0)
                                {
                                    MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;//ShowingUnitsObligedToFight = true;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }

                            foreach (UnitManager unit in MatchManager.UnitsWithEnemies)
                            {
                                unit.MarkerRenderer.sprite = null;
                                if (tempCurrentlyChecked) foreach (UnitManager enemy in tempCurrentlyChecked.AttackedEnemies)
                                        enemy.MarkerRenderer.sprite = null;
                                if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                                {
                                    if (!MatchManager.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                        MatchManager.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                                }
                                else
                                    MatchManager.UnitsAttackingManyOrOne.Add(unit);
                            }
                            MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingFights;
                            TurnButton.enabled = false;
                        }
                        else
                        {
                            foreach (InfantryUnitManager unit in MatchManager.InfantryUnits)
                                if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                            MatchManager.CurrentPhase = 2;
                            MatchManager.CurrentPhaseText.text = MatchManager.Phases[MatchManager.CurrentPhase];
                        }

                        break;

                    case 2:

                        if (MatchManager.CurrentlyChecked)
                        {
                            MatchManager.CurrentlyChecked.IsChecked = false;
                            MatchManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            _grid.HideApproachables();
                            if (MatchManager.CurrentlyChecked.CurrentState != UnitManager.State.Idle) MatchManager.CurrentlyChecked.UnitMovement.RewindMovement();
                            MatchManager.CurrentlyChecked = null;
                        }

                        if (_grid.VikingsCamp.Unit && _grid.VikingsCamp.Unit.Side == MatchManager.Side.Anglosaxons)
                        {
                            ShowWin(true, false);
                            return;
                        }

                        foreach (UnitManager unit in MatchManager.UnitsSecondSide)
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
                                                MatchManager.UnitsWithEnemies.Add(unit);
                                            }
                                            if (!MatchManager.EnemiesInControlZone.Contains(neighbor.Unit))
                                                MatchManager.EnemiesInControlZone.Add(neighbor.Unit);
                                            unit.AvailableEnemies.Add(neighbor.Unit);
                                            if (neighbor.Unit.MarkerRenderer.sprite == null)
                                                neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                        }
                                    }
                                }
                            }
                        }

                        MatchManager.CurrentPhase = 3;
                        MatchManager.CurrentPhaseText.text = MatchManager.Phases[MatchManager.CurrentPhase];

                        break;

                    case 3:

                        tempCurrentlyChecked = MatchManager.CurrentlyChecked;
                        if (MatchManager.CurrentlyChecked)
                            if (MatchManager.CurrentlyChecked)
                            {
                                MatchManager.CurrentlyChecked.IsChecked = false;
                                MatchManager.CurrentlyChecked = null;
                            }
                        if (MatchManager.UnitsWithEnemies.Count != 0)
                        {
                            foreach (UnitManager unit in MatchManager.UnitsWithEnemies)
                            {
                                if (unit.AttackedEnemies.Count == 0)
                                {
                                    MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }
                            foreach (UnitManager enemy in MatchManager.EnemiesInControlZone)
                            {
                                if (enemy.AttackingEnemies.Count == 0)
                                {
                                    MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }

                            foreach (UnitManager unit in MatchManager.UnitsWithEnemies)
                            {
                                unit.MarkerRenderer.sprite = null;
                                if (tempCurrentlyChecked) foreach (UnitManager enemy in tempCurrentlyChecked.AttackedEnemies)
                                        enemy.MarkerRenderer.sprite = null;
                                if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                                {
                                    if (!MatchManager.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                        MatchManager.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                                }
                                else
                                    MatchManager.UnitsAttackingManyOrOne.Add(unit);
                            }
                            MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingFights;
                            TurnButton.enabled = false;
                        }
                        else
                        {
                            if (MatchManager.CurrentTurn == 30)
                            {
                                ShowWin(false, false);
                                return;
                            }
                            MatchManager.CurrentTurnText.text = "Turn:  " + (++MatchManager.CurrentTurn).ToString() + "/30";
                            foreach (UnitManager unit in MatchManager.Units)
                            {
                                unit.Mobility = unit.MaxMobility;
                                unit.SetUnitInfoText();
                                unit.SetUnitBarText();
                                unit.AvailableEnemies.Clear();
                            }
                            foreach (InfantryUnitManager unit in MatchManager.InfantryUnits)
                                if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                            MatchManager.CurrentPhase = 0;
                            MatchManager.CurrentPhaseText.text = MatchManager.Phases[MatchManager.CurrentPhase];
                        }

                        break;
                }
            }
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
}
