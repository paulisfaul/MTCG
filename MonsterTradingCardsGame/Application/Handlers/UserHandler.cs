using System;
using System.Text.Json;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Models.RequestModels;
using MonsterTradingCardsGame.Services;

namespace MonsterTradingCardsGame.Application.Handlers
{
    /// <summary>This class implements a handler for user-specific requests.</summary>
    public class UserHandler : Handler, IHandler
    {
        private readonly UserService _userService;

        /// <summary>
        /// Initializes the used services and Handle-Functions/Methods
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="authenticationService"></param>
        public UserHandler(UserService userService, AuthenticationService authenticationService)
        {
            _userService = userService;
            _finalRouteHandlerFunctions.Add("/", (HandleUsers, authenticationService.AuthorizeAdmin));
            _variableRoute = (HandleSingleUserByUsername, authenticationService.AuthorizePlayer);
        }

        /// <summary>
        /// Handles an incoming HTTP request regarding all users.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="user">Logged in User on the client. Used for querying.</param>
        /// <returns></returns>
        public async Task<bool> HandleUsers(HttpSvrEventArgs e, User user)
        {
            if (e.Method == "GET")
            {
                var result = await _userService.GetAllUsers();

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

        /// <summary>
        /// Handles an incoming HTTP request regarding a single user by username.
        /// </summary>
        /// <param name="e">Event to handle.</param>
        /// <param name="currentPath">Current path.</param>
        /// <returns></returns>
        public async Task<bool> HandleSingleUserByUsername(HttpSvrEventArgs e, string currentPath, User user)
        {
            if (e.Method == "GET")
            {
                var segments = e.RemainingPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0)
                {
                    string username = "";
                    if (currentPath.Length > 1)
                    {
                        username = currentPath.Substring(1);
                    }

                    var result = await _userService.GetUserByUsername(username);
                    if (result.Success)
                    {
                        e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(result.Data));
                        return true;
                    }
                    e.Reply(result.Code, result.Message);
                    return false;
                }
            }

            if (e.Method == "PUT")
            {
                try{
                var segments = e.RemainingPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0)
                {
                    string username = "";
                    if (currentPath.Length > 1)
                    {
                        username = currentPath.Substring(1);
                    }

                    if (username != user.UserCredentials.Username && user.UserCredentials.Role != RoleEnum.Admin)
                    {
                        e.Reply(HttpStatusCode.UNAUTHORIZED, "Only admin or user can edit their data.");

                    }

                    var userUpdateRequest = JsonSerializer.Deserialize<UserUpdateRequestDto>(e.Payload);

                    var result_get = await _userService.GetUserByUsername(username);
                    if (!result_get.Success)
                    {
                        e.Reply(result_get.Code, JsonSerializer.Serialize(result_get.Message));
                        return true;
                    }


                    User userToUpdate = result_get.Data;

                    userToUpdate.UserData.Bio = userUpdateRequest.Bio;
                    userToUpdate.UserData.Image = userUpdateRequest.Image;
                    userToUpdate.UserData.Name = userUpdateRequest.Name;

                    var result_update = await _userService.UpdateUser(userToUpdate);

                    if (result_update.Success)
                    {
                        e.Reply(HttpStatusCode.OK, JsonSerializer.Serialize(result_update.Data));
                        return true;
                    }
                    e.Reply(result_update.Code, result_update.Message);
                    return false;
                }
                }
                catch (JsonException)
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Ungültiges JSON-Format.");
                    return false;
                }
            }
            e.Reply(HttpStatusCode.METHOD_NOT_ALLOWED, "Methode nicht erlaubt.");
            return false;
        }
    }
}