using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class InfantryUnitController : UnitController
    {

        private InfantryUnitMovement _infantryUnitMovement;


        public override void Setup(UnitManager unitManager, UnitMovement unitMovement)
        {
            base.Setup(unitManager, unitMovement);
            _infantryUnitMovement = (InfantryUnitMovement)unitMovement;
        }


        #region Processing Input
        public override void OnLeftMouseDownMovement()
        {
            if (_unitManager.IsChecked)
            {
                if (_unitManager.CurrentState == UnitManager.State.Idle)
                {
                    _unitManager.IsChecked = false;
                    MatchManager.CurrentlyChecked = null;
                    _unitManager.CurrentHex.TurnOffArrowsRenderers();
                    _grid.HideApproachables();
                    if (_grid.ShowRotationFields)
                    {
                        _grid.ShowRotationFields = false;
                        foreach (Hex neighbor in _unitManager.CurrentHex.GetNeighbors())
                            if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                    }
                    _click.Play();
                }
                else _infantryUnitMovement.RewindMovement();
            }
            else
            {
                if (MatchManager.CurrentlyChecked)
                {
                    if (MatchManager.CurrentlyChecked.CurrentState == UnitManager.State.Idle)
                    {
                        if (_grid.ShowRotationFields)
                        {
                            if (_grid.ApproachableHexes.Contains(_unitManager.CurrentHex))
                            {
                                foreach (Hex neighbor in _grid.ApproachableHexes) if (neighbor != _unitManager.CurrentHex)
                                    {
                                        if (!neighbor.Unit) neighbor.TurnOffAllRenderers();
                                        else neighbor.Unit.MarkerRenderer.sprite = null;
                                    }
                                MatchManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                                MatchManager.CurrentlyChecked.Animator.SetBool("Walking", true);
                                MatchManager.CurrentlyChecked.UnitMovement.Rotate(_unitManager.CurrentHex);
                                if (MatchManager.CurrentlyChecked.Mobility == 0) MatchManager.CurrentlyChecked.ShowTurnIcon = false;
                                else MatchManager.CurrentlyChecked.Mobility--;
                                _click.Play();
                            }
                            else
                            {
                                _grid.ShowRotationFields = false;
                                foreach (Hex neighbor in MatchManager.CurrentlyChecked.CurrentHex.GetNeighbors())
                                    if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                                MatchManager.CurrentlyChecked.IsChecked = false;
                                MatchManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                                _grid.HideApproachables();
                                _unitManager.IsChecked = true;
                                MatchManager.CurrentlyChecked = _unitManager;
                                _unitManager.SetUnitBarText();
                                _infantryUnitMovement.FindApproachableHexes();
                                _click.Play();
                            }
                        }
                        else
                        {
                            MatchManager.CurrentlyChecked.IsChecked = false;
                            MatchManager.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            _grid.HideApproachables();
                            _unitManager.IsChecked = true;
                            MatchManager.CurrentlyChecked = _unitManager;
                            _unitManager.SetUnitBarText();
                            _infantryUnitMovement.FindApproachableHexes();
                            _click.Play();
                        }
                    }
                    else MatchManager.CurrentlyChecked.UnitMovement.RewindMovement();
                }
                else
                {
                    _unitManager.IsChecked = true;
                    MatchManager.CurrentlyChecked = _unitManager;
                    _unitManager.SetUnitBarText();
                    _infantryUnitMovement.FindApproachableHexes();
                    _click.Play();
                }

            }
            _grid.ShowPath = false;
        }

        public override void OnLeftMouseDownFight()
        {
            if (_unitManager.IsChecked)
            {
                if (_unitManager.CurrentState == UnitManager.State.Idle)
                {
                    _unitManager.IsChecked = false;
                    MatchManager.CurrentlyChecked = null;
                    foreach (UnitManager enemy in _unitManager.AvailableEnemies)
                    {
                        if (enemy.AttackingEnemies.Count > 0) enemy.MarkerRenderer.sprite = null;
                        else enemy.MarkerRenderer.sprite = enemy.Markers[5];
                    }
                    _click.Play();
                }
                else _infantryUnitMovement.RewindMovement();
            }
            else
            {
                if (MatchManager.CurrentlyChecked)
                {
                    if (MatchManager.CurrentlyChecked.CurrentState == UnitManager.State.Idle)
                    {
                        MatchManager.CurrentlyChecked.IsChecked = false;
                        foreach (UnitManager enemy in MatchManager.CurrentlyChecked.AvailableEnemies)
                        {
                            if (enemy.AttackingEnemies.Count > 0) enemy.MarkerRenderer.sprite = null;
                            else enemy.MarkerRenderer.sprite = enemy.Markers[5];
                        }
                        _unitManager.IsChecked = true;
                        MatchManager.CurrentlyChecked = _unitManager;
                        _unitManager.SetUnitBarText();
                        if (_unitManager.AttackedEnemies.Count == 1 && _unitManager.AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJESZ W ATAKU KILKU NA JEDNEGO
                            _unitManager.AttackedEnemies[0].MarkerRenderer.sprite = _unitManager.Markers[4];
                        else
                        {
                            foreach (UnitManager enemy in _unitManager.AvailableEnemies)
                            {
                                if (_unitManager.AttackedEnemies.Contains(enemy)) enemy.MarkerRenderer.sprite = _unitManager.Markers[4];
                                else if (enemy.AttackingEnemies.Count == 0 ||
                                     (enemy.AttackingEnemies.Count >= 1 && enemy.AttackingEnemies[0].AttackedEnemies.Count == 1 &&
                                     _unitManager.AttackedEnemies.Count == 0))
                                    enemy.MarkerRenderer.sprite = _unitManager.Markers[1];
                            }
                        }
                        _click.Play();
                    }
                    else _infantryUnitMovement.RewindMovement();
                }
                else
                {
                    _unitManager.IsChecked = true;
                    MatchManager.CurrentlyChecked = _unitManager;
                    _unitManager.SetUnitBarText();
                    if (_unitManager.AttackedEnemies.Count == 1 && _unitManager.AttackedEnemies[0].AttackingEnemies.Count > 1)
                        _unitManager.AttackedEnemies[0].MarkerRenderer.sprite = _unitManager.Markers[4];
                    else
                    {
                        foreach (UnitManager enemy in _unitManager.AvailableEnemies)
                        {
                            if (_unitManager.AttackedEnemies.Contains(enemy)) enemy.MarkerRenderer.sprite = _unitManager.Markers[4];
                            else if (enemy.AttackingEnemies.Count == 0 ||
                                    (enemy.AttackingEnemies.Count >= 1 && enemy.AttackingEnemies[0].AttackedEnemies.Count == 1 &&
                                    _unitManager.AttackedEnemies.Count == 0))
                                enemy.MarkerRenderer.sprite = _unitManager.Markers[1];
                        }
                    }
                    _click.Play();
                }
            }
        }

        public override void OnRightMouseDownEnemyFight()
        {
            throw new System.NotImplementedException();
        }

        public override void OnLeftMouseDownEnemyFightUncheck()
        {
            _unitManager.AttackingEnemies.Remove(MatchManager.CurrentlyChecked);
            _unitManager.MarkerRenderer.sprite = _unitManager.Markers[1];
            MatchManager.CurrentlyChecked.AttackedEnemies.Remove(_unitManager);
            if (_unitManager.AttackingEnemies.Count > 0)
            {
                foreach (UnitManager enemy in MatchManager.CurrentlyChecked.AvailableEnemies)
                    enemy.MarkerRenderer.sprite = _unitManager.Markers[1];
            }
            else
            {
                if (MatchManager.CurrentlyChecked.AttackedEnemies.Count > 0)
                {
                    _unitManager.MarkerRenderer.sprite = _unitManager.Markers[1];
                    foreach (UnitManager enemy in MatchManager.CurrentlyChecked.AvailableEnemies)
                        if (enemy.AttackingEnemies.Count > 0 &&
                            !enemy.AttackingEnemies.Contains(MatchManager.CurrentlyChecked))
                            enemy.MarkerRenderer.sprite = null;
                }
                else foreach (UnitManager enemy in MatchManager.CurrentlyChecked.AvailableEnemies)
                        enemy.MarkerRenderer.sprite = _unitManager.Markers[1];
            }
            _click.Play();
        }

        public override void OnLeftMouseDownEnemyFightCheck()
        {
            _unitManager.AttackingEnemies.Add(MatchManager.CurrentlyChecked);
            MatchManager.CurrentlyChecked.AttackedEnemies.Add(_unitManager);
            if (_unitManager.AttackingEnemies.Count > 1)
                foreach (UnitManager enemy in MatchManager.CurrentlyChecked.AvailableEnemies)
                {
                    if (enemy.AttackingEnemies.Count > 0) enemy.MarkerRenderer.sprite = null;
                    else enemy.MarkerRenderer.sprite = enemy.Markers[5];
                }
            else foreach (UnitManager enemy in MatchManager.CurrentlyChecked.AvailableEnemies)
                    if (enemy.AttackingEnemies.Count > 0 &&
                        !enemy.AttackingEnemies.Contains(MatchManager.CurrentlyChecked))
                        enemy.MarkerRenderer.sprite = null;
            _unitManager.MarkerRenderer.sprite = _unitManager.Markers[4];
            _click.Play();
        }

        public override void OnRightMouseDownMovement()
        {
            if (_unitManager.IsChecked)
            {
                if (!_grid.ShowRotationFields) _infantryUnitMovement.FindRotationFields();
                else
                {
                    _grid.ShowRotationFields = false;
                    foreach (Hex neighbor in _unitManager.CurrentHex.GetNeighbors())
                        if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                    _grid.HideApproachables();
                    _grid.ApproachableHexes.Clear();
                    _unitManager.CurrentHex.TurnOffArrowsRenderers();
                    _infantryUnitMovement.FindApproachableHexes();
                }
                _click.Play();
            }
        }

        public override void OnRightMouseDownFight()
        {

        }

        public override void HandleLeftClick(Hex hex)
        {
            if (_unitManager.CurrentState == UnitManager.State.Idle)
            {
                if (hex && _grid.ApproachableHexes.Contains(hex))
                {
                    _click.Play();
                    if (_grid.ShowPath && hex == _grid.Path[_grid.Path.Count - 1]) _infantryUnitMovement.GoToTarget();
                    else if (_grid.ShowRotationFields)
                    {
                        if (!hex.Unit) hex.Renderer.sprite = hex.Sprites[2];
                        foreach (Hex neighbor in _grid.ApproachableHexes) if (neighbor != hex)
                            {
                                if (!neighbor.Unit) neighbor.TurnOffAllRenderers();
                                else neighbor.Unit.MarkerRenderer.sprite = null;
                            }
                        _unitManager.CurrentHex.TurnOffArrowsRenderers();
                        _animator.SetBool("Walking", true);
                        _infantryUnitMovement.Rotate(hex);
                        if (_unitManager.Mobility == 0) _unitManager.ShowTurnIcon = false;
                        else _unitManager.Mobility--;
                    }
                    else _infantryUnitMovement.FindPath(hex);
                }
            }
            else if (_unitManager.CurrentState == UnitManager.State.Moving || _unitManager.CurrentState == UnitManager.State.Rotating) _unitManager.UnitMovement.RewindMovement();
        }
        #endregion
    }
}
