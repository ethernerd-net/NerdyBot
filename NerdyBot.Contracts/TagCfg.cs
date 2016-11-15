using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace NerdyBot
{
  public class TagCfg
  {
    public List<Tag> Tags { get; set; }
    public void Save( string destination )
    {
      File.WriteAllText( destination, JsonConvert.SerializeObject( this ) );
    }
  }

  public class Tag
  {
    public string Name { get; set; }
    public TagType Type { get; set; }
    public string Author { get; set; }
    public List<string> Items { get; set; }
    public DateTime CreateDate { get; set; }
    public long Count { get; set; }
    public short Volume { get; set; }
  }
  public enum TagType { Sound = 0, Text = 1, Picture = 2 }
}
