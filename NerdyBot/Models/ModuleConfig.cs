using SQLite;

namespace NerdyBot.Models
{
  public class ModuleConfig
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Unique]
    public string Name { get; set; }
    public string ApiKey { get; set; }
  }
}
