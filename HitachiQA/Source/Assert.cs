using System;
using System.Collections.Generic;
using System.Text;
using HitachiQA.Helpers;
using HitachiQA.Driver;
using NUnit.Framework;
using NUnitAssert = NUnit.Framework.Assert;
namespace HitachiQA
{
    [Obsolete("please use FluentAssertions")]
    public class Assert
    {
        public static bool Contains(List<String> _object, string value, bool optional = false)
        {
            var list = new StringBuilder();
            list.Append("[\n");
            foreach (var item in _object)
            {
                list.Append(item).Append(", \n");
            }
            list.Append("]");

            if (_object.Contains(value))
            {
                success($"List [{list.ToString()}] Contains [{value}]");
                return true;
            }
            else
            {
                Functions.handleFailure(new Exception($"List { list.ToString() } does not Contain\n[{ value}]"));
                return false;
            }
        }

        public static bool TextContains(String text, String value, bool optional = false)
        {
            if(text== null || value == null)
            {
                Functions.handleFailure(new Exception($"Assert - text: [{text}] does not Contain  value: [{value}]"), optional);
                return false;
            }
            if(text.Contains(value))
            {
                success($"text: {text} contains value: {value}");
                return true;
            }
            Functions.handleFailure(new Exception($"Assert - text: [{text}] does not Contain  value: [{value}]"), optional);
            return false;
        }

        public static bool SoftAreEqual(object A, object B)
        {
            return AreEqual(A, B, true);
        }

        public static bool AreEqual(object A, object B, bool optional = false)
        {
            if(    (A == null || A is string && (string)A == "")     &&      (B == null || B is string && (string)B=="")     )
            {
                success($"Object {A} equals {B}");
                return true;
            }
            else if(A == null || B==null)
            {
                Functions.handleFailure(new Exception($"Assert - type:[{A?.GetType()?.FullName}] value:[{A}]   does not Equal  type: [{A?.GetType()?.FullName}] value: [{B}]"), optional);
                return false;
            }
            else if (A.Equals(B))
            {
                success($"Object {A} equals {B}");
                return true;
            }
            else
            {
                Functions.handleFailure(new Exception($"Assert - type:[{A?.GetType()?.FullName}] value:[{A}]   does not Equal  type: [{A?.GetType()?.FullName}] value: [{B}]"), optional);
                return false;
            }
        }

        public static bool SoftAreEqual(IEnumerable<String> expected, IEnumerable<String> actual)
        {
            return AreEqual(expected, actual, true);
        }

        public static bool AreEqual(IEnumerable<String> expected, IEnumerable<String> actual, bool optional = false)
        {
            (string key, dynamic value)[] parameters = new (string key, dynamic value)[] { ("@expected", expected), ("@actual", actual) };

            if (expected == null && actual == null)
            {
                success("Are Equal - Expected and Actual equal null");
                return true;
            }
            else if (expected == null || actual == null)
            {
                Functions.handleFailure(new Exception($"Assert - type:[{actual?.GetType()?.FullName}] value:[{(expected == null ? "null" : $"{expected}")}]   does not Equal  type: [{actual?.GetType()?.FullName}] value: [{(actual == null ? "null" : $"{actual}")}]"), optional);
                return false;
            }
            else
            {
                try
                {
                    CollectionAssert.AreEqual(expected, actual);
                    success("Are Equal - @expected & @actual", parameters);
                    return true;
                }
                catch (Exception ex)
                {
                    Functions.handleFailure("List @expected does not equal @actual", ex, optional, parameters);
                    return false;
                }
            }
        }

        public static bool AreNotEqual(object A, object B, bool optional = false)
        {
            if (!A.Equals(B))
            {
                success($"Object {A} does not equal {B}");
                return true;
            }
            else
            {
                Functions.handleFailure(new Exception($"Assert - type:[{A?.GetType()?.FullName}] value:[{A}]   Equals  type: [{A?.GetType()?.FullName}] value: [{B}]"), optional);
                return false;
            }
        }

        /// <summary>
        /// checks whether the given condition was false,<br></br>
        /// </summary>
        /// <returns> will return true if the condition was true</returns>
        public static bool IsTrue(bool? condition, bool optional = false)
        {
            if(condition.HasValue && condition.Value == true)
            {
                success($"Condition {condition}");
                return true;
            }
            else
            {
                Functions.handleFailure(new Exception($"Assert is True - for given condition: {condition} was not true"), optional);
                return false;
            }
        }

        /// <summary>
        /// checks whether the given condition was false,<br></br>
        /// </summary>
        /// <returns> will return true if the condition equals false or null</returns>
        public static bool IsFalse(bool? condition, bool optional = false)
        {
            if (condition.HasValue && condition.Value == true)
            {
                Functions.handleFailure(new Exception($"Assert is False - the given condition: {condition} was not false"), optional);
                return false;
            }
            else
            {
                success($"Condition {condition}");
                return true;
            }
        }

        public static bool IsNull(object _object, bool optional = false)
        {
            if (_object == null)
            {
                success("Object is null");
                return true;
            }
            else
            {
                Functions.handleFailure(new Exception($"Assert is Null - the given Object: {_object} was not null"), optional);
                return false;
            }
        }

        public static bool IsNotNull(object _object, bool optional = false)
        {
            if (_object != null)
            {
                success("Object is not null");
                return true;
            }
            else
            {
                Functions.handleFailure(new Exception($"Assert is Not Null - the given Object: {_object} was null"), optional);
                return false;
            }
        }

        public static bool CurrentURLEquals(string URL, bool optional=false)
        {
            throw new NotImplementedException("this funciton shouldn't live here");
            //try
            //{
            //    NUnitAssert.That(() => UserActions.GetCurrentURL(), Is.EqualTo(URL).After(10).Seconds.PollEvery(250).MilliSeconds);
            //    success($"URL {URL} is the current URL");
            //    return true;
            //}
            //catch(Exception ex)
            //{
            //    Functions.handleFailure(ex, optional);
            //    return false;
            //}
        }

        public static bool CurrentURLContains(string URL, bool optional = false)
        {
            throw new NotImplementedException("this function shouldn't live here");
            //try
            //{
            //    NUnitAssert.That(() => UserActions.GetCurrentURL(), Does.Contain(URL).After(15).Seconds.PollEvery(250).MilliSeconds);
            //    success($"URL {URL} is the current URL");
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    Functions.handleFailure(ex, optional);
            //    return false;
            //}
        }

        private static void success(String message, params (string key, dynamic value)[] parameters)
        {
            Log.Info("Success - Assert: " + message, parameters);
        }
    }
}
