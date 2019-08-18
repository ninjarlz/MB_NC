using UnityEngine.EventSystems;
using UnityEngine;

/*public class CavalryUnit : Unit {

    [SerializeField]
    protected GameObject _turnIcon;

    public override int Unlocked { get { return 0; }  }

    public override void OnLeftMouseDownMovement()
    {
        if ((Side && Grid.CurrentPhase == 2) || (!Side && Grid.CurrentPhase == 6))
        {
            if (!EventSystem.current.IsPointerOverGameObject() && CurrentState == State.Idle)
            {
                if (IsChecked)
                {
                    IsChecked = false;
                    Grid.CurrentlyChecked = null;
                    CurrentHex.TurnOffArrowsRenderers();
                    Grid.HideApproachables();
                }
                else
                {
                    if (Grid.CurrentlyChecked)
                    {
                        if (Grid.CurrentlyChecked.CurrentState == State.Idle)
                        {
                            Grid.CurrentlyChecked.IsChecked = false;
                            Grid.CurrentlyChecked.CurrentHex.TurnOffArrowsRenderers();
                            Grid.HideApproachables();
                            IsChecked = true;
                            Grid.CurrentlyChecked = this;
                            SetUnitBarText();
                            FindApproachableHexes();
                        }
                    }
                    else
                    {
                        IsChecked = true;
                        Grid.CurrentlyChecked = this;
                        SetUnitBarText();
                        FindApproachableHexes();
                    }

                }
                Grid.ShowPath = false;
                _click.Play();
            }
        }
    }
}*/
