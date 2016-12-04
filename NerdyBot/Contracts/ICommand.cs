using System.Threading.Tasks;

namespace NerdyBot.Contracts
{
  interface ICommand
  {
    ICommandConfig Config { get; }
    void Init( IClient client );
    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    Task Execute( ICommandMessage msg );
    string QuickHelp();
    string FullHelp();
  }
}
