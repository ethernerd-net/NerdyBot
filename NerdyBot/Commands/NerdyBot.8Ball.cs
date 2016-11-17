using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NerdyBot.Commands.Config;
using System.Text;

namespace NerdyBot.Commands
{
  class Ball8 : ICommand
  {
    private const string DEFAULTKEY = "8ball";
    private static readonly string[] DEFAULTALIASES = new string[] { "8b" };

    private CommandConfig<Ball8Config> conf;

    #region ICommand
    public BaseCommandConfig Config { get { return this.conf; } }

    public void Init()
    {
      this.conf = new CommandConfig<Ball8Config>( DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( args[0] == "add" && msg.User.ServerPermissions.Administrator )
        {
          string answer = string.Join( " ", args.Skip( 1 ) );
          this.conf.Ext.Items.Add( answer );
          this.conf.Write();
        }
        else if ( args[0] == "help" )
        {
          StringBuilder sb = new StringBuilder();
          sb.Append( QuickHelp() );
          sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
          sb.AppendLine();
          sb.AppendLine( "Beispiel: " + client.Config.Prefix + this.conf.Key + " [FRAGE]" );
          msg.User.SendMessage( "```" + sb.ToString() + "```" );
        }
        else
        {
          int idx = ( new Random() ).Next( 0, this.conf.Ext.Items.Count() );
          client.Write( msg.User.Mention + " asked: '" + string.Join( " ", args ) +
            Environment.NewLine + Environment.NewLine +
            "My answer is: " + this.conf.Ext.Items[idx], msg.Channel );
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== 8Ball ========" );
      sb.AppendLine();
      sb.AppendLine( "Magic 8Ball beantwortet dir jede GESCHLOSSENE Frage, die du an ihn richtest" );
      return sb.ToString();
    }
    #endregion ICommand
  }
}