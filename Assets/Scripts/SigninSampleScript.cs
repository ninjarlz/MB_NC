// <copyright file="SigninSampleScript.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations

namespace SignInSample {
  using System.Collections.Generic;
  using System.Collections;
  using Google;
  using UnityEngine;
  using UnityEngine.UI;
  using Firebase;
  using Firebase.Auth;
    using System.Threading.Tasks;

    public class SigninSampleScript : MonoBehaviour {

    public Text statusText;

    private string webClientId = "37762542413-5j5glvidqc7si62bci19kpk0t628f07o.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;

        private FirebaseApp _app;

        private FirebaseAuth _auth;


    private void SetupFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError(string.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });

    }


        public void SignIn()
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                // Copy this value from the google-service.json file.
                // oauth_client with type == 3
                WebClientId = "37762542413-eheri68t14btaml1d1e3suhqruesce0l.apps.googleusercontent.com"
            };


            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

            TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    signInCompleted.SetCanceled();
                    AddStatusText("Exception!");
                }
                else if (task.IsFaulted)
                {
                    signInCompleted.SetException(task.Exception);
                    AddStatusText(task.Exception.Message);
                }
                else
                {
                    Credential credential = GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                    _auth.SignInWithCredentialAsync(credential).ContinueWith(task1 => {
                        if (task1.IsCanceled)
                        {
                            Debug.LogError("SignInWithCredentialAsync was canceled.");
                            AddStatusText("SignInWithCredentialAsync was canceled.");
                            return;
                        }
                        if (task1.IsFaulted)
                        {
                            Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                            AddStatusText("SignInWithCredentialAsync encountered an error: " + task.Exception);
                            return;
                        }

                        AddStatusText(_auth.CurrentUser.DisplayName);

                    });
                }
            });
        }





        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Start() {
            configuration = new GoogleSignInConfiguration {
                WebClientId = webClientId,
                RequestIdToken = true };
            SetupFirebase();


        }
      
    

    public void OnSignIn() {
      //GoogleSignIn.Configuration = configuration;
      //GoogleSignIn.Configuration.UseGameSignIn = false;
      //GoogleSignIn.Configuration.RequestIdToken = true;
      AddStatusText("Calling SignIn");
       SignIn();
       //GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
      //  OnAuthenticationFinished);
    }

    public void OnSignOut() {
      AddStatusText("Calling SignOut");
      GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect() {
      AddStatusText("Calling Disconnect");
      GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task) {
      if (task.IsFaulted) {
        using (IEnumerator<System.Exception> enumerator =
                task.Exception.InnerExceptions.GetEnumerator()) {
          if (enumerator.MoveNext()) {
            GoogleSignIn.SignInException error =
                    (GoogleSignIn.SignInException)enumerator.Current;
            AddStatusText("Got Error: " + error.Status + " " + error.Message + "\n" + error.Data + "\n" + error.InnerException 
                + "\n" + error.Source + "\n" + error.HelpLink);
            //AddStatusText("Got Error: " + error.StackTrace);
          } else {
            AddStatusText("Got Unexpected Exception?!?" + task.Exception);
          }
        }
      } else if(task.IsCanceled) {
        AddStatusText("Canceled");
      } else  {
        AddStatusText("Welcome: " + task.Result.DisplayName + "!");
      }
    }

    public void OnSignInSilently() {
      GoogleSignIn.Configuration = configuration;
      GoogleSignIn.Configuration.UseGameSignIn = false;
      GoogleSignIn.Configuration.RequestIdToken = true;
      AddStatusText("Calling SignIn Silently");

      GoogleSignIn.DefaultInstance.SignInSilently()
            .ContinueWith(OnAuthenticationFinished);
    }


    public void OnGamesSignIn() {
      GoogleSignIn.Configuration = configuration;
      GoogleSignIn.Configuration.UseGameSignIn = true;
      GoogleSignIn.Configuration.RequestIdToken = false;

      AddStatusText("Calling Games SignIn");

      GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
        OnAuthenticationFinished);
    }

    private List<string> messages = new List<string>();
    void AddStatusText(string text) {
      if (messages.Count == 5) {
        messages.RemoveAt(0);
      }
      messages.Add(text);
      string txt = "";
      foreach (string s in messages) {
        txt += "\n" + s;
      }
      statusText.text = txt;
    }


        IEnumerator SignInCoroutine()
        {
            yield return new WaitForSeconds(1);
            SignIn();
        }

    }
}
