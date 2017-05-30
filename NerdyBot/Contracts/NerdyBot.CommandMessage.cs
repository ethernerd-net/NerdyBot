using Discord;
using NerdyBot.Contracts;

namespace NerdyBot
{
  class CommandMessage : ICommandMessage
  {
    public string Text { get; set; }
    public string[] Arguments { get; set; }
    public ICommandUser User { get; set; }
    public ICommandChannel Channel { get; set; }
  }
  class CommandUser : ICommandUser
  {
    public ulong Id { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }
    public string Mention { get; set; }
    //public ServerPermissions Permissions { get; set; }
  }
  class CommandChannel : ICommandChannel
  {
    public ulong Id { get; set; }
    public string Name { get; set; }
    public string Mention { get; set; }
  }
}
