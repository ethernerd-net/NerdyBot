using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NerdyBot.Commands.Config
{
  public class CommandConfig<T> : BaseCommandConfig where T : new()
  {
    public CommandConfig( string key, params string[] aliases )
      : base( key, aliases )
    {
      this.Ext = new T();
    }

    public T Ext { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<CommandConfig<T>>( json );
    }
    protected override void Assign( dynamic conf )
    {
      base.Assign( conf as BaseCommandConfig );
      this.Ext = conf.Ext;
    }
  }


  public class TagConfig
  {
    public TagConfig()
    {
      this.Tags = new List<Tag>();
    }
    public List<Tag> Tags { get; set; }
  }

  public class Tag
  {
    public string Name { get; set; }
    public TagType Type { get; set; }
    public string Author { get; set; }
    public List<string> Entries { get; set; }
    public DateTime CreateDate { get; set; }
    public long Count { get; set; }
    public short Volume { get; set; }
  }
  public enum TagType { Sound = 0, Text = 1, Url = 2 }
}
