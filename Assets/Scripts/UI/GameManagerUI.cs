using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace com.MKG.MB_NC
{
    public class GameManagerUI : MonoBehaviour
    {
        private const string DISCONNECTED_MSG = "You are disconnected from network services";
        private const string CONNECTING_MSG = "Connecting to network services...";
        private const string NOT_LOGGED_MSG = "User is not logged in";
        private const string UID_PREFIX = "UID: ";
        private const string LEVEL_PREFIX = "Level: ";
        private const string XP_PREFIX = "XP: ";
        private const string WINS_PREFIX = "Wins: ";
        private const string LOSES_PREFIX = "Loses: ";
        private const string RATIO_PREFIX = "W/L: ";

        private GameManager _gameManager;
        private SystemUtils _systemUtils;
        private UserService _userService;
        private Auth _auth;

        private Dictionary<string, Tuple<User, int>> _displayedFriends;
        
        [SerializeField] private Button _connectButton;
        [SerializeField] private TextMeshProUGUI _disconnectedMsg;
        [SerializeField] private TextMeshProUGUI _userName;
        [SerializeField] private Image _userImage;
        [SerializeField] private GameObject _profileDetails;
        [SerializeField] private Button _profileDetailsButton;
        [SerializeField] private Button _friendsButton;
        [SerializeField] private Button _chatButton;
        [SerializeField] private Button _inviteFriendButton;
        private List<Button> _acceptInvitationButtons;
        [SerializeField] private TextMeshProUGUI _uid;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private TextMeshProUGUI _xp;
        [SerializeField] private TextMeshProUGUI _wins;
        [SerializeField] private TextMeshProUGUI _loses;
        [SerializeField] private TextMeshProUGUI _ratio;
        [SerializeField] private Color _clickedColor;
        [SerializeField] private Transform _friendsListContext;
        [SerializeField] private GameObject _friendObjectPrefab;
        [SerializeField] private GameObject _friendsView;
        private AudioSource _source;
    
        public bool IsDetailsOverlayActive { get; set;  }
        public bool IsFriendsOverlayActive { get; set;  }

    
        private void Start()
        {
            _gameManager = GameManager.Instance;
            _auth = Auth.Instance;
            _userService = UserService.Instance;
            _systemUtils = SystemUtils.Instance;
            _source = GameObject.Find("Click Source").GetComponent<AudioSource>();
        }
        
        public void OnConnectButton()
        {
            _connectButton.interactable = false;
            _gameManager.Connect();
        }
        
        public void OnDetailsButton()
        {
            if (IsDetailsOverlayActive)
            {
                _profileDetailsButton.GetComponent<Image>().color = Color.white;
                IsDetailsOverlayActive = false;
            }
            else
            {
                IsDetailsOverlayActive = true;
                _profileDetailsButton.GetComponent<Image>().color = _clickedColor;
            }
            _profileDetails.SetActive(IsDetailsOverlayActive);
            _source.Play();
        }
        
        public void OnFriendsButton()
        {
            if (IsFriendsOverlayActive)
            {
                _friendsButton.GetComponent<Image>().color = Color.white;
                IsFriendsOverlayActive = false;
            }
            else
            {
                IsFriendsOverlayActive = true;
                _friendsButton.GetComponent<Image>().color = _clickedColor;
            }
            _friendsView.SetActive(IsFriendsOverlayActive);
            _source.Play();
        }
        
        public void CopyUIDToClipboard()
        {
            _systemUtils.CopyToSystemClipboard(_userService.CurrentUser.UID);
            _source.Play();
        }

        public void OnConnecting()
        {
            _disconnectedMsg.text = CONNECTING_MSG; 
        }

        public void UpdateUserGUI()
        {
            _userName.text = _userService.CurrentUser.DisplayedName;
            _uid.text = UID_PREFIX + _userService.CurrentUser.UID;
            _level.text = LEVEL_PREFIX + _userService.CurrentUser.Level.ToString();
            _xp.text = XP_PREFIX + _userService.CurrentUser.XP.ToString();
            _wins.text = WINS_PREFIX + _userService.CurrentUser.Wins.ToString();
            _loses.text = LOSES_PREFIX + _userService.CurrentUser.Defeats.ToString();
            double ratio = _userService.CurrentUser.WinsDefeatsRatio();
            _ratio.text =  RATIO_PREFIX + (double.IsPositiveInfinity(ratio) ? "-" : ratio.ToString());
        }

        public void ShowConnected()
        {
            _connectButton.gameObject.SetActive(false);
            _disconnectedMsg.text = "";
            _userImage.gameObject.SetActive(true); 
            _profileDetailsButton.GetComponent<Image>().color = Color.white;
            _friendsButton.GetComponent<Image>().color = Color.white;
            IsDetailsOverlayActive = false;
            IsFriendsOverlayActive = false;
            _profileDetailsButton.gameObject.SetActive(true);
            _friendsButton.gameObject.SetActive(true);
        }

        public void OnDisconnected()
        {
            _connectButton.gameObject.SetActive(true);
            _connectButton.interactable = true;
            _profileDetailsButton.gameObject.SetActive(false);
            _friendsButton.gameObject.SetActive(false);
            _disconnectedMsg.text = DISCONNECTED_MSG;
        }
        
        public IEnumerator SetProfileImage()
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(_auth.CurrentUser.PhotoUrl);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
                GameManager.Disconnect();
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
                Sprite image = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
                _userImage.sprite = image;
                //_gameManager.OnUserInitialized();
            }
        }

        public void OnSignOut()
        {
            _userName.text = NOT_LOGGED_MSG;
            _profileDetails.SetActive(false);
            _friendsView.SetActive(false);
            _userImage.gameObject.SetActive(false);
        }

        public void OnFriendDataChange(string uid)
        {
            Tuple<User, int> userTuple= _userService.CurrentFriends[uid];
            Transform friendElement = _friendsListContext.Find(uid);
            if (friendElement == null)
            {
                friendElement = Instantiate(_friendObjectPrefab,
                    _friendsListContext.position -
                    new Vector3(-130f, 45f + 35f * _friendsListContext.childCount, 0f),
                    Quaternion.identity, _friendsListContext).transform;
                friendElement.name = uid;
            }
            TextMeshProUGUI textMeshProUgui =
                friendElement.GetChild(0).GetComponent<TextMeshProUGUI>();
            textMeshProUgui.text = userTuple.Item1.DisplayedName + " " +
                                   ChatListener.StatusToString(userTuple.Item2);
        }
    }
}

