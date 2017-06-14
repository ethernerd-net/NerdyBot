using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Discord.Commands;

using NerdyBot.Models;
using NerdyBot.Services;

namespace NerdyBot.Modules
{
  [Group("music"), Alias("m")]
  public class MusicModule : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
    public YoutubeService YoutubeService { get; set; }

    public MusicModule( AudioService audioService )
    {
      audioService.FinishedSending += AudioServiceFinishedSending;
    }

    [Command( "play" )]
    public void Play()
    {
      Next();
    }
    [Command( "play" )]
    public void Play( string content )
    {
      var queue = MusicModuleHandler.Handler.Queues.GetOrAdd( Context.Guild.Id, new ConcurrentQueue<MusicPlaylistEntry>() );

      queue.Enqueue( new MusicPlaylistEntry() { URL = content, Author = Context.User.ToString(), Title = "" } );

      Next();
    }

    [Command( "next" )]
    public void Next()
    {
      string blockingModule = AudioService.GetBlock( Context.Guild.Id );
      if ( !string.IsNullOrEmpty( blockingModule ) && blockingModule != typeof( MusicModule ).Name )
        throw new ApplicationException( $"The service is already blocked by '{blockingModule}'!" );

      var queue = MusicModuleHandler.Handler.Queues.GetOrAdd( Context.Guild.Id, new ConcurrentQueue<MusicPlaylistEntry>() );

      if ( queue.Count == 0 )
      {
        AudioService.StopPlaying( Context.Guild.Id );
        throw new Exception( "Nothing to play" );
      }

      AudioService.StopPlaying( Context.Guild.Id );
      AudioService.SetBlock( Context.Guild.Id, typeof( MusicModule ).Name );

      Play( new AudioContext() { GuildId = Context.Guild.Id, UserId = Context.User.Id } );
    }

    [Command( "add" )]
    public void QueueAdd( string content )
    {
      var queue = MusicModuleHandler.Handler.Queues.GetOrAdd( Context.Guild.Id, new ConcurrentQueue<MusicPlaylistEntry>() );

      queue.Enqueue( new MusicPlaylistEntry() { URL = content, Author = Context.User.ToString(), Title = "" } );
    }

    [Command( "volume" )]
    public void Volume( int volume )
    {
      string blockingModule = AudioService.GetBlock( Context.Guild.Id );
      if ( !string.IsNullOrEmpty( blockingModule ) && blockingModule != typeof( MusicModule ).Name )
        AudioService.Volume = volume / 100f;
    }

    [Command( "stop" )]
    public void Stop()
    {
      string blockingModule = AudioService.GetBlock( Context.Guild.Id );
      if ( !string.IsNullOrEmpty( blockingModule ) )
      {
        if ( blockingModule != typeof( MusicModule ).Name )
          throw new ApplicationException( $"You can't stop audio transmission this way, because its blocked by '{blockingModule}'!" );
        AudioService.StopPlaying( Context.Guild.Id );
      }
    }

    private void AudioServiceFinishedSending( object sender, GuildIdEventArgs e )
    {
      if ( AudioService.GetBlock( e.Context.GuildId ) != typeof( MusicModule ).Name )
        return;
      var queue = MusicModuleHandler.Handler.Queues.GetOrAdd( Context.Guild.Id, new ConcurrentQueue<MusicPlaylistEntry>() );
      if ( queue.Count == 0 )
      {
        AudioService.StopPlaying( Context.Guild.Id );
        throw new Exception( "Playlist end" );
      }

      Task.Delay( 1000 ).GetAwaiter().GetResult();

      Play( e.Context );
    }

    private void Play( AudioContext context )
    {
      Task.Run( () =>
      {
        var queue = MusicModuleHandler.Handler.Queues.GetOrAdd( Context.Guild.Id, new ConcurrentQueue<MusicPlaylistEntry>() );

        MusicPlaylistEntry entry;
        while ( !queue.TryDequeue( out entry ) )
          ;

        AudioService.SendAudio( context, AudioService.DownloadAudio( entry.URL ), 0.5f, typeof( MusicModule ).Name );
      } );
    }
  }
}
