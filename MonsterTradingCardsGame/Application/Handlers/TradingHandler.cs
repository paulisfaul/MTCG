using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Models.RequestModels;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Application.Handlers
{
    public class TradingHandler:Handler
    {
        private readonly TradingService _tradingService;
        public TradingHandler(AuthenticationService authenticationService, TradingService tradingService)
        {
            _tradingService = tradingService;
            _finalRouteHandlerFunctions.Add("/", (HandleTradings, authenticationService.AuthorizePlayer));
            _variableRoute = (HandleSingleTradingById, authenticationService.AuthorizePlayer);
        }

        public async Task<bool> HandleTradings(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "POST")
            {
                try
                {
                    var tradingOfferRequest = JsonSerializer.Deserialize<TradingOfferRequestDto>(e.Payload);
                    if (tradingOfferRequest != null)
                    {
                        if (Enum.TryParse<CardTypeEnum>(tradingOfferRequest.CardType, true, out var cardType))
                        {
                            var trading = new TradingOffer(user.Id, tradingOfferRequest.CardId, cardType,
                                tradingOfferRequest.MinDmg, tradingOfferRequest.AutomaticAccept);

                            var result = await _tradingService.CreateTradingOffer(trading, user);

                            e.Reply(result.Code, result.Message);
                            return result.Success;

                        }
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Ungültige TradingOffer-Daten.");
                        return false;
                    }

                    e.Reply(HttpStatusCode.BAD_REQUEST, "Ungültige Packagedaten.");
                    return false;
                }
                catch (JsonException)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid JSON format");
                    return false;
                }
            }

            if (e.Method == "GET")
            {
                var tradingOffers = await _tradingService.GetOpenTradingOffers(user);
                e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(tradingOffers));
                return true;
            }

            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Method not allowed.");
            return false;
        }
        public async Task<bool> HandleSingleTradingById(HttpSvrEventArgs e, string currentPath, User user)
        {
            if (e.Method == "POST")
            {
                try
                {
                    var segments = e.RemainingPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length == 0)
                    {
                        string idString = currentPath.Substring(1);


                        if (Guid.TryParse(idString, out Guid tradingId))
                        {
                            TradingAcceptRequestDto tradingAcceptRequest =
                                JsonSerializer.Deserialize<TradingAcceptRequestDto>(e.Payload);

                            var result =
                                await _tradingService.AcceptTrading(user, tradingId, tradingAcceptRequest.CardId);
                            e.Reply(result.Code, result.Message);
                            return result.Success;
                        }

                        e.Reply(HttpStatusCode.BAD_REQUEST, "Trading GUID ungültig.");
                        return false;
                    }
                }
                catch (JsonException)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid JSON format");
                    return false;
                }
            }

            if (e.Method == "DELETE")
            {
                var segments = e.RemainingPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0)
                {
                    string idString = currentPath.Substring(1);
                    if (Guid.TryParse(idString, out Guid tradingId))
                    {
                        var result = await _tradingService.Delete(tradingId, user);
                        e.Reply(result.Code, result.Message);
                        return result.Success;
                    }
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Trading GUID ungültig.");
                    return false;
                }
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }
    }
}
