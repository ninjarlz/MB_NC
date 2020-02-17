namespace com.MKG.MB_NC
{
    public class User
    {
        public int Level;
        public int XP;
        public int Wins;
        public int Defeats;
        
        public User()
        {
            Level = 1;
            XP = 0;
            Wins = 0;
            Defeats = 0;
        }

        public User(int level, int xP, int wins, int defeats)
        {
            Level = level;
            XP = xP;
            Wins = wins;
            Defeats = defeats;
        }

    }
}
