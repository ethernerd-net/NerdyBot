using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Discord.Commands;

using NerdyBot.Models;
using NerdyBot.Services;
using System.Collections.Generic;

namespace NerdyBot.Modules
{
  [Group("music"), Alias("m")]
  public class Music : ModuleBase
  {
    private ConcurrentDictionary<ulong, PlaylistMode> localModes = new ConcurrentDictionary<ulong, PlaylistMode>();
    private ConcurrentDictionary<ulong, List<MusicPlaylistEntry>> localLists = new ConcurrentDictionary<ulong, List<MusicPlaylistEntry>>();

    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
    /*public DatabaseService DatabaseService { get; set; }

    public Music( DatabaseService databaseService )
    {
      databaseService.Database.CreateTable<MusicPlaylist>();
      databaseService.Database.CreateTable<MusicPlaylistEntry>();
    }

    [Group("playlist"), Alias("pl")]
    public class Playlist : ModuleBase
    {
      private ConcurrentDictionary<ulong, string> currentPlaylist = new ConcurrentDictionary<ulong, string>();

      public DatabaseService DatabaseService { get; set; }
      public AudioService AudioService { get; set; }

      [Command( "play" )]
      public async Task Play()
      {
        throw new NotImplementedException();
      }
      [Command( "play" )]
      public async Task Play( string plName )
      {
        string uniquePlId = $"{Context.Guild.Id}{plName}";
        string currentPlName;
        MusicPlaylist pl;

        if ( this.currentPlaylist.TryGetValue( Context.Guild.Id, out currentPlName ) && currentPlName == plName )
          return; //Playlist spielt bereits
        else if ( ( pl = DatabaseService.Database.Table<MusicPlaylist>().Where( mp => mp.UniqueID == uniquePlId ).FirstOrDefault() ) == null )
          return; //Playlist existiert nicht
        else
        {
          MusicPlaylistEntry entry;
          var list = DatabaseService.Database.Table<MusicPlaylistEntry>().Where( mpe => mpe.QueueId == uniquePlId );
          if ( ( entry = list.Where( mpe => mpe.Id == pl.CurrentEntry ).FirstOrDefault() ) == null )
            return; //Existiert nicht mehr D:

        }
      }

      [Command( "create" )]
      public async Task Create()
      {
        throw new NotImplementedException();
      }
      [Command( "delete" )]
      public async Task Delete()
      {
        throw new NotImplementedException();
      }
      [Command( "add" )]
      public async Task Add()
      {
        throw new NotImplementedException();
      }
      [Command( "remove" )]
      public async Task Remove()
      {
        throw new NotImplementedException();
      }
    }*/

    [Command( "play" )]
    public async Task Play()
    {
      throw new NotImplementedException();
    }
    [Command( "next" )]
    public async Task Next()
    {
      throw new NotImplementedException();
    }

    [Command( "add" )]
    public async Task QueueAdd( string content )
    {
      throw new NotImplementedException();
    }

    [Command( "mode" )]
    public async Task Mode( PlaylistMode mode )
    {
      throw new NotImplementedException();
    }
  }
}
