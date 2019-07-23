using UnityEngine;

namespace com.MKG.MB_NC
{
    public class InfantryUnit : Unit
    {/*
    #region Variables & Properties
    [SerializeField]
   protected GameObject _turnIcon;

   private bool _showTurnIcon = true;   

   public override bool ShowTurnIcon
   {
        get { return _showTurnIcon; }
        set
        {
            _showTurnIcon = value;
            if (value) _turnIcon.SetActive(true);
            else _turnIcon.SetActive(false);
        }
   }

    public override int Unlocked
    {
        get
        {
            if (Side)
            {
                switch (Grid.CurrentPhase)
                {
                    case 0:
                        return 1;
                    case 1:
                        if (HasEnemies()) return 2;
                        break;
                    case 3:
                       if (MarkerRenderer.sprite == Markers[4])
                            return 4;
                        else if (MarkerRenderer.sprite == Markers[1])
                            return 3;
                        break;
                }
            }
            else if (!Side)
            {
                switch (Grid.CurrentPhase)
                {
                    case 1:
                        if (MarkerRenderer.sprite == Markers[4])
                            return 4;
                        else if (MarkerRenderer.sprite == Markers[1])
                            return 3;
                        break;
                    case 2:
                        return 1;
                    case 3:
                        if (HasEnemies()) return 2;
                        break;
                }
            }
            return 0;
        }
    }
    #endregion

    #region UnityFuncs - Start, Update...
    protected override void Start()
    {
        base.Start();
        Grid.InfantryUnits.Add(this);
    }
    #endregion

    #region Processing Input
    public override void OnLeftMouseDownMovement()
    {
        if (IsChecked)
        {
            if (CurrentState == State.Idle)
            {
                IsChecked = false;
                Grid.CurrentlyChecked = null;
                CurrentHex.TurnOffArrowsRenderers();
                Grid.HideApproachables();
                if (Grid.ShowRotationFields)
                {
                    Grid.ShowRotationFields = false;
                    foreach (Hex neighbor in CurrentHex.GetNeighbors())
                        if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                }
                _click.Play();
            }
            else RewindMovement();
        }
        else
        {
            if (Grid.CurrentlyChecked)
            {
                if (Grid.CurrentlyChecked.CurrentState == State.Idle)
                {
                    if (Grid.ShowRotationFields)
                    {
                        if (Grid.ApproachableHexes.Contains(CurrentHex))
                        {
                            foreach (Hex neighbor in Grid.ApproachableHexes) if (neighbor != CurrentHex)
                            {
                                if (!neighbor.Unit) neighbor.TurnOffAllRenderers();
                                else neighbor.Unit.MarkerRenderer.sprite = null;
                            }
                            Grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            Grid.CurrentlyChecked.Animator.SetBool("Walking", true);
                            Grid.CurrentlyChecked.Rotate(CurrentHex);
                            if (Grid.CurrentlyChecked.Mobility == 0) Grid.CurrentlyChecked.ShowTurnIcon = false;
                            else Grid.CurrentlyChecked.Mobility--;
                            _click.Play();
                        }
                        else
                        {
                            Grid.ShowRotationFields = false;
                            foreach (Hex neighbor in Grid.CurrentlyChecked.CurrentHex.GetNeighbors())
                                if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                            Grid.CurrentlyChecked.IsChecked = false;
                            Grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            Grid.HideApproachables();
                            IsChecked = true;
                            Grid.CurrentlyChecked = this;
                            SetUnitBarText();
                            FindApproachableHexes();
                            _click.Play();
                        }
                    }
                    else
                    {
                        Grid.CurrentlyChecked.IsChecked = false;
                        Grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                        Grid.HideApproachables();
                        IsChecked = true;
                        Grid.CurrentlyChecked = this;
                        SetUnitBarText();
                        FindApproachableHexes();
                        _click.Play();
                    }
                }
                else Grid.CurrentlyChecked.RewindMovement();
            }
            else
            {
                IsChecked = true;
                Grid.CurrentlyChecked = this;
                SetUnitBarText();
                FindApproachableHexes();
                _click.Play();
            }

        }
        Grid.ShowPath = false;
    }

    public override void OnLeftMouseDownFight()
    {
        if (IsChecked)
        {
            if (CurrentState == State.Idle)
            {
                IsChecked = false;
                Grid.CurrentlyChecked = null;
                foreach (Unit enemy in AvailableEnemies)
                {
                    if (enemy.AttackingEnemies.Count > 0) enemy.MarkerRenderer.sprite = null;
                    else enemy.MarkerRenderer.sprite = enemy.Markers[5];
                }
                _click.Play();
            }
            else RewindMovement();
        }
        else
        {
            if (Grid.CurrentlyChecked)
            {
                if (Grid.CurrentlyChecked.CurrentState == State.Idle)
                {
                    Grid.CurrentlyChecked.IsChecked = false;
                    foreach (Unit enemy in Grid.CurrentlyChecked.AvailableEnemies)
                    {
                        if (enemy.AttackingEnemies.Count > 0) enemy.MarkerRenderer.sprite = null;
                        else enemy.MarkerRenderer.sprite = enemy.Markers[5];
                    }
                    IsChecked = true;
                    Grid.CurrentlyChecked = this;
                    SetUnitBarText();
                    if (AttackedEnemies.Count == 1 && AttackedEnemies[0].AttackingEnemies.Count > 1) // JESLI PARTYCYPUJESZ W ATAKU KILKU NA JEDNEGO
                        AttackedEnemies[0].MarkerRenderer.sprite = Markers[4];
                    else
                    {
                        foreach (Unit enemy in AvailableEnemies)
                        {
                            if (AttackedEnemies.Contains(enemy)) enemy.MarkerRenderer.sprite = Markers[4];
                            else if (enemy.AttackingEnemies.Count == 0 ||
                                 (enemy.AttackingEnemies.Count >= 1 && enemy.AttackingEnemies[0].AttackedEnemies.Count == 1 &&
                                 AttackedEnemies.Count == 0))
                                    enemy.MarkerRenderer.sprite = Markers[1];
                        }
                    }
                    _click.Play();
                }
                else RewindMovement();
            }
            else
            {
                IsChecked = true;
                Grid.CurrentlyChecked = this;
                SetUnitBarText();
                if (AttackedEnemies.Count == 1 && AttackedEnemies[0].AttackingEnemies.Count > 1)
                    AttackedEnemies[0].MarkerRenderer.sprite = Markers[4];
                else
                {
                    foreach (Unit enemy in AvailableEnemies)
                    {
                        if (AttackedEnemies.Contains(enemy)) enemy.MarkerRenderer.sprite = Markers[4];
                        else if (enemy.AttackingEnemies.Count == 0 ||
                                (enemy.AttackingEnemies.Count >= 1 && enemy.AttackingEnemies[0].AttackedEnemies.Count == 1 &&
                                AttackedEnemies.Count == 0))
                                    enemy.MarkerRenderer.sprite = Markers[1];  
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
        AttackingEnemies.Remove(Grid.CurrentlyChecked);
        MarkerRenderer.sprite = Markers[1];
        Grid.CurrentlyChecked.AttackedEnemies.Remove(this);
        if (AttackingEnemies.Count > 0)
        {
            foreach (Unit enemy in Grid.CurrentlyChecked.AvailableEnemies)
                enemy.MarkerRenderer.sprite = Markers[1];
        }
        else
        {
            if (Grid.CurrentlyChecked.AttackedEnemies.Count > 0)
            {
                MarkerRenderer.sprite = Markers[1];
                foreach (Unit enemy in Grid.CurrentlyChecked.AvailableEnemies)
                    if (enemy.AttackingEnemies.Count > 0 &&
                        !enemy.AttackingEnemies.Contains(Grid.CurrentlyChecked))
                        enemy.MarkerRenderer.sprite = null;
            }
            else foreach (Unit enemy in Grid.CurrentlyChecked.AvailableEnemies)
                    enemy.MarkerRenderer.sprite = Markers[1];
        }
        _click.Play();
    }

    public override void OnLeftMouseDownEnemyFightCheck()
    {
        AttackingEnemies.Add(Grid.CurrentlyChecked);
        Grid.CurrentlyChecked.AttackedEnemies.Add(this);
        if (AttackingEnemies.Count > 1)
            foreach (Unit enemy in Grid.CurrentlyChecked.AvailableEnemies)
            {
                if (enemy.AttackingEnemies.Count > 0) enemy.MarkerRenderer.sprite = null;
                else enemy.MarkerRenderer.sprite = enemy.Markers[5];
            }
        else foreach (Unit enemy in Grid.CurrentlyChecked.AvailableEnemies)
                if (enemy.AttackingEnemies.Count > 0 &&
                    !enemy.AttackingEnemies.Contains(Grid.CurrentlyChecked))
                    enemy.MarkerRenderer.sprite = null;
        MarkerRenderer.sprite = Markers[4];
        _click.Play();
    }

    public override void OnRightMouseDownMovement()
    {
        if (IsChecked)
        {
            if (!Grid.ShowRotationFields) FindRotationFields();
            else
            {
                Grid.ShowRotationFields = false;
                foreach (Hex neighbor in CurrentHex.GetNeighbors())
                    if (neighbor && neighbor.Unit) neighbor.Unit.MarkerRenderer.sprite = null;
                Grid.HideApproachables();
                Grid.ApproachableHexes.Clear();
                _currentHex.TurnOffArrowsRenderers();
                FindApproachableHexes();
            }
            _click.Play();
        }
    }

    public override void OnRightMouseDownFight()
    {
        
    }

    public override void HandleLeftClick(Hex hex)
    {
        if (_currentState == State.Idle)
        {
            if (hex && Grid.ApproachableHexes.Contains(hex))
            {
                _click.Play();
                if (Grid.ShowPath && hex == Grid.Path[Grid.Path.Count - 1]) GoToTarget();
                else if (Grid.ShowRotationFields)
                {
                    if (!hex.Unit) hex.Renderer.sprite = hex.Sprites[2];
                    foreach (Hex neighbor in Grid.ApproachableHexes) if (neighbor != hex)
                        {
                            if (!neighbor.Unit) neighbor.TurnOffAllRenderers();
                            else neighbor.Unit.MarkerRenderer.sprite = null;
                        }
                    CurrentHex.TurnOffArrowsRenderers();
                    Animator.SetBool("Walking", true);
                    Rotate(hex);
                    if (Mobility == 0) ShowTurnIcon = false;
                    else Mobility--;
                }
                else FindPath(hex);
            }
        }
        else if (_currentState == State.Moving || _currentState == State.Rotating) RewindMovement();
    }
    #endregion

    #region Pathfinding & Movement
    protected void FindRotationFields()
    {
        if (ShowTurnIcon)
        {
            Grid.ShowRotationFields = true;
            Grid.ShowPath = false;
            Grid.HideApproachables();
            Grid.ResetAproachablesEnemyZone();
            Grid.ApproachableHexes.Clear();
            Grid.Path.Clear();
            for (int i = 0; i < 6; i++)
            {
                Hex neighbor = _currentHex.GetNeighbor((HexDirection)i);
                if (neighbor && !neighbor.IsUnderWater && CurrentHex.GetDirection(neighbor) != CurrentRotation)
                {
                    Grid.ApproachableHexes.Add(neighbor);
                    if (!neighbor.Unit)
                    {
                        neighbor.Renderer.sprite = neighbor.Sprites[5];
                        neighbor.CostText.gameObject.SetActive(true);
                        neighbor.CostText.color = HexDims.CostColors[0];
                        if (Mobility != 0) neighbor.CostText.text = "1";
                        else neighbor.CostText.text = "0";
                    }
                    else neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[1];
                    _currentHex.ArrowRenderers[i].sprite = _currentHex.Sprites[4];
                }
            }
        }
        
    }
    #endregion*/
    }
}
