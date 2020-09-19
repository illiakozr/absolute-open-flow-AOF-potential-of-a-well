using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace Exercise2
{
    public partial class Form1 : Form
    {
        public double Slope { get; set; }
        public double Intercept { get; set; }
        public double PotentialProduction { get; set; }
        public string FilePath { get; private set; }

        public Form1()
        {
            InitializeComponent();

            zedGraphControl1.GraphPane.Title.Text = "the classical isochronous test";
            zedGraphControl1.GraphPane.XAxis.Title.Text = "Production [m3/h]";
            zedGraphControl1.GraphPane.YAxis.Title.Text = "(P1^2 - Pdd^2) / q";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            zedGraphControl1.Visible = true;

            // Получим панель для рисования
            GraphPane pane = zedGraphControl1.GraphPane;

            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            // Создадим список точек
            PointPairList list = new PointPairList();
            PointPairList point = new PointPairList();

            try
            {

                AcceptInputValues();

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                     "Error",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error,
                     MessageBoxDefaultButton.Button1
                    );

                return;
            }

            InputData.Compute_W();

            double[] xAxis = new double[InputData.X.Count - 1];
            double[] yAxis = new double[InputData.Y.Count - 1]; 

            for(int i = 0; i < InputData.X.Count; i++)
            {
                if (i < InputData.X.Count - 1)
                {
                    xAxis[i] = InputData.X[i];
                    yAxis[i] = InputData.Y[i];

                }
                else
                {
                    point.Add(InputData.X[i], InputData.Y[i]);                    
                }
            }

            list.Add(xAxis, yAxis);
            

            // Создадим кривую с названием "Sinc",
            // которая будет рисоваться голубым цветом (Color.Blue),
            // Опорные точки выделяться не будут (SymbolType.None)
            LineItem myCurve = pane.AddCurve("Sinc", list, Color.Blue, SymbolType.Diamond);
            LineItem Point = pane.AddCurve("Point", point, Color.Brown, SymbolType.Square);
            Point.Line.IsVisible = false;
            myCurve.Line.IsSmooth = true;
            myCurve.Line.SmoothTension = 0.5F;
            // Вызываем метод AxisChange (), чтобы обновить данные об осях.
            // В противном случае на рисунке будет показана только часть графика,
            // которая умещается в интервалы по осям, установленные по умолчанию
            zedGraphControl1.GraphPane.AxisChange();

            // Обновляем график
            zedGraphControl1.Invalidate();

            Slope = SlopeDefinition(xAxis, yAxis);
            Intercept = InterceptDefinition(InputData.X[InputData.X.Count-1], InputData.Y[InputData.Y.Count-1], Slope);
            PotentialProduction = PotentialProductionDefinition(Slope, Intercept);

            slopeBox.Text = Slope.ToString("0.00e+00");
            interceptBox.Text = Intercept.ToString("f3");
            PotentialProdBox.Text = Math.Round(PotentialProduction).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            zedGraphControl1.GraphPane.CurveList.Clear();           
            // Обновим график
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            interceptBox.Text = "";
            slopeBox.Text = "";
            PotentialProdBox.Text = "";
        }

        private double SlopeDefinition(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("no match in arguments");
            }

            var x_avg = x.Average();
            var y_avg = y.Average();

            double numerator = 0;

            for (int i = 0; i < x.Length; i++)
            {
                numerator += ((x[i] - x_avg) * (y[i] - y_avg));
            }

            double denominator = 0;

            for (int i = 0; i < x.Length; i++)
            {
                denominator += Math.Pow((x[i] - x_avg), 2);
            }

            return numerator / denominator;
        }

        private double InterceptDefinition(double x, double y, double slope)
        {
            
            return  y - (slope * x);
        }

        private double PotentialProductionDefinition( double slope, double intercept)
        {
            return (-1 * intercept + Math.Sqrt(Math.Pow(intercept, 2) + 4 * slope * (Math.Pow(InputData.Pressure[0] * 0.1, 2) - Math.Pow(0.101325, 2)))) / (2 * slope);
        }

        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FilePath = openFileDialog1.FileName;
                    FilePathTextBox.Text = FilePath;

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void FilePathTextBox_TextChanged(object sender, EventArgs e)
        {
            ImportDataButton.Enabled = true;
        }

        private void ImportDataButton_Click(object sender, EventArgs e)
        {
            //Создаём приложение.
            var ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(FilePath, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            //Выбираем таблицу(лист).
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];

            // Указываем номер столбца (таблицы Excel) из которого будут считываться данные.
            int numCol1 = 1;

            Range usedColumn1 = ObjWorkSheet.UsedRange.Columns[numCol1];
            Array myvalues1 = (Array)usedColumn1.Cells.Value2;
            double[] arrProduction = myvalues1.OfType<object>().Select(o => Convert.ToDouble(o)).ToArray();            

            //////////////////////////////////////////////////////////////////////////

            int numCol2 = 2;

            Range usedColumn2 = ObjWorkSheet.UsedRange.Columns[numCol2];
            Array myvalues2 = (Array)usedColumn2.Cells.Value2;
            double[] arrDelta_t = myvalues2.OfType<object>().Select(o => Convert.ToDouble(o)).ToArray();

            //////////////////////////////////////////////////////////////////////////

            int numCol3 = 3;

            Range usedColumn3 = ObjWorkSheet.UsedRange.Columns[numCol3];
            Array myvalues3 = (Array)usedColumn3.Cells.Value2;
            double[] arrPressure = myvalues3.OfType<object>().Select(o => Convert.ToDouble(o)).ToArray();

            // Выходим из программы Excel.
            ObjExcel.Quit();

            if (arrPressure.Length != arrDelta_t.Length || arrPressure.Length != arrProduction.Length)
            {
                MessageBox.Show(
                   "there are mismatch between input values. Please, check file data for correctness",
                   "Error",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error,
                   MessageBoxDefaultButton.Button1
                  );
                return;
            }

            for (int i = 0; i < arrPressure.Length; i++)
            {
                dataGridView1.Rows.Add(arrProduction[i], arrDelta_t[i], arrPressure[i]);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            dataGridView1.EditingControl.KeyPress += gridKeyPress;
        }

        private void gridKeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar) ||
                ((e.KeyChar == System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0])
                && (DS_Count(((System.Windows.Forms.TextBox)sender).Text) < 1)));
        }

        private int DS_Count(string s)
        {
            string substr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0].ToString();
            int count = (s.Length - s.Replace(substr, "").Length) / substr.Length;
            return count;
        }

        private void AcceptInputValues()
        {
            
            ReadDataGridView();
            
        }

        private void ReadDataGridView()
        {
            //DataGridViewColumn pressureCol = dataGridView1.Columns["Pressure"];

            //for(int i =0; i < dataGridView1.Rows.Count; i++)
            //{

            //}

            InputData.Pressure.Clear();
            InputData.Delta_t.Clear();
            InputData.Production.Clear();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                try
                {
                    if (row.Cells["Pressure"].Value == null || row.Cells["Pressure"].Value.ToString() == string.Empty) continue;
                    if (row.Cells["Production"].Value == null || row.Cells["Production"].Value.ToString() == string.Empty) continue;
                    if (row.Cells["time"].Value == null ||  row.Cells["time"].Value.ToString() == string.Empty) continue;
                    InputData.Pressure.Add(Convert.ToDouble(row.Cells["Pressure"].Value));
                    InputData.Production.Add(Convert.ToDouble(row.Cells["Production"].Value));
                    InputData.Delta_t.Add(Convert.ToDouble(row.Cells["time"].Value));
                }
                catch (InvalidCastException ex)
                {
                    MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1
                   );
                    return;
                }
            }

            if (InputData.Pressure.Count == 0 || InputData.Production.Count == 0 || InputData.Delta_t.Count == 0)
            {
                throw new Exception("Reservoir data is empty. Please input values");
            }

            if (InputData.Pressure.Count != InputData.Production.Count && InputData.Pressure.Count != InputData.Delta_t.Count)
            {
                throw new Exception("there are mismatch between input values. Please, check data for correctness");

            }
        }
    }
}
