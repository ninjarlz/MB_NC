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
            GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingWin;
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
                switch (GameManager.CurrentPhase)
                {
                    case 0:

                        if (GameManager.CurrentlyChecked)
                        {
                            GameManager.CurrentlyChecked.IsChecked = false;
                            GameManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            _grid.HideApproachables();
                            if (GameManager.CurrentlyChecked.CurrentState != UnitManager.State.Idle) GameManager.CurrentlyChecked.UnitMovement.RewindMovement();
                            GameManager.CurrentlyChecked = null;

                        }

                        if (_grid.AngloSaxonsCamp.Unit && _grid.AngloSaxonsCamp.Unit.Side == GameManager.Side.Northman)
                        {
                            ShowWin(true, true);
                            return;
                        }

                        foreach (UnitManager unit in GameManager.UnitsFirstSide)
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
                                                GameManager.UnitsWithEnemies.Add(unit);
                                            }
                                            if (!GameManager.EnemiesInControlZone.Contains(neighbor.Unit))
                                                GameManager.EnemiesInControlZone.Add(neighbor.Unit);
                                            unit.AvailableEnemies.Add(neighbor.Unit);
                                            if (neighbor.Unit.MarkerRenderer.sprite == null)
                                                neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                        }
                                    }
                                }
                            }
                        }

                        GameManager.CurrentPhase = 1;
                        GameManager.CurrentPhaseText.text = GameManager.Phases[GameManager.CurrentPhase];

                        break;

                    case 1:

                        tempCurrentlyChecked = GameManager.CurrentlyChecked;
                        if (GameManager.CurrentlyChecked)
                        {
                            GameManager.CurrentlyChecked.IsChecked = false;
                            GameManager.CurrentlyChecked = null;
                        }
                        if (GameManager.UnitsWithEnemies.Count != 0)
                        {
                            foreach (UnitManager unit in GameManager.UnitsWithEnemies)
                            {
                                if (unit.AttackedEnemies.Count == 0)
                                {
                                    GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }
                            foreach (UnitManager enemy in GameManager.EnemiesInControlZone)
                            {
                                if (enemy.AttackingEnemies.Count == 0)
                                {
                                    GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;//ShowingUnitsObligedToFight = true;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }

                            foreach (UnitManager unit in GameManager.UnitsWithEnemies)
                            {
                                unit.MarkerRenderer.sprite = null;
                                if (tempCurrentlyChecked) foreach (UnitManager enemy in tempCurrentlyChecked.AttackedEnemies)
                                        enemy.MarkerRenderer.sprite = null;
                                if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                                {
                                    if (!GameManager.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                        GameManager.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                                }
                                else
                                    GameManager.UnitsAttackingManyOrOne.Add(unit);
                            }
                            GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingFights;
                            TurnButton.enabled = false;
                        }
                        else
                        {
                            foreach (InfantryUnitManager unit in GameManager.InfantryUnits)
                                if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                            GameManager.CurrentPhase = 2;
                            GameManager.CurrentPhaseText.text = GameManager.Phases[GameManager.CurrentPhase];
                        }

                        break;

                    case 2:

                        if (GameManager.CurrentlyChecked)
                        {
                            GameManager.CurrentlyChecked.IsChecked = false;
                            GameManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            _grid.HideApproachables();
                            if (GameManager.CurrentlyChecked.CurrentState != UnitManager.State.Idle) GameManager.CurrentlyChecked.UnitMovement.RewindMovement();
                            GameManager.CurrentlyChecked = null;
                        }

                        if (_grid.VikingsCamp.Unit && _grid.VikingsCamp.Unit.Side == GameManager.Side.Anglosaxons)
                        {
                            ShowWin(true, false);
                            return;
                        }

                        foreach (UnitManager unit in GameManager.UnitsSecondSide)
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
                                                GameManager.UnitsWithEnemies.Add(unit);
                                            }
                                            if (!GameManager.EnemiesInControlZone.Contains(neighbor.Unit))
                                                GameManager.EnemiesInControlZone.Add(neighbor.Unit);
                                            unit.AvailableEnemies.Add(neighbor.Unit);
                                            if (neighbor.Unit.MarkerRenderer.sprite == null)
                                                neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[5];
                                        }
                                    }
                                }
                            }
                        }

                        GameManager.CurrentPhase = 3;
                        GameManager.CurrentPhaseText.text = GameManager.Phases[GameManager.CurrentPhase];

                        break;

                    case 3:

                        tempCurrentlyChecked = GameManager.CurrentlyChecked;
                        if (GameManager.CurrentlyChecked)
                            if (GameManager.CurrentlyChecked)
                            {
                                GameManager.CurrentlyChecked.IsChecked = false;
                                GameManager.CurrentlyChecked = null;
                            }
                        if (GameManager.UnitsWithEnemies.Count != 0)
                        {
                            foreach (UnitManager unit in GameManager.UnitsWithEnemies)
                            {
                                if (unit.AttackedEnemies.Count == 0)
                                {
                                    GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }
                            foreach (UnitManager enemy in GameManager.EnemiesInControlZone)
                            {
                                if (enemy.AttackingEnemies.Count == 0)
                                {
                                    GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingUnitsObligedToFight;
                                    TurnButton.enabled = false;
                                    return;
                                }
                            }

                            foreach (UnitManager unit in GameManager.UnitsWithEnemies)
                            {
                                unit.MarkerRenderer.sprite = null;
                                if (tempCurrentlyChecked) foreach (UnitManager enemy in tempCurrentlyChecked.AttackedEnemies)
                                        enemy.MarkerRenderer.sprite = null;
                                if (unit.AttackedEnemies.Count == 1 && unit.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJE W ATAKU KILKU NA JEDNEGO
                                {
                                    if (!GameManager.EnemyUnitsAttackedByMany.Contains(unit.AttackedEnemies[0]))
                                        GameManager.EnemyUnitsAttackedByMany.Add(unit.AttackedEnemies[0]);
                                }
                                else
                                    GameManager.UnitsAttackingManyOrOne.Add(unit);
                            }
                            GameManager.Camera.CurrentCameraState = HexMapCamera.CameraState.ShowingFights;
                            TurnButton.enabled = false;
                        }
                        else
                        {
                            if (GameManager.CurrentTurn == 30)
                            {
                                ShowWin(false, false);
                                return;
                            }
                            GameManager.CurrentTurnText.text = "Turn:  " + (++GameManager.CurrentTurn).ToString() + "/30";
                            foreach (UnitManager unit in GameManager.Units)
                            {
                                unit.Mobility = unit.MaxMobility;
                                unit.SetUnitInfoText();
                                unit.SetUnitBarText();
                                unit.AvailableEnemies.Clear();
                            }
                            foreach (InfantryUnitManager unit in GameManager.InfantryUnits)
                                if (!unit.ShowTurnIcon) unit.ShowTurnIcon = true;
                            GameManager.CurrentPhase = 0;
                            GameManager.CurrentPhaseText.text = GameManager.Phases[GameManager.CurrentPhase];
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
