using Casino;
using Casino.BlackJack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BlackJack
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to the game of BlackJack!");

            /** 
             * get the number of people who will be playing 
             */
            bool validInput = false;
            int playerCount = 0;
            while (!validInput)
            {
                Console.WriteLine("How many players will be playing?");
                validInput = int.TryParse(Console.ReadLine(), out playerCount);
                if (!validInput) Console.WriteLine("Please enter a valid number.");
            }

            if (playerCount > 0)
            {
                /**
                 * if people, then start a new blackjack game
                 */
                Game game = new BlackJackGame();
                bool hasActivePlayers = true;

                // get each player's name and how many credits they start with
                for (int i = 0; i < playerCount; i++)
                {
                    Console.WriteLine("Enter Player {0}'s name:", i + 1);
                    string playerName = Console.ReadLine();

                    validInput = false;
                    int bank = 0;
                    while (!validInput)
                    {
                        Console.WriteLine("How many credits does {0} have?", playerName);
                        validInput = int.TryParse(Console.ReadLine(), out bank);
                        if (!validInput || bank < 1)
                        {
                            Console.WriteLine("Please enter a number greater than 0 with no decimals.");
                            validInput = false;
                        }
                    }

                    // add each player to the game
                    Player player = new Player(playerName, bank);
                    game += player;
                    player.isActivelyPlaying = true;
                }

                while (hasActivePlayers)
                {
                    try
                    {
                        /**
                         * if active players, play a round of blackjack
                         */
                        game.Play();
                        hasActivePlayers = false;
                        foreach (Player player in game.Players)
                        {
                            if (player.isActivelyPlaying)
                            {
                                hasActivePlayers = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //UpdateDBWithException(ex); // disable updating database with error message
                    }
                }
            }
            Console.WriteLine("Thank you for playing! Bye for now.");
            Console.ReadLine();
        }

        private static void UpdateDBWithException(Exception ex)
        {
            string connectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=TwentyOneGame;
                                        Integrated Security=True;Connect Timeout=30;Encrypt=False;
                                        TrustServerCertificate=False;ApplicationIntent=ReadWrite;
                                        MultiSubnetFailover=False";

            string queryString = @"INSERT INTO Exceptions (ExceptionType, ExceptionMessage, TimeStamp) VALUES
                                    (@ExceptionType, @ExceptionMessage, @TimeStamp)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.Add("@ExceptionType", SqlDbType.VarChar);
                command.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar);
                command.Parameters.Add("@TimeStamp", SqlDbType.DateTime);

                command.Parameters["@ExceptionType"].Value = ex.GetType().ToString();
                command.Parameters["@ExceptionMessage"].Value = ex.Message;
                command.Parameters["@TimeStamp"].Value = DateTime.Now;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
        private static List<ExceptionEntity> ReadExceptions()
        {
            string connectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=TwentyOneGame;
                                        Integrated Security=True;Connect Timeout=30;Encrypt=False;
                                        TrustServerCertificate=False;ApplicationIntent=ReadWrite;
                                        MultiSubnetFailover=False";

            string queryString = @"SELECT * FROM Exceptions";
            List<ExceptionEntity> Exceptions = new List<ExceptionEntity>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ExceptionEntity exception = new ExceptionEntity();
                    exception.Id = Convert.ToInt32(reader["Id"]);
                    exception.ExceptionType = reader["ExceptionType"].ToString();
                    exception.ExceptionMessage = reader["ExceptionMessage"].ToString();
                    exception.TimeStamp = Convert.ToDateTime(reader["TimeStamp"]);
                    Exceptions.Add(exception);
                }
                connection.Close();
            }
            return Exceptions;
        }
    }
}
