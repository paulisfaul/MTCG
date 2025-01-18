using MonsterTradingCardsGame.Enums;
using System;

namespace MonsterTradingCardsGame.Models
{
    /// <summary>
    /// Represents a spell card in the game.
    /// </summary>
    public class SpellCard : Card
    {
        private static readonly Dictionary<(ElementTypeEnum, ElementTypeEnum), double> DamageModifiers = new()
        {
            { (ElementTypeEnum.Fire, ElementTypeEnum.Water), 0.5 },
            { (ElementTypeEnum.Fire, ElementTypeEnum.Normal), 2.0 },
            { (ElementTypeEnum.Water, ElementTypeEnum.Fire), 2.0 },
            { (ElementTypeEnum.Water, ElementTypeEnum.Normal), 0.5 },
            { (ElementTypeEnum.Normal, ElementTypeEnum.Fire), 0.5 },
            { (ElementTypeEnum.Normal, ElementTypeEnum.Water), 2.0 },
            // Kein Effekt bei anderen Kombinationen
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellCard"/> class with the specified name, damage and element type.
        /// Creates also an instance of the <see cref="Card"/> parent class.
        /// </summary>
        /// <param name="name">The name of the spell card.</param>
        /// <param name="damage">The damage value of the spell card.</param>
        /// <param name="elementType">The element type of the spell card.</param>
        public SpellCard(string name, float damage, ElementTypeEnum elementType)
            : base(name, damage, elementType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellCard"/> class with the specified name, damage and element type.
        /// Creates also an instance of the <see cref="Card"/> parent class.
        /// </summary>
        /// <param name="id">The unique identifier of the spell card.</param>
        /// <param name="name">The name of the spell card.</param>
        /// <param name="damage">The damage value of the spell card.</param>
        /// <param name="elementType">The element type of the spell card.</param>
        public SpellCard(Guid id, string name, float damage, ElementTypeEnum elementType)
            : base(id, name, damage, elementType)
        {
        }

        public double CalculateEffectiveDamage(Card opponentCard)
        {
            if (DamageModifiers.TryGetValue((ElementType, opponentCard.ElementType), out double modifier))
            {
                return Damage * modifier;
            }
            return Damage; // Standard: Kein Effekt
        }


    }
}