using Database.Models;
using OpenVid.Areas.VideoManagement.Models.Upload;
using OpenVid.Models;
using System.Collections.Generic;

namespace OpenVid.Areas.Playback.Models.Update
{
    public class UpdateFormViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public bool IsFlaggedForDeletion { get; set; }
        public int RatingId { get; set; }
        public List<MetadataViewModel> Metadata { get; set; }
        public List<Ratings> PossibleRatings { get; set; }
        public List<SuggestedTagViewModel> SuggestedTags { get; set; }
    }
}