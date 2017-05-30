using System;

namespace NerdyBot.Contracts
{
  class LogMessageEventArgs : EventArgs
  {
    public string Message { get; set; }
    public string Source { get; set; }
  }
}
