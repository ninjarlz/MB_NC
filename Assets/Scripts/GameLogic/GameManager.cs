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

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviourPunCallbacks, IUserListener, ILobbyCallbacks, IAuthListener
    {
        private const int MATCHMAKING_STARTING_LIMIT = 0;
        private const int MAX_LVL_DIFF = 3;
        private const string PRIVATE_ROOM_SUFFIX = " - private";
        private static GameManager _instance;
        public static GameManager Instance { get { return _instance; } }
        public static MatchManager CurrentMatch { get; set; }
        private const string _gameVersion = "1";
        [SerializeField]
        private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        private Auth _auth;
        private UserRepository _userRepository;
        public bool IsOnline { get; set; }
        [SerializeField]
        private Button _connectButton;
        [SerializeField]
        private TextMeshProUGUI _userName;
        [SerializeField]
        private Image _userImage;
        [SerializeField] private GameObject _playerPrefab;
        public GameObject PlayerPrefab { get => _playerPrefab; }
        private RoomInfo _mostSuitable;
        private string _arenaName;
        private List<RoomInfo> _roomList;
        
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

      
        public void EstablishConnection()
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                _connectButton.gameObject.SetActive(false);
                IsOnline = true;
                Setup();
                Connect();
#if UNITY_ANDROID && !UNITY_EDITOR
                _auth.SignIn();
#else 
                _auth.MockSignIn();
#endif
            }
        }

        public void JoinOnlineMatch(string arenaName)
        {
            if (PhotonNetwork.IsConnected)
            {
                _arenaName = arenaName;
                if (_roomList != null & _roomList.Count > MATCHMAKING_STARTING_LIMIT)
                {
                    if (_mostSuitable != null) 
                    {
                        PhotonNetwork.JoinRoom(_mostSuitable.Name);
                    } 
                    else 
                    {
                        CreateRoom();
                    }     
                }
                else
                {
                    foreach (RoomInfo room in  _roomList)
                    {
                        if (room.CustomProperties["arenaName"].Equals(arenaName))
                        {
                            PhotonNetwork.JoinRoom(room.Name);
                            return;
                        }
                    }
                    CreateRoom();
                }
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {Debug.Log("eoooo");
            _roomList = roomList;
            if (_roomList.Count > 0) 
            {
                RoomInfo mostSuitable = roomList[0];
                Debug.Log(mostSuitable.Name);
                int suitableLvl = (int) mostSuitable.CustomProperties["level"];
                double suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                for (int i = 1; i < roomList.Count; i++)
                {
                    ExitGames.Client.Photon.Hashtable currProps = roomList[i].CustomProperties;
                    int currLvl = (int) currProps["level"];
                    double currWDRatio = (double) currProps["w/d"];
                    double currUserWDRatio = _userRepository.CurrentUser.WinsDefeatsRatio();

                    if (currProps["arenaName"].Equals(mostSuitable.CustomProperties["arenaName"]) &&
                        !(bool)currProps["isPrivate"])
                    {
                        if (Math.Abs(_userRepository.CurrentUser.Level - currLvl) <
                            Math.Abs(_userRepository.CurrentUser.Level - suitableLvl))
                        {
                            mostSuitable = roomList[i];
                            suitableLvl = (int) mostSuitable.CustomProperties["level"];
                            suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                        }
                        else if (Math.Abs(_userRepository.CurrentUser.Level - currLvl) ==
                                 Math.Abs(_userRepository.CurrentUser.Level - suitableLvl))
                        {
                            if (Math.Abs(currUserWDRatio - currWDRatio) < Math.Abs(currWDRatio - suitableWDRatio))
                            {
                                mostSuitable = roomList[i];
                                suitableLvl = (int) mostSuitable.CustomProperties["level"];
                                suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                            }
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
                    _mostSuitable = mostSuitable;
                }
                else
                {
                    _mostSuitable = null;
                }
            }
            else 
            {
                _mostSuitable = null;
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
            Debug.Log("Pun Connected");
            PhotonNetwork.GameVersion = _gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
            IsOnline = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            _auth.SignOut();
#endif
        }


        public void CreateRoom() 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userRepository.CurrentUser.WinsDefeatsRatio() },
                {"level", _userRepository.CurrentUser.Level },
                {"arenaName", _arenaName },
                {"isPrivate", false}
            };
            roomOptions.CustomRoomPropertiesForLobby = new string[] {"w/d", "level", "arenaName", "isPrivate"};
            roomOptions.MaxPlayers = _maxPlayersPerRoom;
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
#if UNITY_ANDROID && !UNITY_EDITORF
            PhotonNetwork.CreateRoom(_auth.CurrentUser.UserId, roomOptions);
#else
            PhotonNetwork.CreateRoom(Auth.TEST_PLAYER_NAME, roomOptions);
#endif
        }
        
        
        public void CreatePrivateRoom(string roomName) 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userRepository.CurrentUser.WinsDefeatsRatio() },
                {"level", _userRepository.CurrentUser.Level },
                {"arenaName", _arenaName },
                {"isPrivate", true}
            };
            roomOptions.CustomRoomPropertiesForLobby = new string[] {"w/d", "level", "arenaName", "isPrivate"};
            roomOptions.MaxPlayers = _maxPlayersPerRoom;
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(roomName + PRIVATE_ROOM_SUFFIX, roomOptions);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoom();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                LoadArena(_arenaName);
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

        public void OnUserDataChange()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!string.IsNullOrEmpty(_userRepository.CurrentUser.PhotoUrl))
            {
                StartCoroutine(SetProfileImage());
            }
#else
            _userName.text = _userRepository.CurrentUser.DisplayedName + ", profile updated!";
#endif
        }

        private void OnDestroy()
        {
            _userRepository.UserListeners.Remove(this);
            _auth.AuthListeners.Remove(this);
        }

        public void OnSignIn()
        {
            PhotonNetwork.NickName = _auth.CurrentUser.DisplayName;
            _userName.text = _auth.CurrentUser.DisplayName;
            _userRepository.SetCurrentUser(_auth.CurrentUser.UserId, _auth.CurrentUser.DisplayName,
            _auth.CurrentUser.Email, _auth.CurrentUser.PhotoUrl.ToString());
        }

        public void OnMockSignIn()
        {
            _userName.text = Auth.TEST_PLAYER_NAME;
        }

        public void OnSignOut()
        {
            _userName.text = "-";
            _userImage.gameObject.SetActive(false);
        }
    }
}
