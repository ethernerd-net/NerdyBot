using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace NerdyBot.Services
{
  public class MessageService
  {
    public MessageService( DiscordSocketClient discordClient )
    {
      discordClient.Log += ClientLog;
    }

    private async Task ClientLog( LogMessage arg )
    {
      Console.WriteLine( arg.ToString() );
    }

    public async Task SendMessageToCurrentChannel( ICommandContext context, string message, MessageType mType = MessageType.Normal, bool split = false, string highlight = "" )
    {
      await SendMessage( context.Channel, message, mType, split, highlight );
    }
    public async Task SendMessageToCurrentUser( ICommandContext context, string message, MessageType mType = MessageType.Normal, bool split = false, string highlight = "" )
    {
      var usr = await context.Guild.GetUserAsync( context.User.Id );
      await SendMessage( await usr.CreateDMChannelAsync(), message, mType, split, highlight );
    }    

    public async Task Log( string text, string source = "", LogSeverity logLevel = LogSeverity.Info )
    {
      await ClientLog( new LogMessage( logLevel, source, text ) );
    }

    private readonly int chunkSize = 1950;
    private IEnumerable<string> ChunkMessage( string str )
    {
      if ( str.Length <= chunkSize )
        return new string[] { str };
      else
      {
        var lines = str.Split( new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );
        if ( lines.Count() > 1 )
        {
          var chunkendText = new List<string>();
          string chunk = string.Empty;
          foreach ( var line in lines )
            if ( chunk.Length + line.Length > chunkSize )
            {
              chunkendText.Add( chunk );
              chunk = line + Environment.NewLine;
            }
            else
              chunk += line + Environment.NewLine;
          chunkendText.Add( chunk );
          return chunkendText;
        }
        else
          return Enumerable.Range( 0, ( int )Math.Ceiling( ( double )str.Length / ( double )chunkSize ) )
            .Select( i => str.Substring( i * chunkSize, ( i * chunkSize + chunkSize > str.Length ? str.Length - i * chunkSize : chunkSize ) ) );
      }
    }
    private string FormatMessage( string message, MessageType format, string highlight )
    {
      string formatedMessage = string.Empty;
      switch ( format )
      {
      case MessageType.Block:
        formatedMessage = $"```{highlight}{Environment.NewLine}{message}```";
        break;
      case MessageType.Info:
        formatedMessage = $"`{message}`";
        break;
      case MessageType.Normal:
      default:
        formatedMessage = message;
        break;
      }
      return formatedMessage;
    }
    private async Task SendMessage( IMessageChannel channel, string message, MessageType mType = MessageType.Normal, bool split = false, string highlight = "" )
    {
      if ( !split && message.Length > chunkSize )
      {
        string fn = Guid.NewGuid().ToString();
        File.WriteAllText( fn, message );
        await channel.SendFileAsync( fn );
        File.Delete( fn );
      }
      else
      {
        foreach ( string msg in ChunkMessage( message ) )
          await channel.SendMessageAsync( FormatMessage( msg, mType, highlight ) );
      }
    }
  }
  public enum MessageType { Normal, Block, Info }
}
