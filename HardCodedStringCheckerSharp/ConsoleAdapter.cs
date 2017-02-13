using System;

namespace HardCodedStringCheckerSharp
{
   public class ConsoleAdapter : IConsole
   {
      public void WriteLine( string value ) => Console.WriteLine( value );
   }
}
