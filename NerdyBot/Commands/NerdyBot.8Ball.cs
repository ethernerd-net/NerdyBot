using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NerdyBot.Commands.Config;
using System.Text;
using System.Collections.Generic;
using NerdyBot.Contracts;

namespace NerdyBot.Commands
{
  class Ball8 : ICommand
  {
    private CommandConfig<Ball8Config> conf;
    private const string DEFAULTKEY = "8ball";
    private static readonly IEnumerable<string> DEFAULTALIASES = new string[] { "8b" };

    private IClient client;

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public void Init( IClient client )
    {
      this.conf = new CommandConfig<Ball8Config>( DEFAULTKEY, DEFAULTALIASES.ToArray() );
      this.conf.Read();
      this.client = client;
    }

    public Task Execute( ICommandMessage msg )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( msg.Arguments[0] == "add" && msg.User.Permissions.Administrator )
        {
          string answer = string.Join( " ", msg.Arguments.Skip( 1 ) );
          this.conf.Ext.Items.Add( answer );
          this.conf.Write();
        }
        else if ( msg.Arguments[0] == "help" )
          this.client.SendMessage( FullHelp(), 
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = msg.User.Id, MessageType = MessageType.Block } );
        else
        {
          int idx = ( new Random() ).Next( 0, this.conf.Ext.Items.Count() );
          this.client.SendMessage( msg.User.Mention + " asked: '" + string.Join( " ", msg.Arguments ) +
            Environment.NewLine + Environment.NewLine +
            "My answer is: " + this.conf.Ext.Items[idx],
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== 8Ball ========" );
      sb.AppendLine();
      sb.AppendLine( "Magic 8Ball beantwortet dir jede GESCHLOSSENE Frage, die du an ihn richtest" );
      sb.AppendLine( "Key: " + this.conf.Key );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: " + this.conf.Key + " [FRAGE]" );
      return sb.ToString();
    }
    #endregion ICommand
  }
}