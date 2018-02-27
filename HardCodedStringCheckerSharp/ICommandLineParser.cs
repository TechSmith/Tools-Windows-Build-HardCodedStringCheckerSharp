namespace HardCodedStringCheckerSharp
{
   public interface ICommandLineParser
   {
      CommandLineOptions ParseCommandLine( string[] args );
   }
}
