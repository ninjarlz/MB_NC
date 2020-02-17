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
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine.SceneManagement;

namespace com.MKG.MB_NC
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;
        public static MatchManager CurrentMatch { get; set; }
        private const string _gameVersion = "1";
        [SerializeField]
        private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        public static FirebaseApp App { get { return Instance._app; } }
        private FirebaseAuth _auth;
        public static FirebaseAuth Auth { get { return Instance._auth; } }
        public bool IsOnline { get; set; }
        [SerializeField]
        private Button _connectButton;
        [SerializeField]
        private TextMeshProUGUI _userName;
        [SerializeField]
        private Image _userImage;
        [SerializeField] private GameObject _playerPrefab;
        public GameObject PlayerPrefab { get => _playerPrefab; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                EstablishConnection();
            }
        }

     
        private void SetupFirebase()
        {
            _app = FirebaseApp.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
            _app.SetEditorDatabaseUrl("https://mb-nc-a2dd3.firebaseio.com/");
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
                PhotonNetwork.JoinRandomRoom();
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
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
            IsOnline = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            SignOut();
#endif
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = _maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                LoadArena();
            }
        }


        public void MockSignIn()
        {
            _userName.text = "Test Test";
            DatabaseReference usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Users"); 
            usersRef.GetValueAsync().ContinueWith(dbTask => {
                if (dbTask.IsFaulted) 
                {
                    Debug.LogError("Cannot connect to database");
                }
                else if (dbTask.IsCompleted) {
                    DataSnapshot snapshot = dbTask.Result;
                    String userId = _userName.text;
                    if (!snapshot.HasChild(userId)) {
                        string json = JsonUtility.ToJson(new User());
                        usersRef.Child(userId).SetRawJsonValueAsync(json);
                    }
                }
            });
        }

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
                            DatabaseReference usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Users"); 
                            usersRef.GetValueAsync().ContinueWith(dbTask => {
                                if (dbTask.IsFaulted) 
                                {
                                    Debug.LogError("Cannot connect to database");
                                }
                                else if (dbTask.IsCompleted) {
                                    DataSnapshot snapshot = dbTask.Result;
                                    String userId = _auth.CurrentUser.UserId;
                                    if (!snapshot.HasChild(userId)) {
                                        string json = JsonUtility.ToJson(new User());
                                        usersRef.Child(userId).SetRawJsonValueAsync(json);
                                    }
                                }
                            });
                            StartCoroutine(SetProfileImage());
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
    }
}
