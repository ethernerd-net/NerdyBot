using SQLite;

namespace NerdyBot.Models
{
  public class Ball8Answer
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Answer { get; set; }
  }
}
