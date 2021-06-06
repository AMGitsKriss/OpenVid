using Database.Models;
using OpenVid.Models;
using System.Collections.Generic;

namespace OpenVid.Models.Upload
{
    public class UpdateFormViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Md5 { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal Size { get; set; }
        public string Description { get; set; }
        public string Meta { get; set; }
        public string Tags { get; set; }
        public bool IsFlaggedForDeletion { get; set; }
        public int RatingId { get; set; }
        public List<Ratings> PossibleRatings { get; set; }
    }
}