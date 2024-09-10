using System;

namespace Utilities
{
    public static class Random
    {
        #region fields

        private static System.Random random;

        #endregion


        #region Static Methods

        public static System.Random GetRandom()
        {
            if (random == null)
                random = new System.Random();

            return random;
        }

        #endregion

    }
}