using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {

    private OnlineInputListener _listener;
    [SerializeField] private Behaviour[] _toDisable;
    private Camera _sceneCamera;
    [SerializeField] private HexGrid _hexGrid;



	// Use this for initialization
	void Start ()
    {
        _listener = GetComponentInChildren<OnlineInputListener>();

        if (!isLocalPlayer)
        {
            DisableComponents();
        }
        else
        {
            _hexGrid = GameObject.FindGameObjectWithTag("Map").GetComponent<HexGrid>();
            HexMapCamera camera = transform.GetComponentInChildren<HexMapCamera>();
            _hexGrid.Camera = camera;
            _sceneCamera = Camera.main;
            if (_sceneCamera != null)
                _sceneCamera.gameObject.SetActive(false);
        }

        RegisterPlayer();
	}
	
	void RegisterPlayer()
    {
        int id = (int)GetComponent<NetworkIdentity>().netId.Value;
        _listener.Id = id;
        string name = "Player " + id;
        transform.name = name;
    }

    void DisableComponents()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
