using Firebase.Auth;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google;
using Photon.Pun;
using UnityEngine;

namespace com.MKG.MB_NC
{


    public class Auth
    {
        private FirebaseAuth _firebaseAuth;
        public FirebaseAuth FirebaseAuth => _firebaseAuth;
        public FirebaseUser CurrentUser { get { return _firebaseAuth.CurrentUser; } }
        private static Auth _instance;
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
                }

                return _instance;
            }
        }

        private Auth()
        {
        }

        public void SignIn()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            _authListeners.ForEach(listener => listener.OnSignIn());
#else            
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
                    GameManager.Disconnect();
                }
                else if (task.IsFaulted)
                {
                    signInCompleted.SetException(task.Exception);
                    Debug.LogError("An error with GoogleSignIn occured: " + task.Exception);
                    GameManager.Disconnect();
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
                            GameManager.Disconnect();
                        }
                        else if (authTask.IsFaulted)
                        {
                            signInCompleted.SetException(authTask.Exception);
                            Debug.LogError("An error with GoogleSignIn occured: " + authTask.Exception);
                            GameManager.Disconnect();
                        }
                        else
                        {
                            FirebaseUser user = ((Task<FirebaseUser>) authTask).Result;
                            signInCompleted.SetResult(user);
                            _authListeners.ForEach(listener => listener.OnSignIn());
                        }
                    });
                }
            });
#endif
        }


        public void SignOut()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            GoogleSignIn.DefaultInstance.SignOut();
            _firebaseAuth.SignOut();
            
#endif
            _authListeners.ForEach(listener => listener.OnSignOut());
        }
    }

    public interface IAuthListener
    {
        void OnSignIn();
        void OnSignOut();
    }
}