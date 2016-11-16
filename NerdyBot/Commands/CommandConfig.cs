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

      Read();
    }

    public string Key { get; private set; }
    public IEnumerable<string> Aliases { get; private set; }
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
        this.Items = cfg.Items;
      }
    }
  }
}
