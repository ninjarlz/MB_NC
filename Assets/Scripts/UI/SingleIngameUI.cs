using UnityEngine;


namespace com.MKG.MB_NC
{
    public class SingleIngameUI : IngameUI
    {
        [SerializeField]
        private HexMapCamera _hexCameraScript;
        
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
    }
}

