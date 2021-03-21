using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.Utils
{
    /// <summary>
    /// Utilities relating to helpful maths functions & etc.
    /// </summary>
    public class Maths
    {
        private const float EPSILON = 0.001f;

        /// <summary>
        /// Check if both values are within an acceptable threshold to be considered as approximetally equal.
        /// </summary>
        /// <param name="valueA">Value A to compare</param>
        /// <param name="valueB">Value B to compare</param>
        /// <param name="epsilon">Threshold within which the values are considered as equal</param>
        /// <returns></returns>
        public static bool ApproxEquals(float valueA, float valueB, float epsilon = EPSILON)
        {
            return Math.Abs(valueA - valueB) < epsilon;
        }
    }
}
