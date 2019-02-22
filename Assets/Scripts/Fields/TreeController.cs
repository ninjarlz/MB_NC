
using UnityEngine;

public class TreeController : MonoBehaviour {

    private MeshRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

	void OnTriggerEnter(Collider other)
    {
        _renderer.enabled = false;
    }

    void OnTriggerExit(Collider other)
    {
        _renderer.enabled = true;
    }
}
