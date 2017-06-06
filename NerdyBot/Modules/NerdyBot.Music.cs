using System;
using System.Threading.Tasks;

using Discord.Commands;

using NerdyBot.Models;
using NerdyBot.Services;

namespace NerdyBot.Modules
{
  [Group("music"), Alias("m")]
  public class Music : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
    public DatabaseService DatabaseService { get; set; }

    public Music( DatabaseService databaseService )
    {
      databaseService.Database.CreateTable<MusicPlaylist>();
      databaseService.Database.CreateTable<MusicPlaylistEntry>();
    }

    [Group("playlist"), Alias("pl")]
    public class MusicPlaylist : ModuleBase
    {
      [Command( "play" )]
      public async Task Play()
      {
        throw new NotImplementedException();
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
    }

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

    [Command("add")]
    public async Task QueueAdd( string content )
    {
      throw new NotImplementedException();
    }
  }
}
