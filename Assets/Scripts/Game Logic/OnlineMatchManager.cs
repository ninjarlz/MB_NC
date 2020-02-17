using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class OnlineMatchManager : MatchManager, IPunObservable
    {
        private GameObject _localPlayer;
        private PhotonView _photonView;

        protected void Awake()
        {
            if (Instance != null) Debug.LogError("More than one MatchManager in scene!");
            else
            {
                Instance = this;
                _photonView = GetComponent<PhotonView>();
                Grid = GetComponent<HexGrid>();
                _localPlayer = PhotonNetwork.Instantiate(GameManager.Instance.PlayerPrefab.name, Vector3.zero,
                    Quaternion.identity, 0);
                Camera = _localPlayer.GetComponentInChildren<HexMapCamera>(); 
                if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    Camera.transform.position = new Vector3(100f, 0f, 238.7645f);
                    Camera.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
                }
                CurrentPhaseText = GameObject.Find("Current Phase Text").GetComponent<TextMeshProUGUI>();
                CurrentTurnText = GameObject.Find("Current Turn Text").GetComponent<TextMeshProUGUI>();
                CurrentPhaseText.text = Phases[0];
                CurrentTurnText.text = "Turn:  1/30";
            }
        }

        void Start()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                foreach (UnitManager unitManager in Units)
                {
                    unitManager.CanvasInfo.transform.rotation = Quaternion.Euler(90f, 0f, -180f);
                }
                _photonView.RPC("SetOwnership", RpcTarget.MasterClient);
            }
        }

        [PunRPC]
        public void SetOwnership()
        {
            Player firstPlayer = PhotonNetwork.CurrentRoom.Players[1];
            foreach (UnitManager unitManager in UnitsFirstSide)
            {
                unitManager.GetComponent<PhotonView>().TransferOwnership(firstPlayer);
            }
            
            Player secondPlayer = PhotonNetwork.CurrentRoom.Players[2];
            foreach (UnitManager unitManager in UnitsSecondSide)
            { 
                unitManager.GetComponent<PhotonView>().TransferOwnership(secondPlayer);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}
