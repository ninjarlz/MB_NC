using System.Collections.Generic;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class UserRepository
    {
        private DatabaseReference _usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Users");
        private DatabaseReference _userRef;
        private static UserRepository _instance;
        private User _currentUser;
        public User CurrentUser { get { return _currentUser; } }
        private List<IUserListener> _userListeners = new List<IUserListener>();
        public List<IUserListener> UserListeners { get { return _userListeners; } }

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
                }
                else if (dbTask.IsCompleted) {
                    if (_userRef != null) {
                        _userRef.ValueChanged -= OnUserDataChange;
                    }
                    DataSnapshot snapshot = dbTask.Result;
                    if (!snapshot.HasChild(userId)) {
                        string json = JsonUtility.ToJson(new User(displayedName, email, photoUrl));
                        _userRef = _usersRef.Child(userId);
                        _userRef.SetRawJsonValueAsync(json);
                    } else {
                        _userRef = _usersRef.Child(userId);
                    }
                    _userRef.ValueChanged += OnUserDataChange;
                }
            });
        }

        private void OnUserDataChange(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null) 
            {
                Debug.LogError(args.DatabaseError.Message);
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
}