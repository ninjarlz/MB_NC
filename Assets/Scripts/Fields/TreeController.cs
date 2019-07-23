
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class TreeController : MonoBehaviour
    {

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
}
