using System.Collections.Generic;
using Firebase.Database;
using Photon.Pun;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class UserRepository
    {
        private DatabaseReference _usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Users");
        private DatabaseReference _userRef;
        private static UserRepository _instance;
        private Dictionary<string, User> _users;
        public Dictionary<string, User> Users => _users;
        private User _currentUser;
        public User CurrentUser => _currentUser;
        private List<IUserListener> _userListeners = new List<IUserListener>();
        public List<IUserListener> UserListeners => _userListeners;
        private List<IUsersListener> _usersListeners = new List<IUsersListener>();
        public List<IUsersListener> UsersListeners => _usersListeners;

        public static UserRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserRepository();
                    _instance._usersRef.ValueChanged += _instance.OnUsersDataChange;
                }
                return _instance;
            }
        }

        private UserRepository() { }

        public void SetCurrentUser(string userId, string displayedName, string email, string photoUrl)
        {
            _usersRef.GetValueAsync().ContinueWith(dbTask => {
                if (dbTask.IsFaulted)  
                {
                    Debug.LogError("Cannot connect to database");
                    GameManager.Disconnect();
                }
                else if (dbTask.IsCompleted) {
                    if (_userRef != null) 
                    {
                        _userRef.ValueChanged -= OnUserDataChange;
                    }
                    DataSnapshot snapshot = dbTask.Result;
                    if (!snapshot.HasChild(userId)) 
                    {
                        string json = JsonUtility.ToJson(new User(userId, displayedName, email, photoUrl));
                        _userRef = _usersRef.Child(userId);
                        _userRef.SetRawJsonValueAsync(json);
                    } 
                    else 
                    {
                        _userRef = _usersRef.Child(userId);
                    }
                    _userRef.ValueChanged += OnUserDataChange;
                }
            });
        }

        public void UpdateUser(User updatedUser)
        {
            Debug.Log("update user!");
            if (_userRef != null)
            {
                string json = JsonUtility.ToJson(updatedUser);
                _userRef.SetRawJsonValueAsync(json);
            }
        }

        public void SendInvitationToFriend(string uid)
        {
            
        }
        
        

        private void OnUsersDataChange(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                GameManager.Disconnect();
                return;
            }
            DataSnapshot snapshot = args.Snapshot;
            _users = new Dictionary<string, User>();
            foreach (DataSnapshot children in snapshot.Children)
            {
                _users.Add(children.Key, JsonUtility.FromJson<User>(children.GetRawJsonValue()));
            }
            foreach (IUsersListener usersListener in _usersListeners)
            {
                usersListener.OnUsersDataChange(_users);
            }
        }

        private void OnUserDataChange(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("sync user!");
            if (args.DatabaseError != null) 
            {
                Debug.LogError(args.DatabaseError.Message);
                GameManager.Disconnect();
                return;
            }
            _currentUser = JsonUtility.FromJson<User>(args.Snapshot.GetRawJsonValue());
            foreach (IUserListener userListener in _userListeners)
            {
                userListener.OnUserDataChange();
            }
        }
    }

    public interface IUserListener
    {
        void OnUserDataChange();
    }

    public interface IUsersListener
    {
        void OnUsersDataChange(Dictionary<string,User> users);
    }
}