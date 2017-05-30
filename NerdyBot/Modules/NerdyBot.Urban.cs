﻿using Discord.Commands;
using NerdyBot.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  [Group( "urban" ), Alias( "u" )]
  public class UrbanCommand : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }

    [Command( "help" )]
    public async Task Help()
    {
      MessageService.SendMessage( Context, FullHelp(),
        new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
    }

    [Command( "sound" )]
    public async Task ExecuteSound( params string[] args )
    {
      string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", args ) );
      var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
      if ( urban.sounds != null && urban.sounds.Count() > 0 )
      {
        string soundUrl = urban.sounds.First();
        if ( soundUrl.EndsWith( ".mp3" ) )
        {
          AudioService.StopPlaying = false;
          string path = Path.Combine( "urban", string.Join( " ", args.Skip( 1 ) ) + ".mp3" );
          if ( !File.Exists( path ) )
            AudioService.DownloadAudio( urban.sounds.First(), path );
          AudioService.SendAudio( Context, path );
        }
        else
          MessageService.SendMessage( Context, "404, Urban not found!",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
      else
        MessageService.SendMessage( Context, "404, Urban not found!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
    }

    [Command()]
    public async Task Execute( params string[] args )
    {
      string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", args ) );
      var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
      if ( urban.list != null && urban.list.Count() > 0 )
        MessageService.SendMessage( Context, urban.list.First().permalink.Replace( "\\", "" ),
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      else
        MessageService.SendMessage( Context, "404, Urban not found!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== Urban ========" );
      sb.AppendLine();
      sb.AppendLine( "Sucht auf Urban Dictonary nach dem eingegebenen Keyword." );
      sb.AppendLine( "Key: urban" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: u" );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: urban <sound> [KEYWORD]" );
      sb.AppendLine( "Bei mitangabe des optionalen Parameters 'sound' wird auf urban dictionary nach einem entsprechenden soundfile gesucht!" );
      return sb.ToString();
    }

    #region jsonClasses
    private class UrbanJson
    {
      public List<UrbanEntry> list { get; set; }
      public List<string> sounds { get; set; }
    }
    private class UrbanEntry
    {
      public string permalink { get; set; }
      public string definition { get; set; }
    }
    #endregion jsonClasses
  }
}