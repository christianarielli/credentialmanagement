using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    internal static class AssertionExtensions
    {
        public static void ShouldNotBeNull(this object value)
        {
            Assert.IsNotNull(value);
        }

        public static void ShouldNotBeEmpty(this string value)
        {
            Assert.IsFalse(string.IsNullOrEmpty(value));
        }

        public static void ShouldNotBeEmpty(this ICollection value)
        {
            Assert.IsNotNull(value);
            Assert.AreNotEqual(0, value.Count);
        }

        public static void ShouldBeTrue(this bool value)
        {
            Assert.IsTrue(value);
        }

        public static void ShouldBeFalse(this bool value)
        {
            Assert.IsFalse(value);
        }

        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ShouldBeOfType(this object actual, Type expectedType)
        {
            Assert.IsInstanceOfType(actual, expectedType);
        }

        public static void ShouldHaveCountOf(this ICollection actual, int expectedCount)
        {
            Assert.AreEqual(expectedCount, actual.Count);
        }
    }

    internal static class Testing
    {
        public static TException ShouldThrowException<TException>(Action action)
            where TException : Exception
        {
            return Assert.ThrowsExactly<TException>(action);
        }
    }
}
