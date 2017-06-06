using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Models
{
  public class MusicQueueEntry
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public long QueueId { get; set; }
    public string URL { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
  }
}
