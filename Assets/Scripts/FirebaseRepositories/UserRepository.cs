using System;
using System.Collections.Generic;
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
        private Dictionary<string, DatabaseReference> _friendsReferences;
        private User _currentUser;
        public User CurrentUser => _currentUser;
        private Dictionary<string, Tuple<User, int>> _currentFriends;
        public Dictionary<string, Tuple<User, int>> CurrentFriends => _currentFriends;
        private List<IUserListener> _userListeners = new List<IUserListener>();
        public List<IUserListener> UserListeners => _userListeners;
        private List<IUsersFriendsListener> _usersFriendsListeners = new List<IUsersFriendsListener>();
        public List<IUsersFriendsListener> UsersFriendsListeners => _usersFriendsListeners;
        

        public static UserRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserRepository();
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
                        _userRef.ValueChanged -= OnCurrentUserDataChange;
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
                    _userRef.ValueChanged += OnCurrentUserDataChange;
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


        private void OnFriendDataChange(object sender, ValueChangedEventArgs args)
        { 
            if (args.DatabaseError != null) 
            {
                Debug.LogError(args.DatabaseError.Message);
                GameManager.Disconnect();
                return;
            }
            User friend = JsonUtility.FromJson<User>(args.Snapshot.GetRawJsonValue());
            if (_currentFriends.ContainsKey(friend.UID))
            {
                _currentFriends[friend.UID] = new Tuple<User, int>(friend, _currentFriends[friend.UID].Item2);
            } 
            else 
            {
                _currentFriends.Add(friend.UID, new Tuple<User, int>(friend, ChatUserStatus.Offline));
            }

            foreach (IUsersFriendsListener friendsListener in _usersFriendsListeners)
            {
                friendsListener.OnUsersFriendsDataChange();
            }
        }
        
        private void OnCurrentUserDataChange(object sender, ValueChangedEventArgs args)
        {
            Debug.Log("sync user!");
            if (args.DatabaseError != null) 
            {
                Debug.LogError(args.DatabaseError.Message);
                GameManager.Disconnect();
                return;
            }
            _currentUser = JsonUtility.FromJson<User>(args.Snapshot.GetRawJsonValue());
            _currentFriends = new Dictionary<string, Tuple<User, int>>();
            if (_friendsReferences != null)
            {
                foreach (DatabaseReference friendRef in _friendsReferences.Values)
                {
                    friendRef.ValueChanged -= OnFriendDataChange;
                }
            } 
            _friendsReferences = new Dictionary<string, DatabaseReference>();
            foreach (string friendId in _currentUser.Friends)
            {
                DatabaseReference friendRef = _usersRef.Child(friendId);
                friendRef.ValueChanged += OnFriendDataChange;
                _friendsReferences.Add(friendId, friendRef);
            }
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
        void OnUsersDataChange();
    }

    public interface IUsersFriendsListener
    {
        void OnUsersFriendsDataChange();
    }
}