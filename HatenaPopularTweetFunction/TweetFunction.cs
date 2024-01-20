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

            // ���ϐ����擾����
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

            // �͂Ăȃu�b�N�}�[�N�̃e�N�m���W�[�̐l�C�G���g���[��RSS��URL
            string url = @"https://b.hatena.ne.jp/hotentry/it.rss";

            // RSS����AIEnumerable<FeedItem> �^�̃��X�g���擾����
            var feedItems = ReadRSS.ReadRSSFromURL(url);

            foreach (var item in feedItems)
            {
                if (item.BookmarkCount < 100)
                {
                    continue;
                }
                StringBuilder sb = new();
                sb.AppendLine(item.Title);
                sb.AppendLine("#�G���W�j�A�ƌq���肽��");
                sb.AppendLine("#�삯�o���G���W�j�A�ƌq���肽��");
                sb.AppendLine(item.Link);
                var parameters = new Dictionary<string, string>()
                {
                    { "text",  sb.ToString() },
                };

                // �y�C���[�h��JSON����������
                var pollPayloadJson = JsonConvert.SerializeObject(parameters);
                // �y�C���[�h�����ATwitter API v2 �� application/json ���󂯕t����
                var pollContent = new StringContent(pollPayloadJson, Encoding.UTF8, "application/json");
                // Tweetinvi�̓���API�ł��������ɔF�ؐݒ肵�ă��N�G�X�g����
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
