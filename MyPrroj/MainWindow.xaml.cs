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
           
        }
           
       
        private void Esp() 
        {
            while (true)
            {
                int glow = m.Read<int>(myClient + Offset.dwGlowObjectManager); 
                
                
                for (int i = 1; i < 32; i++)
                {
                    int entity = m.Read<int>(myClient + Offset.dwEntityList + i * 0x10);
                    if (entity != 0) 
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
        }
    }
}
