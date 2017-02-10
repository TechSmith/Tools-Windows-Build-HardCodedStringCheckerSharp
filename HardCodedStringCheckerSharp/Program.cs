namespace HardCodedStringCheckerSharp
{
   internal static class Program
   {
      private static int Main( string[] args )
      {
         var fileSystem = new FileSystem();
         var consoleAdapter = new ConsoleAdapter();

         var appController = new AppController( fileSystem, consoleAdapter );
         return appController.Main( args );
      }
   }
}
