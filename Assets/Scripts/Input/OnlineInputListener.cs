using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.MKG.MB_NC
{
    public class OnlineInputListener : InputListener
    {
        private PhotonView _photonView;
        public PhotonView PhotonView { get => _photonView; }

        protected override void Awake()
        {
            base.Awake();
            _photonView = GetComponent<PhotonView>();
            if (!_photonView.IsMine)
            {
                GetComponentInChildren<Camera>().enabled = false;
                GetComponentInChildren<AudioListener>().enabled = false;
            }
            enabled = false;
        }

        public void Activate()
        {
            GetComponent<HexMapCamera>().enabled = true;
            enabled = true;
        }

    }
    
    
}
