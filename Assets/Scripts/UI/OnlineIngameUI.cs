
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class OnlineIngameUI : IngameUI
    {
        [SerializeField]
        private GameObject UICamera;
        
        public override void OnOptionsButton()
        {
            base.OnOptionsButton();
            UICamera.SetActive(true);
        }

        public override void OnBackButton()
        {
            base.OnBackButton();
            UICamera.SetActive(false);
        }
        
        public override void OnQuitGameButton()
        {
            base.OnQuitGameButton();
            GameManager.Instance.LeaveRoom();
        }
    }    
}

