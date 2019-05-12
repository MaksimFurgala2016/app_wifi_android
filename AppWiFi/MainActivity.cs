using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Net.Wifi;
using System.Collections.Generic;

namespace AppWiFi
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        int count = 1;

        public TextView textview;
        public Button button;

        public WifiManager WifiManager;
        ScanResultBroadcastReceiver m_scanResultBroadcastReceiver;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //размещаем пользовательский интерфейс (шапка)
            SetContentView(Resource.Layout.activity_main);

            //интерфейс app для версий android'a
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //размещаем пользовательский интерфейс (контент)
            SetContentView(Resource.Layout.content_main);

            button = FindViewById<Button>(Resource.Id.buttonSearchWiFi);//кнопка поиска точек доступа

            textview = FindViewById<TextView>(Resource.Id.textView1);//текстовое поле (вывод сообщений)

            
            WifiManager = (WifiManager)GetSystemService(Context.WifiService);
            textview.Text = "Начать поиск!";

            m_scanResultBroadcastReceiver = new ScanResultBroadcastReceiver();
            m_scanResultBroadcastReceiver.Receive += m_scanResultBroadcastReceiver_Receive;

            button.Click += button_Click;
        }

        /// <summary>
        /// поиск точек Wi-Fi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, EventArgs e)
        {
            RegisterReceiver(m_scanResultBroadcastReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));
            button.Text = "Непрерывный поиск...";
            WifiManager.StartScan();
        }

        /// <summary>
        /// получаем список (текст) доступных точек Wi-Fi
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void m_scanResultBroadcastReceiver_Receive(Context arg1, Intent arg2)
        {
            var wifiR = WifiManager.ScanResults;//результаты сканирования доступных точек доступа
            textview.Text = String.Empty;
            for(int i=0; i < wifiR.Count; i++)
            {
                //ssid - имя сети
                if(wifiR[i].Ssid.Length >=1)
                {
                    //level - уровень сигнала
                    int level = wifiR[i].Level;
                    if(level >= -50)
                    {
                        level = 100;
                    }
                    else
                    {
                        if(level <= -100)
                        {
                            level = 0;
                        }
                        else
                        {
                            level = 2 * (level + 100);
                        }
                    }
                    //выводим результат в текстовое поле
                    textview.Text += wifiR[i].Ssid + " - SSID, " + level + " %" + System.Environment.NewLine;
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// подключение по WPA стандарту к точке
        /// </summary>
        /// <param name="networkSSID">имя сети</param>
        /// <param name="password"> пароль </param>
        /// <returns>статус коннекта</returns>
        public bool ConnectToNetworkWPA(string networkSSID, string password)
        {
            try
            {
                WifiConfiguration conf = new WifiConfiguration();//конфигурация
                conf.Ssid = "\"" + networkSSID + "\"";

                conf.PreSharedKey = "\"" + password + "\"";

                conf.StatusField = WifiStatus.Enabled;
                conf.AllowedGroupCiphers.NextSetBit((int)GroupCipherType.Tkip);

                conf.AllowedGroupCiphers.Set((int)GroupCipherType.Tkip);
                conf.AllowedGroupCiphers.Set((int)GroupCipherType.Ccmp);
                conf.AllowedKeyManagement.Set((int)KeyManagementType.WpaPsk);
                conf.AllowedPairwiseCiphers.Set((int)PairwiseCipherType.Tkip);
                conf.AllowedPairwiseCiphers.Set((int)PairwiseCipherType.Ccmp);

                WifiManager.AddNetwork(conf);

                IList<WifiConfiguration> list = WifiManager.ConfiguredNetworks;
                foreach (WifiConfiguration i in list)
                {
                    if (i.Ssid != null && i.Ssid.Equals("\"" + networkSSID + "\""))
                    {
                        WifiManager.Disconnect();
                        WifiManager.EnableNetwork(i.NetworkId, true);
                        WifiManager.Reconnect();

                        break;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                //alertManager();
                return false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScanResultBroadcastReceiver : BroadcastReceiver
    {
        public event Action<Context, Intent> Receive;

        //передача
        public override void OnReceive(Context context, Intent intent)
        {
            if(this.Receive != null && intent != null && intent.Action == "android.net.wifi.SCAN_RESULTS")
            {
                this.Receive(context, intent);
            }
        }
    }
}

