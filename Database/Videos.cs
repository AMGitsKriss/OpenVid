using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    // Scaffold-DbContext "Server=orion;Database=OpenVid;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
    public class Videos
    {
        OpenVidContext _context;

        public Videos()
        {
            _context = new OpenVidContext();
        }

        public List<Video> GetAllVideos()
        {
            return _context.Video.OrderByDescending(x => x.Id).OrderBy(x => Guid.NewGuid()).Take(100).ToList();
        }

        public Video GetVideo(string md5)
        {
            return _context.Video.Include(x => x.VideoTag).ThenInclude(x => x.Tag).FirstOrDefault(x => x.Md5 == md5);
        }

        public Video GetVideo(int id)
        {
            return _context.Video.FirstOrDefault(x => x.Id == id);
        }

        public List<Tag> GetAllTags()
        {
            var result = _context.Tag.Include(x => x.VideoTag).ThenInclude(x => x.Video).Where(x => x.VideoTag.Count() > 0).OrderByDescending(x => x.VideoTag.Count()).ThenBy(x => x.Name).ToList();
            return result;
        }

        public List<Video> Search(string search)
        {
            var query = search.Trim().ToLower().Split(" ");
            var result = (from t in _context.Tag
                          join vt in _context.VideoTag on t.Id equals vt.TagId
                          join v in _context.Video on vt.VideoId equals v.Id
                          where query.Contains(t.Name.ToLower()) || query.Contains(v.Name)
                          select v).DistinctBy(x => x.Id).ToList();
            return result;
        }

        public Video SaveVideo(Video video)
        {
            try
            {
                if (video.Id == 0)
                {
                    _context.Video.Add(video);
                }
                else
                {
                    _context.Video.Update(video);
                }

                _context.SaveChanges();

                return video;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Tag> DefineTags(List<string> tags)
        {
            try
            {
                var existingTags = _context.Tag.Select(x => x.Name.ToLower()).ToList();

                foreach (var unsafeTag in tags)
                {
                    string tag = unsafeTag.Trim().ToLower();
                    if (!existingTags.Contains(tag))
                    {
                        _context.Tag.Add(new Tag() { Name = tag });
                    }
                }

                _context.SaveChanges();

                return _context.Tag.Where(x => tags.Contains(x.Name)).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<Tag> SaveTagsForVideo(Video video, List<Tag> tags)
        {
            try
            {
                var tagIDs = tags.Select(x => x.Id);
                var removeTags = _context.VideoTag.Where(x => video.Id == x.VideoId && !tagIDs.Contains(x.TagId)).ToList();
                var existingTags = _context.VideoTag.Where(x => video.Id == x.VideoId && tagIDs.Contains(x.TagId)).Select(x => x.TagId);

                _context.VideoTag.RemoveRange(removeTags);

                foreach (var tag in tags)
                {
                    if (!existingTags.Contains(tag.Id))
                        _context.VideoTag.Add(new VideoTag() { TagId = tag.Id, VideoId = video.Id });
                }

                _context.SaveChanges();

                return tags;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
