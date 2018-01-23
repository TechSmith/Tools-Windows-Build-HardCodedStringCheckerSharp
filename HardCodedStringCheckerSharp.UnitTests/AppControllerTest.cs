using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace HardCodedStringCheckerSharp.UnitTests
{
   [TestClass]
   public class AppControllerTest
   {
      [TestMethod]
      public void GetEncoding_NoByteOrderMarkerReturn_DefaultsToWindows1252()
      {
         var appController = new AppController( Mock.Of<IFileSystem>(), Mock.Of<IConsole>() );

         appController.GetEncoding( It.IsAny<string>() ).Should().Be( Encoding.GetEncoding( "Windows-1252" ) );
      }
   }
}
