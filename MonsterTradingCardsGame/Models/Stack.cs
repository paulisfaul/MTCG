using System;
using System.Collections;
using System.Collections.Generic;
using MonsterTradingCardsGame.Models.Base;

namespace MonsterTradingCardsGame.Models
{
    public class Stack : BaseModel, IEnumerable<Card>
    {
        private List<Card> _cards;

        public Stack()
        {
            _cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            _cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            _cards.Remove(card);
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}