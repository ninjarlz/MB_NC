using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

namespace com.MKG.MB_NC
{
    public class MainMenu : UIModule
    {
        private const string JOIN_MATCH_ERROR = "There is no match with such name";
        private const string EMPTY_INPUT_ERROR = "You have to input the name of room";
        private List<string> _scenes;
        [SerializeField]
        private Sprite[] _images;
        private int _currentScene = 0;
        private int _currentOnlineScene = 0;
        private int _currentHostOnlineScene = 0;
        [SerializeField]
        private GameObject _scenarioMenu;
        [SerializeField]
        private GameObject _scenarioOnlineMenu;
        [SerializeField]
        private Image _image;
        [SerializeField]
        private Image _onlineImage;
        [SerializeField]
        private Image _onlineHostImage;
        [SerializeField]
        private TextMeshProUGUI _scenarioText;
        [SerializeField]
        private TextMeshProUGUI _onlineScenarioText;
        [SerializeField]
        private TextMeshProUGUI _onlineHostScenarioText;
        [SerializeField]
        private Button _playButton;
        [SerializeField]
        private Button _onlinePlayButton;
        [SerializeField]
        private Button _onlineHostPlayButton;
        [SerializeField]
        private Sprite[] _playButtonImages;
        [SerializeField]
        private GameObject _lockImage;
        [SerializeField]
        private GameObject _onlineLockImage;
        [SerializeField]
        private GameObject _onlineHostLockImage;
        private GameManager _gameManager;
        [SerializeField]
        private TMP_InputField _joinInput;
        [SerializeField] 
        private TextMeshProUGUI _joinInfo;
        [SerializeField]
        private TMP_InputField _hostInput;
        [SerializeField] 
        private TextMeshProUGUI _hostInfo;

        public override void Awake()
        {
            base.Awake();
            _scenes = new List<string>() { "Fulford", "Stamford Bridge", "Hastings", "Test Map" };

        }
        public void Start() {
            _gameManager = GameManager.Instance;
        }

        public void OnNewGameButton()
        {
            _source.Play();
        }

        public void OnPlayButton()
        {
            _source.Play();
            if (_currentScene != 2) SceneManager.LoadScene(_scenes[_currentScene]);
        }
        
        public void OnOnlinePlayButton()
        {
            _source.Play();
            if (_currentOnlineScene == 0) _gameManager.JoinMatchmakingMatch(_scenes[_currentOnlineScene] + " Online");
        }

        public void OnJoinPlayButton()
        {
            _source.Play();
            if (!string.IsNullOrEmpty(_joinInput.text))
            {
                foreach (RoomInfo room in _gameManager.PrivateRoomList)
                {
                    if (room.Name.Equals(_joinInput.text + GameManager.PRIVATE_ROOM_SUFFIX))
                    {
                        _gameManager.JoinHostedMatch(_joinInput.text);
                        _joinInfo.text = "";
                        return;
                    }
                }
                _joinInfo.text = JOIN_MATCH_ERROR;
            }
            else
            {
                _joinInfo.text = EMPTY_INPUT_ERROR;
            }
        }

        public void OnHostPlayButton()
        {
            if (!string.IsNullOrEmpty(_hostInput.text))
            {
                if (_currentOnlineScene == 0)
                {
                    _gameManager.HostMatch(_hostInput.text,
                        _scenes[_currentHostOnlineScene] + " Online");
                    _hostInfo.text = "";
                }
            }
            else
            {
                _hostInfo.text = EMPTY_INPUT_ERROR;
            }
        }

        public void OnLeftButton()
        {
            _source.Play();
            _currentScene = --_currentScene == -1 ? 3 : _currentScene;
            _scenarioText.text = _scenes[_currentScene];
            _image.sprite = _images[_currentScene];
            if (_currentScene == 2)
            {
                _playButton.image.sprite = _playButtonImages[1];
                _playButton.image.SetNativeSize();
                _lockImage.SetActive(true);
            }
            else if (_currentScene == 1)
            {
                _playButton.image.sprite = _playButtonImages[0];
                _playButton.image.SetNativeSize();
                _lockImage.SetActive(false);
            }
        }

        public void OnRightButton()
        {
            _source.Play();
            _currentScene = ++_currentScene == 4 ? 0 : _currentScene;
            _scenarioText.text = _scenes[_currentScene];
            _image.sprite = _images[_currentScene];
            if (_currentScene == 2)
            {
                _playButton.image.sprite = _playButtonImages[1];
                _playButton.image.SetNativeSize();
                _lockImage.SetActive(true);
            }
            else if (_currentScene == 3)
            {
                _playButton.image.sprite = _playButtonImages[0];
                _playButton.image.SetNativeSize();
                _lockImage.SetActive(false); 
            }
        }
        
        public void OnLeftOnlineButton()
        {
            _source.Play();
            _currentOnlineScene = --_currentOnlineScene == -1 ? 3 : _currentOnlineScene;
            _onlineScenarioText.text = _scenes[_currentOnlineScene];
            _onlineImage.sprite = _images[_currentOnlineScene];
            if (_currentOnlineScene == 2)
            {
                _onlinePlayButton.image.sprite = _playButtonImages[1];
                _onlinePlayButton.image.SetNativeSize();
                _onlineLockImage.SetActive(true);
            }
            else if (_currentOnlineScene == 1)
            {
                _onlinePlayButton.image.sprite = _playButtonImages[0];
                _onlinePlayButton.image.SetNativeSize();
                _onlineLockImage.SetActive(false);
            }
        }
        
        public void OnRightOnlineButton()
        {
            _source.Play();
            _currentOnlineScene = ++_currentOnlineScene == 4 ? 0 : _currentOnlineScene;
            _onlineScenarioText.text = _scenes[_currentOnlineScene];
            _onlineImage.sprite = _images[_currentOnlineScene];
            if (_currentOnlineScene == 2)
            {
                _onlinePlayButton.image.sprite = _playButtonImages[1];
                _onlinePlayButton.image.SetNativeSize();
                _onlineLockImage.SetActive(true);
            }
            else if (_currentOnlineScene == 3)
            {
                _onlinePlayButton.image.sprite = _playButtonImages[0];
                _onlinePlayButton.image.SetNativeSize();
                _onlineLockImage.SetActive(false); 
            }
        }
        
        public void OnLeftHostOnlineButton()
        {
            _source.Play();
            _currentHostOnlineScene = --_currentHostOnlineScene == -1 ? 3 : _currentHostOnlineScene;
            _onlineHostScenarioText.text = _scenes[_currentHostOnlineScene];
            _onlineHostImage.sprite = _images[_currentHostOnlineScene];
            if (_currentHostOnlineScene == 2)
            {
                _onlineHostPlayButton.image.sprite = _playButtonImages[1];
                _onlineHostPlayButton.image.SetNativeSize();
                _onlineHostLockImage.SetActive(true);
            }
            else if (_currentHostOnlineScene == 1)
            {
                _onlineHostPlayButton.image.sprite = _playButtonImages[0];
                _onlineHostPlayButton.image.SetNativeSize();
                _onlineHostLockImage.SetActive(false);
            }
        }
        
        public void OnRightHostOnlineButton()
        {
            _source.Play();
            _currentHostOnlineScene = ++_currentHostOnlineScene == 4 ? 0 : _currentHostOnlineScene;
            _onlineHostScenarioText.text = _scenes[_currentHostOnlineScene];
            _onlineHostImage.sprite = _images[_currentHostOnlineScene];
            if (_currentHostOnlineScene == 2)
            {
                _onlineHostPlayButton.image.sprite = _playButtonImages[1];
                _onlineHostPlayButton.image.SetNativeSize();
                _onlineHostLockImage.SetActive(true);
            }
            else if (_currentHostOnlineScene == 3)
            {
                _onlineHostPlayButton.image.sprite = _playButtonImages[0];
                _onlineHostPlayButton.image.SetNativeSize();
                _onlineHostLockImage.SetActive(false); 
            }
        }


        public override void OnQuitGameButton()
        {
            base.OnQuitGameButton();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
            Auth.Instance.SignOut();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
        }
    }
}