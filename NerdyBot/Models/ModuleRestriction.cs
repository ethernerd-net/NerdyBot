using SQLite;

namespace NerdyBot.Models
{
  //TODO IMPLEMENT ROLES
  public class ModuleRestriction
  {
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int ModuleId { get; set; }
    public long GuildId { get; set; }
    public long RestrictedRole { get; set; }
    public RestrictType RestrictionType { get; set; }
  }
  public enum RestrictType { Whitelist = 0, Blacklist = 1 }
}
