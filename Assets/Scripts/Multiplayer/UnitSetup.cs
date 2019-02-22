using UnityEngine;
using UnityEngine.Networking;

public class UnitSetup : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log(transform.GetChild(2).name);
        GetComponent<NetworkTransformChild>().target = transform.GetChild(2);
	}
	
	
}
