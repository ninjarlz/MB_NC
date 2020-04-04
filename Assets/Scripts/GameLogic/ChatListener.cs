using System;
using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public class ChatListener : MonoBehaviour, IChatClientListener
    {
        private GameManager _gameManager;
        private string[] _friends;
        private ChatClient _chatClient;
        private UserRepository _userRepository;
        public ChatClient ChatClient => _chatClient;
        public static string _chatAppId = "2dd7dd28-3447-48a9-ac99-6d11da336e77";

        private void Start()
        {
            _gameManager = GetComponent<GameManager>();
            _userRepository = UserRepository.Instance;
            _chatClient = new ChatClient(this);
        }

        public void Connect(string userId)
        {
            _chatClient.Connect(_chatAppId, GameManager.GAME_VERSION, new AuthenticationValues(userId));
        }

        public void DebugReturn(DebugLevel level, string message)
        {
            Debug.Log(message);
        }
    
        public void OnDisconnected()
        {
            _chatClient.SetOnlineStatus(ChatUserStatus.Offline);
        }
    
        public void OnConnected()
        {
            Debug.Log("Chat connected");
            _chatClient.SetOnlineStatus(ChatUserStatus.Online);
            UpdateFriends(_userRepository.CurrentUser.Friends.ToArray());
        }

        public void UpdateFriends(string[] friends)
        {
            if (_friends != null)
            {
                _chatClient.RemoveFriends(_friends);
            }
            _friends = friends;
            _chatClient.AddFriends(_friends);
        }

        void Update()
        {
            _chatClient.Service();
        }

        public void OnChatStateChange(ChatState state)
        {
           
        }
    
        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            
        }
    
        public void OnPrivateMessage(string sender, object message, string channelName)
        {
           
        }
    
        public void OnSubscribed(string[] channels, bool[] results)
        {
            
        }
    
        public void OnUnsubscribed(string[] channels)
        {
            
        }
    
        public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {
            _userRepository.CurrentFriends[user] = 
                new Tuple<User, int>(_userRepository.CurrentFriends[user].Item1, status);
            _gameManager.OnUsersFriendsDataChange();
        }
    
        public void OnUserSubscribed(string channel, string user)
        {
           
        }
    
        public void OnUserUnsubscribed(string channel, string user)
        {
            
        }

        public static string StatusToString(int status)
        {
            switch (status)
            {
                case ChatUserStatus.Online:
                    return "Online";
                case ChatUserStatus.Playing:
                    return "Playing";
                default:
                    return "Offline";
            }
        }
    }
}