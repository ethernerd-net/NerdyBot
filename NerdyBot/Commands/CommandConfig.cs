using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace NerdyBot.Commands.Config
{
  public class CommandConfig<T>
  {
    private string filePath;
    public CommandConfig( string filePath, string key, params string[] aliases )
    {
      this.filePath = filePath;

      this.Items = new List<T>();
      this.Key = key;
      this.Aliases = aliases;
      this.RestrictedRoles = new List<ulong>();
      this.RestrictionType = RestrictType.None;

      Read();
    }

    public string Key { get; private set; }
    public IEnumerable<string> Aliases { get; private set; }
    public List<ulong> RestrictedRoles { get; private set; }
    public RestrictType RestrictionType { get; set; }
    public List<T> Items { get; set; }

    private object lck = new object();
    public void Write()
    {
      lock ( lck )
      {
        File.WriteAllText( this.filePath, JsonConvert.SerializeObject( this ) );
      }
    }
    public void Read()
    {
      if ( File.Exists( filePath ) )
      {
        var cfg = JsonConvert.DeserializeObject<CommandConfig<T>>( File.ReadAllText( this.filePath ) );
        this.Key = cfg.Key;
        this.Aliases = cfg.Aliases;
        this.RestrictedRoles = cfg.RestrictedRoles;
        this.RestrictionType = cfg.RestrictionType;
        this.Items = cfg.Items;
      }
    }
  }
  public enum RestrictType { None = 0, Allow = 1, Deny = 2, Admin = 3 }
}
