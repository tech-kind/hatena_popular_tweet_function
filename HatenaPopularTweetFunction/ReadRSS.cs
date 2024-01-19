using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HatenaPopularTweetFunction
{
    public static class ReadRSS
    {
        // 引数のurlに、RSSのURLが入ってくる想定
        public static IEnumerable<FeedItem> ReadRSSFromURL(string url)
        {
            // RSSのデータをXmlDocumentクラスを用いて読み込む
            var xml = new XmlDocument();
            xml.Load(url);

            // RSS 1.0 の要素を取得するための事前準備
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            nsmgr.AddNamespace("rss", "http://purl.org/rss/1.0/");
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            nsmgr.AddNamespace("hatena", "http://www.hatena.ne.jp/info/xmlns#");

            // item要素を格納するためにFeedItemクラスのリストを作成
            var items = new List<FeedItem>();

            // すべてのitem要素を取得する
            foreach (XmlElement node in xml.SelectNodes("/rdf:RDF/rss:item", nsmgr))
            {
                // titleやlinkなどの値を取得する
                var title = node.SelectNodes("rss:title", nsmgr)[0].InnerText;
                var link = node.SelectNodes("rss:link", nsmgr)[0].InnerText;
                var description = node.SelectNodes("rss:description", nsmgr)[0].InnerText;
                var date = DateTime.Parse(node.SelectNodes("dc:date", nsmgr)[0].InnerText);
                var bookmarkCount = int.Parse(node.SelectNodes("hatena:bookmarkcount", nsmgr)[0].InnerText);

                // 取得した値をもとにFeedItemを作成してリストに追加
                items.Add(new FeedItem()
                {
                    Title = title,
                    Link = link,
                    Description = description,
                    Date = date,
                    BookmarkCount = bookmarkCount
                });
            }

            // FeedItemのリストを返す
            return items;
        }
    }
}
