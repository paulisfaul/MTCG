using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.Database;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Helpers;
using MonsterTradingCardsGame.Repositories.Interfaces;
using Npgsql;

namespace MonsterTradingCardsGame.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly NpgsqlConnection _connection;
        public static readonly string _tableName = $"\"{DatabaseConfig.SchemaName}\".\"package\"";
        public static readonly IEnumerable<string> _columns = new List<string> { "id"};
        public PackageRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }
        public async Task<OperationResult<Package>> Create(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("Package darf nicht leer sein.");

            }


            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string insertPackageQuery = QueryBuilder.BuildInsertQuery(_tableName, _columns);
                var packageRowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, insertPackageQuery,
                    cmd => AddParameters(cmd, package));
                if (packageRowAffected > 0)
                {
                    return new OperationResult<Package>(true, HttpStatusCode.CREATED, null, package);
                }

                return new OperationResult<Package>(false, HttpStatusCode.BAD_REQUEST, "Package could not be created.");
            }
            catch (Exception ex)
            {
                return new OperationResult<Package>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<Package>> GetRandom()
        {

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableName, _columns, orderByRandom: true);

                var package = await DatabaseHelper.ExecuteReaderAsync(_connection, query,
                    null, async reader =>
                    {
                        if (await reader.ReadAsync())
                        {
                            Package p = new Package(
                                id: Guid.Parse(reader["id"].ToString()));

                            return p;
                        }

                        return null;
                    });
                if (package != null)
                {
                    return new OperationResult<Package>(true, HttpStatusCode.OK, null, package);
                }

                return new OperationResult<Package>(false, HttpStatusCode.NOT_FOUND, "Package not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<Package>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }

        }

        public async Task<OperationResult<bool>> Update(Package package)
        {
            if (package == null)
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
                string updateQuery = QueryBuilder.BuildUpdateQuery(_tableName, _columns.Skip(1), "id");
                var rowAffected =
                    await DatabaseHelper.ExecuteNonQueryAsync(_connection, updateQuery,
                        cmd => AddParameters(cmd, package));
                if (rowAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }

                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "Package not found.");
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


        public Task<OperationResult<IEnumerable<Package>>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<Package>> GetByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult<bool>> Delete(Package package)
        {
            if (package == null)
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
                string deleteQuery = QueryBuilder.BuildDeleteQuery(_tableName, "id");
                var rowAffected =
                    await DatabaseHelper.ExecuteNonQueryAsync(_connection, deleteQuery,
                        cmd => AddParameters(cmd, package));
                if (rowAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }

                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "Package not found.");
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


        //public Task<IEnumerable<Package>> GetAll()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<Package> GetByUserId(Guid userId)
        //{
        //    const string packageQuery = "SELECT id FROM packages WHERE user_id = @user_id";
        //    using var packageCmd = new NpgsqlCommand(packageQuery, _connection);
        //    packageCmd.Parameters.AddWithValue("user_id", userId);

        //    Guid packageId;
        //    using (var reader = await packageCmd.ExecuteReaderAsync())
        //    {
        //        if (!await reader.ReadAsync())
        //        {
        //            throw new InvalidOperationException("Kein Paket für diesen Benutzer gefunden.");
        //        }
        //        packageId = reader.GetGuid(0);
        //    }

        //    const string cardQuery = @"
        //        SELECT c.id, c.name, c.damage, e.name AS element_name, e.description AS element_description
        //        FROM cards c
        //        JOIN elementtypes e ON c.elementtype_id = e.id
        //        WHERE c.package_id = @package_id";
        //    using var cardCmd = new NpgsqlCommand(cardQuery, _connection);
        //    cardCmd.Parameters.AddWithValue("package_id", packageId);

        //    using var cardReader = await cardCmd.ExecuteReaderAsync();

        //    var package = new Package { Id = packageId };
        //    while (await cardReader.ReadAsync())
        //    {
        //        var cardType = cardReader.GetString(3);
        //        Card card;

        //        if (cardType == "Monster")
        //        {
        //            card = new MonsterCard();
        //            {
        //                Id = cardReader.GetGuid(0),
        //                Name = cardReader.GetString(1),
        //                Damage = cardReader.GetInt32(2),
        //                ElementType = new ElementType
        //                {
        //                    Name = cardReader.GetString(4),
        //                    Description = cardReader.GetString(5)
        //                }
        //            };
        //        }
        //        else if (cardType == "Spell")
        //        {
        //            card = new SpellCard
        //            {
        //                Id = cardReader.GetGuid(0),
        //                Name = cardReader.GetString(1),
        //                Damage = cardReader.GetInt32(2),
        //                ElementType = new ElementType
        //                {
        //                    Name = cardReader.GetString(4),
        //                    Description = cardReader.GetString(5)
        //                }
        //            };
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("Unbekannter Kartentyp: " + cardType);
        //        }

        //        package.AddCard(card);
        //    }

        //    return package;
        //}

        private static void AddParameters(NpgsqlCommand command, Package package)
        {
            DatabaseHelper.AddParameter(command, "@id", package.Id);
        }
    }
}   