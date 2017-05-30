using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;

using NerdyBot.Contracts;
using NerdyBot.Config;

namespace NerdyBot.Commands
{
  [Group( "8ball" ), Alias( "8b" )]
  public class Ball8 : ModuleBase
  {
    public MessageService MessageService { get; set; }

    private ModuleConfig<Ball8Config> conf;

    public Ball8()
    {
      this.conf = new ModuleConfig<Ball8Config>( "8ball" );
      this.conf.Read();
    }

    [Command( "add" )]
    public async Task Add( params string[] content )
    {
      string answer = string.Join( " ", content );
      this.conf.Ext.Items.Add( answer );
      this.conf.Write();
    }

    [Command( "help" )]
    public async Task Help()
    {
      MessageService.SendMessage( Context, FullHelp(),
        new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
    }

    [Command()]
    public async Task Execute( params string[] question )
    {
      int idx = ( new Random() ).Next( 0, this.conf.Ext.Items.Count() );
      MessageService.SendMessage( Context, Context.User.Mention + " asked: '" + string.Join( " ", question ) +
        Environment.NewLine + Environment.NewLine +
        "My answer is: " + this.conf.Ext.Items[idx],
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
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
  }
}