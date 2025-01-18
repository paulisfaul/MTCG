using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Application.Handlers
{
    public class StatsHandler:Handler
    {
        private readonly UserService _userService;

        public StatsHandler(UserService userService, AuthenticationService authenticationService)
        {
            _userService = userService;
            _finalRouteHandlerFunctions.Add("/", (HandleStats, authenticationService.AuthorizePlayer));
        }

        public async Task<bool> HandleStats(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "GET")
            {
                var result = await _userService.GetStats(user);

                if (result.Success)
                {
                    e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(result.Data));
                    return true;
                }
                e.Reply(result.Code, result.Message);
                return false;
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }
    }
}
