using MonsterTradingCardsGame.Enums;
using System;

namespace MonsterTradingCardsGame.Models
{
    /// <summary>
    /// Represents a monster card in the game.
    /// </summary>
    public class MonsterCard : Card
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonsterCard"/> class with the specified name, damage and element type.
        /// Creates also an instance of the <see cref="Card"/> parent class.
        /// </summary>
        /// <param name="name">The name of the monster card.</param>
        /// <param name="damage">The damage value of the monster card.</param>
        /// <param name="elementType">The element type of the monster card.</param>
        public MonsterCard(string name, float damage, ElementTypeEnum elementType)
            : base(name, damage, elementType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonsterCard"/> class with the specified name, damage and element type.
        /// Creates also an instance of the <see cref="Card"/> parent class.
        /// </summary>
        /// <param name="id">The unique identifier of the monster card.</param>
        /// <param name="name">The name of the monster card.</param>
        /// <param name="damage">The damage value of the monster card.</param>
        /// <param name="elementType">The element type of the monster card.</param>
        public MonsterCard(Guid id, string name, float damage, ElementTypeEnum elementType)
            : base(id, name, damage, elementType)
        {
        }
    }
}