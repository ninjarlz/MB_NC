using UnityEngine;

public class InfantryUnitManager : UnitManager
{
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
            if (Side == GameManager.Side.Northman)
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
            else if (Side == GameManager.Side.Anglosaxons)
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
}
