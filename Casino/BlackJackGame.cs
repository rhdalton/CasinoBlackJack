using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Casino.Interfaces;

namespace Casino.BlackJack
{
    public class BlackJackGame : Game, IWalkAway
    {
        public BlackJackDealer Dealer { get; set; }
        public override void Play()
        {
            int cardDelay = 700;
            Dealer = new BlackJackDealer();
            foreach (Player player in Players)
            {
                player.Hand = new List<Card>();
                player.Stay = false;
            }
            Dealer.Hand = new List<Card>();
            Dealer.Stay = false;
            Dealer.Deck = new Deck();
            Dealer.Deck.Shuffle();

            foreach (Player player in Players)
            {
                if (player.isActivelyPlaying)
                {
                    Console.WriteLine("\nPlayer {0}, place your bet: (Your current balance is {1})", player.Name, player.Balance);
                    int bet = Convert.ToInt32(Console.ReadLine());
                    bool successfulBet = player.Bet(bet);
                    if (!successfulBet)
                    {
                        return;
                    }
                    Bets[player] = bet;
                }
            }

            Console.WriteLine("\nDealing cards...\n");
            System.Threading.Thread.Sleep(cardDelay);
            for (int i = 0; i < 2; i++)
            {
                foreach (Player player in Players)
                {
                    if (player.isActivelyPlaying)
                    {
                        Console.Write("{0}: ", player.Name);
                        Dealer.Deal(player.Hand);
                        if (i == 1)
                        {
                            bool blackJack = BlackJackRules.CheckForBlackJack(player.Hand);
                            if (blackJack)
                            {
                                Console.WriteLine("Blackjack! {0} wins {1}", player.Name, Bets[player] * 1.5);
                                player.Balance += Convert.ToInt32((Bets[player] * 1.5) + Bets[player]);
                                Bets.Remove(player);
                                player.isActivelyPlaying = false;
                                break;
                            }
                        }
                        System.Threading.Thread.Sleep(cardDelay);
                    }
                }
                Console.Write("Dealer: ");
                if (i == 1)
                {
                    Dealer.Deal(Dealer.Hand, true);
                    bool blackJack = BlackJackRules.CheckForBlackJack(Dealer.Hand);
                    if (blackJack)
                    {
                        Console.WriteLine("Dealer has Blackjack! Everyone loses.");
                        foreach (KeyValuePair<Player, int> entry in Bets)
                        {
                            Dealer.Balance += entry.Value;
                        }
                        foreach (Player player in Players)
                        {
                            Bets.Remove(player);
                            player.isActivelyPlaying = false;
                        }
                        break;
                    }
                }
                else
                {
                    Dealer.Deal(Dealer.Hand);
                }
                System.Threading.Thread.Sleep(cardDelay);
            }

            foreach (Player player in Players)
            {
                while (!player.Stay)
                {
                    Console.WriteLine("\nIt is player {0}'s turn.", player.Name);
                    Console.WriteLine("Your cards are: ", player.Name);
                    foreach (Card card in player.Hand)
                    {
                        Console.Write("({0}) ", card.ToString());
                    }
                    Console.Write("  [Current Highest Value: {0}]\n", BlackJackRules.GetHighestScore(player.Hand));
                    Console.Write("\n\nDo you want to Hit or Stay? ");
                    string answer = Console.ReadLine().ToLower();
                    if (answer == "stay" || answer == "s")
                    {
                        player.Stay = true;
                        break;
                    }
                    else if (answer == "hit" || answer == "h")
                    {
                        Console.Write("{0}: ", player.Name);
                        Dealer.Deal(player.Hand);
                    }
                    bool busted = BlackJackRules.isBusted(player.Hand);
                    if (busted)
                    {
                        Dealer.Balance += Bets[player];
                        Console.WriteLine("Your new total value of {0} is greater than 21!", BlackJackRules.GetLowestScore(player.Hand));
                        Console.WriteLine("{0} Busted! You lose your bet of {1}. Your balance is now {2}.\n\n", player.Name, Bets[player], player.Balance);
                        Bets.Remove(player);
                        player.isActivelyPlaying = false;
                        break;
                    }
                }
            }

            /* Check if any players still have bets left, continue with the Dealer hand */
            if (Bets.Count > 0)
            {
                Console.WriteLine("\nDealer has a hand of:");
                Dealer.ShowHand(Dealer.Hand);

                Dealer.isBusted = BlackJackRules.isBusted(Dealer.Hand);
                Dealer.Stay = BlackJackRules.ShouldDealerStay(Dealer.Hand);
                while (!Dealer.Stay && !Dealer.isBusted)
                {
                    Console.WriteLine("\n\nDealer is hitting...");
                    Dealer.Deal(Dealer.Hand);
                    Dealer.isBusted = BlackJackRules.isBusted(Dealer.Hand);
                    Dealer.Stay = BlackJackRules.ShouldDealerStay(Dealer.Hand);

                    Console.WriteLine("\nDealer has a hand of:");
                    Dealer.ShowHand(Dealer.Hand);
                }
                if (Dealer.Stay)
                {
                    Console.WriteLine("\n\nDealer is staying.\n");
                }
                else
                {
                    Console.WriteLine("Dealer Busted!");
                    foreach (KeyValuePair<Player, int> entry in Bets)
                    {
                        Console.WriteLine("{0} won {1}!", entry.Key.Name, entry.Value);
                        Players.Where(x => x.Name == entry.Key.Name).First().Balance += (entry.Value * 2);
                        Dealer.Balance -= entry.Value;
                    }
                    return;
                }

                foreach (Player player in Players)
                {
                    if (player.isActivelyPlaying)
                    {
                        bool? playerWon = BlackJackRules.CompareHands(player.Hand, Dealer.Hand);
                        if (playerWon == null)
                        {
                            Console.WriteLine("Player {0} pushes. Player bet is returned to player.", player.Name);
                            player.Balance += Bets[player];
                        }
                        else if (playerWon == true)
                        {
                            Console.WriteLine("Player {0} wins! Player wins {1}.", player.Name, Bets[player]);
                            player.Balance += (Bets[player] * 2);
                        }
                        else
                        {
                            Console.WriteLine("Player {0} loses! You lose your bet of {1}.", player.Name, Bets[player]);
                            Dealer.Balance += Bets[player];
                        }
                    }
                }
            }

            foreach (Player player in Players)
            {
                if (player.Balance == 0)
                {
                    Console.WriteLine("Sorry {0}, your balance is 0!", player.Name);
                    player.isActivelyPlaying = false;
                    break;
                }
                Console.WriteLine("Player {0}, do you want to play again?", player.Name);
                string answer = Console.ReadLine().ToLower();
                if (answer == "yes" || answer == "y")
                {
                    player.isActivelyPlaying = true;
                }
                else
                {
                    player.isActivelyPlaying = false;
                }
            }
        }
        public override void ListPlayers()
        {
            Console.WriteLine("Welcome 21 Players:");
            base.ListPlayers();
        }
        public void WalkAway(Player player)
        {
            throw new NotImplementedException();
        }
    }
}
