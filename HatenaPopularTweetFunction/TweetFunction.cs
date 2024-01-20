using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tweetinvi;

namespace HatenaPopularTweetFunction
{
    public class TweetFunction
    {
        [FunctionName("TweetFunction")]
        public void Run([TimerTrigger("0 0 6,20 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // 環境変数を取得する
            var consumerKey = Environment.GetEnvironmentVariable("ConsumerKey", EnvironmentVariableTarget.Process);
            var consumerSecret = Environment.GetEnvironmentVariable("ConsumerSecret", EnvironmentVariableTarget.Process);
            var accessToken = Environment.GetEnvironmentVariable("AccessToken", EnvironmentVariableTarget.Process);
            var accessSecret = Environment.GetEnvironmentVariable("AccessSecret", EnvironmentVariableTarget.Process);

            var userClient = new TwitterClient(
                consumerKey,
                consumerSecret,
                accessToken,
                accessSecret
            );

            // はてなブックマークのテクノロジーの人気エントリーのRSSのURL
            string url = @"https://b.hatena.ne.jp/hotentry/it.rss";

            // RSSから、IEnumerable<FeedItem> 型のリストを取得する
            var feedItems = ReadRSS.ReadRSSFromURL(url);

            foreach (var item in feedItems)
            {
                if (item.BookmarkCount < 100)
                {
                    continue;
                }
                StringBuilder sb = new();
                sb.AppendLine(item.Title);
                sb.AppendLine("#エンジニアと繋がりたい");
                sb.AppendLine("#駆け出しエンジニアと繋がりたい");
                sb.AppendLine(item.Link);
                var parameters = new Dictionary<string, string>()
                {
                    { "text",  sb.ToString() },
                };

                // ペイロードのJSON文字列を作る
                var pollPayloadJson = JsonConvert.SerializeObject(parameters);
                // ペイロードを作る、Twitter API v2 は application/json を受け付ける
                var pollContent = new StringContent(pollPayloadJson, Encoding.UTF8, "application/json");
                // Tweetinviの内部APIでいい感じに認証設定してリクエストする
                _ = userClient.Execute.RequestAsync(pollRequest =>
                {
                    pollRequest.Url = "https://api.twitter.com/2/tweets";
                    pollRequest.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                    pollRequest.HttpContent = pollContent;
                });
            }
        }
    }
}
