using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Photon.Chat;
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
        private Dictionary<string, Tuple<User, int>> _currentFriends;
        public Dictionary<string, Tuple<User, int>> CurrentFriends => _currentFriends;
        private List<IUserListener> _userListeners = new List<IUserListener>();
        public List<IUserListener> UserListeners => _userListeners;
        private List<IUsersListener> _usersListeners = new List<IUsersListener>();
        public List<IUsersListener> UsersListeners => _usersListeners;
        private List<IUsersFriendsListener> _usersFriendsListeners = new List<IUsersFriendsListener>();
        public List<IUsersFriendsListener> UsersFriendsListeners => _usersFriendsListeners;
        

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

        public void SendInvitationToFriend(object sender, ValueChangedEventArgs args)
        {
            
        }


        public void UpdateFriendsList()
        {
            if (_currentUser != null)
            {
                if (_currentFriends == null)
                {
                    _currentFriends = new Dictionary<string, Tuple<User, int>>();
                    foreach (User user in _users.Values)
                    {
                        if (_currentUser.Friends.Contains(user.UID))
                        {
                            _currentFriends.Add(user.UID,
                                new Tuple<User, int>(user, ChatUserStatus.Offline));
                        }
                    }
                }
                else
                {
                    Dictionary<string, Tuple<User, int>> currentFriends = new Dictionary<string, Tuple<User, int>>();
                    foreach (User user in _users.Values)
                    {
                        if (_currentUser.Friends.Contains(user.UID))
                        {
                            if (_currentFriends.ContainsKey(user.UID))
                            {
                                currentFriends.Add(user.UID,
                                    new Tuple<User, int>(user, _currentFriends[user.UID].Item2));
                            }
                            else
                            {
                                currentFriends.Add(user.UID,
                                    new Tuple<User, int>(user, ChatUserStatus.Offline));
                            }
                        }
                    }
                    _currentFriends = currentFriends;
                }

                foreach (IUsersFriendsListener usersFriendsListener in _usersFriendsListeners)
                {
                    usersFriendsListener.OnUsersFriendsDataChange();
                }
            }
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
            UpdateFriendsList();
            foreach (IUsersListener usersListener in _usersListeners)
            {
                usersListener.OnUsersDataChange();
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
            UpdateFriendsList();
        }
    }

    public interface IUserListener
    {
        void OnUserDataChange();
    }

    public interface IUsersListener
    {
        void OnUsersDataChange();
    }

    public interface IUsersFriendsListener
    {
        void OnUsersFriendsDataChange();
    }
}