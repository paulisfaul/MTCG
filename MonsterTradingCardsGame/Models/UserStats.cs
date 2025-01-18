using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace MonsterTradingCardsGame.Models
{
    public class UserStats
    {
        public int Wins { get; set; }
        public int Losses { get; set; }

        public int Draws { get; set; }
        public int Elo { get; set; }

        public UserStats()
        {
            Wins = 0;
            Losses = 0;
            Draws = 0;
            Elo = 100;
        }

        public UserStats(int elo, int wins, int losses, int draws)
        {
            Elo = elo;
            Wins = wins;
            Losses = losses;
            Draws = draws;
        }


        public override string ToString()
        {
            return $@"
  Wins: {Wins}
  Losses: {Losses}
  Draws: {Draws}
  Elo: {Elo}";
        }
    }
}