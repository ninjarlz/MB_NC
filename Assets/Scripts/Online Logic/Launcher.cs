using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Google;
using System.Threading.Tasks;


namespace com.MKG.MB_NC
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        private const string _gameVersion = "1";
        [SerializeField]
        private byte _maxPlayersPerRoom = 2;
        private FirebaseApp _app;
        private FirebaseAuth _auth;



        private void SignInWithGoogle(bool linkWithCurrentAnonUser)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                // Copy this value from the google-service.json file.
                // oauth_client with type == 3
                WebClientId = "[YOUR API CLIENT ID HERE].apps.googleusercontent.com"
            };

            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

            TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWith(task =>
            {
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
                    if (linkWithCurrentAnonUser)
                    {
                    //_auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(HandleLoginResult);
                }
                    else
                    {
                    //SignInWithCredential(credential);
                }
                }
            });
        }


        private void SetupFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    //   app = Firebase.FirebaseApp.DefaultInstance;
                    _app = FirebaseApp.DefaultInstance;
                    Debug.Log(_app.Name);
                    _auth = FirebaseAuth.DefaultInstance;
                    Debug.Log(_auth.App.Name);
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    Debug.LogError(string.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        private void Awake()
        {
            SetupFirebase();
            PhotonNetwork.AutomaticallySyncScene = true;
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        public void Connect()
        {
            if (PhotonNetwork.IsConnected) PhotonNetwork.JoinRandomRoom();
            else
            {
                PhotonNetwork.GameVersion = _gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
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

    }
}