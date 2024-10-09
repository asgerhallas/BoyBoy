using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using OneOf.Types;
using Shouldly;
using Xunit;

namespace BoyBoy.Tests
{
    public class OldBoyBoyTests
    {
        [Fact]
        public void AssertsVoidFunction()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1));

            Should.Throw<ShouldAssertException>(() => fake.VoidFunction(0));
        }

        [Fact]
        public void AssertsMultipleVoidFunctions()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1))
                .Call(x => x.Argument<int>(1).ShouldBe(2));

            fake.VoidFunction(1);

            Should.Throw<ShouldAssertException>(() => fake.VoidFunction(3));
        }

        [Fact]
        public void AssertsMultipleVoidFunctions2()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1))
                .Call(x => x.Argument<int>(1).ShouldBe(2));

            fake.VoidFunction(1);

            Should.NotThrow(() => fake.VoidFunction(2));
        }

        [Fact]
        public void AssertMissingCalls()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1))
                .Call(x => x.Argument<int>(1).ShouldBe(2));

            fake.VoidFunction(1);

            Should.Throw<ExpectationException>(() => fake.AllCallsMustHaveHappened())
                .Message.ShouldBe("Expected 2 calls to 'IInterface', but only 1 were made.");
        }

        [Fact]
        public void AssertTooManyCalls()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1));

            fake.VoidFunction(1);

            Should.Throw<ExpectationException>(() => fake.VoidFunction(1))
                .Message.ShouldBe("Did not expect call number 2 to 'IInterface'.");
        }

        [Fact]
        public void AssertFunction()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1));

            Should.Throw<ShouldAssertException>(() => fake.Functionit(2));
        }

        [Fact]
        public void ReturnValueFromFunction()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => 2);

            fake.Functionit(2).ShouldBe(2);
        }

        [Fact]
        public void ReturnValueFromMultipleFunctions()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => 2)
                .Call(x => 3)
                .Call(x => x.Argument(1));

            fake.Functionit(0).ShouldBe(2);
            fake.Functionit(0).ShouldBe(3);
            fake.Functionit(42).ShouldBe(42);
        }

        [Fact]
        public void ReturnFakedValueFromFunction()
        {
            var fake = A.Fake<IInterface>();

            fake.Call(x => x.Argument<int>(1).ShouldBe(1));

            Fake.GetFakeManager(fake.Functionit(1)).ShouldNotBe(null);
        }

        [Fact]
        public void GetPreviousReturnValueByType()
        {
            var fake = A.Fake<IInterface>();

            var returnValue = fake.Functionit(0);

            fake.ReturnValue<object>().ShouldBe(returnValue);
        }

        //[BoyBoy]
        public interface IInterface
        {
            object Functionit(int number);
            void VoidFunction(int number);
        }

        [Fact]
        public void FunFact()
        {
            var fake = A.Fake<Wrappee>(x => x.Implements<IWrappee>());

            A.CallTo(() => ((IWrappee)fake).GetAll()).Returns(new object[] {1});

            ((IWrappee)fake).GetAll().Single().ShouldBe(1);
        }

        public interface IWrappee 
        {
            IEnumerable<object> GetAll();
        }

        public class Wrappee : IWrappee
        {
            readonly List<object> objects = new();

            public void Add(object obj) => objects.Add(obj);

            public IEnumerable<object> GetAll() => objects;
        }
    }
}
