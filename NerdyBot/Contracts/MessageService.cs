using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NerdyBot.Contracts
{
  public class MessageService
  {
    private ulong responseChannelId;
    public MessageService( ulong defaultResponseChannel )
    {
      responseChannelId = defaultResponseChannel;
    }

    public async void SendMessage( ICommandContext context, string message, SendMessageOptions options )
    {
      switch ( options.TargetType )
      {
      case TargetType.User:
        var usr = await context.Guild.GetUserAsync( context.User.Id );
        var dmcChannel = await usr.GetDMChannelAsync();
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
      case TargetType.Default:
      default:
        {
          var channel = await context.Guild.GetTextChannelAsync( responseChannelId );
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
      }
    }

    public void Log( string text, string source = "", LogSeverity logLevel = LogSeverity.Info )
    {
      //this.client.Log.Log( logLevel, source, text );
    }

    private readonly int chunkSize = 1990;
    private IEnumerable<string> ChunkMessage( string str )
    {
      if ( str.Length > chunkSize )
        return Enumerable.Range( 0, str.Length / chunkSize )
          .Select( i => str.Substring( i * chunkSize, chunkSize ) );
      return new string[] { str };
    }
    private string FormatMessage( string message, MessageType format, string highlight )
    {
      string formatedMessage = string.Empty;
      switch ( format )
      {
      case MessageType.Block:
        formatedMessage = "```" + highlight + Environment.NewLine + message + "```";
        break;
      case MessageType.Info:
        formatedMessage = "`" + message + "`";
        break;
      case MessageType.Normal:
      default:
        formatedMessage = message;
        break;
      }
      return formatedMessage;
    }
  }
}
