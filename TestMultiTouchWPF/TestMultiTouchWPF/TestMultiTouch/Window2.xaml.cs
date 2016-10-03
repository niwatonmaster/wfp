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
using System.Windows.Shapes;

namespace TestMultiTouch
{
    /// <summary>
    /// Window2.xaml の相互作用ロジック
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            Ellipse ell = new Ellipse();
            ell.Width = 100;
            ell.Height = 50;
            ell.Stroke = new SolidColorBrush(Colors.Red);
            ell.StrokeThickness = 1;
            canvas.Children.Add(ell);
        }
    }
}
