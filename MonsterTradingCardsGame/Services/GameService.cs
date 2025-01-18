using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Interfaces;

namespace MonsterTradingCardsGame.Services
{
    public class GameService
    {
        private readonly BattleService _battleService;
        private readonly CardService _cardService;
        private readonly UserService _userService;

        public GameService(BattleService battleService, CardService cardService, UserService userService)
        {
            _battleService = battleService;
            _cardService = cardService;
            _userService = userService;
        }

        private static readonly object _lock = new();
        private const int eloLost = 5;
        private const int eloWon = 3;

        private Queue<(User user, TaskCompletionSource<BattleResult> tcs)> _waitingPlayers = new();

        public Task<BattleResult> StartGameAsync(User user)
        {
            TaskCompletionSource<BattleResult> tcs = new TaskCompletionSource<BattleResult>();

            lock (_lock)
            {
                if (_waitingPlayers.Count > 0)
                {
                    // Es wartet bereits ein Spieler, starte die Schlacht
                    var (opponent, opponentTcs) = _waitingPlayers.Dequeue();

                    // Starte die Schlacht in einem neuen Thread
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            var battleResult = _battleService.ExecuteBattle(user, opponent).Result;

                            var eloResult = UpdateStats(battleResult);

                            var cardUpdateResult = _cardService.ConfigureDeck(battleResult.Player1.Deck.Select(card=>card.Id).ToList(), battleResult.Player1, true);
                            var cardUpdateResult2 = _cardService.ConfigureDeck(battleResult.Player2.Deck.Select(card => card.Id).ToList(), battleResult.Player2, true);
                            
                            tcs.SetResult(battleResult); // Signalisiere dem aktuellen Spieler
                            opponentTcs.SetResult(battleResult); // Signalisiere dem wartenden Spieler
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex); // Setze Ausnahme für den aktuellen Spieler
                            opponentTcs.SetException(ex); // Setze Ausnahme für den wartenden Spieler
                        }
                    });

                    return tcs.Task; // Beide Spieler warten auf dieses Ergebnis
                }
                else
                {
                    // Füge den Spieler in die Warteschlange ein und erstelle ihre TaskCompletionSource
                    _waitingPlayers.Enqueue((user, tcs));
                    return tcs.Task; // Erster Spieler wartet auf einen Gegner
                }
            }
        }

        private bool UpdateStats(BattleResult battleResult)
        {
            int winnerNumber = battleResult.Winner;
            User player1 = battleResult.Player1;
            User player2 = battleResult.Player2;



            if (winnerNumber == 0)
                return true;

            var winner = winnerNumber == 1 ? player1 : player2;
            var loser = winnerNumber == 1 ? player2 : player1;

            winner.UserStats.Elo += eloWon;
            winner.UserStats.Wins += 1;

            loser.UserStats.Elo -= eloLost;
            if (loser.UserStats.Elo < 0)
            {
                loser.UserStats.Elo = 0;
            }

            loser.UserStats.Losses += 1;
            var result1 = _userService.UpdateUser(winner).Result;

            if (!result1.Success)
            {
                return false;
            }

            var result2 = _userService.UpdateUser(loser).Result;

            if (!result2.Success)
            {
                return false;
            }

            return true;
        }

    }
}