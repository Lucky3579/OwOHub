using System;
using System.IO;
using System.Net;
using System.Linq;

namespace OwOHub
{
    internal class Program
    {
        Api _ = new Api();

        static string url = "https://e621.net/";

        static string[] tags = new string[] { };
        static int page = 1;

        static string directoryPath = "out";

        static void Main(string[] args)
        {
            for (int key = 0; key < args.Length; key++)
            {
                args[key] = args[key].ToLower();
            }

            for (int key = 0; key < args.Length; key++)
            {
                if (args[key] == "-h" || args[key] == "--help")
                {
                    Info("USAGE [FLAGS] [OPTIONS]\n\nFLAGS\n-h, --help\n-s, --sfw\n\nOPTIONS\n-o, --out <out>\n--page <page>\n\nARGS\n--tags <tags>");

                    return;
                }

                if (args[key] == "-s" || args[key] == "--sfw")
                {
                    url = "https://e926.net/";
                }

                if (args[key] == "--tags")
                {
                    try
                    {
                        string[] value = args[key + 1].Trim().Split(' ');

                        tags = tags.Concat(value).ToArray();
                    }
                    catch
                    {
                        Error("No tags specified");

                        return;
                    }
                }

                if (args[key] == "--page")
                {
                    try
                    {
                        if (int.TryParse(args[key + 1], out int value))
                        {
                            if (value > 0)
                            {
                                page = value;
                            }
                            else
                            {
                                Error("Invalid page");

                                return;
                            }
                        }
                        else
                        {
                            Error("Invalid number");

                            return;
                        }
                    }
                    catch
                    {
                        Error("Invalid argument");

                        return;
                    }
                }

                if (args[key] == "-o" || args[key] == "--out")
                {
                    try
                    {
                        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

                        foreach (char invalidFileNameChar in invalidFileNameChars)
                        {
                            if (args[key + 1].IndexOf(invalidFileNameChar) != -1)
                            {
                                Error("Invalid directory name");

                                return;
                            }
                        }

                        directoryPath = args[key + 1];
                    }
                    catch
                    {
                        Error("Invalid directory path");

                        return;
                    }
                }
            }

            if (tags.Length == 0)
            {
                Error("Syntax");

                return;
            }

            for (int key = 0; key < tags.Length; key++)
            {
                dynamic tagCheck = Api.IsTag(url, tags[key]);

                if (tagCheck is string)
                {
                    Warn("tag \"" + tags[key] + "\" does not exist has been replaced by \"" + tagCheck + "\"");
                    tags[key] = tagCheck;
                }

                if (tagCheck is bool)
                {
                    if (tagCheck == false)
                    {
                        Error("tag \"" + tags[key] + "\" does not exist and there are no similar tags");

                        return;
                    }
                }
            }

            dynamic posts = Api.GetPosts(url, tags, page);

            if (posts is null)
            {
                return;
            }

            if (posts.Count == 0)
            {
                Error("No posts to download!");

                return;
            }

            for (int key = 0; key < posts.Count; key++)
            {
                DownloadPost(posts[key]);
            }
        }

        static void DownloadPost(dynamic post)
        {
            string postId = Convert.ToString(post["id"]);
            string postExt = Convert.ToString(post["file"]["ext"]);
            string postUrl = Convert.ToString(post["file"]["url"]);

            if (!string.IsNullOrEmpty(postUrl))
            {
                if (!Directory.Exists("./" + directoryPath))
                {
                    Directory.CreateDirectory("./" + directoryPath);
                }

                try
                {
                    var WebClient = new WebClient();
                    WebClient.DownloadFile(postUrl, "./" + directoryPath + "/" + postId + "." + postExt);

                    Info("Downloaded " + postId + "." + postExt + " -> " + Path.GetFullPath("./" + directoryPath + "/" + postId + "." + postExt));
                }
                catch (WebException)
                {
                    Error("WebException");

                    return;
                }
                catch (Exception)
                {
                    Error("Exception");

                    return;
                }
            }
        }

        static void Info(string content)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("INFO ");
            Console.ResetColor();

            Console.WriteLine(content);
        }
        static void Warn(string content)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARN ");
            Console.ResetColor();

            Console.WriteLine(content);
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