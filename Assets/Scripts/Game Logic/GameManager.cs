using UnityEngine;
using UnityEngine.UI;
/*using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Google;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;*/

namespace com.MKG.MB_NC
{
    public class GameManager //: MonoBehaviourPunCallbacks
    {
      /*  public static GameManager Instance;
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
        private Text _userName;
        [SerializeField]
        private Image _userImage;
         
        
        
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
        }

      
        public void EstablishConnection()
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                _connectButton.gameObject.SetActive(false);
                IsOnline = true;
                SetupFirebase();
                Connect();
                SignIn();
            }
        }

        public void Connect()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Debug.Log("Pun Connected");
            //if (PhotonNetwork.IsConnected) PhotonNetwork.JoinRandomRoom();
            //else
            //{
            //    PhotonNetwork.GameVersion = _gameVersion;
            //    PhotonNetwork.ConnectUsingSettings();
            //}
            
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = _maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        }

        public void SignIn()
        {
            Debug.Log("AAA");
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
            _userImage.transform.parent.gameObject.SetActive(false);
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
                _userImage.transform.parent.gameObject.SetActive(true);
            }
        }*/
    }
}
