﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using UnityEngine.Networking;
using System.Collections;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Photon.Chat;
using AuthenticationValues = Photon.Realtime.AuthenticationValues;
using Random = UnityEngine.Random;

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviourPunCallbacks, IUserListener, ILobbyCallbacks, IAuthListener
    {
        private const int MATCHMAKING_STARTING_LIMIT = 0;
        private const int MAX_LVL_DIFF = 3;
        public const string PRIVATE_ROOM_SUFFIX = " - private";
        private const string DISCONNECTED_MSG = "You are disconnected from network services";
        private const string CONNECTING_MSG = "Connecting to network services...";
        private const string NOT_LOGGED_MSG = "User is not logged in";
        private const string UID_PREFIX = "UID: ";
        private const string LEVEL_PREFIX = "Level: ";
        private const string XP_PREFIX = "XP: ";
        private const string WINS_PREFIX = "Wins: ";
        private const string LOSES_PREFIX = "Loses: ";
        private const string RATIO_PREFIX = "W/L: ";

#if UNITY_EDITOR
        private const string TEST_PLAYER_NAME_EDITOR = "Test Test";
#elif !UNITY_ANDROID
        private const string TEST_PLAYER_NAME_BUILD = "Test Test 2";
#endif
        private static GameManager _instance;

        public static GameManager Instance
        {
            get { return _instance; }
        }

        public static MatchManager CurrentMatch { get; set; }
        public static string GAME_VERSION = "1";
        [SerializeField] private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        private Auth _auth;
        private UserRepository _userRepository;
        private SystemUtils _systemUtils;
        private ChatListener _chatListener;
        private bool _isUserFullyInitialized = false;
        private AudioSource _source;
        public bool IsUserFullyInitialized => _isUserFullyInitialized;
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
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Color _clickedColor;
        public bool IsDetailsOverlayActive { get; set;  }
        public bool IsFriendsOverlayActive { get; set;  }

        public GameObject PlayerPrefab
        {
            get => _playerPrefab;
        }

        private string _arenaToLoad;
        private List<RoomInfo> _roomList;
        private Dictionary<string, List<RoomInfo>> _publicRooms;
        private List<RoomInfo> _privateRoomList;

        public List<RoomInfo> PrivateRoomList
        {
            get => _privateRoomList;
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                EstablishConnection();
            }
        }

        private void Setup()
        {
            _app = FirebaseApp.DefaultInstance;
            _auth = Auth.Instance;
            _app.SetEditorDatabaseUrl("https://mb-nc-a2dd3.firebaseio.com/");
            _userRepository = UserRepository.Instance;
            _userRepository.UserListeners.Add(this);
            _auth.AuthListeners.Add(this);
            _systemUtils = SystemUtils.Instance;
            _source = GameObject.Find("Click Source").GetComponent<AudioSource>();
            _chatListener = GetComponent<ChatListener>();
        }

        public void OnConnectButton()
        {
            _connectButton.interactable = false;
            Connect();
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

        public void EstablishConnection()
        {
            Setup();
            Connect();
        }

        public void CopyUIDToClipboard()
        {
            _systemUtils.CopyToSystemClipboard(_userRepository.CurrentUser.UID);
            _source.Play();
        }

        public void HostMatch(string roomName, string arenaName)
        {
            if (PhotonNetwork.IsConnected)
            {
                _arenaToLoad = arenaName;
                CreatePrivateRoom(roomName);
            }
        }
        
        public void JoinHostedMatch(string roomName)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRoom(roomName);
            }
        }

        public void JoinMatchmakingMatch(string arenaName)
        {
            if (PhotonNetwork.IsConnected)
            {
                _arenaToLoad = arenaName;
                if (_publicRooms != null && _publicRooms.ContainsKey(arenaName))
                {
                    List<RoomInfo> matchesOnArena = _publicRooms[arenaName];
                    if (matchesOnArena.Count > MATCHMAKING_STARTING_LIMIT)
                    {
                        RoomInfo room = FindMostSuitableRoom(matchesOnArena);
                        if (room != null)
                        {
                            PhotonNetwork.JoinRoom(room.Name);
                        }
                        else
                        {
                            CreateRoom();
                        }
                    }
                    else
                    {
                        if (matchesOnArena.Count > 0)
                        {
                            PhotonNetwork.JoinRoom(matchesOnArena[0].Name);
                            
                        }
                        else
                        {
                            CreateRoom();
                        }
                    }
                }
                else
                {
                    CreateRoom();
                }
            }
        }


        private RoomInfo FindMostSuitableRoom(List<RoomInfo> matchesOnArena)
        {
            if (matchesOnArena.Count > 0)
            { 
                RoomInfo mostSuitable = matchesOnArena[0];
                Debug.Log(mostSuitable.Name);
                int suitableLvl = (int) mostSuitable.CustomProperties["level"];
                double suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                for (int i = 1; i < matchesOnArena.Count; i++)
                {
                    ExitGames.Client.Photon.Hashtable currProps = matchesOnArena[i].CustomProperties;
                    int currLvl = (int) currProps["level"];
                    double currWDRatio = (double) currProps["w/d"];
                    double currUserWDRatio = _userRepository.CurrentUser.WinsDefeatsRatio();
                    if (Math.Abs(_userRepository.CurrentUser.Level - currLvl) <
                        Math.Abs(_userRepository.CurrentUser.Level - suitableLvl))
                    {
                        mostSuitable = matchesOnArena[i];
                        suitableLvl = (int) mostSuitable.CustomProperties["level"];
                        suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                    }
                    else if (Math.Abs(_userRepository.CurrentUser.Level - currLvl) ==
                             Math.Abs(_userRepository.CurrentUser.Level - suitableLvl))
                    {
                        if (Math.Abs(currUserWDRatio - currWDRatio) <
                            Math.Abs(currWDRatio - suitableWDRatio))
                        {
                            mostSuitable = matchesOnArena[i];
                            suitableLvl = (int) mostSuitable.CustomProperties["level"];
                            suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                        }
                    }
                }
                Debug.Log("Ol je kurwa2");
                Debug.Log("Diff:" + Math.Abs(_userRepository.CurrentUser.Level - suitableLvl).ToString());
                Debug.Log("Curr user lvl: " + _userRepository.CurrentUser.Level.ToString());
                Debug.Log("most suitable lvl: " + suitableLvl.ToString());
                Debug.Log("limit: " + (MAX_LVL_DIFF + 1).ToString());
                if (Math.Abs(_userRepository.CurrentUser.Level - suitableLvl) < MAX_LVL_DIFF + 1)
                {
                    Debug.Log("Ol je kurwa");
                    return  mostSuitable;
                }
                return null;
            }
            return null;
        }


        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("eoooo");
            _roomList = roomList;
            _publicRooms = new Dictionary<string, List<RoomInfo>>();
            foreach (string arenaName in MainMenu.SCENES)
            {
                _publicRooms.Add(arenaName + " Online", new List<RoomInfo>());
            }
            _privateRoomList = new List<RoomInfo>();
            foreach (RoomInfo room in roomList)
            {
                ExitGames.Client.Photon.Hashtable currProps = room.CustomProperties;
                if (!currProps.ContainsKey("isPrivate"))
                {
                    continue;
                }
                if (!(bool) currProps["isPrivate"])
                {
                    _publicRooms[(string) currProps["arenaName"]].Add(room);       
                }
                else
                {
                    _privateRoomList.Add(room);
                }
            }
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("Main Menu");
        }

        void LoadArena(string arenaName)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            PhotonNetwork.LoadLevel(arenaName);
        }
       

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }


        public void Connect()
        {
            _disconnectedMsg.text = CONNECTING_MSG; 
            _auth.SignIn();
        }
        
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        } 
        
        private void OnUserFullyInitialized()
        {
            UpdateUserGUI();
            _connectButton.gameObject.SetActive(false);
            _disconnectedMsg.text = "";
            _userImage.gameObject.SetActive(true); 
            _profileDetailsButton.GetComponent<Image>().color = Color.white;
            IsDetailsOverlayActive = false;
            _profileDetailsButton.gameObject.SetActive(true);
            ConnectToPhoton();
        }

        private void UpdateUserGUI()
        {
            PhotonNetwork.NickName = _userRepository.CurrentUser.DisplayedName;
            _userName.text = _userRepository.CurrentUser.DisplayedName;
            _uid.text = UID_PREFIX + _userRepository.CurrentUser.UID;
            _level.text = LEVEL_PREFIX + _userRepository.CurrentUser.Level.ToString();
            _xp.text = XP_PREFIX + _userRepository.CurrentUser.XP.ToString();
            _wins.text = WINS_PREFIX + _userRepository.CurrentUser.Wins.ToString();
            _loses.text = LOSES_PREFIX + _userRepository.CurrentUser.Defeats.ToString();
            double ratio = _userRepository.CurrentUser.WinsDefeatsRatio();
            _ratio.text =  RATIO_PREFIX + (double.IsPositiveInfinity(ratio) ? "-" : ratio.ToString());
            _chatListener.UpdateFriends(_userRepository.CurrentUser.Friends.ToArray());
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason" + cause);
            _connectButton.gameObject.SetActive(true);
            _connectButton.interactable = true;
            _profileDetailsButton.gameObject.SetActive(false);
            _disconnectedMsg.text = DISCONNECTED_MSG;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_auth.FirebaseAuth.CurrentUser != null)
            {
                _auth.SignOut();
            }
#else
            _auth.SignOut();
#endif
            _isUserFullyInitialized = false;
        }

        public void CreateRoom() 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userRepository.CurrentUser.WinsDefeatsRatio()},
                {"level", _userRepository.CurrentUser.Level},
                {"arenaName", _arenaToLoad},
                {"isPrivate", false}
            };
            roomOptions.CustomRoomPropertiesForLobby = new string[] {"w/d", "level", "arenaName", "isPrivate"};
            roomOptions.MaxPlayers = _maxPlayersPerRoom;
            roomOptions.PublishUserId = true;
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
#if UNITY_ANDROID && !UNITY_EDITOR
            PhotonNetwork.CreateRoom(_auth.CurrentUser.UserId, roomOptions);
#elif UNITY_EDITOR
            PhotonNetwork.CreateRoom(TEST_PLAYER_NAME_EDITOR, roomOptions);
#else
            PhotonNetwork.CreateRoom(TEST_PLAYER_NAME_BUILD, roomOptions);
#endif
        }
        
        
        public void CreatePrivateRoom(string roomName) 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userRepository.CurrentUser.WinsDefeatsRatio()},
                {"level", _userRepository.CurrentUser.Level},
                {"arenaName", _arenaToLoad},
                {"isPrivate", true}
            };
            roomOptions.CustomRoomPropertiesForLobby = new string[] {"w/d", "level", "arenaName", "isPrivate"};
            roomOptions.MaxPlayers = _maxPlayersPerRoom;
            roomOptions.PublishUserId = true;
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoom();
        }


        public override void OnJoinedLobby()
        {
#if UNITY_EDITOR
            //_chatListener.Connect(_userRepository.CurrentUser.UID, _userRepository.CurrentUser.Friends.ToArray());
#else
            //_chatListener.Connect(_userRepository.CurrentUser.UID, null);
#endif
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                LoadArena(_arenaToLoad);
            } 
            else 
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
            } 
        }
        
        private IEnumerator SetProfileImage()
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(_auth.CurrentUser.PhotoUrl);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
                Disconnect();
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
                Sprite image = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
                _userImage.sprite = image;
                OnUserFullyInitialized();
            }
        }

        public static void Disconnect()
        {
            _instance._isUserFullyInitialized = false;
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
        }


        private void ConnectToPhoton()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = GAME_VERSION;
            PhotonNetwork.AuthValues = new AuthenticationValues {UserId = _userRepository.CurrentUser.UID};
            PhotonNetwork.ConnectUsingSettings();
        }

        public void OnUserDataChange()
        {
            if (!_isUserFullyInitialized) {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (!string.IsNullOrEmpty(_userRepository.CurrentUser.PhotoUrl))
                {
                    StartCoroutine(SetProfileImage());
                }
                else 
                {
                    OnUserFullyInitialized();
                }
#else
                OnUserFullyInitialized();
#endif
                _isUserFullyInitialized = true;
            }
            else
            {
                UpdateUserGUI();
            }
        }

        private void OnDestroy()
        {
            _userRepository.UserListeners.Remove(this);
            _auth.AuthListeners.Remove(this);
        }

        public void OnSignIn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _userRepository.SetCurrentUser(_auth.CurrentUser.UserId, _auth.CurrentUser.DisplayName,
            _auth.CurrentUser.Email, _auth.CurrentUser.PhotoUrl.ToString());
#elif UNITY_EDITOR
            _userRepository.SetCurrentUser(TEST_PLAYER_NAME_EDITOR, TEST_PLAYER_NAME_EDITOR, TEST_PLAYER_NAME_EDITOR, null);
#else
            _userRepository.SetCurrentUser(TEST_PLAYER_NAME_BUILD, TEST_PLAYER_NAME_BUILD, TEST_PLAYER_NAME_BUILD, null);
#endif
        }


        public void OnSignOut()
        {
            _userName.text = NOT_LOGGED_MSG;
            _isUserFullyInitialized = false;
            _profileDetails.SetActive(false);
            _userImage.gameObject.SetActive(false);
            Disconnect();
        }
    }
}
