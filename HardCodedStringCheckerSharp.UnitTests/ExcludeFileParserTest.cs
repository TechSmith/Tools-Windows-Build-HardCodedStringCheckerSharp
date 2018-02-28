using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HardCodedStringCheckerSharp.UnitTests
{
   [TestClass]
   public class ExcludeFileParserTest
   {
      [TestMethod]
      public void ParseExcludeFile_FileHasCommentsAndWhitespace_ReturnsCorrectResults()
      {
         // Arrange
         string folder1 = @"\Folder1\To\Exclude";
         string folder2 = @"\Folder2\To\Exclude";

         string[] excludeFileContents = new string[]
         {
            $"// Exclude file for C# Hard Coded String Checker",
            $"//",
            $"// The following directories will be excluded: ",
            $"    ",
            $"{folder1}",
            $"{folder2}  // in-line comment",
            $"  "
         };

         var mockFileSystem = new Mock<IFileSystem>();
         mockFileSystem.Setup( mfs => mfs.ReadAllLines( It.IsAny<string>() ) ).Returns( excludeFileContents );

         // Act
         var parser = new ExcludeFileParser( mockFileSystem.Object );
         List<string> exclusions = parser.ParseExcludeFile( "DoesNotMatter" );

         // Assert
         exclusions.Count.Should().Be( 2 );
         exclusions[0].Should().Be( folder1 );
         exclusions[1].Should().Be( folder2 );
      }

      [TestMethod]
      public void PrependRepoRootDir_ExclusionsHaveLeadingBackslash_CombinesPathsCorrectly()
      {
         // Arrange
         string repoRoot = @"C:\RepoRoot\";
         var exclusions = new List<string>
         {
            @"\Folder1\To\Exclude",  // The leading backslash changes the behavior of Path.Combine
            @"\Folder2\To\Exclude"
         };
         var expectedResult = new List<string>
         {
            @"C:\RepoRoot\Folder1\To\Exclude",
            @"C:\RepoRoot\Folder2\To\Exclude"
         };

         // Act
         var parser = new ExcludeFileParser( Mock.Of<IFileSystem>() );
         var result = parser.PrependRepoRootDir( repoRoot, exclusions );

        // Assert
         result.Should().Equal( expectedResult );
      }

      [TestMethod]
      public void PrependRepoRootDir_RepoRootAndExclusionsDoNotHaveLeadingBackslash_CombinesPathsCorrectly()
      {
         // Arrange
         string repoRoot = @"C:\RepoRoot";
         var exclusions = new List<string>
         {
            @"Folder1\To\Exclude",
            @"Folder2\To\Exclude"
         };
         var expectedResult = new List<string>
         {
            @"C:\RepoRoot\Folder1\To\Exclude",
            @"C:\RepoRoot\Folder2\To\Exclude"
         };

         // Act
         var parser = new ExcludeFileParser( Mock.Of<IFileSystem>() );
         var result = parser.PrependRepoRootDir( repoRoot, exclusions );

         // Assert
         result.Should().Equal( expectedResult );
      }
   }
}
