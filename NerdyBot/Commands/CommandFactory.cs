using System.Collections.Generic;

namespace NerdyBot.Commands
{
  static class CommandFactory
  {
    private static List<ICommand> commandContainer = new List<ICommand>();
    public static ICommand GetCommand<T>() where T : class, ICommand, new()
    {
      ICommand command = null;
      if ( ( command = Get<T>() ) == null )
      {
        command = new T();
        commandContainer.Add( command );
      }
      return command;
    }

    private static ICommand Get<T>()
    {
      foreach ( ICommand cmd in commandContainer )
        if ( cmd is T )
          return cmd;
      return null;
    }
  }
}
