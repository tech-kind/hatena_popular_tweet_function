using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatenaPopularTweetFunction
{
    public class FeedItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int BookmarkCount { get; set; }
    }
}
