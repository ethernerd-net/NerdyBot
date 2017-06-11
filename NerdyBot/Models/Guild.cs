using SQLite;

namespace NerdyBot.Models
{
  public class Guild
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Unique]
    public long GuildId { get; set; }
    public string WelcomeMessage { get; set; }
  }
}
