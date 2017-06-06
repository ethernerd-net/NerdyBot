using Discord.Commands;
using NerdyBot.Models;
using NerdyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      databaseService.Database.CreateTable<MusicQueue>();
      databaseService.Database.CreateTable<MusicQueueEntry>();
    }

    [Command("play")]
    public async Task Play()
    {

    }

    [Command("add")]
    public async Task QueueAdd( string content )
    {

    }
  }
}
