using FakeItEasy;

namespace BoyBoyFiles;

public static class IInterfaceEx 
{
   public static void Call_Functionit(this BoyBoy.Tests.OldBoyBoyTests.IInterface self, System.Func<System.Int32, System.Object> call) {
       FakeItEasy.A.CallTo(self)
           .WithReturnType<System.Object>()
           .WhenArgumentsMatch(collection => 
               collection[0] is System.Int32
           )
           .ReturnsLazily(x => call(
               x.Arguments.Get<System.Int32>(0)
           ));
   
   }
   public static void Call_VoidFunction(this BoyBoy.Tests.OldBoyBoyTests.IInterface self, System.Action<System.Int32> call) {
       FakeItEasy.A.CallTo(self)
           
           .WhenArgumentsMatch(collection => 
               collection[0] is System.Int32
           )
           .Invokes(x => call(
               x.Arguments.Get<System.Int32>(0)
           ));
   
   }}