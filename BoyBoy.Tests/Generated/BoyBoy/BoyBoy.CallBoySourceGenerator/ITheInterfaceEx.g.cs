using FakeItEasy;

namespace BoyBoyFiles;

public static class ITheInterfaceEx 
{
   public static void Call_MyFunction(this BoyBoy.Tests.ITheInterface self, System.Func<System.Int32, System.String, BoyBoy.Tests.OldBoyBoyTests.IInterface, System.Object> call) {
       FakeItEasy.A.CallTo(self)
           .WithReturnType<System.Object>()
           .WhenArgumentsMatch(collection => 
               collection[0] is System.Int32 && collection[1] is System.String && collection[2] is BoyBoy.Tests.OldBoyBoyTests.IInterface
           )
           .ReturnsLazily(x => call(
               x.Arguments.Get<System.Int32>(0), x.Arguments.Get<System.String>(1), x.Arguments.Get<BoyBoy.Tests.OldBoyBoyTests.IInterface>(2)
           ));
   
   }
   public static void Call_VoidFunction(this BoyBoy.Tests.ITheInterface self, System.Action<System.Int32> call) {
       FakeItEasy.A.CallTo(self)
           
           .WhenArgumentsMatch(collection => 
               collection[0] is System.Int32
           )
           .Invokes(x => call(
               x.Arguments.Get<System.Int32>(0)
           ));
   
   }}