using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenVid.Areas.Playback.Models.Play;
using OpenVid.Models.Upload;
using Search;
using TagCache;
using Upload;

namespace OpenVid.Areas.Playback.Controllers
{
    [Area("playback")]
    public class Play : Controller
    {

        private Save _service;
        private UrlResolver _urlResolver;
        private IConfiguration _configuration;
        private TagManager _tagManager;

        public Play(Save service, UrlResolver urlResolver, TagManager tagManager, IConfiguration configuration)
        {
            _service = service;
            _urlResolver = urlResolver;
            _configuration = configuration;
            _tagManager = tagManager;
        }
        [Route("{id}")]
        public IActionResult Index(int id)
        {
            var video = _service.GetVideo(id);

            if (video == null)
                return NotFound();

            PlayViewModel viewModel = new PlayViewModel()
            {
                VideoSources = _urlResolver.GetVideoUrl(video),
                FileBaseUrl = _configuration["FileBaseUrl"]
            };

            if (!viewModel.VideoSources.Any()) return NotFound();

            var tagCollection = video.VideoTag.Select(x => x.Tag.Name);
            var tagSuggestions = new List<SuggestedTagViewModel>();

            // TODO - This can be cached
            foreach (var item in tagCollection)
            {
                tagSuggestions.Add(new SuggestedTagViewModel()
                {
                    TagName = item,
                    RelatedTags = _tagManager.GetRelatedTags(item).Select(t => new RelatedTagViewModel()
                    {
                        TagName = t,
                        AlreadyUsed = tagCollection.Contains(t)
                    }).ToList()
                });
            }

            viewModel.Update = new UpdateFormViewModel()
            {
                Id = video.Id,
                Name = video.Name,
                Description = video.Description,
                Tags = string.Join(" ", tagCollection) + " ",
                IsFlaggedForDeletion = video.IsDeleted,
                RatingId = video.RatingId ?? 0,
                PossibleRatings = _service.GetRatings(),
                FileBaseUrl = _configuration["FileBaseUrl"],
                SuggestedTags = tagSuggestions,
                Metadata = video.VideoSource.Select(s => new MetadataViewModel()
                {
                    Md5 = s.Md5,
                    Extension = s.Extension,
                    Width = s.Width,
                    Height = s.Height,
                    Size = s.Size,

                }).ToList()
            };

            return View(viewModel);
        }
    }
}
