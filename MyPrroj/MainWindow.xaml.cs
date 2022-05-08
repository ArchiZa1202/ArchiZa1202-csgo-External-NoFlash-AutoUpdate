using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace MyPrroj
{
    
    public partial class MainWindow : Window
    {
        private const string Url = "https://github.com/frk1/hazedumper/blob/master/csgo.cs";
        private bool flag;
        private Memory m;
        private int myClient;
        private int myEngine;
        public MainWindow()
        {
            InitializeComponent();
            Hex(Parse(Url));
        }

        //Поиск процессов и модулей, начальная точка входа в бесконечный цикл
        private void Run_Procces(object sender, RoutedEventArgs e)
        {
                try
                {
                    //Поиск процесса
                    Process myProc = Process.GetProcessesByName("csgo")[0];
                    m = new Memory("csgo");
                    //Поиск нужных модулей
                    foreach (ProcessModule item in myProc.Modules)
                    {
                        if (item.ModuleName == "client.dll")
                        {
                            myClient = (int)item.BaseAddress;
                        }
                        if (item.ModuleName == "engine.dll")
                        {
                            myEngine = (int)item.BaseAddress;
                        }
                }
                        if (myClient != 0) 
                        {
                            flag = true;
                            Esp();
                        }
                }
                catch
                {
                    MessageBox.Show("No process found.");
                    return;
                }
        }


        #region State 
        private int GetBase() 
        {
            return m.Read<int>(myEngine + Offsets.dwClientState);
        }
        EState State() 
        {
            int state = m.Read<int>(GetBase() + Offsets.dwClientState_State);
            if (state <= (int)EState.Invalid || state >= (int)EState.InvalidLast) 
            {
                return EState.Invalid;
            }
            return (EState)state;
        }
        #endregion State 



        private void Esp() // Отрисовка объектов
        {
            new Thread(() =>
            {
                if (flag)
                {
                    while (true)
                    {
                        Thread.Sleep(1);
                        if (State() != EState.InGame) 
                        {
                            continue;
                        }
                        int lPlayer = m.Read<int>(myClient + Offsets.dwLocalPlayer);
                        int playrTeam = m.Read<int>(lPlayer + Offsets.m_iTeamNum);
                        int glow = m.Read<int>(myClient + Offsets.dwGlowObjectManager);


                        for (int i = 1; i < 32; i++)
                        {
                            int entity = m.Read<int>(myClient + Offsets.dwEntityList + i * 0x10);
                            int enemyTeam = m.Read<int>(entity + Offsets.m_iTeamNum);
                            if (entity != 0)
                            {
                                if (enemyTeam != 0 && enemyTeam != playrTeam)
                                {
                                    int entityglowing = m.Read<int>(entity + Offsets.m_iGlowIndex);
                                    m.Write(glow + entityglowing * 0x38 + 0x8, 255f);
                                    m.Write(glow + entityglowing * 0x38 + 0xC, 0);
                                    m.Write(glow + entityglowing * 0x38 + 0x10, 0);
                                    m.Write(glow + entityglowing * 0x38 + 0x14, 255f);
                                    m.Write(glow + entityglowing * 0x38 + 0x28, true);
                                }
                            }
                        }
                        if (!flag) return;
                    }
                }
            }).Start();
        } 

        //Отключение Esp
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            flag = false;
        }


        #region Offsets
        struct Offsets 
        {
            public static Int32 dwLocalPlayer;
            public static Int32 m_iTeamNum;
            public static Int32 dwGlowObjectManager;
            public static Int32 dwEntityList;
            public static Int32 m_iGlowIndex;
            public static Int32 dwClientState;
            public static Int32 dwClientState_State;
        }
        private void Hex(Dictionary<string, string> dic) 
        {
            Offsets.dwLocalPlayer = Convert.ToInt32(dic["dwLocalPlayer"], 16);
            Offsets.dwEntityList = Convert.ToInt32(dic["dwEntityList"], 16);
            Offsets.dwGlowObjectManager = Convert.ToInt32(dic["dwGlowObjectManager"], 16);
            Offsets.m_iGlowIndex = Convert.ToInt32(dic["m_iGlowIndex"], 16);
            Offsets.m_iTeamNum = Convert.ToInt32(dic["m_iTeamNum"], 16);
            Offsets.dwClientState = Convert.ToInt32(dic["dwClientState"], 16); ;
            Offsets.dwClientState_State = Convert.ToInt32(dic["dwClientState_State"],16);
    }
        private Dictionary<string,string> Parse(string url) 
        {
            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                using (HttpClientHandler hendler = new HttpClientHandler() { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None }) 
                {
                    using (HttpClient client = new HttpClient(hendler)) 
                    {
                        using (HttpResponseMessage response = client.GetAsync(url).Result) 
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var html = response.Content.ReadAsStringAsync().Result;
                                if (!string.IsNullOrEmpty(html))
                                {
                                    var htmlDoc = new HtmlDocument();
                                    htmlDoc.LoadHtml(html);
                                    if (htmlDoc != null)
                                    {
                                        var doc = htmlDoc.DocumentNode.SelectSingleNode(".//table[@class='highlight tab-size js-file-line-container js-code-nav-container js-tagsearch-file']");
                                        if (doc != null)
                                        {
                                            var line = doc.SelectNodes(".//tr");
                                            if (line != null && line.Count > 0)
                                            {
                                                foreach (var item in line)
                                                {
                                                    var value = item.SelectSingleNode(".//td[@class='blob-code blob-code-inner js-file-line']");
                                                    if (value != null)
                                                    {
                                                        var name = value.SelectNodes(".//span[@class='pl-en']");
                                                        var currentValue = value.SelectNodes(".//span[@class='pl-c1']");
                                                        if (currentValue != null && name != null)
                                                        {
                                                            foreach (var i in name)
                                                            {
                                                                if (i.InnerText == "dwLocalPlayer" | i.InnerText == "m_iTeamNum" | i.InnerText == "dwGlowObjectManager" | i.InnerText == "dwEntityList" | i.InnerText == "m_iGlowIndex"| i.InnerText == "dwClientState"| i.InnerText== "dwClientState_State")
                                                                {
                                                                    foreach (var cur in currentValue)
                                                                    {
                                                                        result[i.InnerText] = "0x" + cur.InnerText;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }
        #endregion Offsets
    }
}
