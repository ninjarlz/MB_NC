using UnityEngine;

namespace com.MKG.MB_NC
{
    public class InfantryUnitManager : UnitManager
    {
        #region Variables & Properties

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
                if (Side == MatchManager.Side.Northman)
                {
                    switch (MatchManager.CurrentPhase)
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
                else if (Side == MatchManager.Side.Anglosaxons)
                {
                    switch (MatchManager.CurrentPhase)
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
            Transform infoAndIcons = transform.GetChild(0).Find("InfoAndIcons");
            _turnIcon = infoAndIcons.GetChild(1).gameObject;
            for (int i = 2; i < infoAndIcons.childCount; i++) _shieldsIcons.Add(infoAndIcons.GetChild(i).gameObject);
            MatchManager.InfantryUnits.Add(this);
        }
        #endregion
    }
}
