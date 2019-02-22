using UnityEngine;

public enum HexDirection
{
    N,
    NE,
    SE,
    S,
    SW,
    NW
}

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static int SmallestDifference(this HexDirection direction, HexDirection next)
    {
        
        if (direction < next) return 6 + direction - next < next - direction ? -(6 + direction - next) : next - direction;
        else return 6 - (int)direction + (int)next < direction - next ? 6 - (int)direction + (int)next : -(direction - next);

        #region OldCode
        /*int i = 0, j = 0;
        HexDirection temp = direction;
        while (temp != next)
        {
            i++;
            temp = temp.Next();
        }
        temp = direction;
        while (temp != next)
        {
            j--;
            temp = temp.Previous();
        }

        if (-j < i) return j;
        return i;*/
        #endregion
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.N ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.N : (direction + 1);
    }

    public static HexDirection Previous2(this HexDirection direction)
    {
        direction -= 2;
        return direction >= HexDirection.N ? direction : (direction + 6);
    }

    public static HexDirection Next2(this HexDirection direction)
    {
        direction += 2;
        return direction <= HexDirection.NW ? direction : (direction - 6);
    }
}


