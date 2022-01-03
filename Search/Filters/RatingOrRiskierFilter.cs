using Database;
using Database.Models;
using Search.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Search.Filters
{
    [Filter(ParameterType.RatingOrRiskier)]
    public class RatingOrRiskierFilter : IFilter
    {
        private Videos _repo;
        public RatingOrRiskierFilter(Videos repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            return Rating(parameter.Value);
        }

        public List<Video> Rating(string rating)
        {
            rating = rating.ToLower();
            var selectedRating = _repo.GetRatings().FirstOrDefault(r => r.Name.ToLower() == rating);

            List<Video> result = _repo.GetAllVideos().Where(x => x.RatingId >= selectedRating.Id).ToList();

            return result.ToList();

        }
    }
}
