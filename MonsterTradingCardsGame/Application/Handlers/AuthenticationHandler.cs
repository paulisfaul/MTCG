using System;
using System.Collections.Generic;
using System.Text.Json;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Models.RequestModels;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Enums;
using BCrypt.Net;


namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class implements a handler for authentication-specific requests.</summary>
    public class AuthenticationHandler : Handler, IHandler
    {
        private readonly AuthenticationService _authenticationService;
        private readonly CardService _cardService;

        public AuthenticationHandler(AuthenticationService authenticationService, CardService cardService)
        {
            _authenticationService = authenticationService;
            _cardService = cardService;
            _finalRouteHandlerFunctions.Add("/register", (HandleRegister, null));
            _finalRouteHandlerFunctions.Add("/login", (HandleLogin, null));
        }

        /// <summary>
        ///  Handles an incoming HTTP request regarding the registration of a user.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="userUnused">Unused, because operating User not needed.</param>
        /// <returns></returns>
        private async Task<bool> HandleRegister(HttpSvrEventArgs e, User userUnused)
        {
            if (e.Method == "POST")
            {
                Console.WriteLine("PlainMessage: " + e.PlainMessage);

                try
                {
                    var userRegisterRequest = JsonSerializer.Deserialize<UserRegisterRequestDto>(e.Payload);
                    if (userRegisterRequest != null)
                    {
                        if (Enum.TryParse<RoleEnum>(userRegisterRequest.Role, true, out var role))
                        {
                            var user = new User(userRegisterRequest.Username, role);
                            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegisterRequest.Password);

                            var result = await _authenticationService.Register(user, hashedPassword);
                            var message = result.Message;
                            if (result.Success)
                            {
                                var deckResult = await _cardService.CreateDeck(user);

                                if (deckResult.Success)
                                {
                                    e.Reply(deckResult.Code, message);
                                    return deckResult.Success;
                                }
                            }

                            e.Reply(result.Code, result.Message);
                            return false;
                        }
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid role.");
                        return false;
                    }
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid data.");
                    return false;
                }
                catch (JsonException)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid JSON format");
                    return false;
                }
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Method not allowed.");
            return false;
        }

        /// <summary>
        ///  Handles an incoming HTTP request regarding the login of a user.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="userUnused">Unused, because operating User not needed.</param>
        /// <returns></returns>
        private async Task<bool> HandleLogin(HttpSvrEventArgs e, User unused)
        {
            if (e.Method == "POST")
            {
                Console.WriteLine("PlainMessage: " + e.PlainMessage);

                try
                {
                    var userLoginRequest = JsonSerializer.Deserialize<UserLoginRequestDto>(e.Payload);
                    if (userLoginRequest != null)
                    {
                        var result = await _authenticationService.Login(userLoginRequest.Username, userLoginRequest.Password);
                        if (result.Success && !string.IsNullOrEmpty(result.Data))
                        {
                            var jsonResponse = JsonSerializer.Serialize(new { token = result.Data });
                            e.Reply(result.Code, jsonResponse);
                            return true;
                        }
                        e.Reply(result.Code, "Login failed.");
                        return false;
                    }
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid data.");
                    return false;
                }
                catch (JsonException)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid JSON format");
                    return false;
                }
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Method not allowed.");
            return false;
        }
    }
}