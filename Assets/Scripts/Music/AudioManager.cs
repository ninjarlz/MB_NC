using UnityEngine;

public class AudioManager : MonoBehaviour
{

    private float _currentTime = 0;
    private AudioSource _track;
    private bool _enabled = false;
    public static bool Enabled = false;

    // Use this for initialization
	void Awake ()
    {
        if (!Enabled)
        {
            DontDestroyOnLoad(gameObject);
            _track = transform.GetChild(0).GetComponent<AudioSource>();
            Enabled = true;
            _enabled = true;
            Application.targetFrameRate = 50;
        }
        else Destroy(gameObject);
    }
	
	// Update is called once per frame
	void Update ()
    {
        _currentTime = _track.time;
	}

    void OnLevelWasLoaded()
    {
        if (_enabled) _track.time = _currentTime;
    }
}
