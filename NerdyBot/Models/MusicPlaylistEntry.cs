using SQLite;

namespace NerdyBot.Models
{
  public class MusicPlaylistEntry
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public string QueueId { get; set; }
    public string URL { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
  }
}
