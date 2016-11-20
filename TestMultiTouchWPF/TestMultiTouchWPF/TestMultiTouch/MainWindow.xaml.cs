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
using System.IO;
using Microsoft.VisualBasic.FileIO;



namespace TestMultiTouch
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // タッチの位置の図形データ
        private Dictionary<int, Mark> touchPoints = new Dictionary<int, Mark>();
        // マウスの位置の図形データ
        private Mark mousePoint;
        // スタイラスの位置の図形データ
        private Dictionary<int, Mark> stylusPoints = new Dictionary<int, Mark>();
        private const int TOUCH_MARK_SIZE = 250;        // タッチの図形のサイズ
        private const int MOUSE_MARK_SIZE = 200;        // マウスの図形のサイズ
        private const int STYLUS_MARK_SIZE = 25;        // スタイラスの図形のサイズ(最小値)
        private const int STYLUS_MARK_SIZE_MAX = 200;   // スタイラスの図形のサイズ(最大値)
        private DateTime End;
        private DateTime Start;
        private double distance = 0;
        private double distance1 = 0;
        private double distance2 = 0;
        private double distance3 = 0;
        private double distance4 = 0;
        private double distance5 = 0;
        private double distance6 = 0;
        private double x = 0;
        private double y = 0;
        private StreamWriter record_distance, record_point;

        private Boolean mouse_key_enable = false;
        private Boolean touch_key_enable = true;

        private TextBox tb1 = new TextBox();
        private TextBox tb2 = new TextBox();


        /// <summary>
        /// 図形データ
        /// </summary>
        class Mark
        {
            // 円の上の文字列を表示するラベル
            public Label label { get; private set; }
            // タッチやクリック位置の円マーク
            public Ellipse ellipse { get; private set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            private Mark()
            {
                label = new Label();
                ellipse = new Ellipse();
            }

            /// <summary>
            /// Markクラスのインスタンス生成関数
            /// </summary>
            /// <param name="text">表示する文字</param>
            /// <param name="size">円のサイズ</param>
            /// <param name="color">色</param>
            /// <returns>Markクラスのインスタンス</returns>
            public static Mark makeMark(string text, int size, Color color)
            {
                Mark mark = new Mark();
                mark.ellipse.Width = size;
                mark.ellipse.Height = size;
                mark.ellipse.Stroke = new SolidColorBrush(color);
                mark.ellipse.StrokeThickness = 1;
                mark.label.Content = text;
                return mark;
            }

            /// <summary>
            /// 座標を指定
            /// </summary>
            /// <param name="pos">座標</param>
            public void SetPos(Point pos)
            {
                Canvas.SetLeft(ellipse, pos.X - ellipse.ActualWidth / 2);
                Canvas.SetTop(ellipse, pos.Y - ellipse.ActualHeight / 2);

                Canvas.SetLeft(label, pos.X - label.ActualWidth / 2);
                Canvas.SetTop(label, pos.Y - ellipse.ActualHeight / 2 - label.ActualHeight);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            //画面分割（いる？）
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            myLine.X1 = (int)(Width / 4);
            myLine.X2 = (int)(Width / 4);
            myLine.Y1 = 0;
            myLine.Y2 = (int)Height;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 2;
            canvas.Children.Add(myLine);
            Line myLine2 = new Line();
            myLine2.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            myLine2.X1 = (int)(Width * 2/ 4);
            myLine2.X2 = (int)(Width * 2/ 4);
            myLine2.Y1 = 0;
            myLine2.Y2 = (int)Height;
            myLine2.HorizontalAlignment = HorizontalAlignment.Left;
            myLine2.VerticalAlignment = VerticalAlignment.Center;
            myLine2.StrokeThickness = 2;
            canvas.Children.Add(myLine2);
            Line myLine3 = new Line();
            myLine3.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            myLine3.X1 = (int)(Width * 3 / 4);
            myLine3.X2 = (int)(Width * 3 / 4);
            myLine3.Y1 = 0;
            myLine3.Y2 = (int)Height;
            myLine3.HorizontalAlignment = HorizontalAlignment.Left;
            myLine3.VerticalAlignment = VerticalAlignment.Center;
            myLine3.StrokeThickness = 2;
            canvas.Children.Add(myLine3);
            Line myLine4 = new Line();
            myLine4.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            myLine4.X1 = 0;
            myLine4.X2 = (int)(Width);
            myLine4.Y1 = (int)(Height / 3);
            myLine4.Y2 = (int)(Height / 3);
            myLine4.HorizontalAlignment = HorizontalAlignment.Left;
            myLine4.VerticalAlignment = VerticalAlignment.Center;
            myLine4.StrokeThickness = 2;
            canvas.Children.Add(myLine4);
            Line myLine5 = new Line();
            myLine5.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            myLine5.X1 = 0;
            myLine5.X2 = (int)(Width);
            myLine5.Y1 = (int)(Height * 2 / 3);
            myLine5.Y2 = (int)(Height * 2 / 3);
            myLine5.HorizontalAlignment = HorizontalAlignment.Left;
            myLine5.VerticalAlignment = VerticalAlignment.Center;
            myLine5.StrokeThickness = 2;
            canvas.Children.Add(myLine5);

            //ファイル書き込み準備
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("Shift_JIS");
            record_distance = new StreamWriter(@"Record_Distance1.csv", true, enc);
            record_point = new StreamWriter(@"Record_Point.csv", true, enc);
            StreamWriter fp = new StreamWriter(@"Test.csv", true, enc);
            fp.WriteLine("1,2,3");
            record_distance.WriteLine("testdata");
            record_distance.WriteLine("koke,koke");


            //ビューの準備
            subcanvas1.Visibility = Visibility.Hidden;
            subcanvas2.Visibility = Visibility.Hidden;
            subcanvas3.Visibility = Visibility.Hidden;
            subcanvas4.Visibility = Visibility.Hidden;
            subcanvas5.Visibility = Visibility.Hidden;
            subcanvas6.Visibility = Visibility.Hidden;
            subcanvas7.Visibility = Visibility.Hidden;
            subcanvas8.Visibility = Visibility.Hidden;
            subcanvas9.Visibility = Visibility.Hidden;
            subcanvas10.Visibility = Visibility.Hidden;
            subcanvas11.Visibility = Visibility.Hidden;
            subcanvas12.Visibility = Visibility.Hidden;

            subcanvas1.Width = Width / 4; subcanvas1.Height = Height / 3;
            subcanvas2.Width = Width / 4; subcanvas2.Height = Height / 3;
            subcanvas3.Width = Width / 4; subcanvas3.Height = Height / 3;
            subcanvas4.Width = Width / 4; subcanvas4.Height = Height / 3;
            subcanvas5.Width = Width / 4; subcanvas5.Height = Height / 3;
            subcanvas6.Width = Width / 4; subcanvas6.Height = Height / 3;
            subcanvas7.Width = Width / 4; subcanvas7.Height = Height / 3;
            subcanvas8.Width = Width / 4; subcanvas8.Height = Height / 3;
            subcanvas9.Width = Width / 4; subcanvas9.Height = Height / 3;
            subcanvas10.Width = Width / 4; subcanvas10.Height = Height / 3;
            subcanvas11.Width = Width / 4; subcanvas11.Height = Height / 3;
            subcanvas12.Width = Width / 4; subcanvas12.Height = Height / 3;

            Canvas.SetTop(subcanvas1, 0); Canvas.SetLeft(subcanvas1, 0);
            Canvas.SetTop(subcanvas2, 0); Canvas.SetLeft(subcanvas2, Width / 4 );
            Canvas.SetTop(subcanvas3, 0); Canvas.SetLeft(subcanvas3, Width * 2 / 4);
            Canvas.SetTop(subcanvas4, 0); Canvas.SetLeft(subcanvas4, Width * 3 / 4);
            Canvas.SetTop(subcanvas5, Height / 3); Canvas.SetLeft(subcanvas5, 0);
            Canvas.SetTop(subcanvas6, Height / 3); Canvas.SetLeft(subcanvas6, Width / 4);
            Canvas.SetTop(subcanvas7, Height / 3); Canvas.SetLeft(subcanvas7, Width  * 2 / 4);
            Canvas.SetTop(subcanvas8, Height / 3); Canvas.SetLeft(subcanvas8, Width * 3 / 4);
            Canvas.SetTop(subcanvas9, Height * 2 / 3); Canvas.SetLeft(subcanvas9, 0);
            Canvas.SetTop(subcanvas10, Height * 2 / 3); Canvas.SetLeft(subcanvas10, Width / 4);
            Canvas.SetTop(subcanvas11, Height * 2 / 3); Canvas.SetLeft(subcanvas11, Width * 2 / 4);
            Canvas.SetTop(subcanvas12, Height * 2 / 3); Canvas.SetLeft(subcanvas12, Width * 3 / 4);



            tb1.Width = 100;tb1.Height = 50;
            tb1.Text = "none";
            canvas.Children.Add(tb1);
            tb2.Width = 100; tb1.Height = 50;
            tb2.Text = "none";
            canvas.Children.Add(tb2);
            Canvas.SetTop(tb2, 50);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }



        /// <summary>
        /// タッチダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_TouchDown(object sender, TouchEventArgs e)
        {

            if (touch_key_enable)
            {

                //Pucs検出
                pucs_recog(e);

                // 座標取得
                TouchPoint p = e.GetTouchPoint(canvas);
                // 図形生成
                Mark mark = Mark.makeMark(e.TouchDevice.Id.ToString(), TOUCH_MARK_SIZE, Colors.Red);
                // 図形をキャンバスへ追加
                canvas.Children.Add(mark.ellipse);
                canvas.Children.Add(mark.label);
                canvas.UpdateLayout();
                // 座標を指定
                mark.SetPos(p.Position);

                // タッチをキャプチャ
                canvas.CaptureTouch(e.TouchDevice);
                // IDごとに図形を保存
                touchPoints[e.TouchDevice.Id] = mark;

                Start = DateTime.UtcNow;
                //Console.WriteLine("Start"+ e.TouchDevice.Id.ToString() + ":"  + Start);

                Console.WriteLine("state:" + "touchdown" + " x:" + p.Position.X + " y:" + p.Position.Y);

                //前のタッチ終了点との距離を算出
                distance = Math.Sqrt((x - p.Position.X) * (x - p.Position.X) + (y - p.Position.Y) * (y - p.Position.Y));

                distance6 = distance5;
                distance5 = distance4;
                distance4 = distance3;
                distance3 = distance2;
                distance2 = distance1;
                distance1 = distance;

                tb1.Text = "" + distance1;
                tb2.Text = "" + distance2;

                tb1.Text = "" + e.TouchDevice.Id　;

                //マーカ判定
                if (p.Position.X < Width / 4)
                {
                    if (p.Position.Y < Height / 3)
                    {
                        recog_marker(subcanvas1);
                    }
                    else if (p.Position.Y < Height * 2 / 3)
                    {
                        recog_marker(subcanvas5);
                    }
                    else
                    {
                        recog_marker(subcanvas9);
                    }
                }
                else if (p.Position.X < Width * 2 / 4)
                {
                    if (p.Position.Y < Height / 3)
                    {
                        recog_marker(subcanvas2);
                    }
                    else if (p.Position.Y < Height * 2 / 3)
                    {
                        recog_marker(subcanvas6);
                    }
                    else
                    {
                        recog_marker(subcanvas10);
                    }
                }
                else if (p.Position.X < Width * 3 / 4)
                {
                    if (p.Position.Y < Height / 3)
                    {
                        recog_marker(subcanvas3);
                    }
                    else if (p.Position.Y < Height * 2 / 3)
                    {
                        recog_marker(subcanvas7);
                    }
                    else
                    {
                        recog_marker(subcanvas11);
                    }
                }else {
                    if (p.Position.Y < Height / 3)
                    {
                        recog_marker(subcanvas4);
                    }
                    else if (p.Position.Y < Height * 2 / 3)
                    {
                        recog_marker(subcanvas8);
                    }
                    else
                    {
                        recog_marker(subcanvas12);
                    }
                }

                Line line = new Line();
                line.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                line.X1 = x;
                line.Y1 = y;
                line.X2 = p.Position.X;
                line.Y2 = p.Position.Y;
                canvas.Children.Add(line);

                Console.WriteLine("distance1:" + distance1);
                Console.WriteLine("distance2:" + distance2);

                //ファイル書き込み
                //record_distance.WriteLine(distance1 + "," + distance2);
            }
        }

        private void subcanvas_select(Canvas subcanvas, Color color)
        {
            SolidColorBrush colorbrush = new SolidColorBrush(color);
            /*if (subcanvas.Background == colorbrush)
            {
                if (subcanvas.IsVisible)
                {
                    subcanvas.Visibility = Visibility.Hidden;
                }
                else
                {
                    subcanvas.Visibility = Visibility.Visible;
                }
                subcanvas.Background = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                subcanvas.Background = colorbrush;*/
            subcanvas.Background = colorbrush;
            if (subcanvas.IsVisible)
            {
                subcanvas.Visibility = Visibility.Hidden;
            }
            else
            {
                subcanvas.Visibility = Visibility.Visible;
            }
        }

        private void recog_marker(Canvas subcanvas){
            if (Range_in(distance1, 35, 25) && Range_in(distance2, 45, 35))
            {
                subcanvas_select(subcanvas, Colors.Red);
            }
            else if (Range_in(distance1, 45, 35) && Range_in(distance2, 35, 25))
            {
                subcanvas_select(subcanvas, Colors.Blue);
            }
            else if (Range_in(distance1, 35, 25) && Range_in(distance2, 35, 25))
            {
                subcanvas_select(subcanvas, Colors.Cyan);
            }

            if (Range_in(distance1, 35, 25) && Range_in(distance2, 25, 15)
                && Range_in(distance3, 25, 15) && Range_in(distance4, 35, 25)
                && Range_in(distance5, 25, 15)
                )
            {
                subcanvas_select(subcanvas, Colors.Yellow);
            }
            else if (Range_in(distance1, 25, 15) && Range_in(distance2, 35, 25)
              && Range_in(distance3, 25, 15) && Range_in(distance4, 25, 15)
              && Range_in(distance5, 35, 25)
              )
            {
                subcanvas_select(subcanvas, Colors.Green);
            }

        }

        private void pucs_recog(TouchEventArgs e){
            int touchcount = e.TouchDevice.Id - 99;

            if (touchcount > 2)
            {
                for (int i = 0; i < touchcount; i++)
                {
                    double j =  e.GetTouchPoint(canvas).Position.X;
                    
                }
            }
        }

        /// <summary>
        /// タッチムーブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_TouchMove(object sender, TouchEventArgs e)
        {
            if (touch_key_enable)
            {
                TouchPoint p = e.GetTouchPoint(canvas);
                if (e.TouchDevice.Captured == canvas)
                {
                    // IDごとの図形を取得
                    Mark mark = touchPoints[e.TouchDevice.Id];
                    // 座標を指定
                    //TouchPoint p = e.GetTouchPoint(canvas);
                    mark.SetPos(p.Position);
                }


                Console.WriteLine("state:" + "touchmove" + " x:" + p.Position.X + " y:" + p.Position.Y);
               
            }
        }

        /// <summary>
        /// タッチアップイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_TouchUp(object sender, TouchEventArgs e)
        {
            if (touch_key_enable)
            {
                // タッチキャプチャをリリース
                canvas.ReleaseTouchCapture(e.TouchDevice);
                // 図形を削除
                canvas.Children.Remove(touchPoints[e.TouchDevice.Id].label);
                canvas.Children.Remove(touchPoints[e.TouchDevice.Id].ellipse);
                touchPoints.Remove(e.TouchDevice.Id);

                End = DateTime.UtcNow;
                //Console.WriteLine("End" + e.TouchDevice.Id.ToString() + ":" + End);

                //タッチ終了時の座標を記録
                x = e.GetTouchPoint(canvas).Position.X;
                y = e.GetTouchPoint(canvas).Position.Y;

                TouchPoint p = e.GetTouchPoint(canvas);
                Console.WriteLine("state:" + "touchup" + " x:" + p.Position.X + " y:" + p.Position.Y);
            }
            subcanvas1.InvalidateVisual();
        }

        private void canvas_TouchEnter(object sender, TouchEventArgs e)
        {
            if (touch_key_enable)
            {
                TouchPoint p = e.GetTouchPoint(canvas);
                if (e.TouchDevice.Captured == canvas)
                {
                    // IDごとの図形を取得
                    Mark mark = touchPoints[e.TouchDevice.Id];
                    // 座標を指定
                    //TouchPoint p = e.GetTouchPoint(canvas);
                    mark.SetPos(p.Position);
                }


                //Console.WriteLine("state:" + "touchenter" + " x:" + p.Position.X + " y:" + p.Position.Y);
            }
        }


        private void canvas_TouchLeave(object sender, TouchEventArgs e)
        {
            if (touch_key_enable)
            {
                TouchPoint p = e.GetTouchPoint(canvas);
                if (e.TouchDevice.Captured == canvas)
                {
                    // IDごとの図形を取得
                    Mark mark = touchPoints[e.TouchDevice.Id];
                    // 座標を指定
                    //TouchPoint p = e.GetTouchPoint(canvas);
                    mark.SetPos(p.Position);
                }


               // Console.WriteLine("state:" + "touchmove" + " x:" + p.Position.X + " y:" + p.Position.Y);
            }
        }

        /// <summary>
        /// マウスのボタン状態を文字列で取得
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string getButtonText(MouseEventArgs e)
        {
            string text = "";
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                text += "L";
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                text += "M";
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                text += "R";
            }
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                text += "1";
            }
            if (e.XButton2 == MouseButtonState.Pressed)
            {
                text += "2";
            }
            return text;
        }

        /// <summary>
        /// マウスダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mouse_key_enable)
            {
                // 座標取得
                Point p = e.GetPosition(canvas);

                // 図形を生成
                Mark mark = Mark.makeMark(getButtonText(e), MOUSE_MARK_SIZE, Colors.Blue);
                if (mousePoint != null)
                {
                    canvas.Children.Remove(mousePoint.label);
                    canvas.Children.Remove(mousePoint.ellipse);
                }

                // 図形をキャンバスへ追加
                canvas.Children.Add(mark.label);
                canvas.Children.Add(mark.ellipse);
                canvas.UpdateLayout();

                // 座標を指定
                mark.SetPos(p);

                // マウスをキャプチャ
                canvas.CaptureMouse();
                // 図形を保存
                mousePoint = mark;

                Start = DateTime.UtcNow;
                //Console.WriteLine("Start mouse" + ":" + Start);

                Console.WriteLine("state:" + "mousedown" + " x:" + p.X + " y:" + p.Y);

                //前のタッチ終了点との距離を算出
                distance = Math.Sqrt((x - p.X) * (x - p.X) + (y - p.Y) * (y - p.Y));

                distance2 = distance1;
                distance1 = distance;

                //マーカ判定
                if (Range_in(distance1, 35, 25) && Range_in(distance2, 25, 15))
                {
                    Console.WriteLine("Mark1");
                    //Canvas.SetTop(canvas3, p.X);
                    //Canvas.SetLeft(canvas3, p.Y);
                    //canvas3.Children.Add();
                }
                else if (Range_in(distance1, 25, 15) && Range_in(distance2, 35, 25))
                {
                    Console.WriteLine("Mark2");
                    //Canvas.SetTop(canvas3, p.X);
                    //Canvas.SetLeft(canvas3, p.Y);
                }

                Console.WriteLine("distance1:" + distance1);
                Console.WriteLine("distance2:" + distance2);

                //ファイル書き込み
                //record_distance.WriteLine(distance1 + "," + distance2);
                //record_point.WriteLine(p.X + "," + p.Y);
            }
        }

        /// <summary>
        /// マウスムーブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouse_key_enable)
            {
                Point p = e.GetPosition(canvas);
                if (e.MouseDevice.Captured == canvas)
                {
                    //Point p = e.GetPosition(canvas);
                    if (mousePoint != null)
                    {
                        // 座標を指定
                        mousePoint.SetPos(p);
                        // ラベルにマウスのボタンの状態を設定
                        mousePoint.label.Content = getButtonText(e);
                    }
                }


                Console.WriteLine("state:" + "mousemove" + " x:" + p.X + " y:" + p.Y);
            }
        }

        /// <summary>
        /// マウスアップイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouse_key_enable)
            {
                // マウスのキャプチャをリリース
                canvas.ReleaseMouseCapture();
                // 図形を削除
                if (mousePoint != null)
                {
                    //canvas.Children.Remove(mousePoint.label);
                    //canvas.Children.Remove(mousePoint.ellipse);
                }
                mousePoint = null;

                End = DateTime.UtcNow;
                //Console.WriteLine("Start" + getButtonText(e) + ":" + Start);

                //タッチ終了時の座標を記録
                x = e.GetPosition(canvas).X;
                y = e.GetPosition(canvas).Y;

                //Touch_marker(@"Record_Distance.csv");

                var win = new Window2();
                win.Width = Width / 3;
                win.Height = Height / 2;
                //win.Show();

                Point p = e.GetPosition(canvas);
                Console.WriteLine("state:" + "mouseup" + " x:" + p.X + " y:" + p.Y);
            }
        }


        private Boolean Range_in(Double num, Double max, Double min)
        {
            if (num > min && num < max)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //マーカの認識処理
        private void Touch_marker(string filename)
        {
            string file = filename;
            Console.WriteLine(file + "================================");

            TextFieldParser parser = new TextFieldParser(file, System.Text.Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ
            parser.CommentTokens = new string[1] { "#" };
            int line = 0, col = 0;
            while (!parser.EndOfData)
            {
                ++line;
                col = 0;
                string[] row = parser.ReadFields(); // 1行読み込み
                Console.WriteLine("{0}", line);
                // 配列rowの要素は読み込んだ行の各フィールドの値
                foreach (string field in row)
                {
                    ++col;
                    Console.WriteLine("{0}:{1}", col, field);
                }
                Console.WriteLine("----------------------------");
            }
            parser.Close();
        }

        //csvのパーサー
        private void CSV_Reader(string filename)
        {
            string file = filename;
            Console.WriteLine(file + "================================");

            TextFieldParser parser = new TextFieldParser(file, System.Text.Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ
            parser.CommentTokens = new string[1] { "#" };
            int line = 0, col = 0;
            while (!parser.EndOfData)
            {
                ++line;
                col = 0;
                string[] row = parser.ReadFields(); // 1行読み込み
                Console.WriteLine("{0}", line);
                // 配列rowの要素は読み込んだ行の各フィールドの値
                foreach (string field in row)
                {
                    ++col;
                    Console.WriteLine("{0}:{1}", col, field);
                }
                Console.WriteLine("----------------------------");
            }
            parser.Close();
        }

        /// <summary>
        /// スタイラスダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_StylusDown(object sender, StylusDownEventArgs e)
        {
            // 座標取得
            Point p = e.GetPosition(canvas);
            // スタイラスの情報取得
            StylusPointCollection pcol = e.GetStylusPoints(canvas);
            // 筆圧から円のサイズを算出
            int size = (int)((STYLUS_MARK_SIZE_MAX - STYLUS_MARK_SIZE) * pcol[0].PressureFactor + STYLUS_MARK_SIZE);
            // 図形生成
            Mark mark = Mark.makeMark(e.StylusDevice.Id.ToString(), size, Colors.Green);

            // 図形をキャンバスへ追加
            canvas.Children.Add(mark.label);
            canvas.Children.Add(mark.ellipse);
            canvas.UpdateLayout();

            // 座標を指定
            mark.SetPos(p);

            // スタイラスをキャプチャ
            canvas.CaptureStylus();
            // IDごとに図形を保存
            stylusPoints[e.StylusDevice.Id] = mark;
        }

        /// <summary>
        /// スタイラスムーブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_StylusMove(object sender, StylusEventArgs e)
        {
            if (e.StylusDevice.Captured == canvas)
            {

                Point p = e.GetPosition(canvas);
                // スタイラスの情報を取得
                StylusPointCollection pcol = e.GetStylusPoints(canvas);
                // 筆圧から円のサイズを算出
                int size = (int)((STYLUS_MARK_SIZE_MAX - STYLUS_MARK_SIZE) * pcol[0].PressureFactor + STYLUS_MARK_SIZE);
                // 図形のサイズを指定
                stylusPoints[e.StylusDevice.Id].ellipse.Width = size;
                stylusPoints[e.StylusDevice.Id].ellipse.Height = size;
                canvas.UpdateLayout();
                // 座標を指定
                stylusPoints[e.StylusDevice.Id].SetPos(p);
            }
        }

        /// <summary>
        /// スタイラスアップイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_StylusUp(object sender, StylusEventArgs e)
        {
            // スタイラスのキャプチャをリリース
            canvas.ReleaseStylusCapture();
            // 図形を削除
            canvas.Children.Remove(stylusPoints[e.StylusDevice.Id].label);
            canvas.Children.Remove(stylusPoints[e.StylusDevice.Id].ellipse);
            stylusPoints.Remove(e.StylusDevice.Id);
        }

        private void canvas3_view(object sender, TouchEventArgs e)
        {
            Console.WriteLine("called");
        }
    }
}

