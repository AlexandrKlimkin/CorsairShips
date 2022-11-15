using System;
using System.Collections.Generic;
using System.Linq;

namespace EloRating
{
    public class EloRatingCalculator
    {
        private const float DifferenceEnoughToWin = 400f; //enough for win with 0.91 probability

        private List<EloRatingCoeff> _ratingCoefficients = new List<EloRatingCoeff>
        {
            new EloRatingCoeff { Coeff = 10, Rating = 2400, PlayedGames = 30 },
            new EloRatingCoeff { Coeff = 20, Rating = 0, PlayedGames = 30 },
            new EloRatingCoeff { Coeff = 40, Rating = 0, PlayedGames = 0 }
        };

        private Dictionary<EloRatingGameResult, float>  _gameResultToPoints = new Dictionary<EloRatingGameResult, float>
        {
            { EloRatingGameResult.GameResultWin, 1 },
            { EloRatingGameResult.GameResultDraw, 0.5f },
            { EloRatingGameResult.GameResultLoose, 0f }
        };

        private Dictionary<EloDivision, int> _pointsToDivisions = new Dictionary<EloDivision, int>
        {
            { EloDivision.Beginners, 400 },
            { EloDivision.Amateur, 800 },
            { EloDivision.SemiProfessional, 1400 },
            { EloDivision.Professional, 2000 },
            { EloDivision.Champions, 2600 },
            { EloDivision.Highest, 3200 }
        };

        public int CalculateNewRating(EloPlayer player, EloPlayer opponent, EloRatingGameResult result)
        {
            var expected = ExpectedValue(player, opponent);
            var coeff = GetCoeff(player);
            var points = _gameResultToPoints[result];

            var newRating = (int) Math.Round(player.Rating + coeff*(points - expected));
            var newDivision = GetDivision(newRating);

            if (newRating < player.Rating)
            {
                if (GetDivision(player.Rating) != newDivision)
                {
                    //we can't move to lower division, so clamp it to lower possible value in current division
                    return _pointsToDivisions[newDivision];
                }
            }

            if (newRating < 0)
            {
                newRating = 0;
            }

            return newRating;
        }

        public EloDivision GetDivision(int rating)
        {
            var result = EloDivision.Beginners;
            foreach (var division in _pointsToDivisions.Keys)
            {
                if (_pointsToDivisions[division] > rating)
                {
                    return result;
                }
                result = division;
            }
            return result;
        }

        public void ProcessGameResult(EloPlayer player, EloPlayer opponent, EloRatingGameResult playerResult)
        {
            var newPlayerRating = CalculateNewRating(player, opponent, playerResult);
            var newOpponentRating = CalculateNewRating(opponent, player, GetOpponentResult(playerResult));
            
            player.Rating = newPlayerRating;
            player.PlayedGames++;

            opponent.Rating = newOpponentRating;
            opponent.PlayedGames++;
        }

        private float ExpectedValue(EloPlayer player, EloPlayer opponent)
        {
            var power = (opponent.Rating - player.Rating) / DifferenceEnoughToWin;
            return 1f / (float)(1 + Math.Pow(10, power));
        }

        public float GetCoeff(EloPlayer player)
        {
            var result = _ratingCoefficients.Where(r => (r.PlayedGames <= player.PlayedGames) && (r.Rating <= player.Rating))
                    .OrderBy(r => r.Coeff)
                    .First()
                    .Coeff;

            return result;
        }

        private EloRatingGameResult GetOpponentResult(EloRatingGameResult result)
        {
            switch (result)
            {
                case EloRatingGameResult.GameResultWin: return EloRatingGameResult.GameResultLoose;
                case EloRatingGameResult.GameResultLoose: return EloRatingGameResult.GameResultWin;
                default: return EloRatingGameResult.GameResultDraw;
            }
        }
    }
}