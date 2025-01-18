using System;
using System.Collections.Generic;
using System.Net;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using HttpStatusCode = MonsterTradingCardsGame.Helper.HttpServer.HttpStatusCode;

namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class provides an abstract implementation of the
    /// <see cref="IHandler"/> interface. It also implements static methods
    /// that handles an incoming HTTP request by discovering and calling
    /// available handlers.</summary>
    public  class Handler : IHandler
    {

        protected Dictionary<string, (Func<HttpSvrEventArgs, User?, Task<bool>> handle,Func<string, Task<OperationResult<User?>>>? authorize)> _finalRouteHandlerFunctions;
        protected Dictionary<string, IHandler> _unfinalRouteHandlers;
        protected (Func<HttpSvrEventArgs, string, User?, Task<bool>> handle, Func<string, Task<OperationResult<User?>>>? authorize) _variableRoute;
        protected Handler()
        {
            _finalRouteHandlerFunctions = new Dictionary<string, (Func<HttpSvrEventArgs, User?, Task<bool>> handle, Func<string, Task<OperationResult<User?>>> authorize)>();
            _unfinalRouteHandlers = new Dictionary<string, IHandler>();
        }

        public async Task<bool> Handle(HttpSvrEventArgs e)
        {
            if (_unfinalRouteHandlers == null && _finalRouteHandlerFunctions == null)
            {
                throw new InvalidOperationException("Route handlers and final routes have not been set.");
            }

            // Extrahiere das erste Segment nach "/api"
            var segments = e.RemainingPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = segments.Length > 0 ? "/" + segments[0] : "/";
            e.RemainingPath = segments.Length > 1 ? "/" + string.Join('/', segments, 1, segments.Length - 1) : string.Empty;

            // Unfinal route handlers
            if (_unfinalRouteHandlers?.TryGetValue(currentPath, out var handler) == true)
            {
                return await handler.Handle(e);
            }

            // Final route handlers
            if (_finalRouteHandlerFunctions?.TryGetValue(currentPath, out var finalHandler) == true)
            {
                
                var authorizedUser = await Authorize(finalHandler.authorize, e);
                if (finalHandler.authorize != null && authorizedUser == null) return false; // Authorization failed or not provided
                return await finalHandler.handle(e, authorizedUser);
            }

            // Variable route handler
            if (_variableRoute.handle != null)
            {
                var authorizedUser = await Authorize(_variableRoute.authorize, e);
                if (_variableRoute.authorize != null && authorizedUser == null) return false; // Authorization failed or not provided
                return await _variableRoute.handle(e, currentPath, authorizedUser);
            }

            // Kein Handler gefunden
            e.Reply(HttpStatusCode.BAD_REQUEST, "Bad request.");
            return false;
        }

        // Autorisierung extrahiert in eine separate Methode
        private async Task<User?> Authorize(
            Func<string, Task<OperationResult<User>>>? authorizeFunc,
            HttpSvrEventArgs e)
        {
            if (authorizeFunc == null) return null; // Keine Autorisierung erforderlich

            var authorizationHeader = e.Headers
                .FirstOrDefault(header => header.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));
            if (authorizationHeader == null)
            {
                e.Reply(HttpStatusCode.BAD_REQUEST, "Authorization header is missing.");
                return null;
            }

            string token = authorizationHeader.Value;
            var result = await authorizeFunc(token);

            if (!result.Success)
            {
                e.Reply(HttpStatusCode.UNAUTHORIZED, "JWT invalid.");
                return null;
            }

            return result.Data;
        }

    }
}