using System;
using System.Collections.Generic;
using System.Linq;
using Casino.BlackJack;

namespace Casino
{
    public class Dealer
    {
        public string Name { get; set; }
        public Deck Deck { get; set; }
        public int Balance { get; set; }

        public void Deal(List<Card> Hand, bool hide = false)
        {
            Hand.Add(Deck.Cards.First());
            if (hide) Console.WriteLine("[ hidden ]\n");
            else Console.WriteLine(Deck.Cards.First().ToString() + "\n");
            Deck.Cards.RemoveAt(0);

            //using (StreamWriter file = new StreamWriter(@"C:\User\RobinDT\log.txt", true))
            //{
            //    file.WriteLine("card");
            //}
        }

        public void ShowHand(List<Card> Hand)
        {
            foreach (Card card in Hand)
            {
                Console.Write("({0} of {1}) ", card.Face, card.Suit);
            }
            Console.Write(" [Current highest value: {0}]", BlackJackRules.GetHighestScore(Hand));
        }
    }
}
