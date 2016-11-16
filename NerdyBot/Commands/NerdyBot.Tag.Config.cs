using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NerdyBot.Commands.Config
{
  public class TagCommandConfig<T> : BaseCommandConfig
  {
    public TagCommandConfig( string filePath, string key, params string[] aliases )
      : base( filePath, key, aliases )
    {
      this.Items = new List<T>();
    }

    public List<T> Items { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<TagCommandConfig<T>>( json );
    }
    protected override void Assign( dynamic conf )
    {
      base.Assign( conf as BaseCommandConfig );
      this.Items = conf.Items;
    }
  }

  public enum RestrictType { None = 0, Allow = 1, Deny = 2, Admin = 3 }

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
