using FakeItEasy;

namespace BoyBoyFiles;

public static class ITheNestedInterfaceEx 
{
   public static void Call_Functionit(this BoyBoy.Tests.BoyBoyTests.ITheNestedInterface self, System.Func<System.Int32, System.Object> call) {
       FakeItEasy.A.CallTo(self)
           .WithReturnType<System.Object>()
           .WhenArgumentsMatch(collection => 
               collection[0] is System.Int32
           )
           .ReturnsLazily(x => call(
               x.Arguments.Get<System.Int32>(0)
           ));
   
   }
   public static void Call_VoidFunction(this BoyBoy.Tests.BoyBoyTests.ITheNestedInterface self, System.Action<System.Int32> call) {
       FakeItEasy.A.CallTo(self)
           
           .WhenArgumentsMatch(collection => 
               collection[0] is System.Int32
           )
           .Invokes(x => call(
               x.Arguments.Get<System.Int32>(0)
           ));
   
   }}