using SQLite;

namespace NerdyBot.Models
{
  public class TagEntry
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int TagId { get; set; }
    public string TextContent { get; set; }
    public byte[] ByteContent { get; set; }
  }
}
