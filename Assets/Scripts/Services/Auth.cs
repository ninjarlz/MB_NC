using Firebase.Auth;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google;
using UnityEngine;

namespace com.MKG.MB_NC
{


    public class Auth
    {

        public const string TEST_PLAYER_NAME = "Test Test";
        private FirebaseAuth _firebaseAuth;
        public FirebaseUser CurrentUser { get { return _firebaseAuth.CurrentUser; } }
        private static Auth _instance;
        private UserRepository _userRepository;
        private List<IAuthListener> _authListeners = new List<IAuthListener>();
        public List<IAuthListener> AuthListeners { get { return _authListeners; } }

        public static Auth Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Auth();
                    _instance._firebaseAuth = FirebaseAuth.DefaultInstance;
                    _instance._userRepository = UserRepository.Instance;
                }

                return _instance;
            }
        }

        private Auth()
        {
        }


#if !UNITY_ANDROID || UNITY_EDITOR
        public void MockSignIn()
        {
            _userRepository.SetCurrentUser(TEST_PLAYER_NAME, TEST_PLAYER_NAME, TEST_PLAYER_NAME, null);
            _authListeners.ForEach(listener => listener.OnMockSignIn());
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
            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    signInCompleted.SetCanceled();
                }
                else if (task.IsFaulted)
                {
                    signInCompleted.SetException(task.Exception);
                    Debug.LogError("An error with GoogleSignIn occured: " + task.Exception);
                }
                else
                {

                    Credential credential =
                        GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>) task).Result.IdToken, null);
                    _firebaseAuth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                    {
                        if (authTask.IsCanceled)
                        {
                            signInCompleted.SetCanceled();
                        }
                        else if (authTask.IsFaulted)
                        {
                            signInCompleted.SetException(authTask.Exception);
                            Debug.LogError("An error with GoogleSignIn occured: " + authTask.Exception);
                        }
                        else
                        {
                            FirebaseUser user = ((Task<FirebaseUser>) authTask).Result;
                            signInCompleted.SetResult(user);
                            Debug.Log("O CHUJ " + user.DisplayName);
                            _authListeners.ForEach(listener => listener.OnSignIn());
                        }
                    });
                }
            });
        }


        public void SignOut()
        {
            GoogleSignIn.DefaultInstance.SignOut();
            _authListeners.ForEach(listener => listener.OnSignOut());
        }

    }

    public interface IAuthListener
    {
        void OnSignIn();
#if !UNITY_ANDROID || UNITY_EDITOR
        void OnMockSignIn();
#endif
        void OnSignOut();
    }
}