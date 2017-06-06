using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Models
{
  public class MusicQueue
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public long GuildId { get; set; }
    public bool Shuffle { get; set; }
    public bool Repeat { get; set; }
    public int CurrentEntry { get; set; }
  }
}
