using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace LifeGameYuta
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string EXTENSION = ".life";
        public const int BASE_CELL_LENGTH = 10;
        public const int CELL_MAEGIN = 1;
        public const int CONTENT_CELL_LENGTH = BASE_CELL_LENGTH - 2 * CELL_MAEGIN;

        private LGData lgdata;
        private bool is_running;
        private Canvas canvas;
        private Grid grid;
        private DispatcherTimer timer;
        
        public MainWindow()
        {
            Debug.WriteLine("MainWindow()");
            InitializeComponent();
            is_running = false;
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Cells_Update;
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Read_Click()");
            string filepath = useOpenFileDialog();
            Debug.WriteLine(filepath);
            
            if (filepath.Equals(null))
            {
                MessageBox.Show("Error - filepath is null");
                return;
            }else if(filepath.IndexOf(EXTENSION) <= 0){
                MessageBox.Show("Error - the file isn't life file.");
                return;
            }

            lgdata = openLifeGameFile(filepath);
            if (canvas != null) grid.Children.Remove(canvas);
            canvas = makeCanvas(lgdata.Height, lgdata.Width);
            grid.Children.Add(canvas);
            makeCells(this.lgdata);

            mainwin.Height = this.lgdata.Height * BASE_CELL_LENGTH + 130;
            mainwin.Width = this.lgdata.Width * BASE_CELL_LENGTH + 40;

            if (!lgdata.Equals(null))
            {
                MessageBox.Show("Finished reading the file.");
            }
            else
            {
                MessageBox.Show("Error - lfdata is null.");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Exit_Click()");
            this.Close();
        }

        public string useOpenFileDialog()
        {
            OpenFileDialog openfiledialog = new OpenFileDialog();
            openfiledialog.FilterIndex = 1;
            openfiledialog.Filter = "FifeGameFIle(.life)|*.life|All Files (*.*)|*.*";
            bool? result = openfiledialog.ShowDialog();
            if (result == true)
            {
                return openfiledialog.FileName;
            }
            else
            {
                return null;
            }
        }

        public LGData openLifeGameFile(string filepath)
        {
            Debug.WriteLine("openLifeGameFile()");

            LGData lgdata;

            using (StreamReader sr = new StreamReader(filepath, Encoding.GetEncoding("UTF-8")))
            {
                // read header
                string[] split = sr.ReadLine().Split(',');
                string height = split[0].Split('=')[1];
                string width = split[1].Split('=')[1];
                // Debug.WriteLine("height = {0}, width = {1}", height, width);
                lgdata = new LGData(int.Parse(height), int.Parse(width));

                // read content
                int countline = 0;
                while (sr.Peek() > 0)
                {
                    string line = sr.ReadLine();
                    // Debug.WriteLine(line);

                    string[] boolsplit = line.Split(',');
                    for (int i = 0; i < boolsplit.Length; i++)
                    {
                        if (boolsplit[i].Equals("1"))
                        {
                            lgdata.Field[countline, i] = true;
                        }
                        else
                        {
                            lgdata.Field[countline, i] = false;
                        }
                    }

                    countline++;
                }
            }

            return lgdata;
        }

        public Canvas makeCanvas(int height, int width)
        {
            return new Canvas()
            {
                Background = Brushes.Gray,
                Height = BASE_CELL_LENGTH * height,
                Width = BASE_CELL_LENGTH * width,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 80, 0, 0)
            };
        }

        public void makeCells(LGData lgdata)
        {
            if (canvas == null) return;

            canvas.Children.Clear();

            for (int i = 0; i < lgdata.Height; i++)
            {
                for (int j = 0; j < lgdata.Width; j++)
                {
                    Rectangle rect = getCell(lgdata.Field[i,j]);
                    canvas.Children.Add(rect);
                    Canvas.SetLeft(rect, BASE_CELL_LENGTH * j);
                    Canvas.SetTop(rect, BASE_CELL_LENGTH * i);
                }
            }
        }

        public Rectangle getCell(bool is_alive)
        {

            Rectangle rect = new Rectangle()
            {
                Height = CONTENT_CELL_LENGTH,
                Width = CONTENT_CELL_LENGTH,
                Margin = new Thickness(CELL_MAEGIN, CELL_MAEGIN, CELL_MAEGIN, CELL_MAEGIN)
            };

            if (is_alive)
            {                
                rect.Fill = Brushes.LightGreen;
            }
            else
            {
                rect.Fill = Brushes.Black;
            }

            return rect;
        }

        public LGData getNext(LGData lgdata)
        {
            LGData nextlgdata = new LGData(lgdata.Height, lgdata.Width);
            for (int i = 0; i < lgdata.Height; i++)
            {
                for (int j = 0; j < lgdata.Width; j++)
                {
                    int alive_cell = checkNextStatus(lgdata, i, j);
                    // Debug.WriteLine("{0} i = {1} j = {2}", alive_cell, i, j);
                    if (lgdata.Field[i, j])
                    {
                        if (alive_cell == 2 || alive_cell == 3)
                        {
                            nextlgdata.Field[i, j] = true;
                        }
                        else
                        {
                            nextlgdata.Field[i, j] = false;
                        }
                    }
                    else
                    {
                        if (alive_cell == 3)
                        {
                            nextlgdata.Field[i, j] = true;
                        }
                        else
                        {
                            nextlgdata.Field[i, j] = false;
                        }
                    }
                }
            }
            return nextlgdata;
        }

        public int checkNextStatus(LGData lgdata, int ypos, int xpos)
        {
            int count_alive = 0;

            for (int i = ypos - 1; i < ypos + 2; i++)
            {
                for (int j = xpos - 1; j < xpos + 2; j++)
                {
                    if (i < 0 || i >= lgdata.Height || j < 0 || j >= lgdata.Width || (i==ypos&&j==xpos))
                    {
                        continue;
                    }
                    if (lgdata.Field[i, j])
                    {
                        // Debug.WriteLine("find alive at [{0},{1}]", i, j);
                        count_alive++;
                    }
                }
            }
            return count_alive;
        }

        private void StartandStop_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("StartandStop_Click()");
            is_running = !is_running;

            if(lgdata == null) return;
            
            Debug.WriteLine("is_running = {0}", is_running);

            if (is_running)
            {
                timer.Start();
            }else
            {
                timer.Stop();
            }
        }

        public void Cells_Update(object sender, EventArgs e)
        {
            this.lgdata = getNext(this.lgdata);
            makeCells(this.lgdata);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Grid_Loaded");
            this.grid = sender as Grid;
        }
    }

    public class LGData
    {
        // field
        private int height;
        private int width;
        private bool[,] field;

        // property
        public int Height
        {
            get
            {
                return this.height;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        public bool[,] Field
        {
            get { return this.field; }
        }

        // constractor
        public LGData(int height, int width)
        {
            this.width = width;
            this.height = height;
            this.field = new bool[height, width];
        }

        // methods
        public void show()
        {
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                {
                    Debug.Write(String.Format("{0} ", field[i, j]));
                }
                Debug.WriteLine("");
            }
        }
    }
}
