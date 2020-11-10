using System;
using System.Collections.Generic;
using System.Linq;

namespace KioskBrains.Common.Contracts
{
    public static class Assure
    {
        public static void ArgumentNotNull<TValue>(TValue value, string parameterName)
            where TValue : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void ArgumentNotNullOrEmpty<T>(IEnumerable<T> value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (!value.Any())
            {
                throw new ArgumentException("Collection is empty.", parameterName);
            }
        }

        public static void ArgumentPositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{value} is not positive.", parameterName);
            }
        }

        public static void ArgumentNonNegative(decimal value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentException($"{value} is negative.", parameterName);
            }
        }

        public static void CheckFlowState(bool flowCheckResult, string errorMessage)
        {
            if (!flowCheckResult)
            {
                throw new InvalidFlowStateException(errorMessage);
            }
        }
    }
}