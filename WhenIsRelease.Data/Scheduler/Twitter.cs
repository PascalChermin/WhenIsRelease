using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WhenIsRelease.Data.Utils;
using WhenIsRelease.Models;
using Game = WhenIsRelease.Models.GameReleases;
using Movie = WhenIsRelease.Models.MovieReleases;

namespace WhenIsRelease.Data.Scheduler
{
    public class Twitter : ISocial
    {
        private readonly ILogger _logger;
        private readonly string _consumerKey;
        private readonly string _consumerKeySecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
        private readonly HMACSHA1 _sigHasher;
        private readonly DateTime _epochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Twitter endpoint for sending tweets
        /// </summary>
        private readonly string _TwitterStatusAPI;
        /// <summary>
        /// Twitter endpoint for uploading images
        /// </summary>
        private readonly string _TwitterMediaAPI;
        /// <summary>
        /// Current tweet limit
        /// </summary>
        private readonly int _limit;
        private readonly string GAMEDEFAULTIMAGE = "https://www.giantbomb.com/api/image/original/3026329-gb_default-16_9.png";

        public Twitter(ILogger<IMaintenance> logger)
        {
            _logger = logger;
            _TwitterStatusAPI = Environment.GetEnvironmentVariable("WhenIsReleaseTwitterStatusAPI") ?? "https://api.twitter.com/1.1/statuses/update.json";
            _TwitterMediaAPI = Environment.GetEnvironmentVariable("WhenIsReleaseTwitterMediaAPI") ?? "https://upload.twitter.com/1.1/media/upload.json";

            _consumerKey = Environment.GetEnvironmentVariable("WhenIsReleaseTwitterConsumerKey");
            _consumerKeySecret = Environment.GetEnvironmentVariable("WhenIsReleaseTwitterConsumerKeySecret");
            _accessToken = Environment.GetEnvironmentVariable("WhenIsReleaseTwitterAccessToken");
            _accessTokenSecret = Environment.GetEnvironmentVariable("WhenIsReleaseTwitterAccessTokenSecret");
            int.TryParse(Environment.GetEnvironmentVariable("WhenIsReleaseTwitterLimit") ?? "280", out _limit);

            _sigHasher = new HMACSHA1(
                new ASCIIEncoding().GetBytes($"{_consumerKeySecret}&{_accessTokenSecret}")
            );
        }

        public void PublishReleasesOfTheDay(IEnumerable<IRelease> releases)
        {
            _logger.LogTrace("Publishing the releases of the day");
            if (releases.Count() > 0)
            {
                _logger.LogTrace($"{releases.Count()} releases found");
                var releaseTweets = this.GroupReleases(releases);
                var tweets = this.PrepareTweets(releaseTweets);
                _logger.LogTrace($"Sending out {tweets.Count()} tweets");
                this.PostToTwitter(tweets);
            }
            else
            {
                _logger.LogInformation("No releases to tweet today");
            }
        }

        private IEnumerable<TweetLine> GroupReleases(IEnumerable<IRelease> releases)
        {
            var result = new List<TweetLine>();
            foreach (var rel in releases.OrderBy(r => r.Name))
            {
                var sameNameReleases = releases.Where(r => r.Name == rel.Name);
                if (sameNameReleases.Count() > 1)
                {
                    if (result.Where(t => t.text.ToLower().Contains(rel.Name.ToLower())).Count() > 0)
                        continue;

                    var line = string.Empty;
                    switch (rel)
                    {
                        case Game.Release game:
                            line += $"[{string.Join('/', sameNameReleases.Select(r => (r as Game.Release).Platform?.Abbreviation).Distinct().ToList())}] ";
                            line += $"{game.Name}";
                            line += $" ({string.Join('/', sameNameReleases.Select(r => ReleaseHelper.GetFlag((r as Game.Release).Region)).Distinct().ToList())})";
                            break;
                        default:
                            line += rel.Name;
                            break;
                    }
                    
                    result.Add(new TweetLine {
                        text = line,
                        image = this.GetImage(rel)
                    });
                }
                else
                {
                    result.Add(new TweetLine {
                        text = ReleaseHelper.GetReleaseTitle(rel),
                        image = this.GetImage(rel)
                    });
                }
            }

            return result;
        }

        private string GetImage(IRelease rel)
        {
            switch (rel)
            {
                case Game.Release game:
                    return game.Image != GAMEDEFAULTIMAGE ? game.Image : null;
                default:
                    return null;
            }
        }

        private IEnumerable<Tweet> PrepareTweets(IEnumerable<TweetLine> releaseTweets)
        {
            var CrLf = "\n";
            var result = new List<Tweet>();
            CultureInfo ci = new CultureInfo("en-UK");
            var tweet = new Tweet
            {
                text = $"Releases of {DateTime.Today.ToString(ci.DateTimeFormat.LongDatePattern, ci)}:{CrLf}{CrLf}",
                images = new List<string>()
            };
            foreach (var tw in releaseTweets)
            {
                if (tweet.text.Length + tw.text.Length < _limit && tweet.images.Count < 4)
                {
                    tweet.text += $"{tw.text}{CrLf}";
                    if (tw.image != null) tweet.images.Add(tw.image);
                }
                else
                {
                    result.Add(tweet);

                    tweet = new Tweet
                    {
                        text = $"Releases {DateTime.Today.ToShortDateString()} cont.{CrLf}{CrLf}{tw.text}{CrLf}",
                        images = new List<string>()
                    };

                    if (tw.image != null) tweet.images.Add(tw.image);
                }
            }

            result.Add(tweet);
            return result;
        }

        private void PostToTwitter(IEnumerable<Tweet> tweets)
        {
            foreach (var tweet in tweets)
            {
                _logger.LogInformation($"Posting tweet result: {this.PublishToTwitter(tweet.text, tweet.images)}");
            }
        }

        /// <summary>
        /// Publish a post with image
        /// </summary>
        /// <returns>result</returns>
        /// <param name="post">post to publish</param>
        /// <param name="pathToImages">images to attach</param>
        public string PublishToTwitter(string post, List<string> pathToImages)
        {
            try
            {
                var mediaIdList = new List<string>();
                
                foreach (var image in pathToImages)
                {
                    // first, upload the image
                    string mediaID = string.Empty;
                    var rezImage = Task.Run(async () =>
                    {
                        var response = await TweetImage(image);
                        return response;
                    });
                    var rezImageJson = JObject.Parse(rezImage.Result.Item2);

                    if (rezImage.Result.Item1 != 200)
                    {
                        try // return error from JSON
                        {
                            return $"Error uploading image to Twitter. {rezImageJson["errors"][0]["message"].Value<string>()}";
                        }
                        catch (Exception ex) // return unknown error
                        {
                            // log exception somewhere
                            return $"Unknown error uploading image to Twitter ({ex})";
                        }
                    }
                    mediaIdList.Add(rezImageJson["media_id_string"].Value<string>());
                }
                

                // second, send the text with the uploaded image
                var rezText = Task.Run(async () =>
                {
                    var response = await TweetText(CutTweetToLimit(post), mediaIdList);
                    return response;
                });
                var rezTextJson = JObject.Parse(rezText.Result.Item2);

                if (rezText.Result.Item1 != 200)
                {
                    try // return error from JSON
                    {
                        return $"Error sending post to Twitter. {rezTextJson["errors"][0]["message"].Value<string>()}";
                    }
                    catch (Exception ex) // return unknown error
                    {
                        // log exception somewhere
                        return $"Unknown error sending post to Twitter ({ex})";
                    }
                }

                return "OK";
            }
            catch (Exception ex)
            {
                // log exception somewhere
                return $"Unknown error publishing to Twitter ({ex})";
            }
        }

        /// <summary>
        /// Send a tweet with some image attached
        /// </summary>
        /// <returns>HTTP StatusCode and response</returns>
        /// <param name="text">Text</param>
        /// <param name="mediaIDs">Media ID for the uploaded image. Pass empty string, if you want to send just text</param>
        public Task<Tuple<int, string>> TweetText(string text, List<string> mediaIDs)
        {
            var textData = new Dictionary<string, string> {
                { "status", text },
                { "trim_user", "1" },
                { "media_ids", string.Join(',', mediaIDs)}
            };

            return SendText(_TwitterStatusAPI, textData);
        }

        /// <summary>
        /// Upload some image to Twitter
        /// </summary>
        /// <returns>HTTP StatusCode and response</returns>
        /// <param name="pathToImage">Path to the image to send</param>
        public async Task<Tuple<int, string>> TweetImage(string pathToImage)
        {
            using (var client = new HttpClient())
            {
                var imgdata = await client.GetByteArrayAsync(pathToImage);
                var imageContent = new ByteArrayContent(imgdata);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");

                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(imageContent, "media");

                return await SendImage(_TwitterMediaAPI, multipartContent);
            }            
        }

        async Task<Tuple<int, string>> SendText(string URL, Dictionary<string, string> textData)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", PrepareOAuth(URL, textData, "POST"));

                var httpResponse = await httpClient.PostAsync(URL, new FormUrlEncodedContent(textData));
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>((int)httpResponse.StatusCode, httpContent);
            }
        }

        async Task<Tuple<int, string>> SendImage(string URL, MultipartFormDataContent multipartContent)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", PrepareOAuth(URL, null, "POST"));

                var httpResponse = await httpClient.PostAsync(URL, multipartContent);
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>((int)httpResponse.StatusCode, httpContent);
            }
        }

        #region Some OAuth magic
        string PrepareOAuth(string URL, Dictionary<string, string> data, string httpMethod)
        {
            // seconds passed since 1/1/1970
            var timestamp = (int)((DateTime.UtcNow - _epochUtc).TotalSeconds);

            // Add all the OAuth headers we'll need to use when constructing the hash
            Dictionary<string, string> oAuthData = new Dictionary<string, string>();
            oAuthData.Add("oauth_consumer_key", _consumerKey);
            oAuthData.Add("oauth_signature_method", "HMAC-SHA1");
            oAuthData.Add("oauth_timestamp", timestamp.ToString());
            oAuthData.Add("oauth_nonce", Guid.NewGuid().ToString());
            oAuthData.Add("oauth_token", _accessToken);
            oAuthData.Add("oauth_version", "1.0");

            if (data != null) // add text data too, because it is a part of the signature
            {
                foreach (var item in data)
                {
                    oAuthData.Add(item.Key, item.Value);
                }
            }

            // Generate the OAuth signature and add it to our payload
            oAuthData.Add("oauth_signature", GenerateSignature(URL, oAuthData, httpMethod));

            // Build the OAuth HTTP Header from the data
            return GenerateOAuthHeader(oAuthData);
        }

        /// <summary>
        /// Generate an OAuth signature from OAuth header values
        /// </summary>
        string GenerateSignature(string url, Dictionary<string, string> data, string httpMethod)
        {
            var sigString = string.Join(
                "&",
                data
                    .Union(data)
                    .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            var fullSigData = string.Format("{0}&{1}&{2}",
                httpMethod,
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(sigString.ToString()
                )
            );

            return Convert.ToBase64String(_sigHasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData.ToString())));
        }

        /// <summary>
        /// Generate the raw OAuth HTML header from the values (including signature)
        /// </summary>
        string GenerateOAuthHeader(Dictionary<string, string> data)
        {
            return string.Format(
                "OAuth {0}",
                string.Join(
                    ", ",
                    data
                        .Where(kvp => kvp.Key.StartsWith("oauth_"))
                        .Select(
                            kvp => string.Format("{0}=\"{1}\"",
                            Uri.EscapeDataString(kvp.Key),
                            Uri.EscapeDataString(kvp.Value)
                            )
                        ).OrderBy(s => s)
                    )
                );
        }
        #endregion

        /// <summary>
        /// Cuts the tweet text to fit the limit
        /// </summary>
        /// <returns>Cutted tweet text</returns>
        /// <param name="tweet">Uncutted tweet text</param>
        string CutTweetToLimit(string tweet)
        {
            while (tweet.Length >= _limit)
            {
                tweet = tweet.Substring(0, tweet.LastIndexOf(" ", StringComparison.Ordinal));
            }
            return tweet;
        }

        protected struct Tweet
        {
            public string text;
            public List<string> images;
        }

        protected struct TweetLine
        {
            public string text;
            public string image;
        }
    }
}