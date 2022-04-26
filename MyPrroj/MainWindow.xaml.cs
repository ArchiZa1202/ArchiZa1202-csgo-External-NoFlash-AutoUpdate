using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;


namespace MyPrroj
{
    
    public partial class MainWindow : Window
    {
        public static Memory m;
        public static int myClient;
        public static int myEngine;
        public static bool gesp = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void ESP_Proj(object sender, EventArgs e)
        {
            gesp = (bool)chEsp.IsChecked;
        }
        //Поиск процессов и модулей
        private void Run_Procces(object sender, RoutedEventArgs e)
        {
            new Thread(() => {  
                while (true)
            {
                    Thread.Sleep(1000);
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
                        if (myClient != 0 && myEngine != 0) 
                        {
                            Esp();
                        }
                }
                catch
                {
                    MessageBox.Show("No process found.");
                    return;
                }
            }
            });
        }
           


        //Метод чита
        private void Esp() 
        {
            while (true)
            {
                int lPlayer = m.Read<int>(myClient + Offset.dwLocalPlayer);
                int myTeam = m.Read<int>(lPlayer + Offset.m_iTeamNum);//Команнда игрока
                
                for (int i = 1; i < 64; i++)
                {
                    int entityList = m.Read<int>(myClient + Offset.dwEntityList + i * 0x10);//id всех игроков
                    int enemyTeam = m.Read<int>(entityList + Offset.m_iTeamNum);//Команда врага
                    if (enemyTeam != myTeam && entityList != 0)
                    {
                        int glowIndex = m.Read<int>(entityList + Offset.m_iGlowIndex);//id glow index
                        ColormyEnemy(glowIndex, 255, 160, 160);
                    }
                }
            }
        }


        //Метод отрисовки объекта
        private void ColormyEnemy(int enemyglowIndex, int red, int green, int blue) 
        {
            //Представляет глобальные объекты которые могут иметь обводку(glowindex)
            int glowObject = m.Read<int>(myClient + Offset.dwGlowObjectManager);
            //Color
            m.Write(glowObject + (enemyglowIndex * 0x38) + 4, red / 100f);
            m.Write(glowObject + (enemyglowIndex * 0x38) + 8, green / 100f);
            m.Write(glowObject + (enemyglowIndex * 0x38) + 12, blue / 100f);
            //Alpha chanel
            m.Write(glowObject + (enemyglowIndex * 0x38) + 0x10, 255 / 100f);
            //health enemy
            m.Write(glowObject + (enemyglowIndex * 0x38) + 0x24, true);
            m.Write(glowObject + (enemyglowIndex * 0x38) + 0x25, false);
        }
    }
}
