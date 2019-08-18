namespace com.MKG.MB_NC
{
    public class InfantryUnitMovement : UnitMovement
    {

        public void FindRotationFields()
        {
            if (_unitManager.ShowTurnIcon)
            {
                _grid.ShowRotationFields = true;
                _grid.ShowPath = false;
                _grid.HideApproachables();
                _grid.ResetAproachablesEnemyZone();
                _grid.ApproachableHexes.Clear();
                _grid.Path.Clear();
                for (int i = 0; i < 6; i++)
                {
                    Hex neighbor = _unitManager.CurrentHex.GetNeighbor((HexDirection)i);
                    if (neighbor && !neighbor.IsUnderWater && _unitManager.CurrentHex.GetDirection(neighbor) != _unitManager.CurrentRotation)
                    {
                        _grid.ApproachableHexes.Add(neighbor);
                        if (!neighbor.Unit)
                        {
                            neighbor.Renderer.sprite = neighbor.Sprites[5];
                            neighbor.CostText.gameObject.SetActive(true);
                            neighbor.CostText.color = HexDims.CostColors[0];
                            if (_unitManager.Mobility != 0) neighbor.CostText.text = "1";
                            else neighbor.CostText.text = "0";
                        }
                        else neighbor.Unit.MarkerRenderer.sprite = neighbor.Unit.Markers[1];
                        _unitManager.CurrentHex.ArrowRenderers[i].sprite = _unitManager.CurrentHex.Sprites[4];
                    }
                }
            }

        }
    }
}
