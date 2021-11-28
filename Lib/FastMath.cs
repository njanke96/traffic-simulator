using System;

namespace CSC473.Lib
{
    /// <summary>
    /// Fast and unsafe hacky math!
    /// </summary>
    public static class FastMath
    {
        // https://en.wikipedia.org/wiki/Fast_inverse_square_root
        public static float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            return x;
        }

        public static float FastVecLength(Godot.Vector3 vec)
        {
            return InvSqrt(vec.LengthSquared());
        }
    }
}