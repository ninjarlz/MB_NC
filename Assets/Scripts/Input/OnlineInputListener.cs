using Photon.Pun;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class OnlineInputListener : InputListener
    {
        private PhotonView _photonView;
        public PhotonView PhotonView { get => _photonView; }

        protected void Awake()
        {
            base.Awake();
            _photonView = GetComponent<PhotonView>();
            if (!_photonView.IsMine)
            {
                GetComponent<HexMapCamera>().enabled = false;
                GetComponentInChildren<Camera>().enabled = false;
                GetComponentInChildren<AudioListener>().enabled = false;
                enabled = false;
            }
        }

    }
}
