using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.MKG.MB_NC
{
    public class InputListener : MonoBehaviour
    {


        private HexGrid _grid;
        private IngameUI _ingameUI;
        private float _acumTime = 0f;
        private readonly float _holdTime = 0.8f;
        private bool _touchHeldProcessed = false;
        private Hex _touchedHex;
        private PointerEventData _pointerEventData;
        private GraphicRaycaster _graphicRaycaster;
        private EventSystem _eventSystem;
        public static bool TouchedOnContext = false;
        public static int CouroutineCounter = 0;

        void Awake()
        {
            Input.simulateMouseWithTouches = false;
        }

        void Start()
        {
            _eventSystem = GetComponent<EventSystem>();
            _graphicRaycaster = GameObject.Find("Canvas UI").GetComponent<GraphicRaycaster>();
            _ingameUI = GameObject.Find("Ingame UI Logic").GetComponent<IngameUI>();
            _grid = gameObject.GetComponent<HexMapCamera>().Grid;
            if (_grid == null) Debug.Log("chuj");
        }


        public bool IsPointerOverUIObject(Vector3 inputPosition)
        {
            _pointerEventData = new PointerEventData(_eventSystem) { position = inputPosition };
            List<RaycastResult> results = new List<RaycastResult>();
            _graphicRaycaster.Raycast(_pointerEventData, results);
            return results.Count > 0;
        }

        void Update()
        {
            if (_ingameUI.WinStatementActive)
            {
                if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) ||
                        Input.GetMouseButtonDown(0))
                {
                    if (MatchManager.Camera.SlidingThroughUnits) MatchManager.Camera.ShowWinInstantly();
                    else _ingameUI.OnQuitGameButton();
                }
            }
            else if (!_ingameUI.InGameUIActive)
            {
                if (MatchManager.Camera.CurrentCameraState == HexMapCamera.CameraState.ShowingUnitsObligedToFight)
                {
                    if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) ||
                        Input.GetMouseButtonDown(0))
                        MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.Free;
                }
                else if (MatchManager.Camera.CurrentCameraState == HexMapCamera.CameraState.ShowingFights)
                {
                    if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) ||
                        Input.GetMouseButtonDown(0))
                    {
                        if (MatchManager.Camera.Index == MatchManager.Camera.Units.Count)
                        {
                            _ingameUI.TurnButton.enabled = true;
                            MatchManager.Camera.HideMarkersFromPreviousFight();
                            MatchManager.Camera.CurrentCameraState = HexMapCamera.CameraState.Free;

                        }
                        else
                        {
                            if (MatchManager.Camera.SlidingThroughUnits)
                                MatchManager.Camera.ShowNextFight();
                            else
                                MatchManager.Camera.SlideToNextFight();
                        }
                    }
                }
                else
                {
                    if (Input.touchCount == 1)
                    {
                        _acumTime += Input.GetTouch(0).deltaTime;

                        if (_acumTime >= _holdTime && !_touchHeldProcessed)
                        {
                            _touchHeldProcessed = true;
                            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;
                            if (!IsPointerOverUIObject(Input.GetTouch(0).position) && Physics.Raycast(inputRay, out hit))
                            {
                                Hex hex = _grid.GetHex(hit.point);
                                if (hex == _touchedHex)
                                {
                                    if (hex && hex.Unit)
                                    {
                                        switch (hex.Unit.Unlocked)
                                        {
                                            case 1:
                                                hex.Unit.UnitController.OnRightMouseDownMovement();
                                                break;
                                            case 2:
                                                hex.Unit.UnitController.OnRightMouseDownFight();
                                                break;
                                            case 3:
                                                hex.Unit.UnitController.OnRightMouseDownEnemyFight();
                                                break;
                                            default:
                                                if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleRightClick(hex);
                                                break;
                                        }
                                    }
                                    else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleRightClick(hex);
                                }
                            }
                            else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleRightClick(null);
                        }

                        if (Input.GetTouch(0).phase == TouchPhase.Ended)
                        {
                            if (_acumTime < _holdTime)
                            {
                                Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;
                                if (!IsPointerOverUIObject(Input.GetTouch(0).position) && Physics.Raycast(inputRay, out hit))
                                {
                                    Hex hex = _grid.GetHex(hit.point);
                                    if (hex == _touchedHex)
                                    {
                                        if (hex && hex.Unit)
                                        {
                                            switch (hex.Unit.Unlocked)
                                            {
                                                case 1:
                                                    hex.Unit.UnitController.OnLeftMouseDownMovement();
                                                    break;
                                                case 2:
                                                    hex.Unit.UnitController.OnLeftMouseDownFight();
                                                    break;
                                                case 3:
                                                    hex.Unit.UnitController.OnLeftMouseDownEnemyFightCheck();
                                                    break;
                                                case 4:
                                                    hex.Unit.UnitController.OnLeftMouseDownEnemyFightUncheck();
                                                    break;
                                                default:
                                                    if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleLeftClick(hex);
                                                    break;
                                            }
                                        }
                                        else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleLeftClick(hex);
                                    }
                                }
                                else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleLeftClick(null);
                            }
                            _acumTime = 0;
                            _touchHeldProcessed = false;
                        }
                        else if (Input.GetTouch(0).phase == TouchPhase.Began)
                        {
                            if (IsPointerOverUIObject(Input.GetTouch(0).position))
                                StartCoroutine(TouchDelay());
                            else
                            {
                                Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;
                                if (Physics.Raycast(inputRay, out hit))
                                {
                                    _touchedHex = _grid.GetHex(hit.point);

                                    if (_touchedHex)
                                    {
                                        if (((_touchedHex.Unit && _touchedHex.Unit.Unlocked != 0) || (MatchManager.CurrentlyChecked && _grid.ApproachableHexes.Contains(_touchedHex))))
                                            StartCoroutine(TouchDelay());
                                    }
                                }
                            }
                        }
                    }
#if UNITY_EDITOR
                    else if (Input.touchCount == 0)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;
                            if (!IsPointerOverUIObject(Input.mousePosition) && Physics.Raycast(inputRay, out hit))
                            {
                                Hex hex = _grid.GetHex(hit.point);
                                if (hex && hex.Unit)
                                {
                                    switch (hex.Unit.Unlocked)
                                    {
                                        case 1:
                                            hex.Unit.UnitController.OnLeftMouseDownMovement();
                                            break;
                                        case 2:
                                            hex.Unit.UnitController.OnLeftMouseDownFight();
                                            break;
                                        case 3:
                                            hex.Unit.UnitController.OnLeftMouseDownEnemyFightCheck();
                                            break;
                                        case 4:
                                            hex.Unit.UnitController.OnLeftMouseDownEnemyFightUncheck();
                                            break;
                                        default:
                                            if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleLeftClick(hex);
                                            break;
                                    }
                                }
                                else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleLeftClick(hex);
                            }
                            else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleLeftClick(null);
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;
                            if (!IsPointerOverUIObject(Input.mousePosition) && Physics.Raycast(inputRay, out hit))
                            {
                                Hex hex = _grid.GetHex(hit.point);
                                if (hex && hex.Unit)
                                {
                                    switch (hex.Unit.Unlocked)
                                    {
                                        case 1:
                                            hex.Unit.UnitController.OnRightMouseDownMovement();
                                            break;
                                        case 2:
                                            hex.Unit.UnitController.OnRightMouseDownFight();
                                            break;
                                        case 3:
                                            hex.Unit.UnitController.OnLeftMouseDownEnemyFightCheck();
                                            break;
                                        case 4:
                                            hex.Unit.UnitController.OnLeftMouseDownEnemyFightUncheck();
                                            break;
                                        default:
                                            if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleRightClick(hex);
                                            break;
                                    }
                                }
                                else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleRightClick(hex);
                            }
                            else if (MatchManager.CurrentlyChecked) MatchManager.CurrentlyChecked.UnitController.HandleRightClick(null);
                        }

                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            UnityEditor.EditorApplication.isPlaying = false;
                            Application.Quit();
                        }
                        //else if (Input.GetKeyDown(KeyCode.Backspace)) SceneManager.LoadScene("Main Menu");
                    }
#endif
                    else if (Input.touchCount > 1)
                    {
                        _touchedHex = null;
                    }
                }
            }
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
    }
}
