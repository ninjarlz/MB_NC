using System.Collections.Generic;

namespace com.MKG.MB_NC
{
    public class FightResult
    {

        public int Damage { get; set; }

        FightResult(int damage)
        {
            Damage = damage;
        }

        public static List<FightResult> Parser(string fightResultName)
        {
            if (fightResultName == "-") return new List<FightResult> { null, null };
            else
            {
                List<FightResult> result = new List<FightResult>();
                if (fightResultName[0] == '-') result.Add(null);
                else result.Add(new FightResult(fightResultName[0] - '0'));
                if (fightResultName[2] == '-') result.Add(null);
                else result.Add(new FightResult(fightResultName[2] - '0'));
                return result;
            }
        }
    }
}



#region SystemImplementation
/*
public class FightResult
{
    public bool IsAttacking { get; set; }
    public int FieldsToRetreat { get; set; }
    public bool PowerPenalty { get; set; }
    public bool Dispersion { get; set; }
    //public string Name { get; set; }

    public FightResult(bool isAttacking, int fieldsToRetreat, bool powerPenalty, bool dispersion)
    {
        IsAttacking = isAttacking;
        FieldsToRetreat = fieldsToRetreat;
        PowerPenalty = powerPenalty;
        Dispersion = dispersion;
    }

    public static List<FightResult> Parser(string fightResultName)
    {
        if (fightResultName == "-") return new List<FightResult> { null, null };
        else
        {
            if (fightResultName[0] == 'B')
            {
                if (fightResultName.Length == 2) // form of Bx
                    return new List<FightResult> { null, new FightResult(false, fightResultName[1] - '0', false, false) };
                else if (fightResultName.Length == 3) // form of BxR
                    return new List<FightResult> { null, new FightResult(false, fightResultName[1] - '0', false, true) };
                else // form of Bx-1
                    return new List<FightResult> { null, new FightResult(false, fightResultName[1] - '0', true, false) };
            }
            else if (fightResultName[0] == 'A')
            {
                if (fightResultName.Length == 2) // form of Ax
                    return new List<FightResult> { new FightResult(true, fightResultName[1] - '0', false, false), null };
                else if (fightResultName.Length == 3) // form of AxR
                    return new List<FightResult> { null, new FightResult(true, fightResultName[1] - '0', false, true) };
                else if (fightResultName.Length == 4) // form of Ax-1
                    return new List<FightResult> { null, new FightResult(true, fightResultName[1] - '0', true, false) };
                else // form of Ax-1R
                    return new List<FightResult> { null, new FightResult(true, fightResultName[1] - '0', true, true) };
            }
            else
            {
                if (fightResultName == "-1/-")
                    return new List<FightResult> { new FightResult(true, 0, true, false), null };
                else if (fightResultName == "-1/-1") 
                    return new List<FightResult> { new FightResult(true, 0, true, false), new FightResult(false, 0, true, false) };
                else // form of -1/Bxx
                {
                    if (fightResultName.Length == 5) // form of -1/Bx
                        return new List<FightResult> { new FightResult(true, 0, true, false), new FightResult(false, fightResultName[4] - '0', false, false) };
                    else if (fightResultName.Length == 6) // form of -1/BxR
                        return new List<FightResult> { new FightResult(true, 0, true, false), new FightResult(false, fightResultName[4] - '0', false, true) };
                    else // form of -1/Bx-1
                        return new List<FightResult> { new FightResult(true, 0, true, false), new FightResult(false, fightResultName[4] - '0', true, false)};
                }
            }
        }
    }

}
*/
#endregion


