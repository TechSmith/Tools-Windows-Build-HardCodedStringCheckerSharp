using System;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HardCodedStringCheckerSharp.UnitTests
{
   [TestClass]
   public class AppControllerTest
   {
      [TestMethod]
      public void ShouldProcessFile_FileIsAssemblyInfo_Ignores()
      {
         const string assemblyInfo = "AssemblyInfo.cs";
         AppController.ShouldProcessFile( assemblyInfo ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsCurrentVersion_Ignores()
      {
         const string currentVersion = "CurrentVersion.cs";
         AppController.ShouldProcessFile( currentVersion ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsDesignerFile_Ignores()
      {
         const string designerFile = ".Designer.cs";
         AppController.ShouldProcessFile( designerFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsFeatureFile_Ignores()
      {
         const string designerFile = ".Designer.cs";
         AppController.ShouldProcessFile( designerFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsPackagesFile_Ignores()
      {
         const string packages = "packages";
         AppController.ShouldProcessFile( packages ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTemporaryGeneratedFile_Ignores()
      {
         const string temporaryGeneratedFile = "TemporaryGeneratedFile";
         AppController.ShouldProcessFile( temporaryGeneratedFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsiFile_Ignores()
      {
         const string iFile = ".i.";
         AppController.ShouldProcessFile( iFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsgFile_Ignores()
      {
         const string gFile = ".g.";
         AppController.ShouldProcessFile( gFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTestFile_Ignores()
      {
         const string testFile = "SomethingTest.cs";
         AppController.ShouldProcessFile( testFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTestsFile_Ignores()
      {
         const string testsFile = "SomethingTests.cs";
         AppController.ShouldProcessFile( testsFile ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTypicalSourceFile_Processes()
      {
         const string file = "SomeFile.cs";
         AppController.ShouldProcessFile( file ).Should().BeTrue();
      }

      [TestMethod]
      public void GetEncoding_NoByteOrderMarkerReturn_DefaultsToWindows1252()
      {
         var appController = new AppController( Mock.Of<IFileSystem>(), Mock.Of<IConsole>() );

         appController.GetEncoding( It.IsAny<string>() ).Should().Be( Encoding.GetEncoding( "Windows-1252" ) );
      }
   }
}
