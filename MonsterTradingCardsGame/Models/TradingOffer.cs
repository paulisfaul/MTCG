using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models.Base;

namespace MonsterTradingCardsGame.Models
{
    public class TradingOffer : BaseModel
    {
        public Guid Offerer { get; set; }

        public Guid OfferedCardId { get; set; }

        public Guid? RequestedCardId { get; set; }



        public CardTypeEnum RequestedCardTypeEnum { get; set; }
        
        public float RequestedMinimumDamage { get; set; }

        public bool Open { get; set; }

        public bool AutomaticAccept { get; set; }

        public TradingOffer( Guid offerer, Guid offeredCardId, CardTypeEnum requestedCardTypeEnum, float requestedMinimumDamage, bool automaticAccept)
        {
            Id = Guid.NewGuid();
            Offerer = offerer;
            OfferedCardId = offeredCardId;
            RequestedCardId = null;
            RequestedCardTypeEnum = requestedCardTypeEnum;
            RequestedMinimumDamage = requestedMinimumDamage;
            Open = true;
            AutomaticAccept = automaticAccept;
        }

        public TradingOffer(Guid id, Guid offerer, Guid offeredCardId, Guid? requestedCardId, CardTypeEnum requestedCardTypeEnum, float requestedMinimumDamage, bool open, bool automaticAccept)
        {
            Id = id;
            Offerer = offerer;
            OfferedCardId = offeredCardId;
            RequestedCardId = requestedCardId;
            RequestedCardTypeEnum = requestedCardTypeEnum;
            RequestedMinimumDamage = requestedMinimumDamage;
            Open = open;
            AutomaticAccept = automaticAccept;
        }
    }
}
