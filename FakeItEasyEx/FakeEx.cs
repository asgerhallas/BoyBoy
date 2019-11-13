using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FakeItEasy.Configuration;
using FakeItEasy.Core;
using OneOf;

namespace FakeItEasyEx
{
    public static class FakeEx
    {
        public static object Call(this object fake, string methodName, Action<Asserter> expect) =>
            ExpectCall(fake, (Action<Asserter>)(x =>
            {
                if (x.Method.Name != methodName)
                {
                    throw new ExpectationException($"Expected a call to '{methodName}', but '{x.Method.Name}' was called instead.");
                }

                expect(x);
            }));

        public static object Call(this object fake, string methodName, Func<Asserter, object> expect) =>
            ExpectCall(fake, (Func<Asserter, object>)(x =>
            {
                if (x.Method.Name != methodName)
                {
                    throw new ExpectationException($"Expected a call to '{methodName}', but '{x.Method.Name}' was called instead.");
                }

                return expect(x);
            }));

        public static object Call(this object fake, Action<Asserter> expect) => ExpectCall(fake, expect);

        public static object Call(this object fake, Func<Asserter, object> expect) => ExpectCall(fake, expect);

        static object ExpectCall(object fake, ExpectatedCall expect)
        {
            var fakeManager = Fake.GetFakeManager(fake);

            var rule = fakeManager.Rules.OfType<AssertCallRule>().SingleOrDefault();

            if (rule == null)
            {
                fakeManager.AddRuleFirst(new AssertCallRule(expect));
            }
            else
            {
                rule.ExpectCall(expect);
            }

            return fake;
        }

        public static object AllCallsMustHaveHappened(this object fake)
        {
            var fakeManager = Fake.GetFakeManager(fake);

            var rule = fakeManager.Rules.OfType<AssertCallRule>().SingleOrDefault();

            if (rule == null)
            {
                throw new InvalidOperationException("No calls were configured with fake.Call().");
            }

            if (rule.ActualCalls.Count == rule.ExpectedCalls.Count) return fake;

            throw new ExpectationException($"Expected {rule.ExpectedCalls.Count} calls to '{Fake.GetFakeManager(fake).FakeObjectType.Name}', but only {rule.ActualCalls.Count} were made.");
        }

        public class AssertCallRule : IFakeObjectCallRule
        {
            public AssertCallRule(ExpectatedCall assert) => ExpectedCalls.Add(assert);

            public List<ExpectatedCall> ExpectedCalls { get; } = new List<ExpectatedCall>();

            public List<IFakeObjectCall> ActualCalls { get; } = new List<IFakeObjectCall>();

            public void ExpectCall(ExpectatedCall expect) => ExpectedCalls.Add(expect);

            public bool IsApplicableTo(IFakeObjectCall fakeObjectCall)
            {
                if (fakeObjectCall.Method.DeclaringType == typeof(FakeEx)) return false;

                if (ActualCalls.Contains(fakeObjectCall)) return false;

                ActualCalls.Add(fakeObjectCall);

                if (ActualCalls.Count > ExpectedCalls.Count)
                {
                    throw new ExpectationException($"Did not expect call number {ActualCalls.Count} to '{Fake.GetFakeManager(fakeObjectCall.FakedObject).FakeObjectType.Name}'.");
                }

                var expectedCall = ExpectedCalls[ActualCalls.Count - 1];

                var isApplicableTo = expectedCall.Match(
                    func => true,
                    action =>
                    {
                        action(new Asserter(fakeObjectCall));
                        return false;
                    });

                return isApplicableTo;
            }

            public void Apply(IInterceptedFakeObjectCall fakeObjectCall)
            {
                var expectedCall = ExpectedCalls[ActualCalls.Count - 1].AsT0;

                fakeObjectCall.SetReturnValue(expectedCall(new Asserter(fakeObjectCall)));
            }

            public int? NumberOfTimesToCall => null;

        }

        public class ExpectatedCall : OneOfBase<Func<Asserter, object>, Action<Asserter>>
        {
            public ExpectatedCall(Func<Asserter, object> func) : base(0, value0: func) { }
            public ExpectatedCall(Action<Asserter> action) : base(1, value1: action) { }

            public static implicit operator ExpectatedCall(Func<Asserter, object> value) => new ExpectatedCall(value);
            public static implicit operator ExpectatedCall(Action<Asserter> value) => new ExpectatedCall(value);
        }

        public class Asserter
        {
            readonly IFakeObjectCall call;

            public Asserter(IFakeObjectCall call) => this.call = call;

            public MethodInfo Method => call.Method;
            public object Argument(int index) => call.Arguments.Get<object>(index - 1);
            public T Argument<T>(int index) => call.Arguments.Get<T>(index - 1);
            public ArgumentCollection Arguments => call.Arguments;
        }
    }
}
