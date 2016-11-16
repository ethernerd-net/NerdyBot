using Discord;
using System;
using System.Collections.Generic;
using NerdyBot.Commands.Config;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  class TranslateCommand : ICommand
  {
    private CommandConfig<Translate> cfg;
    private const string CFGPATH = "translate.json";
    private const string DEFAULTKEY = "translate";
    private static readonly string[] DEFAULTALIASES = new string[] { "g" };

    #region ICommand
    public string Key { get { return this.cfg.Key; } }
    public IEnumerable<string> Aliases { get { return this.cfg.Aliases; } }
    public bool NeedAdmin { get { return false; } }

    public void Init()
    {
      this.cfg = new CommandConfig<Translate>( CFGPATH, DEFAULTKEY, DEFAULTALIASES );
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        switch ( args[0].ToLower() )
        {
        case "create":
          if ( args.Count() >= 4 )
            Create( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "delete":
          if ( args.Count() == 2 )
            Delete( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "edit":
          if ( args.Count() >= 4 )
            Edit( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "info":
          if ( args.Count() >= 2 )
            Info( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "raw":
          if ( args.Count() >= 2 )
            Raw( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "list":
          List( msg, client );
          break;

        case "stop":
          stop = true;
          break;

        default:
          if ( args.Count() == 1 )
          {
            stop = false;
            Send( msg, args, client );
          }
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;
        }
      }, TaskCreationOptions.None );
    }
    #endregion ICommand
  }
}
