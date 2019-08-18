using UnityEngine;

namespace com.MKG.MB_NC
{
    public static class HexDims
    {
        public const float OuterToInner = 0.866025404f;
        public const float InnerToOuter = 1f / OuterToInner;
        public const float OuterRadius = 10f;
        public const float InnerRadius = OuterRadius * OuterToInner;
        public const int ChunkSizeX = 5, ChunkSizeZ = 5;

        public static Color[] CostColors =
        {
        new Color(0f, 0f, 1f), // blue
        new Color(0, 0.5f, 1f), // light blue
        new Color(1f, 0.5f, 0f), // light orange
        new Color(0.8f, 0.4f, 0f), // orange
        new Color(1f, 0, 0), // red
        new Color(0.8f, 0, 0), // dark red
        new Color(0, 0, 0) // black 
    };

        public static Color[] AdditionalCostColors =
        {
        new Color(0, 0.8f, 0), // green
        new Color(0, 1f, 0) // light green
    };

        public static float Angle(Vector2 pos1, Vector2 pos2)
        {
            Vector2 from = pos2 - pos1;
            Vector2 to = new Vector2(1, 0);

            float result = Vector2.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);

            if (cross.z > 0)
            {
                result = 360f - result;
            }

            return result;
        }
    }
}

