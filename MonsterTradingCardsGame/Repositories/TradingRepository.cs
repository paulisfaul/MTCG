using System;
using System.Collections.Generic;
using System.Data;
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
    public class TradingRepository : ITradingRepository
    {
        private readonly NpgsqlConnection _connection;
        public static readonly string _tableName = $"\"{DatabaseConfig.SchemaName}\".\"tradingoffer\"";
        public static readonly IEnumerable<string> _columns = new List<string> { "id", "offerer_id", "requested_card_type", "requested_minimum_dmg", "offered_card_id", "requested_card_id" ,"open", "automatic_accept"};
        public TradingRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }
        public async Task<OperationResult<TradingOffer>> Create(TradingOffer tradingOffer)
        {
            if (tradingOffer == null)
            {
                throw new ArgumentNullException("TradingOffer darf nicht leer sein.");

            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string insertTradingOfferQuery = QueryBuilder.BuildInsertQuery(_tableName, _columns);
                var tradingRowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, insertTradingOfferQuery,
                    cmd => AddParameters(cmd, tradingOffer));
                if (tradingRowAffected > 0)
                {
                    return new OperationResult<TradingOffer>(true, HttpStatusCode.CREATED, null, tradingOffer);
                }

                return new OperationResult<TradingOffer>(false, HttpStatusCode.BAD_REQUEST, "TradingOffer could not be created.");
            }
            catch (Exception ex)
            {
                return new OperationResult<TradingOffer>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<IEnumerable<TradingOffer>>> GetOpenTradingOffers(User user)
        {
            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {

                string query = QueryBuilder.BuildSelectQuery(_tableName, _columns, "open");

                var tradingOffers = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@open", true);
                }, async reader =>
                {
                    var offers = new List<TradingOffer>();
                    while (await reader.ReadAsync())
                    {
                        var cardTypeEnum = (CardTypeEnum)Enum.Parse(typeof(CardTypeEnum), reader["requested_card_type"].ToString());
                        Guid? requestedCardId = reader["requested_card_id"] != DBNull.Value ? Guid.Parse(reader["requested_card_id"].ToString()) : (Guid?)null;

                        var offer = new TradingOffer
                        (
                            id: Guid.Parse(reader["id"].ToString()),
                            offerer: Guid.Parse(reader["offerer_id"].ToString()),
                            offeredCardId: Guid.Parse(reader["offered_card_id"].ToString()),
                            requestedCardId: requestedCardId,
                            requestedCardTypeEnum: cardTypeEnum,
                            requestedMinimumDamage: Convert.ToSingle(reader["requested_minimum_dmg"]),
                            open: Convert.ToBoolean(reader["open"]),
                            automaticAccept: Convert.ToBoolean(reader["automatic_accept"])
                        );

                        // IMPORTANT: THIS MUST BE USED TO ONLY SHOW TRADING OFFERS OF OTHER PEOPLE;
                        if (offer.Offerer == user.Id)
                        {
                            continue;
                        }

                        offers.Add(offer);
                    }

                    return offers;
                });

                return new OperationResult<IEnumerable<TradingOffer>>(true, HttpStatusCode.OK, null, tradingOffers);
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<TradingOffer>>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<TradingOffer>> GetTradingOfferById(Guid tradingOfferId)
        {
            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableName, _columns, "id");

                var tradingOffer = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@id", tradingOfferId);
                }, async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        var cardTypeEnum = (CardTypeEnum)Enum.Parse(typeof(CardTypeEnum), reader["requested_card_type"].ToString());
                        Guid? requestedCardId = reader["requested_card_id"] != DBNull.Value ? Guid.Parse(reader["requested_card_id"].ToString()) : (Guid?)null;

                        return new TradingOffer
                        (
                            id: Guid.Parse(reader["id"].ToString()),
                            offerer: Guid.Parse(reader["offerer_id"].ToString()),
                            offeredCardId: Guid.Parse(reader["offered_card_id"].ToString()),
                            requestedCardId: requestedCardId,
                            requestedCardTypeEnum: cardTypeEnum,
                            requestedMinimumDamage: Convert.ToSingle(reader["requested_minimum_dmg"]),
                            open: Convert.ToBoolean(reader["open"]),
                            automaticAccept: Convert.ToBoolean(reader["automatic_accept"])

                        );
                    }
                    return null;
                });

                if (tradingOffer != null)
                {
                    return new OperationResult<TradingOffer>(true, HttpStatusCode.OK, null, tradingOffer);
                }
                return new OperationResult<TradingOffer>(false, HttpStatusCode.NOT_FOUND, "TradingOffer not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<TradingOffer>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<bool>> Update(TradingOffer offer)
        {
            if (offer == null)
            {
                throw new ArgumentNullException(nameof(offer));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildUpdateQuery(_tableName, _columns, "id");

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, query, cmd =>
                {
                    AddParameters(cmd, offer);
                    DatabaseHelper.AddParameter(cmd, "@id_kc", offer.Id);
                });

                if (rowsAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }

                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "TradingOffer not found.");
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

        public async Task<OperationResult<bool>> Delete(Guid tradingId)
        {
            if (tradingId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(tradingId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildDeleteQuery(_tableName, "id");

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, query, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@id", tradingId);
                });

                if (rowsAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }

                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "TradingOffer not found.");
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

        public async Task<OperationResult<Guid>> GetOwnerIdOfTradingById(Guid tradingId)
        {
            if (tradingId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(tradingId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableName, new List<string> { "offerer_id" }, "id");

                Guid userId = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                    DatabaseHelper.AddParameter(cmd, "@id", tradingId), async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        var tradingId = reader["offerer_id"] as Guid?;
                        return tradingId ?? Guid.Empty;
                    }

                    return Guid.Empty;
                });

                if (userId != Guid.Empty)
                {
                    return new OperationResult<Guid>(true, HttpStatusCode.OK, null, userId);

                }

                return new OperationResult<Guid>(false, HttpStatusCode.NOT_FOUND, "Owner Id not found.");

            }
            catch (Exception ex)
            {
                return new OperationResult<Guid>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        private static void AddParameters(NpgsqlCommand command, TradingOffer tradingOffer)
        {
            DatabaseHelper.AddParameter(command, "@id", tradingOffer.Id);
            DatabaseHelper.AddParameter(command, "@offered_card_id", tradingOffer.OfferedCardId);
            DatabaseHelper.AddParameter(command, "@requested_card_id", tradingOffer.RequestedCardId);
            DatabaseHelper.AddParameter(command, "@offerer_id", tradingOffer.Offerer);
            var cardtypeParam = command.Parameters.AddWithValue("requested_card_type", tradingOffer.RequestedCardTypeEnum.ToString());
            cardtypeParam.DataTypeName = "cardtype"; // Name des benutzerdefinierten Enum-Typs in der Datenbank
            DatabaseHelper.AddParameter(command, "@open", tradingOffer.Open);
            DatabaseHelper.AddParameter(command, "@automatic_accept", tradingOffer.AutomaticAccept );
            DatabaseHelper.AddParameter(command, "@requested_minimum_dmg", tradingOffer.RequestedMinimumDamage);
        }
    }
}