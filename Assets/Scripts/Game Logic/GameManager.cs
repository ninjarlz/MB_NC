using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Google;
using UnityEngine.Networking;
using System.Collections;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviourPunCallbacks, IUserListener, ILobbyCallbacks
    {


#if !UNITY_ANDROID || UNITY_EDITOR
        public const string _testPlayerName = "Test Test"; 
#endif
        private static GameManager _instance;
        public static GameManager Instance { get { return _instance; } }
        public static MatchManager CurrentMatch { get; set; }
        private const string _gameVersion = "1";
        [SerializeField]
        private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        public static FirebaseApp App { get { return Instance._app; } }
        private FirebaseAuth _auth;
        public static FirebaseAuth Auth { get { return Instance._auth; } }
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

        private void SetupFirebase()
        {
            _app = FirebaseApp.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
            _app.SetEditorDatabaseUrl("https://mb-nc-a2dd3.firebaseio.com/");
            _userRepository = UserRepository.Instance;
            _userRepository.UserListeners.Add(this);
        }

      
        public void EstablishConnection()
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                _connectButton.gameObject.SetActive(false);
                IsOnline = true;
                SetupFirebase();
                Connect();
#if UNITY_ANDROID && !UNITY_EDITOR
                SignIn();
#else 
                MockSignIn();
#endif
            }
        }

        public void JoinOnlineDemo()
        {
            if (PhotonNetwork.IsConnected)
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
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList) 
        {
            if (roomList.Count != 0) {
                
                RoomInfo mostSuitable = roomList[0];
                double currUserWDRatio = _userRepository.CurrentUser.WinsDefeatsRatio();
                for (int i = 1; i < roomList.Count; i++) {
                    ExitGames.Client.Photon.Hashtable currProps = roomList[i].CustomProperties;
                    ExitGames.Client.Photon.Hashtable mostSuitableProps = mostSuitable.CustomProperties;
                    int currLvl = (int)currProps["level"];
                    int suitableLvl = (int)mostSuitableProps["level"];
                    int currWDRatio = (int)currProps["w/d"];
                    int suitableWDRatio = (int)mostSuitableProps["w/d"];
                    if (Math.Abs(_userRepository.CurrentUser.Level - currLvl) <
                        Math.Abs(_userRepository.CurrentUser.Level - suitableLvl)) {
                        mostSuitable = roomList[i];
                    } else if (Math.Abs(_userRepository.CurrentUser.Level - currLvl) ==
                          Math.Abs(_userRepository.CurrentUser.Level - suitableLvl)) {
                        if (Math.Abs(currUserWDRatio - currWDRatio) < Math.Abs(currWDRatio - suitableWDRatio)) {
                            mostSuitable = roomList[i];
                        }
                    }
                }
                _mostSuitable = mostSuitable;
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

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            PhotonNetwork.LoadLevel("Fulford Online");
        }
       

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }


        public void Connect()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Debug.Log("Pun Connected");
#if UNITY_ANDROID && !UNITY_EDITOR
            PhotonNetwork.NickName = _auth.CurrentUser.DisplayName;
#endif
            PhotonNetwork.GameVersion = _gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinLobby();
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
            IsOnline = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            SignOut();
#endif
        }


        public void CreateRoom() 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userRepository.CurrentUser.WinsDefeatsRatio() },
                {"level", _userRepository.CurrentUser.Level}
            };
            roomOptions.MaxPlayers = _maxPlayersPerRoom;
            Debug.Log("CreateRoom() was called. No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
#if UNITY_ANDROID && !UNITY_EDITOR
            PhotonNetwork.CreateRoom(_auth.CurrentUser.UserId, roomOptions);
#else
            PhotonNetwork.CreateRoom(_testPlayerName, roomOptions);
#endif
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                {"w/d", _userRepository.CurrentUser.WinsDefeatsRatio() },
                {"level", _userRepository.CurrentUser.Level}
            };
            roomOptions.MaxPlayers = _maxPlayersPerRoom;
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(_auth.CurrentUser.UserId, roomOptions);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                LoadArena();
            } 
            else 
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
            } 
        }

#if !UNITY_ANDROID || UNITY_EDITOR
        public void MockSignIn()
        {
            _userName.text = _testPlayerName;
            _userRepository.SetCurrentUser(_testPlayerName, _testPlayerName, _testPlayerName, null);
        }
#endif

        public void SignIn()
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = "37762542413-5j5glvidqc7si62bci19kpk0t628f07o.apps.googleusercontent.com"
            };
            
            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();
            
            TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWith(task => {
                if (task.IsCanceled)
                {
                    signInCompleted.SetCanceled();
                }
                else if (task.IsFaulted)
                {
                    signInCompleted.SetException(task.Exception);
                }
                else
                {

                    Credential credential = GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                    _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask => {
                        if (authTask.IsCanceled)
                        {
                            signInCompleted.SetCanceled();
                        }
                        else if (authTask.IsFaulted)
                        {
                            signInCompleted.SetException(authTask.Exception);
                        }
                        else
                        {
                            FirebaseUser user = ((Task<FirebaseUser>)authTask).Result;
                            signInCompleted.SetResult(user);
                            _userName.text = _auth.CurrentUser.DisplayName;
                            _userRepository.SetCurrentUser(_auth.CurrentUser.UserId, _auth.CurrentUser.DisplayName,
                                _auth.CurrentUser.Email, _auth.CurrentUser.PhotoUrl.ToString());
                        }
                    });
                }
            });
        }


        public void SignOut()
        {
            GoogleSignIn.DefaultInstance.SignOut();
            _userName.text = "-";
            _userImage.gameObject.SetActive(false);
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
        }
    }
}
