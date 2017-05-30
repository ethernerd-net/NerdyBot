using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using NerdyBot.Contracts;
using NerdyBot.Commands.Config;
using Discord.Commands;

namespace NerdyBot.Commands
{
  class Ball8 : ModuleBase
  {
    private MessageService svcMessage;
    private CommandConfig<Ball8Config> conf;

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public Ball8( MessageService svcMessage )
    {
      this.conf = new CommandConfig<Ball8Config>( "8ball" );
      this.svcMessage = svcMessage;
    }

    [Command( "8ball" ), Alias( "8b" )]
    public async Task Execute( params string[] args )
    {
      if ( args[0] == "add" )
      {
        string answer = string.Join( " ", args.Skip( 1 ) );
        this.conf.Ext.Items.Add( answer );
        this.conf.Write();
      }
      else if ( args[0] == "help" )
        this.svcMessage.SendMessage( Context, FullHelp(),
          new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
      else
      {
        int idx = ( new Random() ).Next( 0, this.conf.Ext.Items.Count() );
        this.svcMessage.SendMessage( Context, Context.User.Mention + " asked: '" + string.Join( " ", args ) +
          Environment.NewLine + Environment.NewLine +
          "My answer is: " + this.conf.Ext.Items[idx],
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      }
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== 8Ball ========" );
      sb.AppendLine();
      sb.AppendLine( "Magic 8Ball beantwortet dir jede GESCHLOSSENE Frage, die du an ihn richtest" );
      sb.AppendLine( "Key: 8ball" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: 8b" );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: 8ball [FRAGE]" );
      return sb.ToString();
    }
    #endregion ICommand
  }
}