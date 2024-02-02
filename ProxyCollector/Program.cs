using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Telegram.Bot;

namespace ProxyCollector
{
    class Program
    {


        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Bot started at " + DateTime.Now.ToString());
            var cli = new TelegramBotClient("telegram token here");

            while (true)
            {
                if ((DateTime.Now.Hour > 5) && (DateTime.Now.Hour % 2 == 1 && DateTime.Now.Minute == 0))
                {
                    Console.WriteLine("Report started at " + DateTime.Now.ToString());
                    var sbLst = new List<string>();
                    var sb = new StringBuilder();
                    var flPath = Path.Combine(Directory.GetCurrentDirectory(), "CHANNELS.txt");
                    var Cnl = File.ReadLines(flPath).ToList();
                    foreach (var cnl in Cnl)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            try
                            {
                                // Create a dictionary for the payload
                                var payload = new Dictionary<string, string>
                                {
                                    { "api_key", "taas token here" },
                                    { "@type", "getChatHistory" },
                                    { "chat_id", cnl },
                                    { "limit", "15" },
                                    { "offset", "0" },
                                    { "from_message_id", "0" }
                                };

                                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                                var response = await client.PostAsync("https://api.tdlib.org/client", content);
                                int tmp = 0;
                                while (!response.IsSuccessStatusCode && tmp < 6)
                                {
                                    Thread.Sleep(1000);
                                    response = await client.PostAsync("https://api.tdlib.org/client", content);
                                    tmp++;
                                }
                                var res = response.Content.ReadAsStringAsync();
                                dynamic data = JsonConvert.DeserializeObject(await res);
                                tmp = 0;
                                while (data.total_count < 15 && tmp < 6)
                                {
                                    Thread.Sleep(1000);
                                    content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                                    response = await client.PostAsync("https://api.tdlib.org/client", content);
                                    res = response.Content.ReadAsStringAsync();
                                    data = JsonConvert.DeserializeObject(await res);
                                    tmp++;
                                }
                                foreach (var msg in data.messages) if ((object)msg.content.text != null)
                                    {

                                        var lst = new List<string>();
                                        var txt = Convert.ToString(msg.content.text.text);
                                        if (txt.Contains("https://t.me/proxy?server=") || txt.Contains("tg://proxy?server="))
                                        {
                                            using (StringReader sr = new StringReader(txt))
                                            {
                                                string line;
                                                while ((line = sr.ReadLine()) != null)
                                                {
                                                    lst.Add(line);
                                                }
                                            }
                                            foreach (var itm in lst)
                                            {
                                                if (itm.StartsWith("https://t.me/proxy?server=") || itm.StartsWith("tg://proxy?server="))
                                                {
                                                    sbLst.Add(itm);
                                                }
                                            }
                                        }
                                    }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("An error occurred at getting proxies: " + ex.Message);
                                Console.WriteLine("#################################################################");
                            }
                        }
                    }

                    Console.WriteLine("Sending to Tg started at " + DateTime.Now.ToString());
                    //SEND TO TELEGRAM
                    try
                    {

                        var sbLstArr = new List<string>[1000];
                        for (int q = 0; q < 1000; q++)
                        {
                            sbLstArr[q] = new List<string>();
                        }
                        for (int q = 0; q < sbLst.Count; q++)
                        {
                            int ans = q / 5;
                            sbLstArr[ans].Add(" \U0001F6A5 " + sbLst[q]);
                            sbLstArr[ans].Add(" ");
                        }
                        foreach (var masg in sbLstArr) if(masg.Count() > 0)
                        {
                            var sbToCnl = new StringBuilder();
                            sbToCnl.AppendLine(" \U000023F0 " + "لیست پروکسی جدید" + " \U000023F0 ");
                            sbToCnl.AppendLine(" ");
                            foreach (var tmpMsg in masg)
                            {
                                sbToCnl.AppendLine(tmpMsg);
                            }
                            sbToCnl.AppendLine(" ");
                            sbToCnl.AppendLine(" \U0001F194 " + "@---");
                            await cli.SendTextMessageAsync("chatId", sbToCnl.ToString());
                            Thread.Sleep(60000);
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Error happened(Tg Bot Send):" + exception.Message);
                        Console.WriteLine("#################################################################");
                    }
                    Console.WriteLine("Sending to Tg ended at " + DateTime.Now.ToString());

                    Thread.Sleep(60000);

                }


            }
        }
    }
}
