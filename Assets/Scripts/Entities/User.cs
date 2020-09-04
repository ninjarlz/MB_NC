using System.Collections.Generic;

namespace com.MKG.MB_NC
{
    public class User
    {
        public string UID;
        public string DisplayedName;
        public string Email;
        public string PhotoUrl;
        public int Level;
        public int XP;
        public int Wins;
        public int Defeats;
        public List<string> Friends;
        public List<string> PendingInvitations;

        public User()
        {
        }

        public User(string uid, string displayedName, string email, string photoUrl)
        {
            UID = uid;
            DisplayedName = displayedName;
            Email = email;
            PhotoUrl = photoUrl;
            Level = 1;
            XP = 0;
            Wins = 0;
            Defeats = 0;
        }

        public User(string uid, string displayedName, string email, string photoUrl,
            int level, int xP, int wins, int defeats, List<string> friends, List<string> pendingInvitations)
        {
            UID = uid;
            DisplayedName = displayedName;
            Email = email;
            PhotoUrl = photoUrl;
            Level = level;
            XP = xP;
            Wins = wins;
            Defeats = defeats;
            Friends = friends;
            PendingInvitations = pendingInvitations;
        }

        public double WinsDefeatsRatio() 
        {
            if (Defeats == 0) {
                return double.PositiveInfinity;
            } 
            return ((double) Wins) / Defeats;
        }
    }
}
