using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Helper.HttpServer;

namespace MonsterTradingCardsGame.Application.Handlers
{
    public class BaseHandler : Handler, IHandler
    {
        public BaseHandler(ApiHandler apiHandler, AuthenticationHandler authenticationHandler)
        {
            _unfinalRouteHandlers.Add("/api", apiHandler);
        }
    }
}