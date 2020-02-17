using Photon.Pun;
using UnityEngine;

namespace com.MKG.MB_NC
{
   public class OnlineUnitSerializer : MonoBehaviourPunCallbacks, IPunObservable
   {
      private UnitManager _unitManager;
      private HexGrid _grid;

      private void Start()
      {
         _unitManager = GetComponent<UnitManager>();
         _grid = MatchManager.Grid;
      }

      public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
         if (stream.IsWriting)
         {
            stream.SendNext(_unitManager.CurrentHex.Coordinates.X);
            stream.SendNext(_unitManager.CurrentHex.Coordinates.Z);
            stream.SendNext(_unitManager.Mobility);
            stream.SendNext(_unitManager.Power);
         }
         else
         {
            int x = (int) stream.ReceiveNext();
            int z = (int) stream.ReceiveNext();
            if (x != _unitManager.CurrentHex.Coordinates.X && z != _unitManager.CurrentHex.Coordinates.Z)
            {
               _unitManager.SetHexByIntCoordinates(x, z);
            }
            _unitManager.Mobility = (float) stream.ReceiveNext();
            _unitManager.Power = (int) stream.ReceiveNext();
         }
      }
   }
}
