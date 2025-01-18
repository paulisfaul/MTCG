using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.Database;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Helpers;
using MonsterTradingCardsGame.Repositories.Interfaces;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace MonsterTradingCardsGame.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly NpgsqlConnection _connection;
        private readonly string _tableNameCard = $"\"{DatabaseConfig.SchemaName}\".\"card\"";
        private readonly string _tableNameDeck = $"\"{DatabaseConfig.SchemaName}\".\"deck\"";

        private readonly IEnumerable<string> _columnsCard = new List<string> { "id", "name", "damage", "package_id", "elementtype", "user_id", "cardtype"};
        private readonly IEnumerable<string> _columnsDeck = new List<string> { "id", "user_id"};

        public CardRepository(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<OperationResult<Card>> Create(Card card, Guid? packageId = null)
        {
            if(card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string insertCardQuery = QueryBuilder.BuildInsertQuery(_tableNameCard, _columnsCard);
                var cardRowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, insertCardQuery, cmd => AddCardParameters(cmd, card, packageId));
                if (cardRowAffected > 0)
                {
                    return new OperationResult<Card>(true, HttpStatusCode.CREATED, null, card);

                }
                return new OperationResult<Card>(false, HttpStatusCode.BAD_REQUEST, $"Card {card.Name} could not be created.");
            }
            catch
            {
                return new OperationResult<Card>(false, HttpStatusCode.BAD_REQUEST, $"Card {card.Name} could not be created.");
            }

            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<Deck>> CreateDeck(Deck deck, Guid userId)
        {
            if (deck.Id == Guid.Empty|| userId == Guid.Empty)
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
                string insertDeckQuery = QueryBuilder.BuildInsertQuery(_tableNameDeck, _columnsDeck);
                var deckRowAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, insertDeckQuery, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@id", deck.Id);
                    DatabaseHelper.AddParameter(cmd, "@user_id", userId);
                });

                if(deckRowAffected > 0)
                {
                    return new OperationResult<Deck>(true, HttpStatusCode.CREATED, null, deck);
                }

                return new OperationResult<Deck>(false, HttpStatusCode.BAD_REQUEST, "Deck could not be created.");
            }
            catch (NpgsqlException ex)
            {
                return new OperationResult<Deck>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, "Deck could not be created.");
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<IEnumerable<Card>>> CreateCardsForPackage(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package darf nicht null sein.");
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            using var transaction = await _connection.BeginTransactionAsync();

            var createdCards = new List<Card>();
            try
            {
                foreach (var card in package)
                {
                    var result = await Create(card, package.Id);
                    if (!result.Success)
                    {
                        await transaction.RollbackAsync();
                        return new OperationResult<IEnumerable<Card>>(false, HttpStatusCode.BAD_REQUEST, result.Message);
                    }

                    createdCards.Add((Card)result.Data);
                }

                if (!createdCards.Any())
                {
                    return new OperationResult<IEnumerable<Card>>(true, HttpStatusCode.CREATED, "No cards were created.", null);
                }

                await transaction.CommitAsync();
                return new OperationResult<IEnumerable<Card>>(true, HttpStatusCode.CREATED, "Cards for Package created.", createdCards);
            }
            catch
            {
                await transaction.RollbackAsync();
                return new OperationResult<IEnumerable<Card>>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, "Cards for Package could not be created.", null);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<IEnumerable<Card>>> GetCardsByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            try
            {

                string query = QueryBuilder.BuildSelectQuery(_tableNameCard, _columnsCard, "user_id");

                var cards = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                    DatabaseHelper.AddParameter(cmd, "@user_id", userId), async reader =>
                {
                    var cards = new List<Card>();
                    while (await reader.ReadAsync())
                    {
                        var cardtypeString = Convert.ToString(reader["cardtype"]);
                        if (!Enum.TryParse<CardTypeEnum>(cardtypeString, true, out var cardType))
                        {
                            cardType = CardTypeEnum.Monster;
                        }

                        var id = reader.GetGuid(reader.GetOrdinal("id"));
                        var name = reader["name"] as string ?? string.Empty;
                        var damage = Convert.ToSingle(reader["damage"]);
                        var elementType = (ElementTypeEnum)Enum.Parse(typeof(ElementTypeEnum), reader["elementtype"].ToString());



                        switch (cardType)
                        {
                            case CardTypeEnum.Monster:
                            {
                                MonsterCard monsterCard = new MonsterCard(
                                    id: id,
                                    name: name,
                                    damage: damage,
                                    elementType: elementType
                                );
                                cards.Add(monsterCard);
                                break;
                            }
                            case CardTypeEnum.Spell:
                            {
                                SpellCard spellCard = new SpellCard(
                                    id: id,
                                    name: name,
                                    damage: damage,
                                    elementType: elementType
                                );
                                cards.Add(spellCard);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    }

                    return cards;
                });

                if (cards.Any())
                {
                    return new OperationResult<IEnumerable<Card>>(true, HttpStatusCode.OK, null, cards);
                }
                return new OperationResult<IEnumerable<Card>>(false, HttpStatusCode.NOT_FOUND, "No cards found.", null);

            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<Card>>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message, null);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<Card>> GetCardById(Guid cardId)
        {
            if (cardId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(cardId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableNameCard, _columnsCard, "id");

                var card = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@id", cardId);
                }, async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        var cardtypeString = Convert.ToString(reader["cardtype"]);
                        if (!Enum.TryParse<CardTypeEnum>(cardtypeString, true, out var cardType))
                        {
                            cardType = CardTypeEnum.Monster;
                        }

                        var id = reader.GetGuid(reader.GetOrdinal("id"));
                        var name = reader["name"] as string ?? string.Empty;
                        var damage = Convert.ToSingle(reader["damage"]);
                        var elementType = (ElementTypeEnum)Enum.Parse(typeof(ElementTypeEnum), reader["elementtype"].ToString());

                        switch (cardType)
                        {
                            case CardTypeEnum.Monster:
                                return new MonsterCard(
                                    id: id,
                                    name: name,
                                    damage: damage,
                                    elementType: elementType
                                ) as Card;
                            case CardTypeEnum.Spell:
                                return new SpellCard(
                                    id: id,
                                    name: name,
                                    damage: damage,
                                    elementType: elementType
                                ) as Card;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    return null;
                });

                if (card != null)
                {
                    return new OperationResult<Card>(true, HttpStatusCode.OK, null, card);
                }
                return new OperationResult<Card>(false, HttpStatusCode.NOT_FOUND, "Card not found.");
            }
            catch (Exception ex)
            {
                return new OperationResult<Card>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<Guid>> GetOwnerIdOfCardById(Guid cardId)
        {
            if (cardId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(cardId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildSelectQuery(_tableNameCard, new List<string> { "user_id" }, "id");

                Guid userId = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                    DatabaseHelper.AddParameter(cmd, "@id", cardId), async reader =>
                {
                    if (await reader.ReadAsync())
                    {
                        var userId = reader["user_id"] as Guid?;
                        return userId ?? Guid.Empty;
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



        public async Task<OperationResult<bool>> Update(Package package, User user)
        {
            if (package == null || user == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {

                string query = QueryBuilder.BuildUpdateQuery(_tableNameCard, ["user_id", "package_id"], "package_id");

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, query, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@package_id", null);
                    DatabaseHelper.AddParameter(cmd, "@user_id", user.Id);
                    DatabaseHelper.AddParameter(cmd, "@package_id_kc", package.Id);

                });


                if (rowsAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }

                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "Card not found.");
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


        public async Task<OperationResult<Deck>> GetDeckCardsByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                // Erstellen der benutzerdefinierten Abfrage
                string query = QueryBuilder.BuildCustomSelectQuery(
                    _tableNameCard,
                    new List<string> { $"{_tableNameCard}.id", $"{_tableNameCard}.name", $"{_tableNameCard}.damage", $"{_tableNameCard}.elementtype", $"{_tableNameCard}.cardtype" },
                    $"INNER JOIN {_tableNameDeck} ON {_tableNameDeck}.id = {_tableNameCard}.deck_id",
                    $"{_tableNameDeck}.user_id = @user_id"
                );

                var cards = await DatabaseHelper.ExecuteReaderAsync(_connection, query, cmd =>
                    DatabaseHelper.AddParameter(cmd, "@user_id", userId), async reader =>
                    {
                        var cards = new List<Card>();
                        while (await reader.ReadAsync())
                        {
                            var cardtypeString = Convert.ToString(reader["cardtype"]);
                            if (!Enum.TryParse<CardTypeEnum>(cardtypeString, true, out var cardType))
                            {
                                cardType = CardTypeEnum.Monster;
                            }

                            var id = reader.GetGuid(reader.GetOrdinal("id"));
                            var name = reader["name"] as string ?? string.Empty;
                            var damage = Convert.ToSingle(reader["damage"]);
                            var elementType = (ElementTypeEnum)Enum.Parse(typeof(ElementTypeEnum), reader["elementtype"].ToString());

                            switch (cardType)
                            {
                                case CardTypeEnum.Monster:
                                    {
                                        MonsterCard monsterCard = new MonsterCard(
                                    id: id,
                                    name: name,
                                    damage: damage,
                                    elementType: elementType
                                );
                                        cards.Add(monsterCard);
                                        break;
                                    }
                                case CardTypeEnum.Spell:
                                    {
                                        SpellCard spellCard = new SpellCard(
                                    id: id,
                                    name: name,
                                    damage: damage,
                                    elementType: elementType
                                );
                                        cards.Add(spellCard);
                                        break;
                                    }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        return cards;
                    });

                if (cards.Any())
                {
                    var deck = new Deck
                    {
                        Id = Guid.NewGuid(), // Hier können Sie die tatsächliche Deck-ID setzen, falls verfügbar
                        UserId = userId,
                    };

                    foreach (var card in cards)
                    {
                        deck.AddCard(card);
                    }
                    return new OperationResult<Deck>(true, HttpStatusCode.OK, null, deck);
                }
                return new OperationResult<Deck>(false, HttpStatusCode.NOT_FOUND, "No cards found in the deck.", null);
            }
            catch (Exception ex)
            {
                return new OperationResult<Deck>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message, null);
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }


        public async Task<OperationResult<bool>> ConfigureDeck(IEnumerable<Guid> cardIds, Guid userId)
        {
            if (userId == Guid.Empty || !cardIds.Any())
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "UserId und Deck dürfen nicht leer sein.");
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }
            using var transaction = await _connection.BeginTransactionAsync();

            try
            {
                // Abfrage, um die deck_id des Benutzers zu erhalten
                string getDeckIdQuery = QueryBuilder.BuildSelectQuery(_tableNameDeck, new List<string> { "id" }, "user_id");

                var deckId = await DatabaseHelper.ExecuteReaderAsync(_connection, getDeckIdQuery, cmd =>
                    DatabaseHelper.AddParameter(cmd, "@user_id", userId), async reader =>
                    {
                        if (await reader.ReadAsync())
                        { 
                            return reader.GetGuid(reader.GetOrdinal("id"));
                        }
                        return Guid.Empty;
                    });

                if (deckId == Guid.Empty)
                {
                    return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "Deck für den Benutzer nicht gefunden.");
                }

                // Abfrage, um die deck_id aller Karten des Benutzers auf null zu setzen
                string resetUserCardsQuery = $@"
                UPDATE {_tableNameCard}
                SET deck_id = NULL, user_id = @user_id
                WHERE deck_id = (SELECT id FROM {_tableNameDeck} WHERE user_id = @user_id)";


                var resetRowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, resetUserCardsQuery, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@user_id", userId);
                });


                // Abfrage, um die neuen Karten zu aktualisieren und die deck_id zu setzen
                string updateCardsQuery = $@"
                UPDATE {_tableNameCard}
                SET deck_id = @deck_id, user_id = NULL
                WHERE id = ANY(@card_ids)";

                var updateRowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, updateCardsQuery, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@deck_id", deckId);
                    DatabaseHelper.AddParameter(cmd, "@card_ids", cardIds);
                });

                if (updateRowsAffected == 0)
                {
                    await transaction.RollbackAsync();
                    return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Keine Karten zum Aktualisieren gefunden.");
                }

                await transaction.CommitAsync();
                return new OperationResult<bool>(true, HttpStatusCode.OK, "Deck erfolgreich konfiguriert.", true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OperationResult<bool>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, $"Ein Fehler ist aufgetreten: {ex.Message}");
            }
            finally
            {
                if (wasClosed)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<OperationResult<bool>> Update(Card card, Guid userId)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var wasClosed = _connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await _connection.OpenAsync();
            }

            try
            {
                string query = QueryBuilder.BuildUpdateQuery(_tableNameCard, new List<string> { "user_id" }, "id");

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(_connection, query, cmd =>
                {
                    DatabaseHelper.AddParameter(cmd, "@user_id", userId);
                    DatabaseHelper.AddParameter(cmd, "@id", card.Id);
                });

                if (rowsAffected > 0)
                {
                    return new OperationResult<bool>(true, HttpStatusCode.OK, null, true);
                }

                return new OperationResult<bool>(false, HttpStatusCode.NOT_FOUND, "Card not found.");
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

        private static void AddCardParameters(NpgsqlCommand command, Card card, Guid? packageId)
        {
            command.Parameters.AddWithValue("id", card.Id);
            command.Parameters.AddWithValue("name", card.Name);
            command.Parameters.AddWithValue("damage", card.Damage);
            if (packageId != null)
            {
                command.Parameters.AddWithValue("package_id", packageId);
            }
            else
            {
                command.Parameters.Add("package_id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = DBNull.Value;
            }

            command.Parameters.Add("user_id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = DBNull.Value;

            var elementTypeParam = command.Parameters.AddWithValue("elementtype", card.ElementType.ToString());
            elementTypeParam.DataTypeName = "elementtype"; // Name des benutzerdefinierten Enum-Typs in der Datenbank

            var cardType = card is MonsterCard ? "Monster" : "Spell";

            var cardTypeParam = command.Parameters.AddWithValue("cardtype", cardType);
            cardTypeParam.DataTypeName = "cardtype"; // Name des benutzerdefinierten Enum-Typs in der Datenbank
        }

    }

}