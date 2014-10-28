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
using System.Windows.Threading;
using System.Threading;

namespace Pong
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// This class is the game logic behind the game of pong.
    /// </summary>
    /*
    * Colton Prettyman Final Project CSCD 306 EWU
    * Professor Thomas Capaul.
    * A simple game of pong
     *  
     * All aspects of the game are tested and seem
     * to work accurately without any non-working aspects
     * 
     *     ---Things Possibliy Worth Extra Credit---
     * A human like AI Player. Random yet accurate
     * Ability for game cheating
     * Multiple keyboard shortcuts
     * Resizable game window with rescaling
     * Sounds incorporated
     * Icons used
     * Manmade button using color changing labels
     * Buttons change action based on previouse click
     * ie. Start switches to reset etc.
     * Color schemes
    */
    public partial class MainWindow : Window
    {
        #region Members
        private double _changey = 9;
        private double _changex = 9;
        private int _balldt = 1;
        private double _lpaddledy = 7;
        private double _rpaddledy = 10;
        private int _paddledt = 1;
        private double _dy;
        private double _dx;
        private DispatcherTimer pongballTimer;
        private DispatcherTimer rpaddleTimer;
        private DispatcherTimer lpaddleTimer;
        private bool _lpaddleIsEnabled = false;
        private int _rightpoint = 0;
        private int _leftpoint = 0;
        private int _losepoint=9;
        private int _hitcount = 0;
        private int _winlosedt = 2000;
        private Random _rand = new Random();
        private bool _userinput = false;
        private bool _StartIsStart = true;
        private bool _PauseIsPause = true;
        private Line[] CenterLine;
        private int _dotlinecount = 20;
        private double _dotspace = 10;
        private Line[] EdgeLines;
        private double _edgelinespace = 32;
        private double _linepady = 5;
        private bool _mute = false;
        private bool _MuteIsMute = true;
        private double _AIoffsetdy = 0;
        private double _AIDesty;
        private double _epsilon = 8;
        #endregion



        public MainWindow()
        {
            InitializeComponent();

            pongballTimer = new DispatcherTimer(DispatcherPriority.Send);
            pongballTimer.Tick += new EventHandler(pongballTimer_Tick);
            pongballTimer.Interval = new TimeSpan(0, 0, 0, 0, _balldt);

            rpaddleTimer = new DispatcherTimer();
            rpaddleTimer.Tick += new EventHandler(rpaddleTimer_Tick);
            rpaddleTimer.Interval = new TimeSpan(0, 0, 0, 0, _paddledt);

            lpaddleTimer = new DispatcherTimer();
            lpaddleTimer.Tick += new EventHandler(lpaddleTimer_Tick);
            lpaddleTimer.Interval = new TimeSpan(0, 0, 0, 0, _paddledt);

            LeftPaddle.SnapsToDevicePixels = true;
            RightPaddle.SnapsToDevicePixels = true;
            PongBall.SnapsToDevicePixels = true;

            meHitSound.MediaEnded += new RoutedEventHandler(meHitSound_MediaEnded);
            meDeflectSound.MediaEnded += new RoutedEventHandler(meDeflectSound_MediaEnded);
            meMissSound.MediaEnded += new RoutedEventHandler(meMissSound_MediaEnded);
            CenterLine = new Line[_dotlinecount];
            DrawCenterLine();
            EdgeLines = new Line[2];
            DrawEdgeLines();

            _dx = _changex;
            _dy = _changey;
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            InitializePongBall();
        }
        #region Media Related Events
        void meHitSound_MediaEnded(object sender, RoutedEventArgs e)
        {
            meHitSound.Stop();
        }

        void meDeflectSound_MediaEnded(object sender, RoutedEventArgs e)
        {
            meDeflectSound.Stop();
        }

        void meMissSound_MediaEnded(object sender, RoutedEventArgs e)
        {
            meMissSound.Stop();
        }
        #endregion


        #region Misc. Drawing/ReDrawing/Scaling/Animation Methods
        private void DrawEdgeLines(bool scale = false )
        {
            double startx = _edgelinespace;
            double endx = PongCanvas.Width - _edgelinespace;
            double y=5;
            int myzindex = Canvas.GetZIndex(PongBall)+1;

            if( ! scale )
                for (int i = 0; i < EdgeLines.Length; i++)
                   EdgeLines[i] = new Line();

            foreach (Line l in EdgeLines)
            {
                l.X1 = startx;
                l.X2 = endx;
                l.Y1 = l.Y2 = y;
                if (!scale)
                {
                    l.StrokeThickness = 6;
                    Canvas.SetZIndex(l, myzindex);
                    PongCanvas.Children.Add(l);
                    l.SnapsToDevicePixels = true;
                    l.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF363636"));
                }

                y += PongCanvas.Height-10;
            }
        }
        private void DrawCenterLine(bool scale = false)
        {
            double starty = 0;
            double endy = PongCanvas.Height;
            double dotlength = (PongCanvas.Height - ((_dotlinecount +1 )* _dotspace)) / _dotlinecount;
            int myzindex = Canvas.GetZIndex(PongBall) - 1;
            if( ! scale )
                 for (int i = 0; i < CenterLine.Length; i++)
                     CenterLine[i] = new Line();

            foreach (Line l in CenterLine)
            {
                l.X1 = l.X2 = PongCanvas.Width/2;
                l.Y1 = _dotspace + starty;
                l.Y2 = l.Y1 + dotlength;

                if (! scale )
                {
                    l.StrokeThickness = 5;
                    Canvas.SetZIndex(l, myzindex);
                    PongCanvas.Children.Add(l);
                    l.SnapsToDevicePixels = true;
                    l.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF363636"));
                }

                starty = l.Y2;
            }
        }

        private void pongballTimer_Tick(object sender, EventArgs e)
        {
            double pongtop = Canvas.GetTop(PongBall);
            double pongleft = Canvas.GetLeft(PongBall);
            double rpadleleft = Canvas.GetLeft(RightPaddle);
            double lpadleft = Canvas.GetLeft(LeftPaddle);

            if (pongleft + PongBall.Width > rpadleleft)
                if (!CheckRightCollision())
                    return;
            if (pongleft < (lpadleft + LeftPaddle.Width))
                if (!CheckLeftCollision())
                    return;

            if (pongtop + _dy <= _linepady ||
                pongtop + PongBall.Height + _dy >= PongCanvas.Height - _linepady)
            {
                if (!_mute)
                    meDeflectSound.Play();
                _dy *= -1;
            }

            Canvas.SetTop(PongBall, pongtop + _dy);
            Canvas.SetLeft(PongBall, pongleft + _dx);
            // UpdateLeftPaddle();

            if (_dx < 0)
                UpdateLeftPaddle();
            if (_dx > 0)
            {
                lpaddleTimer.Stop();
                _lpaddleIsEnabled = false;
            }
        }

        private void rpaddleTimer_Tick(object sender, EventArgs e)
        {
            double top = Canvas.GetTop(RightPaddle);
            double height = RightPaddle.Height;

            // Move Down case
            if (_rpaddledy > 0)
            {
                if (top + height + _rpaddledy > PongCanvas.Height)
                {
                    Canvas.SetTop(RightPaddle, PongCanvas.Height - height);
                    return;
                }
            }
            else // Move up case
            {
                if (top + _rpaddledy < 0)
                {
                    Canvas.SetTop(RightPaddle, 0);
                    return;
                }
            }
            Canvas.SetTop(RightPaddle, top + _rpaddledy);
        }

        #region AI Player Specific Methods....Left Paddle
        private void lpaddleTimer_Tick(object sender, EventArgs e)
        {
            double top = Canvas.GetTop(LeftPaddle);
            double center = top + LeftPaddle.Height / 2;
            double height = LeftPaddle.Height;

            // Move Down case
            if (_lpaddledy > 0)
            {
                if (top + height + _lpaddledy > PongCanvas.Height)
                {
                    Canvas.SetTop(LeftPaddle, PongCanvas.Height - height);
                    return;
                }
            }
            else // Move up case
            {
                if (top + _lpaddledy < 0)
                {
                    Canvas.SetTop(LeftPaddle, 0);
                    return;
                }
            }
            Canvas.SetTop(LeftPaddle, top + _lpaddledy);
        }

        private void UpdateLeftPaddle()
        {
            SetAIDestY();
            double top = Canvas.GetTop(LeftPaddle);

            if( _AIDesty >= (top + _AIoffsetdy - _epsilon) &&
                _AIDesty <= (top + _AIoffsetdy + _epsilon ) )
            {
                _lpaddleIsEnabled = false;
                lpaddleTimer.Stop();
                return;
            }

            // Adjust the direction we need to travel
            if ( top + _AIoffsetdy > _AIDesty)
                _lpaddledy = _lpaddledy < 0 ? _lpaddledy : -1 * _lpaddledy;
            else
                _lpaddledy = _lpaddledy > 0 ? _lpaddledy : -1 * _lpaddledy;
        
            if (!_lpaddleIsEnabled)
            {
                lpaddleTimer.Start();
                _lpaddleIsEnabled = true;
            }
        }

        // Generates an AI Offset for the AI Paddle
        private void GenAIOffSet()
        {
            _AIoffsetdy = _rand.Next(0, (int)LeftPaddle.Height);//(int)(LeftPaddle.Height / 2));
        }

        private void SetAIDestY()
        {
            // Calculates the intersection point of the pongball and the leftpadle
            // By setting up an equation for the path of the pongball
            // y=mx+b --> y = (_dy/_dx)x + b --> (pong_y ) = ( _dy/dx)(pong_x) + b
            // b = (pong_y)/((_dy/_dx)(pong_x) )
            // desty = (_dy/dx)(X_leftpaddle) + ( (pong_y)/((_dy/_dx)(pong_x)
            // This y is the place it will intersect the paddle path
            double lpad_x = Canvas.GetLeft( LeftPaddle ) + LeftPaddle.Width;
            double pong_y = Canvas.GetTop(PongBall) + PongBall.Height / 2;
            double pong_x = Canvas.GetLeft(PongBall);
            double y_int = pong_y - ((_dy / _dx) * (pong_x));
            _AIDesty = (_dy/_dx)*(lpad_x) + y_int;

            if (_AIDesty < 0)
                _AIDesty = 0;
            if (_AIDesty > PongCanvas.Height)
                _AIDesty = PongCanvas.Height;
        }
        #endregion

        private bool CheckRightCollision()
        {
            double pongtop = Canvas.GetTop(PongBall);
            double pongleft = Canvas.GetLeft(PongBall);
            double paddletop = Canvas.GetTop(RightPaddle);
            double paddleleft = Canvas.GetLeft(RightPaddle);

            if (((pongtop < paddletop + RightPaddle.Height) && (pongtop > paddletop)) ||
           ((pongtop + PongBall.Height < paddletop + RightPaddle.Height) &&
           (pongtop + PongBall.Height > paddletop)))
            {
                _dx *= -1;
                SetBalldy(pongtop, PongBall.Height, paddletop, RightPaddle.Height);
                if (!_mute)
                    meHitSound.Play();
                GenAIOffSet();
                _hitcount++;
                lblHitCount.Content = _hitcount.ToString();
                return true;
            }
         // The ball went past the paddle without a collision
             _leftpoint++;
             lblLeftPoint.Content = _leftpoint.ToString();
             if (!_mute)
                 meMissSound.Play();

             if (_leftpoint >= _losepoint)
             {
                 LoseHappened("You Lost!!");
                 return false;
             }
             RespawnPongBall(true);
             GenAIOffSet();
             return false;
        }

        private bool CheckLeftCollision()
        {
            double pongtop = Canvas.GetTop(PongBall);
            double pongleft = Canvas.GetLeft(PongBall);
            double paddletop = Canvas.GetTop(LeftPaddle);
            double paddleleft = Canvas.GetLeft(LeftPaddle);

            if (((pongtop < paddletop + LeftPaddle.Height) && (pongtop > paddletop)) ||
           ((pongtop + PongBall.Height < paddletop + LeftPaddle.Height) &&
           (pongtop + PongBall.Height > paddletop)))
            {
                _dx *= -1;
                SetBalldy(pongtop, PongBall.Height, paddletop, LeftPaddle.Height);
                if (!_mute)
                    meHitSound.Play();
                return true;
            }
            // The ball went past the paddle without a collision
            _rightpoint++;
            lblRightPoint.Content = _rightpoint.ToString();
            if (!_mute)
                meMissSound.Play();

            if (_rightpoint >= _losepoint)
            {
                LoseHappened("You Win!!");
                return false;
            }
            RespawnPongBall(false);
            return false;
        }
       
        private void LoseHappened(string text)
        {
            pongballTimer.Stop();
            rpaddleTimer.Stop();
            _userinput = false;
            lblWinLose.Content = text;


            DispatcherTimer soundtimer = new DispatcherTimer();
            soundtimer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            soundtimer.Tick += delegate(object sender, EventArgs args)
            { 
                if(! _mute ) 
                   this.meMissSound.Play();
            };
            soundtimer.Start();
            // Print to a label using the text label
            DispatcherTimer mytimer = new DispatcherTimer();
            mytimer.Interval = new TimeSpan(0, 0, 0, 0,_winlosedt );
            mytimer.Tick += delegate ( object sender, EventArgs args )
                { 
                    mytimer.Stop(); lblWinLose.Content = ""; ResetGame();
                    InitializePongBall(); soundtimer.Stop();
                };
            mytimer.Start();
        }

        // Set the pong ball dy according to where it hit the paddle
        private void SetBalldy(double pongtop, double pongheight, double padtop, double padheight)
        { // Divide the paddle into fifths. Center meanding _dy=0
            double pongcenter =  pongheight/2 + pongtop;

            // Pong Ball is in lower fifth portion of the paddle
            if (pongcenter > 4*(padheight / 5) + padtop)
            { _dy = _changey; return; }
            // In lower fourth 
            if (pongcenter > 3 * (padheight / 5) + padtop)
            { _dy = _changey / 2; return; }
            // In Middle Portion
            if (pongcenter > 2 * (padheight / 5) + padtop)
            { _dy = 0; return; }
            // In upper second portion from top
            if (pongcenter > (padheight / 5) + padtop)
            { _dy = -1 * _changey / 2; return; }
            // In the top portion
            _dy = -1 * _changey;
        }

        private void RespawnPongBall(bool left = true)
        {
            double spawny = _rand.Next( (int)_linepady, (int)(PongCanvas.Height - PongBall.Height-_linepady));
            int y = _rand.Next() % 5 + 1;

            if (left)
            {
                Canvas.SetTop(PongBall, spawny);
                Canvas.SetLeft(PongBall, 0);
                switch (y)
                {
                    case 1:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(LeftPaddle)); } break;
                    case 2:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(LeftPaddle) + LeftPaddle.Height / 4
                              - PongBall.Height / 4);} break;
                    case 3:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(LeftPaddle) + LeftPaddle.Height/3
                            - PongBall.Height/3);} break;
                    case 4:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(LeftPaddle) + LeftPaddle.Height/2
                            - PongBall.Height/2);} break;
                    case 5:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(LeftPaddle) + LeftPaddle.Height
                            - PongBall.Height);} break;
                }

                _dx =  _changex;
                Canvas.SetLeft(PongBall, Canvas.GetLeft(LeftPaddle) + LeftPaddle.Width);
            }
            else
            {
                switch (y)
                {
                    case 1:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(RightPaddle)); } break;
                    case 2:
                        {Canvas.SetTop(PongBall, Canvas.GetTop(RightPaddle) + RightPaddle.Height / 4
                                - PongBall.Height / 4);} break;
                    case 3:
                        {Canvas.SetTop(PongBall, Canvas.GetTop(RightPaddle) + RightPaddle.Height / 3
                              - PongBall.Height / 3);} break;
                    case 4:
                        {Canvas.SetTop(PongBall, Canvas.GetTop(RightPaddle) + RightPaddle.Height / 2
                              - PongBall.Height / 2);} break;
                    case 5:
                        { Canvas.SetTop(PongBall, Canvas.GetTop(RightPaddle) + RightPaddle.Height
                              - PongBall.Height);} break;
                }

                _dx = -1 * _changex;
                Canvas.SetLeft(PongBall, Canvas.GetLeft( RightPaddle ) - PongBall.Width);
            }

            switch (y)
            {
                case 1:
                   { _dy = -1*_changey;} break;
                case 2:
                    { _dy = -1*_changey/2; } break;
                case 3:
                    { _dy = 0; } break;
                case 4:
                    { _dy = _changey / 2; } break;
                case 5:
                    { _dy = _changey; } break;
            }

            // Don't allow a spawn outside of edge padding
            double top = Canvas.GetTop(PongBall);
            if (top < _linepady)
                Canvas.SetTop(PongBall, _linepady);
            if (top + PongBall.Height > PongCanvas.Height - _linepady)
                Canvas.SetTop(PongBall, (PongCanvas.Height - _linepady) - PongBall.Height );
        }

        private void PongWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pongballTimer.Stop();
            double pbtop = Canvas.GetTop(PongBall);
            double pbleft = Canvas.GetLeft(PongBall);
            double rptop = Canvas.GetTop(RightPaddle);
            double rpleft = Canvas.GetLeft(RightPaddle);
            double oldwidth = PongCanvas.Width;
            double oldheight = PongCanvas.Height;
            double rplblleft = Canvas.GetLeft(lblRightPoint);
            double lbltop = Canvas.GetTop(lblWinLose);
            double lblleft = Canvas.GetLeft(lblWinLose);
            double lptop = Canvas.GetTop(LeftPaddle);
            double hitleft = Canvas.GetLeft(lblHitCount);

            PongCanvas.Width = e.NewSize.Width - 50;
            PongCanvas.Height = e.NewSize.Height - 150;

            double dx = (PongCanvas.Width - oldwidth) / 2;
            double dy = (PongCanvas.Height - oldheight) / 2;

            // Scale the _dchange _dx _dy accordingly using ratios
            // Between previose and new dimensions
            _changey = Math.Abs((PongCanvas.Height * _changey) / oldheight);
            _changex = Math.Abs((PongCanvas.Width * _changex) / oldwidth);
            _dx = _dx < 0 ? -1 * _changey : _changey;
            _dx = _dx < 0 ? -1 * _changex : _changex;
            _rpaddledy = Math.Abs((PongCanvas.Height * _rpaddledy) / oldheight);
            _lpaddledy = Math.Abs((PongCanvas.Height * _lpaddledy) / oldheight);

            this.DrawCenterLine(true);
            this.DrawEdgeLines(true);

            Canvas.SetTop(PongBall, pbtop + dy);
            Canvas.SetLeft(PongBall, pbleft + dx);
            Canvas.SetTop(RightPaddle, rptop + dy);
            Canvas.SetLeft(RightPaddle, rpleft + dx * 2);
            Canvas.SetLeft(lblRightPoint, rplblleft + dx * 2);
            Canvas.SetTop(lblWinLose, lbltop + dy);
            Canvas.SetLeft(lblWinLose, lblleft + dx);
            Canvas.SetLeft(lblHitCount, hitleft + dx);
            Canvas.SetTop(LeftPaddle, lptop + dy);

            if (_userinput)
                pongballTimer.Start();
        }

        private void InitializePongBall()
        {
            Canvas.SetLeft(PongBall, PongCanvas.Width / 2 - PongBall.Width / 2);
            Canvas.SetTop(PongBall, PongCanvas.Height / 2 - PongBall.Height / 2);
            if (_rand.Next() % 2 == 1)
                _dx = -1 * _dx;
        }
        #endregion


        #region Game Play HUD Input events
        private void PongWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.W) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                CheatLosePoint();
                return;
            }
            if ((e.Key == Key.M) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                ToggleMute();
                return;
            }

            if ((e.Key == Key.R) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                ResetGame();
                return;
            }

            if (_userinput)
            {
                if (e.Key == Key.Down || e.Key == Key.Up)
                {
                    if (e.Key == Key.Down)
                        _rpaddledy = _rpaddledy > 0 ? _rpaddledy : -1 * _rpaddledy;
                    else if (e.Key == Key.Up)
                        _rpaddledy = _rpaddledy < 0 ? _rpaddledy : -1 * _rpaddledy;

                    rpaddleTimer.Start();
                }

                if (e.Key == Key.Escape)
                    if (_PauseIsPause)
                        PauseGame();
                return;
            }
            if (e.Key == Key.Enter)
                if (_StartIsStart)
                  StartGame();
            if (e.Key == Key.Escape)
                if (!_PauseIsPause)
                    ResumeGame();
        }

        private void CheatLosePoint()
        {
            pongballTimer.Stop();
            MessageBoxResult result = MessageBox.Show("Ignore Lose Point?",
                    "Game of Pong", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                _losepoint = int.MaxValue;
            if (result == MessageBoxResult.No)
                _losepoint = 9;

            if (_userinput)
                pongballTimer.Start();
        }
        private void PongWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_userinput)
                return;
            if (e.Key == Key.Down || e.Key ==  Key.Up)
                rpaddleTimer.Stop();
        }

        private void PongWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_userinput)
                return;

            Point p = e.GetPosition(PongCanvas);
            double newtop = p.Y - RightPaddle.Height / 2;

            if (newtop < 0)
            {
                Canvas.SetTop(RightPaddle, 0);
                return;
            }
            if (newtop + RightPaddle.Height > PongCanvas.Height)
            {
                Canvas.SetTop(RightPaddle, PongCanvas.Height - RightPaddle.Height);
                return;
            }
            Canvas.SetTop(RightPaddle, newtop);
        }
        #endregion


        #region Canvas Mouse Events
        private void PongCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_userinput)
                return;
           // pongballTimer.Start();
            this.Cursor = Cursors.Cross;
        }

        private void PongCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
          //  pongballTimer.Stop();
            this.Cursor = Cursors.Arrow;
        }
        #endregion


        #region Game State Input Events
        private void StartNewGameFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_StartIsStart)
            { StartGame(); return; }

            ResetGame();
        }

        private void PauseFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_PauseIsPause)
            { PauseGame(); return; }

            ResumeGame();
        }

        private void lblStart_Reset_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_StartIsStart)
            { StartGame(); return; }

            ResetGame();
        }

        private void lblPause_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_PauseIsPause)
            { PauseGame(); return; }

            ResumeGame();
        }
        #endregion


        #region Game State Methods
        private void StartGame()
        {
            pongballTimer.Start();
            StartNewGameFileMenuItem.IsEnabled = false;
            _userinput = true;
            lblStart_Reset.Content = "_Reset";
            StartNewGameFileMenuItem.Header = "_Reset";
            _StartIsStart = false;
            lblPause.IsEnabled = true;
            PauseFileMenuItem.IsEnabled = true;
            StartNewGameFileMenuItem.IsEnabled = true;
        }

        private void ResetGame()
        {
            pongballTimer.Stop();
            rpaddleTimer.Stop();
            lpaddleTimer.Stop();
            _lpaddleIsEnabled = false;
            _rightpoint = 0;
            _leftpoint = 0;
            _hitcount = 0;
            _userinput = false;
            _StartIsStart = true;
            _PauseIsPause = true;
            lblRightPoint.Content = _rightpoint.ToString();
            lblLeftPoint.Content = _leftpoint.ToString();
            lblStart_Reset.Content = "Start";
            lblPause.Content = "Pause";
            lblHitCount.Content = _hitcount.ToString();
            PauseFileMenuItem.Header = "_Pause";
            StartNewGameFileMenuItem.Header = "_Start";

            Canvas.SetTop(RightPaddle, PongCanvas.Height / 2 - RightPaddle.Height / 2);
            Canvas.SetTop(LeftPaddle, PongCanvas.Height / 2 - LeftPaddle.Height / 2);
           /* int r = _rand.Next() % 2;

            if (r == 0)
            { RespawnPongBall(true); }
            else
            { RespawnPongBall(false); }*/
            InitializePongBall();

            lblPause.IsEnabled = false;
            PauseFileMenuItem.IsEnabled = false;
        }

        private void ResumeGame()
        {
            lblPause.Content = "_Pause";
            PauseFileMenuItem.Header = "_Pause";
            _PauseIsPause = true;
            pongballTimer.Start();
            _userinput = true;
            if (_lpaddleIsEnabled)
                lpaddleTimer.Start();
        }

        private void PauseGame()
        {
            pongballTimer.Stop();
            rpaddleTimer.Stop();
            lpaddleTimer.Stop();
            _userinput = false;
            lblPause.Content = "_Resume";
            PauseFileMenuItem.Header = "_Resume";
            _PauseIsPause = false;
        }
        #endregion


        #region Label_Button Related Events
        private void lblStart_Reset_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            lblStart_Reset.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF35AAF8"));
        }

        private void lblStart_Reset_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            lblStart_Reset.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF355FF8"));
        }

        private void lblPause_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            lblPause.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF35AAF8"));
        }

        private void lblPause_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            lblPause.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF355FF8"));
        }
        #endregion


        #region Misc Input Events
        //Provid a closing dialog box...
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pongballTimer.Stop();
            lpaddleTimer.Stop();
            MessageBoxResult result = MessageBox.Show("Exit Game of Pong?",
                    "Game of Pong", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                e.Cancel = true;
            if (_userinput)
                pongballTimer.Start();
            if (_lpaddleIsEnabled)
                lpaddleTimer.Start();
        }

        private void CloseFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MuteFilemenuItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleMute();
        }

        private void ToggleMute()
        {
            if (_MuteIsMute)
            {
                _mute = true;
                MuteFilemenuItem.IsChecked = true;
                _MuteIsMute = false;
            }
            else
            {
                _mute = false;
                MuteFilemenuItem.IsChecked = false;
                _MuteIsMute = true;
            }
        }
        private void AboutHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            pongballTimer.Stop();
            lpaddleTimer.Stop();
            PongAboutBox p = new PongAboutBox();
            p.ShowDialog();
            if (_userinput)
                pongballTimer.Start();
            if (_lpaddleIsEnabled)
                lpaddleTimer.Start();
        }

        private void UsageHelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            pongballTimer.Stop();
            lpaddleTimer.Stop();
            UsageWindow u = new UsageWindow();
            u.ShowDialog();
            if (_userinput)
                pongballTimer.Start();
            if (_lpaddleIsEnabled)
                lpaddleTimer.Start();
        }
        #endregion
    }
}
