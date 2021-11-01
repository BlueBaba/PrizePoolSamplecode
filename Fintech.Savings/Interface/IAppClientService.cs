using App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Interface
{
   public interface IAppClientService
    {
        Task<GenericMessage<ClientAppAuthResponse>> Authenticate(ClientAppAuthRequest model, string ipAddress);
        Task<GenericMessage<NewClientResponse>> CreateNewClient(NewClientRequest request);
    }
}
