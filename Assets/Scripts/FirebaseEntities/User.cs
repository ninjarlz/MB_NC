namespace com.MKG.MB_NC
{
    public class User
    {
        public string DisplayedName;
        public string Email;
        public string PhotoUrl;
        public int Level;
        public int XP;
        public int Wins;
        public int Defeats;

        public User()
        {
        }

        public User(string displayedName, string email, string photoUrl)
        {
            DisplayedName = displayedName;
            Email = email;
            PhotoUrl = photoUrl;
            Level = 1;
            XP = 0;
            Wins = 0;
            Defeats = 0;
        }

        public User(string displayedName, string email, string photoUrl, int level, int xP, int wins, int defeats)
        {
            DisplayedName = displayedName;
            Email = email;
            PhotoUrl = photoUrl;
            Level = level;
            XP = xP;
            Wins = wins;
            Defeats = defeats;
        }

        public double WinsDefeatsRatio() {
            if (Defeats == 0) {
                return double.PositiveInfinity;
            } else {
                return ((double) Wins) / Defeats;
            }
        }
    }
}
