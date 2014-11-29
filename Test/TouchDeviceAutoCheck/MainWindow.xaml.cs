using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Threading;

namespace TouchDeviceAutoCheck
{
    /// <summary>
    /// removed some antipatterns 
    /// </summary>
    public partial class MainWindow : Window
    {
        Hashtable ht = new Hashtable();
        private  Mutex mutex = null;

        public MainWindow()
        {
            InitializeComponent();
            TouchProvider.BeginSimulation(this, 0.03);
        }


        private void myCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            if (mutex == null)
                mutex = new Mutex(false, "test");

            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                System.Diagnostics.Debug.WriteLine(e.TouchDevice.Id + "wdown");
                TouchPoint tp = e.GetTouchPoint(this);

                Ellipse ep = new Ellipse();
                ep.Width = 50;
                ep.Height = 50;
                ep.Fill = new SolidColorBrush(Colors.White);

                if (ht.Contains(e.TouchDevice.Id))
                {
                   if(ht.Count > 5) // test for human or woodpecker
                    { 
                    TouchProvider.StopSimulation();
                    MessageBox.Show("I can't receive touchpoint id:" + e.TouchDevice.Id.ToString() + " up message. Please refer to re.log");
                    this.Close();
                    }
                }
                else
                {
                    ht.Add(e.TouchDevice.Id, ep);

                    myCanvas.Children.Add(ep);
                    ep.SetValue(Canvas.LeftProperty, tp.Position.X - tp.Size.Width / 2);
                    ep.SetValue(Canvas.TopProperty, tp.Position.Y - tp.Size.Height / 2);
                }
            }
        }

        private void myCanvas_TouchMove(object sender, TouchEventArgs e)
        {
            if (mutex == null)
                mutex = new Mutex(false, "test");

            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                int id = e.TouchDevice.Id;
                TouchPoint tp = e.GetTouchPoint(this);
                if (ht.Contains(id))
                {
                    Ellipse ep = ht[id] as Ellipse;

                    ep.SetValue(Canvas.LeftProperty, tp.Position.X - tp.Size.Width / 2);
                    ep.SetValue(Canvas.TopProperty, tp.Position.Y - tp.Size.Height / 2);
                }
            }
        }

        private void myCanvas_TouchUp(object sender, TouchEventArgs e)
        {
            if (mutex == null)
                mutex = new Mutex(false, "test");

            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                System.Diagnostics.Debug.WriteLine(e.TouchDevice.Id + "wup");
                int id = e.TouchDevice.Id;
                if (ht.Contains(id))
                {
                    Ellipse ep = ht[id] as Ellipse;

                    ht.Remove(id);
                    myCanvas.Children.Remove(ep);
                }
            }
        }
    }
}
