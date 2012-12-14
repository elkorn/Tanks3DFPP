using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tanks3DFPP.Utilities
{
    public static class Utilities
    {
        /// <summary>
        /// Flattens the specified two-dimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns>A one-dimensional array representing the source.</returns>
        public static T[] Flatten<T>(this T[,] source)
        {
            int width = source.GetLength(0),
                height = source.GetLength(1);
            T[] result = new T[width * height];

            for (int i = 0; i < height; ++i)
            {
                for (int a = 0; a < width; a++)
                {
                    result[i * width + a] = source[i, a];
                }
            }

            return result;
        }
    }
}
