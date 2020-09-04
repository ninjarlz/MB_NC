using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Photon.Chat;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class UserService
    {
        private static UserService _instance;

        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserService();
                    _instance._userRepository = UserRepository.Instance;
                }

                return _instance;
            }
        }

        private UserRepository _userRepository;
        
        private User _currentUser;
        public User CurrentUser => _currentUser;
        private Dictionary<string, Tuple<User, int>> _currentFriends;
        public Dictionary<string, Tuple<User, int>> CurrentFriends => _currentFriends;
        private List<IUserListener> _userListeners = new List<IUserListener>();
        public List<IUserListener> UserListeners => _userListeners;
        private List<IUsersFriendsListener> _usersFriendsListeners = new List<IUsersFriendsListener>();
        public List<IUsersFriendsListener> UsersFriendsListeners => _usersFriendsListeners;
        private int _onUserDataChangeCounter = 0;
        
        private void OnUserDataChange(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                GameManager.Disconnect();
                return;
            }
            Debug.Log("KURWA1");
            _onUserDataChangeCounter++;
            _currentUser = JsonUtility.FromJson<User>(args.Snapshot.GetRawJsonValue());
            if (_onUserDataChangeCounter == 1)
            {
                Debug.Log("KURWA");
                _userRepository.SubscribeFriends(_currentUser.Friends, OnFriendDataChange);
                UserListeners.ForEach(userListener => userListener.OnUserInitialized());
            }
            else
            {
                UserListeners.ForEach(userListener => userListener.OnUserDataChange());
            }
        }

        private void OnFriendDataChange(object sender, ValueChangedEventArgs args)
        {
            User friend = JsonUtility.FromJson<User>(args.Snapshot.GetRawJsonValue());
            if (_currentFriends == null)
            {
                _currentFriends = new Dictionary<string, Tuple<User, int>>();
                _currentFriends.Add(friend.UID, new Tuple<User, int>(friend, ChatUserStatus.Offline));
            }
            else
            {
                if (_currentFriends.ContainsKey(friend.UID))
                {
                    _currentFriends[friend.UID] = new Tuple<User, int>(friend, ChatUserStatus.Offline);
                }
                else
                {
                    _currentFriends.Add(friend.UID, new Tuple<User, int>(friend, ChatUserStatus.Offline));
                }
            }
            foreach (IUsersFriendsListener usersFriendsListener in _usersFriendsListeners)
            {
                usersFriendsListener.OnUsersFriendDataChange(friend.UID);
            }
            
        }

        public async Task SetCurrentUser(string userId, string displayedName, string email, string photoUrl)
        {
            Debug.Log("OOO1");
            _onUserDataChangeCounter = 0;
            _userRepository.CancelUserSubscription();
            Debug.Log("OOO2");
            await _userRepository.SubscribeCurrentUser(userId, displayedName, email, photoUrl,
                OnUserDataChange);
            Debug.Log("OOO3");
        }
        
        public void SignOut()
        {
            _userRepository.CancelUserSubscription();
            _onUserDataChangeCounter = 0;
            _currentFriends = null;
            _currentUser = null;
        }
        
        public async Task UpdateUser(User user)
        {
            await _userRepository.UpdateUser(user);
        }
    }
    
    public interface IUserListener
    {
        void OnUserDataChange();
        void OnUserInitialized();
    }

    public interface IUsersFriendsListener
    {
        void OnUsersFriendDataChange(string uid);
    }
}