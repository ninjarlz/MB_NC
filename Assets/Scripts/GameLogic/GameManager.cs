using System;
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

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviourPunCallbacks, IUserListener, ILobbyCallbacks, IAuthListener
    {
        private const int MATCHMAKING_STARTING_LIMIT = 0;
        private const int MAX_LVL_DIFF = 3;
        public const string PRIVATE_ROOM_SUFFIX = " - private";
        public const string DISCONNECTED_MSG = "You are disconnected from network services";
#if !UNITY_ANDROID || UNITY_EDITOR
        public const string TEST_PLAYER_NAME = "Test Test";
#endif
        private static GameManager _instance;
        public static GameManager Instance { get { return _instance; } }
        public static MatchManager CurrentMatch { get; set; }
        private const string _gameVersion = "1";
        [SerializeField]
        private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        private Auth _auth;
        private UserRepository _userRepository;
        [SerializeField]
        private Button _connectButton;
        [SerializeField] 
        private TextMeshProUGUI _disconnectedMsg;
        [SerializeField]
        private TextMeshProUGUI _userName;
        [SerializeField]
        private Image _userImage;
        [SerializeField] private GameObject _playerPrefab;
        public GameObject PlayerPrefab { get => _playerPrefab; }
        private string _arenaToLoad;
        private List<RoomInfo> _roomList;
        private Dictionary<string, List<RoomInfo>> _publicRooms;
        private List<RoomInfo> _privateRoomList;
        public List<RoomInfo> PrivateRoomList { get => _privateRoomList; }
        
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
        }

        public void OnConnectButton()
        {
            _connectButton.interactable = false;
            Connect();
        }

        public void EstablishConnection()
        {
            Setup();
            Connect();
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
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = _gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinLobby(TypedLobby.Default);
            _auth.SignIn();
            _connectButton.gameObject.SetActive(false);
            _disconnectedMsg.text = "";
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason" + cause);
            _connectButton.gameObject.SetActive(true);
            _connectButton.interactable = true;
            _disconnectedMsg.text = DISCONNECTED_MSG;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_auth.FirebaseAuth.CurrentUser != null)
            {
                _auth.SignOut();
            }
#else
            _auth.SignOut();
#endif
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
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
#if UNITY_ANDROID && !UNITY_EDITOR
            PhotonNetwork.CreateRoom(_auth.CurrentUser.UserId, roomOptions);
#else
            PhotonNetwork.CreateRoom(TEST_PLAYER_NAME, roomOptions);
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
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoom();
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
                Debug.LogError(request.error);
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite image = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
                _userImage.sprite = image;
                _userImage.gameObject.SetActive(true);
            }
        }

        public static void DisconnectFromPhoton()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
        }

        public void OnUserDataChange()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!string.IsNullOrEmpty(_userRepository.CurrentUser.PhotoUrl))
            {
                StartCoroutine(SetProfileImage());
            }
#else
            _userName.text = _userRepository.CurrentUser.DisplayedName + ", profile updated!";
            _userImage.gameObject.SetActive(true);
#endif
        }

        private void OnDestroy()
        {
            _userRepository.UserListeners.Remove(this);
            _auth.AuthListeners.Remove(this);
        }

        public void OnSignIn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            PhotonNetwork.NickName = _auth.CurrentUser.DisplayName;
            _userName.text = _auth.CurrentUser.DisplayName;
            _userRepository.SetCurrentUser(_auth.CurrentUser.UserId, _auth.CurrentUser.DisplayName,
            _auth.CurrentUser.Email, _auth.CurrentUser.PhotoUrl.ToString());
#else
            _userName.text = TEST_PLAYER_NAME;
            _userRepository.SetCurrentUser(TEST_PLAYER_NAME, TEST_PLAYER_NAME, TEST_PLAYER_NAME, null);
#endif
        }


        public void OnSignOut()
        {
            _userName.text = "-";
            _userImage.gameObject.SetActive(false);
        }
    }
}
