using CatalogManager;
using Database;
using Database.Models;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace VideoHandler
{
    public class DuplicateFinder
    {
        private readonly ILogger _logger;
        private readonly IOptions<CatalogImportOptions> _configuration;
        private readonly IVideoRepository _repository;

        private Dictionary<int, Histogram> _postThumbnails;
        private Dictionary<int, List<Video>> _candidates;
        private int _threshold;

        private Dictionary<int, Guid> _checked;
        private Dictionary<Guid, DuplicateContainer> _duplicateGroups;

        public DuplicateFinder(ILogger logger, IOptions<CatalogImportOptions> configuration, IVideoRepository repository)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = repository;

            _threshold = 1000;
            _postThumbnails = LoadHistograms();
            _candidates = LoadCandidates();

            _checked = LoadChecked();
            _duplicateGroups = LoadGroups();
        }

        public Dictionary<Guid, DuplicateContainer> Get()
        {
            return _duplicateGroups.Where(kv => kv.Value.Videos.Count > 1).ToDictionary();
        }

        public void Update()
        {
            // We'll redo the last one, but skip all previous videos. Everything must be done in ID order.
            var startFrom = _checked.Keys.Any() ? _checked.Keys.Max() : 1;
            var pendingVideos = _repository.GetViewableVideos().OrderBy(v => v.Id).Where(v => v.Id >= startFrom).ToList();

            foreach (var nextVideo in pendingVideos)
            {
                var foundGroup = false;
                foreach (var checkedId in _checked.Keys)
                {
                    // Don't compare against self. It'll always succeed.
                    if (nextVideo.Id == checkedId)
                    {
                        foundGroup = true;
                        continue;
                    }

                    var compareVideo = _repository.GetVideo(checkedId);

                    // If they're not the same length as each other, we can skip.
                    if (!AreSameLength(nextVideo.Length, compareVideo.Length))
                        continue;

                    // Compare the thumbnails
                    var h1 = GetHistogram(nextVideo.Id);
                    var h2 = GetHistogram(checkedId);
                    var distance = Compare(h1, h2);

                    if (distance < 300)
                    {
                        // On a match we want to add it to the previously checked video's group.
                        var groupGuid = _checked[checkedId];
                        var groupObj = _duplicateGroups[groupGuid];
                        groupObj.Videos.Add(nextVideo);
                        _checked.TryAdd(nextVideo.Id, groupGuid);
                        foundGroup = true;
                        break; // We're done for this video. No need to keep comparing. 
                    }
                }
                if (!foundGroup)
                {
                    var newGroupId = Guid.NewGuid();
                    var newGroupObj = new DuplicateContainer(newGroupId, nextVideo);
                    _duplicateGroups.TryAdd(newGroupId, newGroupObj);
                    _checked.TryAdd(nextVideo.Id, newGroupId);
                }
                SaveGroups();
                SaveChecked();
            }
        }

        private bool AreSameLength(TimeSpan a, TimeSpan b)
        {
            var distance = Math.Abs(a.TotalSeconds - b.TotalSeconds);

            return distance <= 1;
        }

        public int Compare(Histogram post1, Histogram post2)
        {
            int[] rDistance = [];
            int[] gDistance = [];
            int[] bDistance = [];

            for (int i = 0; i < post1.Buckets; i++)
            {
                rDistance = HistogramDistance(post1.Buckets, post1.Red, post2.Red);
                gDistance = HistogramDistance(post1.Buckets, post1.Green, post2.Green);
                bDistance = HistogramDistance(post1.Buckets, post1.Blue, post2.Blue);
            }

            var sum = rDistance.Sum() + gDistance.Sum() + bDistance.Sum();
            return sum;
        }

        private int[] HistogramDistance(int bucketCount, int[] post1, int[] post2)
        {
            var distance = new int[bucketCount];
            for (int i = 0; i < bucketCount; i++)
            {
                var d = Math.Abs(post1[i] - post2[i]);
                distance[i] = d;
            }
            return distance;
        }

        public Video TryGetPost(int postId)
        {
            var post = _repository.GetVideo(postId);
            return post;
        }

        public Histogram GetHistogram(int postId)
        {

            if (_postThumbnails.ContainsKey(postId))
                return _postThumbnails[postId];

            var post = TryGetPost(postId);

            if (post == null)
            {
                _postThumbnails.Add(postId, null);
                return null;
            }

            var url = _configuration.Value.InternalUrl + "/playback/Thumbnail/" + postId;
            var histogram = BuildHistogram(url);
            _postThumbnails.Add(postId, histogram);

            SaveHistograms();

            return histogram;
        }

        public Histogram BuildHistogram(string thumbnailUrl)
        {
            var image = GetImage(thumbnailUrl);
            var histogram = new Histogram(thumbnailUrl);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color c = image.GetPixel(x, y);
                    histogram.AddColour(c);
                }
            }
            return histogram;
        }

        public Bitmap GetImage(string thumbnailUrl)
        {
            using WebClient wc = new WebClient();
            var imageBytes = wc.DownloadData(new Uri(thumbnailUrl));
            using var stream = new MemoryStream(imageBytes);
            var image = new Bitmap(stream);
            return image;
        }

        public void SaveHistograms()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_postThumbnails, options);
            var path = Path.Combine(_configuration.Value.BucketDirectory, "histograms.json");
            File.WriteAllText(path, json);
        }

        public Dictionary<int, Histogram> LoadHistograms()
        {
            var path = Path.Combine(_configuration.Value.BucketDirectory, "histograms.json");
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Dictionary<int, Histogram>>(json);
            }
            catch (Exception ex)
            {
                return new();
            }
        }

        public Dictionary<int, List<Video>> LoadCandidates()
        {
            var path = Path.Combine(_configuration.Value.BucketDirectory, "duplicates.json");
            try
            {
                string json = File.ReadAllText(path);
                var idsOnly = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(json);
                return idsOnly.ToDictionary(k => k.Key, v => v.Value.Select(p => _repository.GetVideo(p)).ToList());
            }
            catch (Exception ex)
            {
                return new();
            }
        }

        public void SaveChecked()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_checked, options);
            var path = Path.Combine(_configuration.Value.BucketDirectory, "checked.json");
            File.WriteAllText(path, json);
        }

        public Dictionary<int, Guid> LoadChecked()
        {
            var path = Path.Combine(_configuration.Value.BucketDirectory, "checked.json");
            try
            {
                string json = File.ReadAllText(path);
                var idsOnly = JsonSerializer.Deserialize<Dictionary<int, Guid>>(json);
                return idsOnly;
            }
            catch (Exception ex)
            {
                return new();
            }
        }

        public void SaveGroups()
        {
            var dehydrated = _duplicateGroups.ToDictionary(k => k.Key, v => v.Value.Videos.Select(i => i.Id).ToList());
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(dehydrated, options);
            var path = Path.Combine(_configuration.Value.BucketDirectory, "groups.json");
            File.WriteAllText(path, json);
        }

        public Dictionary<Guid, DuplicateContainer> LoadGroups()
        {
            Dictionary<Guid, DuplicateContainer> _duplicateGroups;
            var path = Path.Combine(_configuration.Value.BucketDirectory, "groups.json");
            try
            {
                string json = File.ReadAllText(path);
                var idsOnly = JsonSerializer.Deserialize<Dictionary<Guid, List<int>>>(json);
                var hydrated = idsOnly.ToDictionary(k => k.Key, v => new DuplicateContainer(v.Key, v.Value.Select(p => _repository.GetVideo(p)).ToList()));
                return hydrated;
            }
            catch (Exception ex)
            {
                return new();
            }
        }

        public static List<List<int>> GroupConnectedElements(List<HashSet<int>> pairs)
        {
            Dictionary<int, HashSet<int>> graph = new Dictionary<int, HashSet<int>>();

            foreach (var pair in pairs)
            {
                int first = pair.First();
                int second = pair.Last();

                if (!graph.ContainsKey(first))
                    graph[first] = new HashSet<int>();
                if (!graph.ContainsKey(second))
                    graph[second] = new HashSet<int>();

                graph[first].Add(second);
                graph[second].Add(first);
            }

            List<List<int>> groups = new List<List<int>>();
            HashSet<int> visited = new HashSet<int>();

            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    List<int> group = new List<int>();
                    Stack<int> stack = new Stack<int>();
                    stack.Push(node);

                    while (stack.Count > 0)
                    {
                        int current = stack.Pop();
                        if (!visited.Contains(current))
                        {
                            visited.Add(current);
                            group.Add(current);
                            foreach (var neighbor in graph[current])
                            {
                                if (!visited.Contains(neighbor))
                                {
                                    stack.Push(neighbor);
                                }
                            }
                        }
                    }

                    groups.Add(group);
                }
            }

            // Sort each group for consistency with expected output format
            foreach (var group in groups)
            {
                group.Sort();
            }

            return groups;
        }

        public class Histogram
        {
            public Histogram() { }
            internal Histogram(string url)
            {
                Buckets = 4;
                Red = new int[Buckets];
                Green = new int[Buckets];
                Blue = new int[Buckets];
                Url = url;
            }
            public int Buckets { get; set; }
            public int[] Red { get; set; }
            public int[] Green { get; set; }
            public int[] Blue { get; set; }
            public string Url { get; set; }

            private int GetBucket(int value)
            {
                var bucketSize = 256 / Buckets;
                return value / bucketSize;
            }

            internal void AddColour(Color c)
            {
                Red[GetBucket(c.R)]++;
                Green[GetBucket(c.G)]++;
                Blue[GetBucket(c.B)]++;
            }
        }
    }

    public class DuplicateContainer
    {
        public DuplicateContainer(Guid groupId, Video video)
        {
            GroupId = groupId;
            TotalSeconds = (int)video.Length.TotalSeconds;
            Videos = new List<Video> { video };
        }
        public DuplicateContainer(Guid groupId, List<Video> videos)
        {
            GroupId = groupId;
            TotalSeconds = (int)videos.First().Length.TotalSeconds;
            Videos = videos;
        }

        public Guid GroupId { get; }
        public int TotalSeconds { get; }
        public List<Video> Videos { get; }
        public List<int> Ids
        {
            get
            {
                return Videos.Select(v => v.Id).Distinct().ToList();
            }
        }
    }
}
