using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models.Base;
using MonsterTradingCardsGame.Models.RequestModels;

namespace MonsterTradingCardsGame.Models
{
    /// <summary>
    /// Represents a package of cards.
    /// </summary>
    public class Package : BaseModel, IEnumerable<Card>
    {
        private List<Card> _cards = new List<Card>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> class with the specified package request.
        /// Creates a new GUID for the package.
        /// </summary>
        /// <param name="packageRequest">The package request containing the cards to be added to the package.</param>
        public Package(PackageRequestDto packageRequest)
        {
            Id = Guid.NewGuid();
            foreach (var card in packageRequest)
            {
                ElementTypeEnum element;
                try
                {
                    element = (ElementTypeEnum)Enum.Parse(typeof(ElementTypeEnum), card.ElementType);
                }
                catch
                {
                    throw new ArgumentException();
                }
                if (card.CardType == CardTypeEnum.Monster.ToString())
                {
                    this.AddCard(new MonsterCard(card.Name, card.Damage, element));
                }
                if (card.CardType == CardTypeEnum.Spell.ToString())
                {
                    this.AddCard(new SpellCard(card.Name, card.Damage, element));
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> class with a specific id and empty cards.
        /// Creates a new GUID for the package.
        /// </summary>
        /// <param name="id">The package id of the in the database stored package.</param>
        public Package(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Adds a card to the package.
        /// </summary>
        /// <param name="card">The card to be added.</param>
        public void AddCard(Card card)
        {
            _cards.Add(card);
        }

        /// <summary>
        /// Removes a card from the package.
        /// </summary>
        /// <param name="card">The card to be removed.</param>
        public void RemoveCard(Card card)
        {
            _cards.Remove(card);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of cards.
        /// </summary>
        /// <returns>An enumerator for the collection of cards.</returns>
        public IEnumerator<Card> GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of cards.
        /// </summary>
        /// <returns>An enumerator for the collection of cards.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}