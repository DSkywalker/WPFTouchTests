using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TouchDeviceAutoCheck
{
    public class TouchProvider:TouchDevice
    {
        private static Window _window;
        private static double _w, _h;
        private static readonly Dictionary<int, TouchProvider> Contacts = new Dictionary<int, TouchProvider>();

        public Point Position { get; set; }

        private double width;
        private double height;

        private static int MaxDownPointsFrame = 10;
        private static double UpRate = 0.5;
        private static double MaxUpdateDistance = 500;

        private static DispatcherTimer timer;
        private static Random r = new Random();

        private static Mutex mutex = null;

        protected TouchProvider(int deviceId)
            : base(WaitglobalMutex(deviceId))
        {
            Position = new Point();
            width = 0;
            height = 0;
        }

        private static int WaitglobalMutex(int DevID)
        {
            if (mutex == null)
                mutex = new Mutex(false, "test");

            if (!mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                Console.WriteLine("Another instance is running");
                Environment.Exit(0);
            }

            return (DevID);
        }

        public static void BeginSimulation(Window w, double delay)
        {
            _window = w;

            _w = w.Width;
            _h = w.Height;

            w.SizeChanged += delegate
            {
                _w = w.ActualWidth;
                _h = w.ActualHeight;
            };


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(delay);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        public static void StopSimulation()
        {
            timer.Stop();
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            if (mutex == null)
                mutex = new Mutex(false, "test");

            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                System.Diagnostics.Debug.WriteLine("------------------------------------");
                int num = r.Next(MaxDownPointsFrame + 1);


                for (int i = 0; i < num; i++)
                {
                    System.Diagnostics.Debug.WriteLine(i.ToString() + "down point");
                    CursorAdded(TouchCursor.GetRandomCursor(GetAvailableID(), _w, _h));
                }

                for (int i = 0; i < Contacts.Count; i++)
                {
                    double probability = r.NextDouble();
                    if (probability < UpRate)
                    {
                        TouchProvider tp = Contacts.ElementAt(i).Value as TouchProvider;
                        TouchCursor tc = new TouchCursor(tp.Id, tp.Position.X, tp.Position.Y);
                        CursorRemoved(tc);
                    }
                    else
                    {
                        TouchProvider tp = Contacts.ElementAt(i).Value as TouchProvider;
                        double nx = tp.Position.X;
                        double ny = tp.Position.Y;
                        if (r.NextDouble() < 0.5)
                        {
                            nx += MaxUpdateDistance * r.NextDouble();
                            if (nx >= _w)
                                nx = _w - 1;
                        }
                        else
                        {
                            nx -= MaxUpdateDistance * r.NextDouble();
                            if (nx < 0)
                                nx = 0;
                        }

                        if (r.NextDouble() < 0.5)
                        {
                            ny += MaxUpdateDistance * r.NextDouble();
                            if (ny > _h - 1)
                                ny = _h - 1;
                        }
                        else
                        {
                            ny -= MaxUpdateDistance * r.NextDouble();
                            if (ny < 0)
                                ny = 0;
                        }
                    }
                }
            }
        }

        static int GetAvailableID()
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                Console.WriteLine("Another instance is running");
            }
                List<KeyValuePair<int, TouchProvider>> ids = Contacts.OrderBy(c => c.Key).ToList();
                for (int i = 0; i < ids.Count; i++)
                {
                    if (ids[i].Key != i)
                    {
                        System.Diagnostics.Debug.WriteLine("get id " + i.ToString());
                        return i;
                    }
                }
                System.Diagnostics.Debug.WriteLine("get id " + ids.Count.ToString());
          
            return ids.Count;
        }

        public static void CursorAdded(TouchCursor b)
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
            {
                System.Diagnostics.Debug.WriteLine(b.ID + "down");
                if (Contacts.ContainsKey(b.ID))
                    return;

                var device = new TouchProvider(b.ID);
                device.SetActiveSource(PresentationSource.FromVisual(_window));
                device.Position = new Point(b.X, b.Y);
                device.width = b.Width;
                device.height = b.Height;
                device.Activate();
                device.ReportDown();

                if (!Contacts.ContainsKey(b.ID))
                    Contacts.Add(b.ID, device);
            }));
            }
        }

        public static void CursorUpdated(TouchCursor b)
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
            {
                if (!Contacts.ContainsKey(b.ID))
                    CursorAdded(b);

                var device = Contacts[b.ID];

                if (device == null || !device.IsActive)
                    return;
                device.Position = new Point(b.X, b.Y);
                device.width = b.Width;
                device.height = b.Height;
                device.ReportMove();
            }));

            }
        }

        public static void CursorRemoved(TouchCursor b)
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
            {
                System.Diagnostics.Debug.WriteLine(b.ID + "up");
                if (!Contacts.ContainsKey(b.ID))
                    CursorAdded(b);
                var device = Contacts[b.ID];
                if (device == null || !device.IsActive)
                    return;

                device.Position = new Point(b.X, b.Y);
                device.width = b.Width;
                device.height = b.Height;
                device.ReportUp();
                device.Deactivate();

                Contacts.Remove(b.ID);
            }));
            }
        }

        public override TouchPoint GetTouchPoint(IInputElement relativeTo)
        {

                var point = Position;
            if (mutex.WaitOne(TimeSpan.FromSeconds(0.0), false))
            {
                if (relativeTo != null && ActiveSource != null)
                    point = ActiveSource.RootVisual.TransformToDescendant((Visual)relativeTo).Transform(Position);
            }
                var rect = new Rect(point, new Size(width, height));
            
            return new TouchPoint(this, point, rect, System.Windows.Input.TouchAction.Move);


        }

        public override TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo)
        {
            return new TouchPointCollection();
        }
    }
}
