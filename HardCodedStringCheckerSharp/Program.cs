namespace HardCodedStringCheckerSharp
{
   internal static class Program
   {
      private static int Main( string[] args )
      {
         var appController = new AppController();

         return appController.Main( args );
      }
   }
}
