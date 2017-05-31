using System;
using System.Collections.Generic;

namespace NerdyBot.Models
{
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
