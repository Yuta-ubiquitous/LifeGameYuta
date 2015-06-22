using System;
using System.Collections.Generic;
using System.IO;
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

namespace LGFmaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string EXTENSION = ".life";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button_Click()");

            string name = textboxname.Text;
            int height = default(int);
            int width = default(int);

            if ((!int.TryParse(textboxheight.Text, out height) || (!int.TryParse(textboxwidth.Text, out width))))
            {
                MessageBox.Show("Please input numeric values.");
            }

            Console.WriteLine("name = {0}, height = {1}, width = {2}",name ,height ,width);

            Console.WriteLine("slider = {0}",slider.Value);

            string filename = name + EXTENSION;

            using(StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding("UTF-8")))
            {
                string seed = DateTime.Now.ToString("mmss");
                Random rnd = new Random(int.Parse(seed));

                sw.WriteLine(getHeader(height, width));
                for(int i=1;i<=height;i++){
                    for(int j=1;j<=width;j++){
                        if(j==width){
                            if (rnd.Next(100) < (double)slider.Value * 10.0)
                            {
                                sw.Write("1\n");
                            }
                            else
                            {
                                sw.Write("0\n");
                            }
                            
                        }else{
                            if (rnd.Next(100) < (double)slider.Value * 10.0)
                            {
                                sw.Write("1,");
                            }
                            else
                            {
                                sw.Write("0,");
                            }   
                        }
                    }
                }
            }
            MessageBox.Show(String.Format("Finished making {0}.life", name));
        }

        public string getHeader(int height, int width)
        {
            return String.Format("#height={0}, width={1}",height, width);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Console.WriteLine((double)slider.Value / 10.0);
            lableratio.Content = String.Format("{0:F2}",(double)slider.Value / 10.0);
        }
    }
}
