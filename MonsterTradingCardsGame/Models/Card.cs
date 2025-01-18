using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models.Base;

namespace MonsterTradingCardsGame.Models
{
    public abstract class Card : BaseModel
    {

        public string Name { get; private set; }

        public float Damage { get; private set; }

        //fire water normal
        public ElementTypeEnum ElementType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class with the specified name, damage and element type. Creates a new GUID.
        /// </summary>
        /// <param name="name">The name of the card.</param>
        /// <param name="damage">The damage of the card.</param>
        /// <param name="elementType">The element type of the card.</param>
        public Card(string name, float damage, ElementTypeEnum elementType)
        {
            Id = Guid.NewGuid();
            Name = name;
            Damage = damage;
            ElementType = elementType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class with the specified id, name, damage, and element type.
        /// This constructor is used when the card already exists in the database and therefore has a GUID.
        /// </summary>
        /// <param name="id">The unique identifier of the card.</param>
        /// <param name="name">The name of the card.</param>
        /// <param name="damage">The damage value of the card.</param>
        /// <param name="elementType">The element type of the card.</param>
        public Card(Guid id, string name, float damage, ElementTypeEnum elementType)
        {
            Id = id;
            Name = name;
            Damage = damage;
            ElementType = elementType;
        }
    }
}
