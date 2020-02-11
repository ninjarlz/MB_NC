using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace com.MKG.MB_NC
{
    public class MainMenu : UIModule
    {
        private List<string> _scenes;
        [SerializeField]
        private Sprite[] _images;
        private int _currentScene = 0;
        [SerializeField]
        private GameObject _scenarioMenu;
        [SerializeField]
        private Image _image;
        [SerializeField]
        private TextMeshProUGUI _scenarioText;
        [SerializeField]
        private Button _playButton;
        [SerializeField]
        private Sprite[] _playButtonImages;
        [SerializeField]
        private GameObject _lockImage;
        private GameManager _gameManager;

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
            else _gameManager.JoinOnlineDemo();
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


        public override void OnQuitGameButton()
        {
            base.OnQuitGameButton();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            _gameManager.SignOut();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}