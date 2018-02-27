namespace HardCodedStringCheckerSharp
{
   internal static class Program
   {
      private static int Main( string[] args )
      {
         var fileSystem = new FileSystem();
         var consoleAdapter = new ConsoleAdapter();
         var commandLineParser = new CommandLineParser();

         var appController = new AppController( fileSystem, consoleAdapter, commandLineParser );
         return appController.Main( args );
      }
   }
}
