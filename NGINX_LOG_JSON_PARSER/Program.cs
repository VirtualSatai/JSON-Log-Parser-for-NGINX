using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace NGINX_LOG_JSON_PARSER
{
    class Program
    {
        static List<JSON_ITEM> list;

        static void Main(string[] args)
        {
            ParseList("tv.log");


            if (args.Length > 0)
                cmdlineoptions(args);
            else
                cmdlineoptions(new string[] { "ma", "" });

            /*
            mostActiveUsers();
            mostRequestedFiles(10);
            whoHasWatched("Kingsman.The.Secret.Service.2014.1080p.BluRay.x264-SPARKS.mkv");
            whoHasWatched("Jurassic.World.2015.720p.BluRay.x264-SPARKS.mkv");
            whoHasWatched("Psych");
            whatHasWhoWatched("claus"); */
        }

        static void cmdlineoptions(string[] args)
        {
            switch (args[0])
            {
                case "who":
                    if (args[1] != null)
                        WhoHasWatched(args[1]);
                    break;
                case "what":
                    if (args[1] != null)
                        WhatHasWhoWatched(args[1]);
                    break;
                case "ma":
                    mostActiveUsers();
                    break;
                case "mrf":
                    int i = 10;
                    try
                    {
                        Int32.TryParse(args[1], out i);
                    }
                    catch
                    {

                    }
                    mostRequestedFiles(i);
                    break;
                default:
                    Console.WriteLine("Valid arguments: who *person*, what *string*, ma (most active), mrf (most requested files)");
                    return;
            }
        }

        static void WhatHasWhoWatched(string str)
        {
            Console.WriteLine("What has " + str + " watched ?");
            foreach (var el in list.Where(x => x.remote_user == str && x.body_bytes_sent > 1024 * 1024 * 100).GroupBy(x => x.request).OrderBy(x => ConvertToDatetime(x.First().time_local)))
            {
                Console.WriteLine("{0,10}: {1,10} ({2,6} MB) {3}",
                    el.First().remote_user,
                    ConvertToDatetime(el.First().time_local).ToShortDateString(),
                    el.Sum(x => x.body_bytes_sent) / (1024 * 1024),
                    FileNameFromRequest(el.First(x => x.body_bytes_sent > 1024 * 1024 * 1).request));
            }
        }

        static void WhoHasWatched(string str)
        {
            Console.WriteLine("Who has watched " + str + " ?");
            foreach (var el in list.Where(x => x.request.ToLower().Contains(str.ToLower())).GroupBy(x => x.remote_user))
            {
                foreach (var instance in el.GroupBy(x => ConvertToDatetime(x.time_local)).Where(x => x.Sum(y => y.body_bytes_sent) > 1024 * 1024 * 100))
                {
                    Console.WriteLine("{0,10}: {1,10} ({2,6} MB) {3}",
                    instance.First().remote_user,
                    ConvertToDatetime(instance.First().time_local).ToShortDateString(),
                    instance.Sum(x => x.body_bytes_sent) / (1024 * 1024),
                    FileNameFromRequest(instance.First(x => x.body_bytes_sent > 1024 * 1024 * 1).request));
                }
            }
        }

        static DateTime ConvertToDatetime(string x)
        {
            var format = "dd/MMM/yyyy";
            return DateTime.ParseExact(x.Substring(0, 11), format, CultureInfo.InvariantCulture);
        }

        static void mostActiveUsers()
        {

            var dateSortedList = list.OrderBy(x => ConvertToDatetime(x.time_local));
            Console.WriteLine("First log: {0}\nLast log: {1}", dateSortedList.First().time_local, dateSortedList.Last().time_local);

            var groupedList = list.GroupBy(x => x.remote_user);

            foreach (var el in groupedList
                .Where(x => x.Sum(y => y.body_bytes_sent) > 1024 * 1024 * 1024)
                .OrderByDescending(x => x.Sum(y => y.body_bytes_sent)))
            {
                var size = el.Sum(x => x.body_bytes_sent) / 1024 / 1024 / 1024;

                Console.WriteLine("{0,-15} {1,5} GB {2,20} Requests", el.First().remote_user, size, el.Count());
            }
        }

        static void mostRequestedFiles(long size)
        {
            var mostRequestedFile = list.GroupBy(x => x.request).OrderByDescending(x => x.Sum(y => y.body_bytes_sent));

            foreach (var mrf in mostRequestedFile.Where(x => x.Sum(y => y.body_bytes_sent) > (long)1024 * 1024 * 1024 * size)) // 5gb
            {
                Console.WriteLine("{0,10} GB {1}", mrf.Sum(x => x.body_bytes_sent) / (1024 * 1024 * 1024), FileNameFromRequest(mrf.First().request));
            }
        }

        static string FileNameFromRequest(string str)
        {
            return str.Substring("GET ".Length).Split(' ')[0].Split('/').Last().Replace("%20", " ");
        }

        static void ParseList(string filename)
        {
            if (list == null)
                list = new List<JSON_ITEM>();

            var oldCount = list.Count();

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                JSON_ITEM lineobj = null;
                try
                {
                    lineobj = JsonConvert.DeserializeObject<JSON_ITEM>(line);
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine("Failed reading a line");
                }

                if (lineobj != null)
                {
                    list.Add(lineobj);
                }
            }

            Console.WriteLine("\nParsed {0}: {1} new enteties", filename, (list.Count - oldCount));
        }
    }
}
