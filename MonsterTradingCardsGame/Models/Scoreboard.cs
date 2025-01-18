using System;

namespace MonsterTradingCardsGame.Models
{
    public class ScoreboardEntry
    {
        public string Username { get; private set; }
        public UserStats Stats { get; private set; }

        public ScoreboardEntry(string username, UserStats stats)
        {
            Username = username;
            Stats = stats;
        }
    }

    public class Scoreboard
    {
        public IEnumerable<ScoreboardEntry> Entries { get; private set; }

        public Scoreboard(IEnumerable<ScoreboardEntry> entries)
        {
            Entries = entries;
        }
    }
}