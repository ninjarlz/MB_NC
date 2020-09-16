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

        public FirebaseUser CurrentUser
        {
            get { return _firebaseAuth.CurrentUser; }
        }

        private static Auth _instance;

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

        public async Task SignIn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = "37762542413-5j5glvidqc7si62bci19kpk0t628f07o.apps.googleusercontent.com"
            };

            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();
            TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            await signIn;
            if (signIn.IsCanceled)
            {
                signInCompleted.SetCanceled();
                GameManager.Disconnect();
            } 
            else if (signIn.IsFaulted)
            {
                signInCompleted.SetException(signIn.Exception);
                Debug.LogError("An error with GoogleSignIn occured: " + signIn.Exception);
                GameManager.Disconnect();
            }
            else
            {
                Credential credential =
                    GoogleAuthProvider.GetCredential(signIn.Result.IdToken, null);
                Task<FirebaseUser> authTask = _firebaseAuth.SignInWithCredentialAsync(credential);
                await authTask;
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
                    FirebaseUser user = authTask.Result;
                    signInCompleted.SetResult(user);
                }  
              
            }
#endif
        }

        public void SignOut()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            GoogleSignIn.DefaultInstance.SignOut();
            _firebaseAuth.SignOut();
#endif

        }
    }

}