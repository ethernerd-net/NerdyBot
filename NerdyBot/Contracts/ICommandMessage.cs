using Discord;

namespace NerdyBot.Contracts
{
  interface ICommandMessage
  {
    string Text { get; }
    string[] Arguments { get; }
    ICommandUser User { get; }
    ICommandChannel Channel { get; }
  }
  interface ICommandUser
  {
    ulong Id { get; }
    string Name { get; }
    string FullName { get; }
    string Mention { get; }
    //ServerPermissions Permissions { get; }
  }
  interface ICommandChannel
  {
    ulong Id { get; }
    string Name { get; }
    string Mention { get; }
  }
}
