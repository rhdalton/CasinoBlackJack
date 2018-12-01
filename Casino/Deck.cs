using System;
using System.Collections.Generic;

namespace Casino
{
    public class Deck
    {
        public Deck()
        {
            Cards = new List<Card>();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Card card = new Card();
                    card.Face = (Face)j;
                    card.Suit = (Suit)i;
                    Cards.Add(card);
                }
            }
        }

        public List<Card> Cards { get; set; }

        public void Shuffle(int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                List<Card> TempList = new List<Card>();
                Random rnd = new Random();

                while (Cards.Count > 0)
                {
                    int rndIndex = rnd.Next(0, Cards.Count);
                    TempList.Add(Cards[rndIndex]);
                    Cards.RemoveAt(rndIndex);
                }
                Cards = TempList;
            }
        }
    }
}
