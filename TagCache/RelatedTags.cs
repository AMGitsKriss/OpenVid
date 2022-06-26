using CacheMeIfYouCan;
using Database;
using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TagCache
{
    public class RelatedTags
    {
        private readonly Videos _repository;
        private ICachedObject<Dictionary<string, List<string>>> _relatedTags;

        public RelatedTags(Videos repository)
        {
            _repository = repository;
        }

        public Dictionary<string, List<string>> Build()
        {
            var tags = _repository.GetAllTags();
            Dictionary<string, List<string>> relatedTags = new Dictionary<string, List<string>>();

            foreach (var tag in tags)
            {
                var tagGroup = GetMutualTags(tag).GroupBy(t => t).OrderByDescending(t => t.Count()).Select(t => t.Key).Take(15);

                relatedTags.TryAdd(tag.Name, tagGroup.ToList());
            }

            return relatedTags;
        }

        private IQueryable<string> GetMutualTags(Tag tag)
        {
            var videoTags = _repository.VideosByTag().Where(t => t.TagId == tag.Id);
            var allTags = videoTags.SelectMany(vt => vt.Video.VideoTag).Where(t => t.TagId != tag.Id).Select(t => t.Tag.Name);

            return allTags;
        }

        public List<string> Get(string tag)
        {
            if (_relatedTags == null)
                Build();

            _relatedTags.Value.TryGetValue(tag, out var result);
            return result;
        }

        public void Refresh()
        {
            _relatedTags.RefreshValue(TimeSpan.MinValue);
        }

        public void Initialize()
        {

            _relatedTags = CachedObjectFactory
                                        .ConfigureFor(Build)
                                        .WithRefreshInterval(TimeSpan.FromHours(6))
                                        .Build();

            _relatedTags.Initialize();
        }
    }
}
