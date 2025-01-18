using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Helpers;
using MonsterTradingCardsGame.Repositories.Interfaces;
using Npgsql;
using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.Database;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Common;

namespace MonsterTradingCardsGame.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly NpgsqlConnection _connection;
        private readonly string _tableName = $"\"{DatabaseConfig.SchemaName}\".\"user\"";
        private readonly IEnumerable<string> _columns = new List<string> { "id", "username", "password", "created_at", "last_login_at", "name", "bio", "image", "role", "coins", "elo", "wins","losses","draws" };

        public UserRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<OperationResult<User>> Create(User user, string hashedPassword)
        {
            if (user == null || hashedPassword == string.Empty)
            {
                throw new ArgumentNullException("DeckId und UserId dürfen nicht leer sein.");
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string insertUserQuery = QueryBuilder.BuildInsertQuery(_tableName, _columns);
                var userRowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, insertUserQuery,
                    cmd => AddParameters(cmd, user, hashedPassword));
                if (userRowAffected > 0)
                {
                    return new OperationResult<User>(true, HttpStatusCode.CREATED, null, user);
                }

                return new OperationResult<User>(false, HttpStatusCode.BAD_REQUEST, "User could not be created.");
            }
            catch
            {
                return new OperationResult<User>(false, HttpStatusCode.INTERNAL_SERVER_ERROR,
                    "User could not be created.");
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<IEnumerable<User>>> GetAll()
        {

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableName, _columns);
                var users = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd => { }, async reader =>
                {
                    var userList = new List<User>();
                    while (await reader.ReadAsync())
                    {
                        var roleString = Convert.ToString(reader["role"]);
                        if (!Enum.TryParse<RoleEnum>(roleString, true, out var role))
                        {
                            role = RoleEnum.Player;
                        }

                        var user = new User(
                            id: Guid.Parse(reader["id"].ToString()),
                            username: Convert.ToString(reader["username"]),
                            role: role,
                            lastLoginAt: reader["last_login_at"] as DateTime?,
                            createdAt: Convert.ToDateTime(reader["created_at"]),
                            name: Convert.ToString(reader["name"]),
                            bio: Convert.ToString(reader["bio"]),
                            image: Convert.ToString(reader["image"]),
                            coins: Convert.ToInt32(reader["coins"]),
                            elo: Convert.ToInt32(reader["elo"]),
                            wins: Convert.ToInt32(reader["wins"]),
                            losses: Convert.ToInt32(reader["losses"]),
                            draws: Convert.ToInt32(reader["draws"])
                        );
                        userList.Add(user);
                    }
                    return userList;
                });

                return new OperationResult<IEnumerable<User>>(true, HttpStatusCode.OK, null, users);
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<User>>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<User>> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new OperationResult<User>(false, HttpStatusCode.BAD_REQUEST, "Invalid ID.");
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableName, _columns, "id");
                var user = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd => DatabaseHelper.AddParameter(cmd, "@Id", id), async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        var roleString = Convert.ToString(reader["role"]);
                        if (!Enum.TryParse<RoleEnum>(roleString, true, out var role))
                        {
                            role = RoleEnum.Player;
                        }

                        return new User(
                            id: Guid.Parse(reader["id"].ToString()),
                            username: Convert.ToString(reader["username"]),
                            role: role,
                            lastLoginAt: reader["last_login_at"] as DateTime?,
                            createdAt: Convert.ToDateTime(reader["created_at"]),
                            name: Convert.ToString(reader["name"]),
                            bio: Convert.ToString(reader["bio"]),
                            image: Convert.ToString(reader["image"]),
                            coins: Convert.ToInt32(reader["coins"]),
                            elo: Convert.ToInt32(reader["elo"]),
                            wins: Convert.ToInt32(reader["wins"]),
                            losses: Convert.ToInt32(reader["losses"]),
                            draws: Convert.ToInt32(reader["draws"])
                        );
                    }
                    return null;
                });

                if (user != null)
                {
                    return new OperationResult<User>(true, HttpStatusCode.OK, null, user);
                }
                return new OperationResult<User>(false, HttpStatusCode.NOT_FOUND, "User not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<User>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<(User user, string hashedPassword)>> GetByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new OperationResult<(User, string)>(false, HttpStatusCode.BAD_REQUEST, "Invalid username.");
            }


            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableName, _columns, "username");
                var result = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd => DatabaseHelper.AddParameter(cmd, "@Username", username), async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        var roleString = Convert.ToString(reader["role"]);
                        if (!Enum.TryParse<RoleEnum>(roleString, true, out var role))
                        {
                            role = RoleEnum.Player;
                        }

                        var user = new User(
                            id: Guid.Parse(reader["id"].ToString()),
                            username: Convert.ToString(reader["username"]),
                            role: role,
                            lastLoginAt: reader["last_login_at"] as DateTime?,
                            createdAt: Convert.ToDateTime(reader["created_at"]),
                            name: Convert.ToString(reader["name"]),
                            bio: Convert.ToString(reader["bio"]),
                            image: Convert.ToString(reader["image"]),
                            coins: (int)reader["coins"],
                            elo: Convert.ToInt32(reader["elo"]),
                            wins: Convert.ToInt32(reader["wins"]),
                            losses: Convert.ToInt32(reader["losses"]),
                            draws: Convert.ToInt32(reader["draws"])

                        );

                        var hashedPassword = Convert.ToString(reader["password"]);

                        return (user, hashedPassword);
                    }
                    return (null, null);
                });

                if (result.user != null)
                {
                    return new OperationResult<(User, string)>(true, HttpStatusCode.OK, null, result);
                }
                return new OperationResult<(User, string)>(false, HttpStatusCode.NOT_FOUND, "User not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<(User, string)>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<bool>> Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            try
            {
                string updateQuery = QueryBuilder.BuildUpdateQuery(_tableName, _columns.Where((item, index) => index != 0 && index != 2).ToList(), "id");
                var rowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, updateQuery, cmd => AddParameters(cmd, user, null));
                if (rowAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }
                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "User not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<bool>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<bool>> Delete(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Invalid ID.");
            }


            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            try
            {
                string deleteQuery = QueryBuilder.BuildDeleteQuery(_tableName, "id");
                var rowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, deleteQuery, cmd => DatabaseHelper.AddParameter(cmd, "@Id", userId));
                if (rowAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }
                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "User not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<bool>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<IEnumerable<User>>> GetHighestElo(int top)
        {
            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildCustomSelectQuery(
                    _tableName,
                    _columns.ToList(),
                    null,
                    "role = 'Player'",
                    "elo DESC",
                    top
                );

                var users = await DatabaseHelper.ExecuteReaderAsync(_connection, query, null, async reader =>
                {
                    var userList = new List<User>();
                    while (await reader.ReadAsync())
                    {
                        var roleString = Convert.ToString(reader["role"]);
                        if (!Enum.TryParse<RoleEnum>(roleString, true, out var role))
                        {
                            role = RoleEnum.Player;
                        }

                        var user = new User(
                            id: Guid.Parse(reader["id"].ToString()),
                            username: Convert.ToString(reader["username"]),
                            role: role,
                            lastLoginAt: reader["last_login_at"] as DateTime?,
                            createdAt: Convert.ToDateTime(reader["created_at"]),
                            name: Convert.ToString(reader["name"]),
                            bio: Convert.ToString(reader["bio"]),
                            image: Convert.ToString(reader["image"]),
                            coins: Convert.ToInt32(reader["coins"]),
                            elo: Convert.ToInt32(reader["elo"]),
                            wins: Convert.ToInt32(reader["wins"]),
                            losses: Convert.ToInt32(reader["losses"]),
                            draws: Convert.ToInt32(reader["draws"])
                        );
                        userList.Add(user);
                    }
                    return userList;
                });

                return new OperationResult<IEnumerable<User>>(true, HttpStatusCode.OK, null, users);
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<User>>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        private static void AddParameters(NpgsqlCommand command, User user, string hashedPassword)
        {
            DatabaseHelper.AddParameter(command, "@id", user.Id);
            DatabaseHelper.AddParameter(command, "@username", user.UserCredentials.Username ?? string.Empty);
            DatabaseHelper.AddParameter(command, "@password", hashedPassword?? string.Empty);
            DatabaseHelper.AddParameter(command, "@created_at", user.UserData.CreatedAt);
            DatabaseHelper.AddParameter(command, "@last_login_at", user.UserData.LastLoginAt ?? (object)DBNull.Value);
            DatabaseHelper.AddParameter(command, "@name", user.UserData.Name ?? string.Empty);
            DatabaseHelper.AddParameter(command, "@bio", user.UserData.Bio ?? string.Empty);
            DatabaseHelper.AddParameter(command, "@image", user.UserData.Image ?? string.Empty);
            var roleParam = command.Parameters.AddWithValue("role", user.UserCredentials.Role.ToString());
            roleParam.DataTypeName = "role"; // Name des benutzerdefinierten Enum-Typs in der Datenbank
            DatabaseHelper.AddParameter(command, "@coins", user.Coins);
            DatabaseHelper.AddParameter(command, "@elo", user.UserStats.Elo);
            DatabaseHelper.AddParameter(command, "@wins", user.UserStats.Wins);
            DatabaseHelper.AddParameter(command, "@losses", user.UserStats.Losses);
            DatabaseHelper.AddParameter(command, "@draws", user.UserStats.Draws);


        }
    }
}