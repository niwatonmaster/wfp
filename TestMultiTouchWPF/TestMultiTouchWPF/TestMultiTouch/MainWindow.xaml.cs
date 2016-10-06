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
        private Dictionary<int, Mark> stylusPoints = new Dictionary<int,Mark>();
        private const int TOUCH_MARK_SIZE = 250;        // タッチの図形のサイズ
        private const int MOUSE_MARK_SIZE = 200;        // マウスの図形のサイズ
        private const int STYLUS_MARK_SIZE = 25;        // スタイラスの図形のサイズ(最小値)
        private const int STYLUS_MARK_SIZE_MAX = 200;   // スタイラスの図形のサイズ(最大値)
        private DateTime End;
        private DateTime Start;
        private double distance = 0;
        private double distance1 = 0;
        private double distance2 = 0;
        private double x = 0;
        private double y = 0;
        private StreamWriter record_distance, record_point;


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
            public static Mark makeMark(string text, int size, Color color) {
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
            myLine.X1 = (int)(Width / 2);
            myLine.X2 = (int)(Width / 2);
            myLine.Y1 = 0;
            myLine.Y2 = (int) Height;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 2;
            canvas.Children.Add(myLine);
            Console.WriteLine(Width + " "  + Height);

            //ファイル書き込み準備
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("Shift_JIS");
            record_distance = new StreamWriter(@"Record_Distance1.csv",true, enc);
            record_point = new StreamWriter(@"Record_Point.csv",true, enc);
            StreamWriter fp = new StreamWriter(@"Test.csv", true, enc);
            fp.WriteLine("1,2,3");
            record_distance.WriteLine("testdata");
            record_distance.WriteLine("koke,koke");

           
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
            Console.WriteLine("Start"+ e.TouchDevice.Id.ToString() + ":"  + Start);

            //前のタッチ終了点との距離を算出
            distance = Math.Sqrt((x - p.Position.X) * (x - p.Position.X) + (y - p.Position.Y) * (y - p.Position.Y) );

            distance2 = distance1;
            distance1 = distance;

            //マーカ判定
            if (Range_in(distance1, 35, 25) && Range_in(distance2, 25, 15)) {
                Console.WriteLine("Mark1");
                Canvas.SetTop(canvas3, p.Position.X);
                Canvas.SetLeft(canvas3, p.Position.Y);
            }
            else if( Range_in(distance1, 25, 15) && Range_in(distance2, 35, 25)){
                Console.WriteLine("Mark2");
                Canvas.SetTop(canvas3, p.Position.X);
                Canvas.SetLeft(canvas3, p.Position.Y);
            }

            Console.WriteLine("distance1:" + distance1);
            Console.WriteLine("distance2:" + distance2);

            //ファイル書き込み
            //record_distance.WriteLine(distance1 + "," + distance2);
        }

        /// <summary>
        /// タッチムーブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_TouchMove(object sender, TouchEventArgs e)
        {
            if (e.TouchDevice.Captured == canvas)
            {
                // IDごとの図形を取得
                Mark mark = touchPoints[e.TouchDevice.Id];
                // 座標を指定
                TouchPoint p = e.GetTouchPoint(canvas);
                mark.SetPos(p.Position);
            }
        }

        /// <summary>
        /// タッチアップイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_TouchUp(object sender, TouchEventArgs e)
        {
            // タッチキャプチャをリリース
            canvas.ReleaseTouchCapture(e.TouchDevice);
            // 図形を削除
            //canvas.Children.Remove(touchPoints[e.TouchDevice.Id].label);
            //canvas.Children.Remove(touchPoints[e.TouchDevice.Id].ellipse);
            touchPoints.Remove(e.TouchDevice.Id);

            End = DateTime.UtcNow;
            Console.WriteLine("End" + e.TouchDevice.Id.ToString() + ":" + End);

            //タッチ終了時の座標を記録
            x = e.GetTouchPoint(canvas).Position.X;
            y = e.GetTouchPoint(canvas).Position.Y;
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
            Console.WriteLine("Start mouse" + ":" + Start);

            //前のタッチ終了点との距離を算出
            distance = Math.Sqrt((x - p.X) * (x - p.X) + (y - p.Y) * (y - p.Y));

            distance2 = distance1;
            distance1 = distance;

            //マーカ判定
            if (Range_in(distance1, 35, 25) && Range_in(distance2, 25, 15))
            {
                Console.WriteLine("Mark1");
                Canvas.SetTop(canvas3,p.X);
                Canvas.SetLeft(canvas3, p.Y);
            }
            else if (Range_in(distance1, 25, 15) && Range_in(distance2, 35, 25))
            {
                Console.WriteLine("Mark2");
                Canvas.SetTop(canvas3, p.X);
                Canvas.SetLeft(canvas3, p.Y);
            }

            Console.WriteLine("distance1:" + distance1);
            Console.WriteLine("distance2:" + distance2);

            //ファイル書き込み
            //record_distance.WriteLine(distance1 + "," + distance2);
            //record_point.WriteLine(p.X + "," + p.Y);
        }

        /// <summary>
        /// マウスムーブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.Captured == canvas)
            {
                Point p = e.GetPosition(canvas);
                if (mousePoint != null)
                {
                    // 座標を指定
                    mousePoint.SetPos(p);
                    // ラベルにマウスのボタンの状態を設定
                    mousePoint.label.Content = getButtonText(e);
                }
            }
        }

        /// <summary>
        /// マウスアップイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // マウスのキャプチャをリリース
            canvas.ReleaseMouseCapture();
            // 図形を削除
            if(mousePoint != null)
            {
                //canvas.Children.Remove(mousePoint.label);
                //canvas.Children.Remove(mousePoint.ellipse);
            }
            mousePoint = null;

            End = DateTime.UtcNow;
            Console.WriteLine("Start" + getButtonText(e) + ":" + Start);

            //タッチ終了時の座標を記録
            x = e.GetPosition(canvas).X;
            y = e.GetPosition(canvas).Y;

            //Touch_marker(@"Record_Distance.csv");

            var win = new Window2();
            win.Width = Width / 3;
            win.Height = Height / 2;
            //win.Show();
        }


        private Boolean Range_in(Double num, Double max, Double min) {
            if (num > min && num < max)
            {                  
                return true;
            }
            else {
                return false;
            }
        }

        //マーカの認識処理
        private void Touch_marker(string filename){
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
        private void CSV_Reader(string filename){
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
    }
}
