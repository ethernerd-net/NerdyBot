using System;
using System.Collections.Generic;

namespace NerdyBot.Contracts
{
  public class ServiceProvider : IServiceProvider
  {
    Dictionary<Type, object> services = new Dictionary<Type, object>();
    public object GetService( Type serviceType )
    {
      return this.services[serviceType];
    }

    public void AddService( object service )
    {
      this.services.Add( service.GetType(), service );
    }
  }
}
