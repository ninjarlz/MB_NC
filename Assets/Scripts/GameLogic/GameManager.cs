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
using Photon.Chat;
using AuthenticationValues = Photon.Realtime.AuthenticationValues;
using Random = UnityEngine.Random;

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviourPunCallbacks, IUserListener, IUsersFriendsListener, IAuthListener
    {
        private const int MATCHMAKING_STARTING_LIMIT = 0;
        private const int MAX_LVL_DIFF = 3;
        public const string PRIVATE_ROOM_SUFFIX = " - private";
#if UNITY_EDITOR
        private const string TEST_PLAYER_NAME_EDITOR = "Test Test";
#elif !UNITY_ANDROID
        private const string TEST_PLAYER_NAME_BUILD = "Test Test 2";
#endif
        private static GameManager _instance;

        public static GameManager Instance => _instance;

        public static MatchManager CurrentMatch { get; set; }
        public static string GAME_VERSION = "1";
        [SerializeField] private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        private Auth _auth;
        private UserService _userService;
        
        private SystemUtils _systemUtils;
        private ChatListener _chatListener;
        private GameManagerUI _gameManagerUi;
        
         private bool _isUserFullyInitialized = false;
         public bool IsUserFullyInitialized => _isUserFullyInitialized;
        
        [SerializeField] private GameObject _playerPrefab;
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
            _userService = UserService.Instance;
            _userService.UserListeners.Add(this);
            _userService.UsersFriendsListeners.Add(this);
            _auth.AuthListeners.Add(this);
            _systemUtils = SystemUtils.Instance;
            _chatListener = GetComponent<ChatListener>();
            _gameManagerUi = GetComponent<GameManagerUI>();
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
                    double currUserWDRatio = _userService.CurrentUser.WinsDefeatsRatio();
                    if (Math.Abs(_userService.CurrentUser.Level - currLvl) <
                        Math.Abs(_userService.CurrentUser.Level - suitableLvl))
                    {
                        mostSuitable = matchesOnArena[i];
                        suitableLvl = (int) mostSuitable.CustomProperties["level"];
                        suitableWDRatio = (double) mostSuitable.CustomProperties["w/d"];
                    }
                    else if (Math.Abs(_userService.CurrentUser.Level - currLvl) ==
                             Math.Abs(_userService.CurrentUser.Level - suitableLvl))
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
                Debug.Log("Diff:" + Math.Abs(_userService.CurrentUser.Level - suitableLvl).ToString());
                Debug.Log("Curr user lvl: " + _userService.CurrentUser.Level.ToString());
                Debug.Log("most suitable lvl: " + suitableLvl.ToString());
                Debug.Log("limit: " + (MAX_LVL_DIFF + 1).ToString());
                if (Math.Abs(_userService.CurrentUser.Level - suitableLvl) < MAX_LVL_DIFF + 1)
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
            _gameManagerUi.OnConnecting();
            _auth.SignIn();
        }
        
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            _chatListener.Connect(_userService.CurrentUser.UID);
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        private void UpdateUserData()
        {
            PhotonNetwork.NickName = _userService.CurrentUser.DisplayedName;
            _gameManagerUi.UpdateUserGUI();
            _chatListener.UpdateFriends(_userService.CurrentUser.Friends.ToArray());
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason" + cause);
            _gameManagerUi.OnDisconnected();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_auth.FirebaseAuth.CurrentUser != null)
            {
                _auth.SignOut();
                _userService.SignOut();
            }
#else
            _auth.SignOut();
            _userService.SignOut();
#endif
        }

        public void CreateRoom() 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userService.CurrentUser.WinsDefeatsRatio()},
                {"level", _userService.CurrentUser.Level},
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
                {"w/d", _userService.CurrentUser.WinsDefeatsRatio()},
                {"level", _userService.CurrentUser.Level},
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
        
        

        public static void Disconnect()
        {
            if (PhotonNetwork.IsConnected)
            {
               PhotonNetwork.Disconnect();
               _instance._isUserFullyInitialized = false;
            }
        }


        private void ConnectToPhoton()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = GAME_VERSION;
            PhotonNetwork.AuthValues = new AuthenticationValues {UserId = _userService.CurrentUser.UID};
            PhotonNetwork.ConnectUsingSettings();
        }

        public void OnUserDataChange()
        {
            UpdateUserData();
        }

        public void OnUserInitialized()
        {
            _gameManagerUi.UpdateUserGUI();
            _gameManagerUi.ShowConnected();
            ConnectToPhoton();
            _isUserFullyInitialized = true;
        }

        private void OnDestroy()
        {
            if (_userService != null)
            {
                _userService.UserListeners.Remove(this);
                _userService.UsersFriendsListeners.Remove(this);
            }
            _auth?.AuthListeners.Remove(this);
        }

        public void OnSignIn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _userService.SetCurrentUser(_auth.CurrentUser.UserId, _auth.CurrentUser.DisplayName,
            _auth.CurrentUser.Email, _auth.CurrentUser.PhotoUrl.ToString());
#elif UNITY_EDITOR
            _userService.SetCurrentUser(TEST_PLAYER_NAME_EDITOR, TEST_PLAYER_NAME_EDITOR, TEST_PLAYER_NAME_EDITOR, null);
#else
            _userService.SetCurrentUser(TEST_PLAYER_NAME_BUILD, TEST_PLAYER_NAME_BUILD, TEST_PLAYER_NAME_BUILD, null);
#endif
        }


        public void OnSignOut()
        {
            _gameManagerUi.OnSignOut();
            Disconnect();
            _isUserFullyInitialized = false;
        }


        public void OnUsersFriendDataChange(string uid)
        {
            _gameManagerUi.OnFriendDataChange(uid);
            /*if (_chatListener.ChatClient.State == ChatState.ConnectedToFrontEnd)
            {
                _chatListener.UpdateFriends(_userService.CurrentUser.Friends.ToArray());
            }*/
        }
    }
}
