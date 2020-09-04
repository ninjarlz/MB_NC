using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Photon.Chat;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class UserRepository
    {
        private DatabaseReference _usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Users");
        private DatabaseReference _userRef;
        private List<DatabaseReference> _friendsRefs;
        private static UserRepository _instance;
        private EventHandler<ValueChangedEventArgs> _onUserDataChange;
        private EventHandler<ValueChangedEventArgs> _onFriendDataChange;


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

        
        public async Task SubscribeCurrentUser(String userId, string displayedName, string email, string photoUrl,
            EventHandler<ValueChangedEventArgs> onUserDataChange)
        {
            Debug.Log("OOO");
            DataSnapshot result = await _usersRef.Child(userId).GetValueAsync();
            if (!result.Exists)
            {
                await UpdateUser(new User(userId, displayedName, email, photoUrl));
            }
            _userRef = _usersRef.Child(userId);
            _onUserDataChange = onUserDataChange;
            _userRef.ValueChanged += _onUserDataChange;
        }
        
        public void CancelUserSubscription()
        {
            if (_onUserDataChange != null)
            {
                _userRef.ValueChanged -= _onUserDataChange;
                _friendsRefs.ForEach(friendRef => friendRef.ValueChanged -= _onFriendDataChange);
                _friendsRefs.Clear();
                _onUserDataChange = null;
                _userRef = null;  
            }
        }
        
        public void SubscribeFriends(List<String> friends, EventHandler<ValueChangedEventArgs> onFriendDataChange)
        {
            if (_friendsRefs != null && _friendsRefs.Count > 0)
            {
                _friendsRefs.ForEach(friendRef => friendRef.ValueChanged -= _onFriendDataChange);
            }
            _friendsRefs = new List<DatabaseReference>();
            _onFriendDataChange = onFriendDataChange;
            friends.ForEach(friend =>
            {
                DatabaseReference friendRef = _usersRef.Child(friend);
                friendRef.ValueChanged += _onFriendDataChange;
                _friendsRefs.Add(friendRef);
            });
        }


        public async Task UpdateUser(User updatedUser)
        {
            string json = JsonUtility.ToJson(updatedUser);
            await _usersRef.Child(updatedUser.UID).SetRawJsonValueAsync(json);
        }

        public void SendInvitationToFriend(object sender, ValueChangedEventArgs args)
        {
            
        }
    }

   
}