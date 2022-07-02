using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace OwOHub
{
    internal class Api
    {
        static string userAgent = "OwOHub";

        public static dynamic IsTag(string url, string tag)
        {
            string requestUrl = url + "tags.json?search[name_matches]=" + tag;

            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                httpRequest.UserAgent = userAgent;
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                var streamReader = new StreamReader(httpResponse.GetResponseStream());
                dynamic result = JsonConvert.DeserializeObject<dynamic>(streamReader.ReadToEnd());

                try
                {
                    if (result[0]["name"] == tag)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    requestUrl = url + "tag_aliases.json?search[name_matches]=" + tag;

                    httpRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                    httpRequest.UserAgent = userAgent;
                    httpRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                    streamReader = new StreamReader(httpResponse.GetResponseStream());
                    result = JsonConvert.DeserializeObject<dynamic>(streamReader.ReadToEnd());

                    try
                    {
                        if (result[0]["antecedent_name"] == tag)
                        {
                            return Convert.ToString(result[0]["consequent_name"]);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch (WebException)
            {
                Error("WebException");

                return null;
            }
            catch (Exception)
            {
                Error("Exception");

                return null;
            }
        }

        public static dynamic GetPosts(string url, string[] tags, int page)
        {
            string requestUrl = url + "posts.json?tags=";

            try
            {
                for (int key = 0; key < tags.Length; key++)
                {
                    string tag = tags[key];

                    requestUrl += tag;

                    if (key != tags.Length - 1)
                    {
                        requestUrl += "+";
                    }
                }
                requestUrl += "&page=" + page;

                var httpRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                httpRequest.UserAgent = userAgent;
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                var streamReader = new StreamReader(httpResponse.GetResponseStream());
                dynamic result = JsonConvert.DeserializeObject<dynamic>(streamReader.ReadToEnd());

                return result["posts"];
            }
            catch (WebException)
            {
                Error("WebException");

                return null;
            }
            catch (Exception)
            {
                Error("Exception");

                return null;
            }
        }

        static void Error(string content)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR ");
            Console.ResetColor();

            Console.WriteLine(content);
        }
    }
}