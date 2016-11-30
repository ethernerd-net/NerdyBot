using System;

namespace NerdyBot
{
  class Program
  {
    static void Main( string[] args )
    {
      try
      {
        ( new NerdyBot() ).Start();
      }
      catch ( Exception )
      {
        //ja
      }
    }
  }
}