using Grauenwolf.TravellerTools.Maps;
using System.Collections.Generic;

namespace Grauenwolf.TravellerTools.TradeCalculator
{
    /// <summary>
    /// A manifest is the cargo, people, etc. that want to travel from one location to another.
    /// </summary>
    public class Manifest
    {

        public List<TradeBid> Bids { get; } = new List<TradeBid>();
        public World Destination { get; set; }
        public FreightList FreightList { get; set; }
        public List<TradeOffer> Offers { get; } = new List<TradeOffer>();
        public World Origin { get; set; }
        public PassengerList PassengerList { get; set; }
    }
}
