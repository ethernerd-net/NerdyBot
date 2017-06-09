using SQLite;

namespace NerdyBot.Models
{
  public class MusicPlaylist
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public long GuildId { get; set; }
    [Unique]
    public string UniqueID { get; set; } // GuildId + Name
    public string Name { get; set; }
    public PlaylistMode PlaylistMode { get; set; }
    public int CurrentEntry { get; set; }
  }
  public enum PlaylistMode { Normal, Shuffle, Repeat }
}
