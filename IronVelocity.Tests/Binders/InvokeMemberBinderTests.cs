﻿using IronVelocity.Binders;
using NUnit.Framework;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using VelocityExpressionTree.Binders;
using System.Linq;

namespace IronVelocity.Tests.Binders
{
    public class InvokeMemberBinderTests
    {
        // Overloading rules http://csharpindepth.com/Articles/General/Overloading.aspx
        // http://msdn.microsoft.com/en-us/library/aa691336(v=vs.71).aspx

        [Test]
        public void NullInput()
        {
            object input = null;
            var result = test(input, "DoStuff");
            Assert.Null(result);
        }



        [Test]
        public void MethodOnPrimative()
        {
            var input = 472;
            var result = test(input, "ToString");
            Assert.AreEqual("472", result);
        }

        [Test]
        public void PrivateMethod()
        {
            object input = null;
            var result = test(input, "TopSecret");
            Assert.Null(result);
        }


        [Test]
        public void MethodNameNotExist()
        {
            object input = null;
            var result = test(input, "DoStuff");
            Assert.Null(result);
        }

        [Test]
        public void MethodNameExactMatch()
        {
            var input = new MethodTests();
            var result = test(input, "StringResult");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void BasicMethodDiffersInCase()
        {
            var input = new MethodTests();
            var result = test(input, "stringRESULT");
            Assert.AreEqual("hello world", result);
        }

        [Test]
        public void MethodNamePartialMatch()
        {
            object input = null;
            var result = test(input, "StringRes");
            Assert.Null(result);
        }

        [Test]
        public void MethodOneParamaterExactTypeMatch()
        {
            var target = new MethodTests();
            var param1 = new object();
            var result = test(target, "OneParameter", param1);

            Assert.AreEqual("One Param Success", result);
        }

        [Test]
        public void MethodOneParamaterAssignableTypeMatch()
        {
            var target = new MethodTests();
            var param1 = "hello world";
            var result = test(target, "OneParameter", param1);

            Assert.AreEqual("One Param Success", result);
        }

        //null input returns null
        //Void returns null??

        private object test(object input, string methodName, params object[] paramaters)
        {
            var binder = new VelocityInvokeMemberBinder(methodName, new CallInfo(paramaters.Length));

            var argExpressions = new Expression[] { Expression.Constant(input) }
                .Concat(paramaters.Select(x => Expression.Constant(x)));

            var value = Expression.Constant(input);
            var expression = Expression.Dynamic(binder, typeof(object), argExpressions);

            var action = Expression.Lambda<Func<object>>(expression)
                .Compile();

            return action();
        }

        public class AmbigiousMethods
        {
            public int Ambigious(string param1, object param2)
            {
                return -1;
            }
            public string Ambigious(object param1, string param2)
            {
                return "fail";
            }
            public float Ambigious(object param1, object param2)
            {
                return 0.5f;
            }
        }

        #region Suitability
        [Test]
        public void Suitability_Object()
        {
            SuitabilityTests<object>(-1);
        }
        [Test]
        public void Suitability_Parent()
        {
            SuitabilityTests<Parent>("Failure");
        }
        [Test]
        public void Suitability_Child()
        {
            SuitabilityTests<Child>(true);
        }

        [Test]
        public void Suitability_Son()
        {
            SuitabilityTests<Son>(Guid.Empty);
        }

        [Test]
        public void Suitability_Daughter()
        {
            SuitabilityTests<Daughter>(true);
        }

        public void SuitabilityTests<T>(object expectedResult)
            where T : new()
        {
            var input = new Suitability();
            var result = test(input, "Overload", new T());

            Assert.AreEqual(expectedResult, result);
        }

        public class Suitability
        {
            public int Overload(object param) { return -1; }
            public string Overload(Parent param) { return "Failure"; }
            public bool Overload(Child param) { return true; }
            public Guid Overload(Son param) { return Guid.Empty; }
        }

        #endregion

        public class MethodTests
        {
            public string StringResult() { return "hello world"; }

            private string TopSecret() { return "The password is ********"; }

            public object OneParameter(object param)
            {
                return "One Param Success";
            }
        }

        public class Parent { }
        public class Child : Parent { }
        public class Son : Child { }
        public class Daughter : Child { }

    }
}
