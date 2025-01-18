using System;
using System.Collections.Generic;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Services
{
    public class BattleService
    {
        private const int MaxRounds = 100;

        public async Task<BattleResult> ExecuteBattle(User player1, User player2)
        {
            var battleLog = new List<string>();
            int round = 0;

            while (player1.Deck.Cards.Count > 0 && player2.Deck.Cards.Count > 0 && round < MaxRounds)
            {
                round++;
                battleLog.Add($"Round {round}:");

                // Wähle zufällige Karten aus den Decks
                var card1 = player1.Deck.GetRandomCard();
                var card2 = player2.Deck.GetRandomCard();

                battleLog.Add($"{player1.UserCredentials.Username} plays {card1.Name} (Damage: {card1.Damage})");
                battleLog.Add($"{player2.UserCredentials.Username} plays {card2.Name} (Damage: {card2.Damage})");

                // Berechne den Gewinner der Runde
                var roundResult = CalculateRoundResult(card1, card2);

                switch (roundResult.Winner)
                {
                    case RoundWinner.Player1:
                        player1.Deck.AddCard(card2); // Spieler 1 übernimmt die Karte von Spieler 2
                        player2.Deck.RemoveCard(card2);
                        battleLog.Add($"{player1.UserCredentials.Username} wins the round and takes {card2.Name}!");
                        break;
                    case RoundWinner.Player2:
                        player2.Deck.AddCard(card1); // Spieler 2 übernimmt die Karte von Spieler 1
                        player1.Deck.RemoveCard(card1);
                        battleLog.Add($"{player2.UserCredentials.Username} wins the round and takes {card1.Name}!");
                        break;
                    case RoundWinner.Draw:
                        battleLog.Add("It is a draw. No cards are moved.");
                        break;
                }
            }

            // Bestimme den Gewinner des Spiels
            int winner = 0;
            if (player1.Deck.Cards.Count > player2.Deck.Cards.Count)
            {
                winner = 1;
            }
            else if (player2.Deck.Cards.Count > player1.Deck.Cards.Count)
            {
                winner = 2;
            }

            return new BattleResult
            {
                Winner = winner,
                Player1 = player1,
                Player2 = player2,
                Log = battleLog,
                IsDraw = winner == null
            };
        }

        private RoundResult CalculateRoundResult(Card card1, Card card2)
        {
            var specialRules = new List<Func<Card, Card, RoundResult?>>
            {
                // Goblins vs Dragons
                (c1, c2) => c1.Name == MonsterTypeEnum.Goblin.ToString() && c2.Name == MonsterTypeEnum.Dragon.ToString()
                    ? new RoundResult { Winner = RoundWinner.Player2 }
                    : null,
                (c1, c2) => c2.Name == MonsterTypeEnum.Goblin.ToString() && c1.Name == MonsterTypeEnum.Dragon.ToString()
                    ? new RoundResult { Winner = RoundWinner.Player1 }
                    : null,

                // Wizzard vs Ork
                (c1, c2) => c1.Name == MonsterTypeEnum.Wizzard.ToString() && c2.Name == MonsterTypeEnum.Ork.ToString()
                    ? new RoundResult { Winner = RoundWinner.Player1 }
                    : null,
                (c1, c2) => c2.Name == MonsterTypeEnum.Wizzard.ToString() && c1.Name == MonsterTypeEnum.Ork.ToString()
                    ? new RoundResult { Winner = RoundWinner.Player2 }
                    : null,

                // Knight vs WaterSpell
                (c1, c2) => c1.Name == MonsterTypeEnum.Knight.ToString() && c2 is SpellCard && c2.ElementType == ElementTypeEnum.Water
                    ? new RoundResult { Winner = RoundWinner.Player2 }
                    : null,
                (c1, c2) => c2.Name == MonsterTypeEnum.Knight.ToString() && c1 is SpellCard && c1.ElementType == ElementTypeEnum.Water
                    ? new RoundResult { Winner = RoundWinner.Player1 }
                    : null,

                // Kraken vs Spells
                (c1, c2) => c1.Name == MonsterTypeEnum.Kraken.ToString() && c2 is SpellCard
                    ? new RoundResult { Winner = RoundWinner.Player1 }
                    : null,
                (c1, c2) => c2.Name == MonsterTypeEnum.Kraken.ToString() && c1 is SpellCard
                    ? new RoundResult { Winner = RoundWinner.Player2 }
                    : null,

                // FireElves vs Dragons
                (c1, c2) => c1.Name == MonsterTypeEnum.Elv.ToString() && c2.Name == MonsterTypeEnum.Dragon.ToString()
                    ? new RoundResult { Winner = RoundWinner.Player1 }
                    : null,
                (c1, c2) => c2.Name == MonsterTypeEnum.Elv.ToString() && c1.Name == MonsterTypeEnum.Dragon.ToString()
                    ? new RoundResult { Winner = RoundWinner.Player2 }
                    : null
            };

            // Prüfe spezielle Regeln
            foreach (var rule in specialRules)
            {
                var result = rule(card1, card2);
                if (result != null)
                {
                    return result;
                }
            }

            // Allgemeine Schadensberechnung
            double damage1 = card1.Damage;
            double damage2 = card2.Damage;

            if (card1 is SpellCard)
                damage1 = ((SpellCard)card1).CalculateEffectiveDamage(card2);

            if (card2 is SpellCard)
                damage2 = ((SpellCard)card2).CalculateEffectiveDamage(card1);

            return CompareDamage(damage1, damage2);
        }




        private RoundResult CompareDamage(double damage1, double damage2)
        {
            if (damage1 > damage2)
                return new RoundResult { Winner = RoundWinner.Player1 };
            if (damage2 > damage1)
                return new RoundResult { Winner = RoundWinner.Player2 };
            return new RoundResult { Winner = RoundWinner.Draw };
        }
    }

    public class RoundResult
    {
        public RoundWinner Winner { get; set; }
    }

    public enum RoundWinner
    {
        Player1,
        Player2,
        Draw
    }

    public class BattleResult
    {
        public int Winner { get; set; }
        public User Player1 { get; set; }
        public User Player2 { get; set; }
        public List<string> Log { get; set; } = new();
        public bool IsDraw { get; set; }
    }
}
