﻿using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
      Directory.CreateDirectory( "logs" );
      Trace.Listeners.Clear();

      TextWriterTraceListener twtl = new TextWriterTraceListener( Path.Combine( "logs", $"{DateTime.Now.ToString( "yyyy-MM-dd--hh-mm-ss" )}.log" ) );
      twtl.Name = "TextLogger";
      twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

      ConsoleTraceListener ctl = new ConsoleTraceListener( false );
      ctl.TraceOutputOptions = TraceOptions.DateTime;

      Trace.Listeners.Add( twtl );
      Trace.Listeners.Add( ctl );
      Trace.AutoFlush = true;

      discordClient.Log += ClientLog;
    }

    private async Task ClientLog( LogMessage arg )
    {
      await Task.Run( () =>
        {
          Trace.WriteLine( arg.ToString() );
        } );
    }
    
    public void SendMessageToCurrentChannel( ICommandContext context, string message, MessageType mType = MessageType.Normal, bool split = false, string highlight = "" )
    {
      SendMessageAsync( context.Channel, message, mType, split, highlight );
    }
    public void SendMessageToCurrentUser( ICommandContext context, string message, MessageType mType = MessageType.Normal, bool split = false, string highlight = "" )
    {
      var usr = context.Guild.GetUserAsync( context.User.Id ).Result;
      SendMessageAsync( usr.CreateDMChannelAsync().Result, message, mType, split, highlight );
    }

    public void Log( string text, string source = "", LogSeverity logLevel = LogSeverity.Info )
    {
      Task.Run( async () =>
        {
          await ClientLog( new LogMessage( logLevel, source, text ) );
        } );
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
    private Task SendMessageAsync( IMessageChannel channel, string message, MessageType mType = MessageType.Normal, bool split = false, string highlight = "" )
    {
      return Task.Run( async () =>
      {
        if ( !split && message.Length > chunkSize )
        {
          string fn = $"{Guid.NewGuid().ToString()}.txt";
          File.WriteAllText( fn, message );
          await channel.SendFileAsync( fn );
          File.Delete( fn );
        }
        else
        {
          foreach ( string msg in ChunkMessage( message ) )
            await channel.SendMessageAsync( FormatMessage( msg, mType, highlight ) );
        }
      } );
    }
  }
  public enum MessageType { Normal, Block, Info }
}
