using UnityEngine;

namespace com.MKG.MB_NC
{
    public abstract class UnitMovement : MonoBehaviour
    {

        protected Animator _animator;
        protected UnitManager _unitManager;
        protected bool _achievedFirstHex;
        protected HexGrid _grid;
        [SerializeField]
        protected bool _isBlockedByEnemyControlZone = false;

        public void Setup(UnitManager unitManager)
        {
            _unitManager = unitManager;
            _grid = GameManager.Grid;
            _animator = GetComponent<Animator>();
        }


        public void GoToTarget()
        {
            _animator.SetBool("Walking", true);
            _unitManager.CurrentHex.TurnOffArrowsRenderers();
            _grid.HideApproachablesWithoutPath();
            OnArrivalToHex(_unitManager.CurrentHex);
        }

        public void OnArrivalToHex(Hex arrivalHex)
        {
            if (_achievedFirstHex)
            {
                _unitManager.CurrentHex = arrivalHex;
                PathRemoveFirst();
            }
            else _achievedFirstHex = true;

            if (_grid.Path.Count == 0)
            {
                _animator.SetBool("Walking", false);
                if (_unitManager.CurrentHex.IsInEnemyZone) _unitManager.Mobility = 0;
                _unitManager.CurrentState = UnitManager.State.Idle;
                _grid.ShowPath = false;
                _grid.ResetAproachablesCosts();
                FindApproachableHexes();
            }
            else
            {
                if (_unitManager.CurrentHex.GetNeighbor(_unitManager.CurrentRotation) == _grid.Path[0])
                    _unitManager.CurrentState = UnitManager.State.Moving;
                else Rotate(_grid.Path[0]);
            }
        }

        public void FindApproachableHexes()
        {
            _grid.ResetAproachablesEnemyZone();
            _grid.ApproachableHexes.Clear();
            if (_isBlockedByEnemyControlZone) return;
            _grid.Frontier.Clear();
            _grid.Frontier.Enqueue(_unitManager.CurrentHex);
            _grid.CameFrom.Clear();
            _grid.CameFrom.Add(_unitManager.CurrentHex, null);
            _grid.CostSoFar.Clear();
            _grid.CostSoFar.Add(_unitManager.CurrentHex, 0f);
            _grid.FromDirection.Clear();
            _grid.FromDirection.Add(_unitManager.CurrentHex, (int)_unitManager.CurrentRotation);

            if (_unitManager.Mobility == 0) return;

            bool isAlreadyInEnemyControlZone = false;
            foreach (Hex hex in _unitManager.CurrentHex.GetNeighbors())
            {
                if (hex && hex.Unit && hex.Unit.Side != _unitManager.Side)
                {
                    if (hex.GetDirection(_unitManager.CurrentHex) == hex.Unit.CurrentRotation.Previous() ||
                             hex.GetDirection(_unitManager.CurrentHex) == hex.Unit.CurrentRotation ||
                             hex.GetDirection(_unitManager.CurrentHex) == hex.Unit.CurrentRotation.Next())
                        isAlreadyInEnemyControlZone = true;
                }
            }

            if (isAlreadyInEnemyControlZone)
            {
                for (HexDirection direction = HexDirection.N; direction <= HexDirection.NW; direction++)
                {
                    Hex hex = _unitManager.CurrentHex.GetNeighbor(direction);
                    if (hex && !hex.IsUnderWater && !hex.Unit)
                    {
                        bool hexAvailable = true;
                        foreach (Hex neighbor in hex.GetNeighbors())
                        {
                            if (neighbor && neighbor.Unit && neighbor.Unit.Side != _unitManager.Side)
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
                            _grid.CameFrom[hex] = _unitManager.CurrentHex;
                            _grid.ApproachableHexes.Add(hex);
                            _grid.CostSoFar[hex] = _unitManager.Mobility;
                            hex.CostText.text = _unitManager.Mobility.ToString();
                            hex.SetColorOfCost(_unitManager.Mobility);
                            hex.CostWithTurnings = _unitManager.Mobility;
                            _grid.FromDirection[hex] = (int)direction;
                        }
                    }

                }
                foreach (Hex hex in _grid.ApproachableHexes)
                {
                    hex.Renderer.sprite = hex.Sprites[0];
                    if (_grid.FromDirection[hex] != (int)_unitManager.CurrentRotation) _unitManager.CurrentHex.ArrowRenderers[_grid.FromDirection[hex]].sprite = hex.Sprites[4];
                    hex.CostText.gameObject.SetActive(true);
                }
            }
            else
            {
                while (_grid.Frontier.Count != 0)
                {
                    Hex current = _grid.Frontier.Dequeue();
                    for (HexDirection direction = HexDirection.N; direction <= HexDirection.NW; direction++)
                    {
                        Hex neighbor = current.GetNeighbor(direction);
                        if (neighbor != null && !neighbor.IsUnderWater && !neighbor.Unit)
                        {
                            bool HasRoadOrBridgeConnection = (current.HasRoadOrBridge() && neighbor.HasRoadOrBridge());
                            bool heightDiff = false;
                            float newCost = _grid.CostSoFar[current];
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
                            if ((int)direction != _grid.FromDirection[current])
                            {
                                newCost++;
                                isTurning = true;
                            }

                            if ((!_grid.CostSoFar.ContainsKey(neighbor) && newCost <= _unitManager.Mobility) || (_grid.CostSoFar.ContainsKey(neighbor) && newCost < _grid.CostSoFar[neighbor]))
                            {
                                if (isTurning)
                                {
                                    _grid.FromDirection[neighbor] = (int)direction;
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
                                    _grid.FromDirection[neighbor] = _grid.FromDirection[current];
                                }

                                if (!neighbor.IsInEnemyZone) foreach (Hex hex in neighbor.GetNeighbors())
                                    {
                                        if (hex && hex.Unit && hex.Unit.Side != _unitManager.Side)
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

                                _grid.CostSoFar[neighbor] = newCost;
                                neighbor.CostText.text = newCost.ToString();
                                neighbor.SetColorOfCost(newCost);
                                _grid.CameFrom[neighbor] = current;
                                _grid.ApproachableHexes.Add(neighbor);
                                if (!neighbor.IsInEnemyZone) _grid.Frontier.Enqueue(neighbor);
                            }
                        }
                    }
                }
                foreach (Hex hex in _grid.ApproachableHexes)
                {
                    if (hex.IsInEnemyZone) hex.Renderer.sprite = hex.Sprites[6];
                    else hex.Renderer.sprite = hex.Sprites[0];
                    Hex fromHex = _grid.CameFrom[hex];
                    if (_grid.FromDirection[hex] != _grid.FromDirection[fromHex]) fromHex.ArrowRenderers[_grid.FromDirection[hex]].sprite = hex.Sprites[4];
                    hex.CostText.gameObject.SetActive(true);
                }
            }
        }

        public void FindPath(Hex goal)
        {
            if (_grid.Path.Count != 0) // when the path is not empty 
            {
                foreach (Hex hex in _grid.Path) if (_grid.ApproachableHexes.Contains(hex))
                    {
                        if (!hex.IsInEnemyZone) hex.Renderer.sprite = hex.Sprites[0];
                        else hex.Renderer.sprite = hex.Sprites[6];
                        if (_grid.ShowPath) // if the path is displayed
                        {
                            _grid.Path[0].SetColorOfCost(_grid.CostSoFar[_grid.Path[0]]);  // resetting a color of the first field in path
                            if (_grid.Path.Count > 1) _grid.Path[1].SetColorOfCost(_grid.CostSoFar[_grid.Path[1]]); // resetting a color of the second field in path
                        }
                    }
                _grid.Path.Clear(); // clearing the path
            }

            Hex current = goal;
            while (current != _unitManager.CurrentHex)
            {

                if (_grid.CostSoFar[current] == 1) current.CostText.color = HexDims.AdditionalCostColors[0]; // we change a color of displayed cost to light green
                else if (_grid.CostSoFar[current] == 2) current.CostText.color = HexDims.AdditionalCostColors[1]; // we change a color of displayed cost to green
                current.Renderer.sprite = current.Sprites[3];
                _grid.Path.Add(current);
                current = _grid.CameFrom[current];
            }
            _grid.Path.Reverse();
            _grid.Path[_grid.Path.Count - 1].Renderer.sprite = _grid.Path[_grid.Path.Count - 1].Sprites[2];
            _grid.ShowPath = true;
            _achievedFirstHex = false;
        }



        protected void PathRemoveFirst()
        {
            _grid.Path[0].TurnOffAllRenderers();
            _unitManager.Mobility -= _grid.Path[0].CostWithTurnings;
            _grid.Path[0].ResetCostWithTurnings();
            _grid.Path.RemoveAt(0);
        }

        protected Quaternion UnitRotation(HexDirection currentDirection, HexDirection nextDirection)
        {
            return Quaternion.Euler(0, (int)_unitManager.CurrentRotation * 60 + currentDirection.SmallestDifference(nextDirection) * 60, 0);
        }

        protected Quaternion UnitRotation(HexDirection currentDirection, Hex currentHex, Hex neighbor)
        {
            HexDirection nextDirection = currentHex.GetDirection(neighbor);
            return UnitRotation(currentDirection, nextDirection);
        }

        public void Rotate(Hex neighbor)
        {
            _unitManager.CurrentState = UnitManager.State.Rotating;
            _unitManager.DestinationRotation = UnitRotation(_unitManager.CurrentRotation, _unitManager.CurrentHex, neighbor);
        }

        protected Quaternion ArrowRotation(HexDirection currentDirection, HexDirection nextDirection)
        {
            return Quaternion.Euler(90, 0, (int)_unitManager.CurrentRotation * 60 + currentDirection.SmallestDifference(nextDirection) * 60);
        }

        protected Quaternion ArrowRotation(HexDirection currentDirection, Hex currentHex, Hex neighbor)
        {
            HexDirection nextDirection = currentHex.GetDirection(neighbor);
            return ArrowRotation(currentDirection, nextDirection);
        }

        public void RewindMovement()
        {
            if (!_grid.ShowRotationFields)
            {
                _unitManager.CurrentHex = _grid.Path[_grid.Path.Count - 1];
                _unitManager.CurrentRotation = (HexDirection)_grid.FromDirection[_unitManager.CurrentHex];
                _unitManager.Model.transform.rotation = Quaternion.Euler(0f, (int)_unitManager.CurrentRotation * 60f, 0f);
                while (_grid.Path.Count > 0) PathRemoveFirst();
                if (_unitManager.CurrentHex.IsInEnemyZone) _unitManager.Mobility = 0;
                _animator.SetBool("Walking", false);
                _unitManager.CurrentState = UnitManager.State.Idle;
                _grid.ShowPath = false;
                FindApproachableHexes();
            }
            else
            {
                _unitManager.CurrentRotation = (HexDirection)(_unitManager.DestinationRotation.eulerAngles.y / 60);
                _unitManager.Model.transform.rotation = _unitManager.DestinationRotation;
            }
        }
    }
}
