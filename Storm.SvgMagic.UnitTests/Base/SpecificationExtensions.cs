using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Storm.SvgMagic.UnitTests.Base
{
    public delegate void MethodThatThrows();

    public static class SpecificationExtensions
    {
        [DebuggerHidden]
        public static void ShouldBeFalse(this bool condition)
        {
            Assert.IsFalse(condition);
        }

        [DebuggerHidden]
        public static void ShouldBeTrue(this bool condition)
        {
            Assert.IsTrue(condition);
        }

        [DebuggerHidden]
        public static object ShouldEqual(this object actual, object expected)
        {
            Assert.AreEqual(expected, actual);
            return expected;
        }

        [DebuggerHidden]
        public static object ShouldNotEqual(this object actual, object expected)
        {
            Assert.AreNotEqual(expected, actual);
            return expected;
        }

        [DebuggerHidden]
        public static void ShouldBeNull(this object anObject)
        {
            Assert.IsNull(anObject);
        }

        [DebuggerHidden]
        public static void ShouldNotBeNull(this object anObject)
        {
            Assert.IsNotNull(anObject);
        }

        [DebuggerHidden]
        public static object ShouldBeTheSameAs(this object actual, object expected)
        {
            Assert.AreSame(expected, actual);
            return expected;
        }

        [DebuggerHidden]
        public static object ShouldNotBeTheSameAs(this object actual, object expected)
        {
            Assert.AreNotSame(expected, actual);
            return expected;
        }

        [DebuggerHidden]
        public static void ShouldBeOfType(this object actual, Type expected)
        {
            Assert.IsInstanceOf(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldBe(this object actual, Type expected)
        {
            Assert.IsInstanceOf(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldNotBeOfType(this object actual, Type expected)
        {
            Assert.IsNotInstanceOf(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldContain(this IList actual, object expected)
        {
            Assert.Contains(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldContain<T>(this IEnumerable<T> actual, object expected)
        {
            Assert.Contains(expected, actual.ToList());
        }

        [DebuggerHidden]
        public static void ShouldNotContain(this IList collection, object expected)
        {
            CollectionAssert.DoesNotContain(collection, expected);
        }

        [DebuggerHidden]
        public static void ShouldNotContain<T>(this IEnumerable<T> actual, object expected)
        {
            CollectionAssert.DoesNotContain(actual.ToList(), expected);
        }

        [DebuggerHidden]
        public static IComparable ShouldBeGreaterThan(this IComparable arg1, IComparable arg2)
        {
            Assert.Greater(arg1, arg2);
            return arg2;
        }

        [DebuggerHidden]
        public static IComparable ShouldBeGreaterOrEqualThan(this IComparable arg1, IComparable arg2)
        {
            Assert.GreaterOrEqual(arg1, arg2);
            return arg2;
        }

        [DebuggerHidden]
        public static IComparable ShouldBeLessOrEqualThan(this IComparable arg1, IComparable arg2)
        {
            Assert.LessOrEqual(arg1, arg2);
            return arg2;
        }

        [DebuggerHidden]
        public static IComparable ShouldBeLessThan(this IComparable arg1, IComparable arg2)
        {
            Assert.Less(arg1, arg2);
            return arg2;
        }

        [DebuggerHidden]
        public static void ShouldBeEmpty(this ICollection collection)
        {
            Assert.IsEmpty(collection);
        }

        [DebuggerHidden]
        public static void ShouldBeEmpty(this string aString)
        {
            Assert.IsEmpty(aString);
        }

        [DebuggerHidden]
        public static void ShouldNotBeEmpty(this ICollection collection)
        {
            Assert.IsNotEmpty(collection);
        }

        [DebuggerHidden]
        public static void ShouldNotBeEmpty(this string aString)
        {
            Assert.IsNotEmpty(aString);
        }

        [DebuggerHidden]
        public static void ShouldContain(this string actual, string expected)
        {
            StringAssert.Contains(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldNotContain(this string actual, string expected)
        {
            try
            {
                StringAssert.Contains(expected, actual);
            }
            catch (AssertionException)
            {
                return;
            }

            throw new AssertionException(String.Format("\"{0}\" should not contain \"{1}\".", actual, expected));
        }

        [DebuggerHidden]
        public static string ShouldBeEqualIgnoringCase(this string actual, string expected)
        {
            StringAssert.AreEqualIgnoringCase(expected, actual);
            return expected;
        }

        [DebuggerHidden]
        public static void ShouldStartWith(this string actual, string expected)
        {
            StringAssert.StartsWith(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldNotStartWith(this string actual, string expected)
        {
            StringAssert.DoesNotStartWith(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldEndWith(this string actual, string expected)
        {
            StringAssert.EndsWith(expected, actual);
        }

        [DebuggerHidden]
        public static void ShouldBeSurroundedWith(this string actual, string expectedStartDelimiter, string expectedEndDelimiter)
        {
            StringAssert.StartsWith(expectedStartDelimiter, actual);
            StringAssert.EndsWith(expectedEndDelimiter, actual);
        }

        [DebuggerHidden]
        public static void ShouldBeSurroundedWith(this string actual, string expectedDelimiter)
        {
            StringAssert.StartsWith(expectedDelimiter, actual);
            StringAssert.EndsWith(expectedDelimiter, actual);
        }

        [DebuggerHidden]
        public static void ShouldContainErrorMessage(this Exception exception, string expected)
        {
            StringAssert.Contains(expected, exception.Message);
        }

        [DebuggerHidden]
        public static Exception ShouldBeThrownBy(this Type exceptionType, MethodThatThrows method)
        {
            Exception exception = method.GetException();

            Assert.IsNotNull(exception);
            Assert.AreEqual(exceptionType, exception.GetType());

            return exception;
        }

        [DebuggerHidden]
        public static Exception Ignore(this Type exceptionType, MethodThatThrows method)
        {
            Exception exception = method.GetException();
            return exception;
        }

        [DebuggerHidden]
        public static Exception GetException(this MethodThatThrows method)
        {
            Exception exception = null;

            try
            {
                method();
            }
            catch (Exception e)
            {
                exception = e;
            }

            return exception;
        }
    }
}