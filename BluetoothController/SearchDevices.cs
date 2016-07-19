using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Graphics.Drawables;

namespace BluetoothController
{

    // --------------- DOKUMENTATION --------------------

    [Activity(Label = "SearchedDevices")]
    public class SearchDevices : Activity, IEstablishConnection
    {
        // Member Variablen
        private BluetoothAdapter m_BtAdapter;
        private IntentFilter m_Filter;          // Filtert Events beim Suchen
        private MyBroadcastreciver m_Receiver;
        private Button m_BtSearch;
        private LinearLayout m_Linear;
        private BluetoothDevice m_Device;
        private ListView m_ListView;
        private List<String> m_Uuids;           // enth�lt die UUID�s (UUID = Universally Unique Identifier)
        private GradientDrawable m_Drawable;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchedLayout);

            Init();

            m_BtAdapter.StartDiscovery(); // Startet die Suche nach Devices in der N�he
        }

        public void Init()
        {

            // Erzeugen der Objekte
            m_Uuids = new List<string>();
            m_ListView = FindViewById<ListView>(Resource.Id.listViewSearched);
            m_BtSearch = FindViewById<Button>(Resource.Id.btSearchDevices);
            m_Drawable = new GradientDrawable();
            m_Linear = FindViewById<LinearLayout>(Resource.Id.linear3);
            m_BtAdapter = BluetoothAdapter.DefaultAdapter;
            m_Filter = new IntentFilter();
            m_Receiver = new MyBroadcastreciver(this);

            // Background Color [ListView]
            m_ListView.SetBackgroundColor(Android.Graphics.Color.Black); // Setzt die Background Color auf Schwarz

            // Event beim Klicken auf ein ListViewItem 
            m_ListView.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) => { OnItemClick(sender, e); };

            
            // Text Color [Button]
            m_BtSearch.SetTextColor(Android.Graphics.Color.Black); // Setzt die Text Color Schwarz

            // Border
            m_Drawable.SetShape(ShapeType.Rectangle);              // Formt das Drawable Objekt in ein Rechteckt
            m_Drawable.SetStroke(2, Android.Graphics.Color.Black); // Setzt die Border St�rke auf 2 und die Farbe Schwarz
            m_Drawable.SetColor(Android.Graphics.Color.White);     // Setzt die Background auf Wei�
            m_BtSearch.SetBackgroundDrawable(m_Drawable);          // Button �bernimmt das Desgin des Drawable Objekt

            // Event beim Klicken des Buttons
            m_BtSearch.Touch += (object sender, Android.Views.View.TouchEventArgs e2) =>
            {
                if (e2.Event.Action == MotionEventActions.Down)
                    m_BtSearch.SetBackgroundColor(Android.Graphics.Color.Aquamarine); // Setzt Button Background, wenn der Button ber�hrt wird
                else if (e2.Event.Action == MotionEventActions.Up)
                {
                    OnSearch();                                   // Startet die Suche nach neuen Devices in der N�he
                    m_BtSearch.SetBackgroundDrawable(m_Drawable); // Setzt das Standart Design vom Button zur�ck
                }
            };


            // Activity Background
            m_Linear.SetBackgroundColor(Android.Graphics.Color.White); // Setzt die Background Color Wei�

            // Filtert nach bestimmten Events
            m_Filter.AddAction(BluetoothDevice.ActionFound);               // ActionFound Event wird zum Filter hinzugef�gt
            m_Filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);   // ActionDiscoveryStarted Event wird zum Filter hinzugef�gt
            m_Filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);  // ActionDiscoveryFinished Event wird zum Filter hinzugef�gt
            m_Filter.AddAction(BluetoothDevice.ActionUuid);                // ActionUuid Event wird zum Filter hinzugef�gt

            // Die hinzugef�gten Events werden registriert und an das Broadcast Objekt weitergeleitet
            RegisterReceiver(m_Receiver, m_Filter);
        }

        // Gibt einen Toast, der vom Parameter (message) abh�ngt, aus
        public void GiveAMessage(String message)
        {
            Toast.MakeText(ApplicationContext, message, 0).Show();
        }

        /// <summary>
        /// Startet die Suche nach neuen Devices in der N�he
        /// </summary>
        public void OnSearch()
        {
            m_ListView.SetAdapter(null);  // Setzt den Adapter der ListView auf Null, damit er die alten gefunden Devices l�scht
            m_Receiver.ResetList();       // Setzt die Liste mit den gefunden Devices auf null

            // Die CancelDiscovery-Methode ist daf�r da damit nicht beim Spammen des Buttons unn�tige Suchvorg�nge gestartet werden
            m_BtAdapter.CancelDiscovery();   // Bricht die Suche nach Devices ab
            m_BtAdapter.StartDiscovery();    // Startet die Suche nach Devices in de N�he
        }

       
        /// <summary>
        /// Anzeigen der gefundenen Devices
        /// </summary>
        /// <param name="l"></param>
        public void SetAdapterToListView(List<String> l)
        {
            ArrayAdapter<String> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, l); // Wandelt die �bergebene List<String> in ein Adapter Array um
            m_ListView.SetAdapter(adapter);                             // Setzt den Adapter der ListView 
            m_ListView.SetBackgroundColor(Android.Graphics.Color.Gray); // Setz den Background jedes Item auf Grau    
        }


        /// <summary>
        /// Click Event [ListView]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnItemClick(object sender, Android.Widget.AdapterView.ItemClickEventArgs e)
        {
            TextView view = (TextView)e.View;                        // Wandelt aus ausgew�hlte Item der ListView in eine TextView um
            view.SetBackgroundColor(Android.Graphics.Color.Blue);    // Setzt den Background der TextView auf Blau
            String address = view.Text.Split('\n')[1];               // Filter, durch eine Split-Anwendung, die Adresse des Devices

            BluetoothDevice btDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(address);  // Erstellt ein BluetoothDevice Objekt mithilfe der Device-Adresse
            ConnectedThread connect = new ConnectedThread(btDevice, m_Uuids[e.Position]);         // Erstellt ein Objekt von Connection Thread mit dem Bluetooth Device und mit der jeweiligen UUID
            connect.Start();                                                                      // Startet den Thread, um sich mit dem Device zu verbinden

            var activity2 = new Intent(this, typeof(ConnectedDevices));
            IList<String> ll = new List<string>();
            ll.Add(btDevice.Name);
            ll.Add(btDevice.Address);
            activity2.PutStringArrayListExtra("MyData", ll);
            StartActivity(activity2);


        }

        /// <summary>
        ///  F�gt eine UUID zur Liste hinzu
        /// </summary>
        /// <param name="uuid"></param>
        public void AddUuid(String uuid)
        {
            m_Uuids.Add(uuid); // Hinzuf�gen einer UUID zur Liste
        }

        public void BuildConnection()
        {
            
        }
    }
}