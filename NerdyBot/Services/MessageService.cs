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

    public async Task SendMessage( ICommandContext context, string message, SendMessageOptions options )
    {
      switch ( options.TargetType )
      {
      case TargetType.User:
        var usr = await context.Guild.GetUserAsync( context.User.Id );
        var dmcChannel = await usr.CreateDMChannelAsync();
        if ( !options.Split && message.Length > 1990 )
        {
          File.WriteAllText( context.User.Id + "_raw.txt", message );
          await dmcChannel.SendFileAsync( context.User.Id + "_raw.txt" );
          File.Delete( context.User.Id + "_raw.txt" );
        }
        else
        {
          foreach ( string msg in ChunkMessage( message ) )
            await dmcChannel.SendMessageAsync( FormatMessage( msg, options.MessageType, options.Hightlight ) );
        }
        break;
      case TargetType.Channel:
        {
          var channel = await context.Guild.GetTextChannelAsync( options.TargetId );
          if ( !options.Split && message.Length > 1990 )
          {
            File.WriteAllText( context.User.Id + "_raw.txt", message );
            await channel.SendFileAsync( context.User.Id + "_raw.txt" );
            File.Delete( context.User.Id + "_raw.txt" );
          }
          else
          {
            foreach ( string msg in ChunkMessage( message ) )
              await channel.SendMessageAsync( FormatMessage( msg, options.MessageType, options.Hightlight ) );
          }
        }
        break;
      default:
        throw new Exception( "WTF?!!?" );
      }
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
  }

  public class SendMessageOptions
  {
    public ulong TargetId { get; set; }
    public TargetType TargetType { get; set; }
    public bool Split { get; set; }
    public MessageType MessageType { get; set; }
    public string Hightlight { get; set; }
  }
  public enum TargetType { User = 0, Channel = 1 }
  public enum MessageType { Normal, Block, Info }
}
