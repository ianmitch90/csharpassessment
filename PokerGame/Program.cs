using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pokerGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var stdin = Console.In;
            var players = new List<Player>();

            using (var stream = new StreamReader(Console.OpenStandardInput()))
            {
                var lines = stream.ReadToEnd().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                players = lines.Select(line => BuildPlayer(line)).ToList();
            }
            var winnerIds = Winners(players).Select(player => player.Id);

            Console.WriteLine("{0}", string.Join(" ", winnerIds));
        }
        public static Player BuildPlayer(string inputLine)
        {
            string[] inputs = inputLine.Split();

            var hand = new Hand();

            for (int i = 1; i < inputs.Length; i++)
            {

                hand.Cards.Add(RebuildCard(inputs[i]));
            }
            return new Player(inputs[0], hand);
        }
        public static class ValueSetter<T>
        {
            public static T DisplayValue(string name)
            {
                var type = typeof(T);

                foreach (var field in type.GetFields())
                {
                    if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attribute)
                    {
                        if (attribute.Name == name)
                        {
                            return (T)field.GetValue(null);
                        }
                    }
                    else
                    {
                        if (field.Name == name)
                            return (T)field.GetValue(null);
                    }
                }

                throw new InvalidOperationException();
            }
        }

        public static PlayingCard RebuildCard(string input)
        {

            return new PlayingCard()
            {
                Suit = (Suit)ValueSetter<Suit>.DisplayValue(input[1].ToString()),
                Rank = (Rank)ValueSetter<Rank>.DisplayValue(input[0].ToString())
            };
        }

        public static List<Player> Winners(List<Player> players)
        {
            var reorderPlayers = players.OrderByDescending(player => player.Hand).ToList();
            return reorderPlayers.Where(player => player.Hand.CompareTo(reorderPlayers[0].Hand) == 0).ToList();
        }
    }
    public class Hand : IComparable<Hand>
    {
        public List<PlayingCard> Cards { get; set; }

        public Hand()
        {
            Cards = new List<PlayingCard>();
        }

        public int Score
        {
            get
            {
                var score = 0;

                if (IsStraightFlush)
                {
                    score = 5;
                }
                else if (IsThreeOfKind)
                {
                    score = 4;
                }
                else if (IsStraight)
                {
                    score = 3;
                }
                else if (IsFlush)
                {
                    score = 2;
                }
                else if (IsPair)
                {
                    score = 1;
                }
                return score;
            }
        }
        public bool HasCard(Rank rank)
        {
            return Cards.Where(card => card.Rank == rank).Any();
        }

        public bool MatchRank(List<PlayingCard> cards, int numberOfMatches)
        {
            return cards.GroupBy(card => card.Rank).Where(item => item.Count() == numberOfMatches).Any();
        }

        public bool IsPair
        {
            get
            {
                return MatchRank(Cards, 2);
            }
        }

        public bool IsThreeOfKind
        {
            get
            {
                return MatchRank(Cards, 3);
            }
        }

        public bool IsFlush
        {
            get
            {
                return Cards.GroupBy(card => card.Suit).Count() == 1;
            }
        }

        public bool IsStraight
        {
            get
            {
                var result = false;
                if (Cards.Count > 0)
                {
                    if (HasCard(Rank.Ace)
                        && HasCard(Rank.Two)
                        && HasCard(Rank.Three))
                    {
                        result = true;
                    }
                    else
                    {
                        var orderedCards = Cards.OrderBy(card => card.Rank).ToList();
                        result = !orderedCards.Where((card, index) => index != orderedCards.Count - 1 && (int)card.Rank + 1 != (int)orderedCards[index + 1].Rank).Any();
                    }
                }

                return result;
            }
        }

        public bool IsStraightFlush
        {
            get
            {
                return IsStraight && IsFlush;
            }
        }

        public int CompareTo(Hand other)
        {
            int score = 0;
            if (Score > other.Score)
            {
                score = 1;
            }
            else if (Score < other.Score)
            {
                score = -1;
            }
            else
            {
                int index = 0;

                var orderedCards = Cards.OrderByDescending(item => item.Rank).ToList();
                var otherOrderedCards = Cards.OrderByDescending(item => item.Rank).ToList();
                while (index < orderedCards.Count() && score == 0)
                {
                    score = orderedCards[index].Rank.CompareTo(otherOrderedCards[index].Rank);
                    index++;
                }
            }

            return score;
        }

    }

    // definitions below ------
    public enum Rank
    {
        [Display(Name = "2")]
        Two,

        [Display(Name = "3")]
        Three,

        [Display(Name = "4")]
        Four,

        [Display(Name = "5")]
        Five,

        [Display(Name = "6")]
        Six,

        [Display(Name = "7")]
        Seven,

        [Display(Name = "8")]
        Eight,

        [Display(Name = "9")]
        Nine,

        [Display(Name = "T")]
        Ten,

        [Display(Name = "J")]
        Jack,

        [Display(Name = "Q")]
        Queen,

        [Display(Name = "K")]
        King,

        [Display(Name = "A")]
        Ace
    }

    public enum Suit
    {
        [Display(Name = "h")]
        Hearts,

        [Display(Name = "d")]
        Diamonds,

        [Display(Name = "s")]
        Spades,

        [Display(Name = "c")]
        Clovers
    }

    public class Player
    {
        public Hand Hand { get; set; }
        public string Id { get; set; }

        public Player(string id, Hand hand)
        {
            Id = id;
            Hand = hand;
        }
    }

    public class PlayingCard
    {
        public Rank Rank { get; set; }
        public Suit Suit { get; set; }
    }


}
