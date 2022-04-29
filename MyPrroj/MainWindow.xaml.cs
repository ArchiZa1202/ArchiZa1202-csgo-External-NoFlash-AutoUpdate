using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace MyPrroj
{
    
    public partial class MainWindow : Window
    {
        bool flag;
        public static Memory m;
        public static int myClient;
        public static bool gesp = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        
       
        //Поиск процессов и модулей
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
        private void Esp() 
        {
            new Thread(() =>
            {
                if (flag)
                {
                    while (true)
                    {
                        Thread.Sleep(1);
                        int lPlayer = m.Read<int>(myClient + Offset.dwLocalPlayer);
                        int playrTeam = m.Read<int>(lPlayer + Offset.m_iTeamNum);
                        int glow = m.Read<int>(myClient + Offset.dwGlowObjectManager);


                        for (int i = 1; i < 32; i++)
                        {
                            int entity = m.Read<int>(myClient + Offset.dwEntityList + i * 0x10);
                            int enemyTeam = m.Read<int>(entity + Offset.m_iTeamNum);
                            if (entity != 0)
                            {
                                if (enemyTeam != 0 && enemyTeam != playrTeam)
                                {
                                    int entityglowing = m.Read<int>(entity + Offset.m_iGlowIndex);
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

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            flag = false;
        }
    }
}
