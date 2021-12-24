using Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Vector2
    {
        public float x, y;

        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    public class PlayerData 
    {
        public Vector2 pos;
        public float r, g, b;

        public PlayerData(Vector2 pos, float r, float g, float b) 
        {
            this.pos = pos;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    public partial class GameWindow : Window
    {
        private static ConcurrentDictionary<int, PlayerData> localPlayers = new ConcurrentDictionary<int, PlayerData>();
        private PlayerData localPlayer;

        private Thread updateThread;

        public GameWindow()
        {
            InitializeComponent();

            //Client.GetInst().Connect("127.0.0.1", 4444);
            Random rand = new Random();
            localPlayer = new PlayerData(new Vector2(-50f, -50f), (float)rand.Next(0,255) / 255f, (float)rand.Next(0, 255) / 255f, (float)rand.Next(0, 255) / 255f);
            SetPlayerPos(new Vector2((float)Width / 2f, (float)Height / 2f));

            updateThread = new Thread(new ThreadStart(Update));
            updateThread.Start();
        }

        private void Update() // Update thread.
        {
            while (true)
            {
                Dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate 
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        Point mousePos = Mouse.GetPosition(GameCanvas);

                        if (mousePos.Y > 0f && mousePos.Y < Height - 30f && mousePos.X > 0f && mousePos.X < Width)
                            SetPlayerPos(new Vector2((float)mousePos.X - 5f, (float)mousePos.Y - 5f));
                    }
                });

                Render();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            updateThread.Abort();
        }

        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetPlayerPos(new Vector2(-50f, -50f));
                this.Close();
            }
        }

        private void OnMaximizeClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnMinimizeClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                WindowState = System.Windows.WindowState.Minimized;
        }

        private void WindowOnRender(object sender, EventArgs e)
        {
            Render();
        }

        public static void OnReceivePosUpdate(int uniqueId, Vector2 position, float r = 0, float g = 0, float b = 0) 
        {
            if (!localPlayers.Keys.Contains(uniqueId))
                localPlayers.TryAdd(uniqueId, new PlayerData(position, r, g, b));

            localPlayers[uniqueId].pos = position;
        }

        private void RenderPlayer(PlayerData player, int uniqueId) 
        {
            TextBlock txt = new TextBlock();
            Rectangle rect = new Rectangle();

            txt.Text = uniqueId.ToString();
            txt.Foreground = new SolidColorBrush(Colors.White);
            txt.HorizontalAlignment = HorizontalAlignment.Center;

            rect.Fill = new SolidColorBrush(Color.FromScRgb(1f, player.r, player.g, player.b));

            rect.RadiusX = 20f;
            rect.RadiusY = 20f;

            rect.Width = 10f;
            rect.Height = 10f;

            GameCanvas.Children.Add(rect);
            GameCanvas.Children.Add(txt);

            Canvas.SetLeft(rect, player.pos.x);
            Canvas.SetTop(rect, player.pos.y);

            Canvas.SetLeft(txt, player.pos.x - txt.Text.Length * 2.5f);
            Canvas.SetTop(txt, player.pos.y - 15f);
        }

        private void Render() 
        {
            Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate 
            {
                GameCanvas.Children.Clear();

                foreach (var player in localPlayers)
                    RenderPlayer(player.Value, player.Key);

                RenderPlayer(localPlayer, Client.GetInst().GetInfo().uniqueId);
            });
        }

        private void MovePosition(Vector2 pos) 
        {
            if (pos.x == 0f && pos.y == 0f)
                return;

            pos.x = localPlayer.pos.x + pos.x;
            pos.y =  localPlayer.pos.y + pos.y;

            pos.x = pos.x < 0f ? 0f : pos.y > (float)GameCanvas.Width ? (float)GameCanvas.Width - 10f : pos.x;
            pos.y = pos.y < 0f ? 0f : pos.y > (float)GameCanvas.Height ? (float)GameCanvas.Height - 10f : pos.y;

            SetPlayerPos(pos);
        }

        private void SetPlayerPos(Vector2 pos) 
        {
            localPlayer.pos = pos;

            Client.GetInst().UDP_SendPacket(new UDP_PositionPacket(pos.x, pos.y, localPlayer.r, localPlayer.g, localPlayer.b));
        }

        private void WinKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W || e.Key == Key.Up)
                MovePosition(new Vector2(0f, -3f));
            else if (e.Key == Key.S || e.Key == Key.Down)
                MovePosition(new Vector2(0f, 3f));

            if (e.Key == Key.D || e.Key == Key.Right)
                MovePosition(new Vector2(3f, 0f));
            else if (e.Key == Key.A || e.Key == Key.Left)
                MovePosition(new Vector2(-3f, 0f));
        }

        private void WinKeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}