using BoyBoyFiles;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace BoyBoy.Tests
{
    public class BoyBoyTests
    {
        [Fact]
        public void CallFunction_AssertionFails()
        {
            var fake = A.Fake<ITheInterface>();

            fake.Call_VoidFunction(x => x.ShouldBe(2));

            Should.Throw<ShouldAssertException>(() => fake.MyFunction(1, "test", A.Fake<OldBoyBoyTests.IInterface>()));
        }

        [Fact]
        public void CallFunction_ReturnValue()
        {
            var fake = A.Fake<ITheInterface>();

            fake.Call_MyFunction((x, y, z) => $"hallo{y}");

            var result = fake.MyFunction(1, "test", A.Fake<OldBoyBoyTests.IInterface>());

            result.ShouldBe("hallotest");
        }

        [Fact]
        public void CallFunction_NestedInterface_ReturnValue()
        {
            var fake = A.Fake<ITheNestedInterface>();

            fake.Call_Functionit(x => $"hallo{x}");

            var result = fake.Functionit(1);

            result.ShouldBe("hallo1");
        }

        [BoyBoy]
        public interface ITheNestedInterface
        {
            object Functionit(int number);
            void VoidFunction(int number);
        }
    }
}