using System;

namespace NerdyBot.Helper
{
  public static class EventHandlerExtensions
  {
    public static void SafeInvoke<T>( this EventHandler<T> evt, object sender, T e ) where T : EventArgs
    {
      if ( evt != null )
      {
        evt( sender, e );
      }
    }
    public static void SafeInvoke( this EventHandler evt, object sender, EventArgs e )
    {
      if ( evt != null )
      {
        evt( sender, e );
      }
    }
  }
}
