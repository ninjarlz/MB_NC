using UnityEngine;
using UnityEngine.Networking;

namespace com.MKG.MB_NC
{
    public class UnitSetup : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            Debug.Log(transform.GetChild(2).name);
            //GetComponent<NetworkTransformChild>().target = transform.GetChild(2);
        }


    }
}
