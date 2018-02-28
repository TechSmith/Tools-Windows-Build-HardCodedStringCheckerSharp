using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HardCodedStringCheckerSharp.UnitTests
{
   [TestClass]
   public class AppControllerTest
   {
      private readonly List<string> _noExclusions = new List<string>();

      [TestMethod]
      public void ShouldProcessFile_FileIsAssemblyInfo_Ignores()
      {
         const string assemblyInfo = "AssemblyInfo.cs";
         AppController.ShouldProcessFile( assemblyInfo, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsCurrentVersion_Ignores()
      {
         const string currentVersion = "CurrentVersion.cs";
         AppController.ShouldProcessFile( currentVersion, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsDesignerFile_Ignores()
      {
         const string designerFile = ".Designer.cs";
         AppController.ShouldProcessFile( designerFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsFeatureFile_Ignores()
      {
         const string designerFile = ".Designer.cs";
         AppController.ShouldProcessFile( designerFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsPackagesFile_Ignores()
      {
         const string packages = "packages";
         AppController.ShouldProcessFile( packages, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTemporaryGeneratedFile_Ignores()
      {
         const string temporaryGeneratedFile = "TemporaryGeneratedFile";
         AppController.ShouldProcessFile( temporaryGeneratedFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsiFile_Ignores()
      {
         const string iFile = ".i.";
         AppController.ShouldProcessFile( iFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsgFile_Ignores()
      {
         const string gFile = ".g.";
         AppController.ShouldProcessFile( gFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTestFile_Ignores()
      {
         const string testFile = "SomethingTest.cs";
         AppController.ShouldProcessFile( testFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTestsFile_Ignores()
      {
         const string testsFile = "SomethingTests.cs";
         AppController.ShouldProcessFile( testsFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsStepsFile_Ignores()
      {
         const string stepsFile = "Steps.cs";
         AppController.ShouldProcessFile( stepsFile, _noExclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileIsTypicalSourceFile_Processes()
      {
         const string file = "SomeFile.cs";
         AppController.ShouldProcessFile( file, _noExclusions ).Should().BeTrue();
      }

      [TestMethod]
      public void ShouldProcessFile_FileInExcludedFolder_ReturnsFalse()
      {
         var exclusions = new List<string> { @"C:\RepoRoot\ExcludedFolder" };
         const string file = @"C:\RepoRoot\ExcludedFolder\SomeFile.cs";
         AppController.ShouldProcessFile( file, exclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileInSubFolderOfExcludedFolder_ReturnsFalse()
      {
         var exclusions = new List<string> { @"C:\RepoRoot\ExcludedFolder" };
         const string file = @"C:\RepoRoot\ExcludedFolder\Subfolder\SomeFile.cs";
         AppController.ShouldProcessFile( file, exclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileExactMatchInExclusionsList_ReturnsFalse()
      {
         const string file = @"C:\RepoRoot\SomeFolder\SomeFile.cs";
         var exclusions = new List<string> { file };
         AppController.ShouldProcessFile( file, exclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileInExclusionsListWithDifferentCase_ReturnsFalse()
      {
         const string file = @"C:\RepoRoot\SomeFolder\SomeFile.cs";
         var exclusions = new List<string> { @"c:\reporoot\somefolder\somefile.cs" };
         AppController.ShouldProcessFile( file, exclusions ).Should().BeFalse();
      }

      [TestMethod]
      public void ShouldProcessFile_FileNotInExcludedFolder_ReturnsTrue()
      {
         var exclusions = new List<string> { @"C:\RepoRoot\ExcludedFolder" };
         const string file = @"C:\RepoRoot\SomeFolder\SomeFile.cs";
         AppController.ShouldProcessFile( file, exclusions ).Should().BeTrue();
      }

      [TestMethod]
      public void GetEncoding_NoByteOrderMarkerReturn_DefaultsToWindows1252()
      {
         var appController = new AppController( Mock.Of<IFileSystem>(), Mock.Of<IConsole>(), Mock.Of<ICommandLineParser>(), Mock.Of<IExcludeFileParser>() );

         appController.GetEncoding( It.IsAny<string>() ).Should().Be( Encoding.GetEncoding( "Windows-1252" ) );
      }
   }
}
