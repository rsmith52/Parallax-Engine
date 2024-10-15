using System;

namespace Utilities
{
    public static class BooleanExtensions
    {
        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }
    }
}
