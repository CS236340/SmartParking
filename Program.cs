using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using System.Threading;

namespace LCCNTests
{
    class Program
    {
        static Random r = new Random();
        static int c = 0;
        static void Main(string[] args)
        {
            Uri uri = null;
            int num = -1;
            int msInterval = -1;
            try
            {
                uri = new Uri(args[0]);
                num = int.Parse(args[1]);
                msInterval = int.Parse(args[2]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wrong input!" + ex);
                Console.WriteLine("run your .exe <http://url:port> <num> <interval in ms>");
                return;
            }
            DateTime start = DateTime.Now;
            for (int i = 0; i < num; i++)
            {
                MyTest(uri, msInterval);
            }
            DateTime end = DateTime.Now;
            TimeSpan timer = end - start;
            Console.WriteLine();
            Console.WriteLine($"[+] Finished! {num} requests. Timer: {timer.TotalSeconds} seconds.");
        }
        public static void MyTest(Uri p_uri, int p_interval)
        {
            Thread.Sleep(p_interval);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(p_uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                string json = "{" +
                                    "\"refId\":\"" + c.ToString("00000") + "\"," +
                                    "\"carId\":\"12-345-67\"," +
                                    "\"subscription\":\"" + r.Next(0, 2) + "\"," +
                                    "\"action\":\"" + r.Next(0, 2) + "\""
                                + "}";
                c++;
                Console.WriteLine(json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            Task.Factory.StartNew(() =>
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            });
        }
    }
}
