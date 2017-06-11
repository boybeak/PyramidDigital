using Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 金译彩
{
    public partial class MainForm : Form
    {
        private string mOutPutFilePath;

        private Thread mWorkingThread;

        public MainForm()
        {
            InitializeComponent();

            this.Text = "金译彩";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(800, 600);
        }

        private void resetMainGridView ()
        {
            mainGridView.Columns.Clear();
            mainGridView.Rows.Clear();
            mainGridView.AutoGenerateColumns = false;
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                MessageBox.Show("有正在运行的任务，暂时无法创建表格。");
                return;
            }
            decimal result = Prompt.ShowDialog("表格列数", "新建表格");
            int columnCount = Convert.ToInt32(result);
            if (columnCount <= 0)
            {
                return;
            }
            if (columnCount >= 1000)
            {
                MessageBox.Show("输入数字过大,暂时支持1000以内");
                return;
            }
            resetMainGridView();
            
            /*mWorkingThread = new Thread(() => newGrid(columnCount));
            mWorkingThread.Start();*/
            newGrid(columnCount);
        }

        private void newGrid (int columnCount)
        {
            tipLable.Text = "开始创建";
            resetMainGridView();
            for (int c = 0; c < columnCount; c++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = (c + 1) + "";
                column.Width = 30;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                //column.Frozen = false;
                column.Resizable = DataGridViewTriState.False;
                mainGridView.Columns.Add(column);
            }
            for (int r = 0; r < columnCount; r++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.HeaderCell.Value = (r + 1) + "";

                row.Resizable = DataGridViewTriState.False;
                mainGridView.Rows.Add(row);
            }
            tipLable.Text = "创建完成";
            /*Random rnd = new Random();
            for (int c = 0; c < columnCount; c++)
            {
                mainGridView.Rows[columnCount - 1].Cells[c].Value = rnd.Next(0, 10);
            }*/
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                MessageBox.Show("有任务正在运行中，请稍后再试");
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                resetMainGridView();
                string path = openFileDialog.FileName;
                string fileExt = Path.GetExtension(path); //get the file extension  
                if (fileExt.CompareTo(".xlsx") == 0)
                {
                    try
                    {
                        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
                        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        excelReader.IsFirstRowAsColumnNames = true;
                        DataSet result = excelReader.AsDataSet();
                        
                        int count = excelReader.ResultsCount;
                        int fieldCount = excelReader.FieldCount;
                        int depth = excelReader.Depth;

                        newGrid(fieldCount);

                        Console.WriteLine("count=" + count + " fieldCount=" + fieldCount + " depth=" + depth);
                        DataReader reader = new DataReader(excelReader, mainGridView, tipLable);
                        mWorkingThread = new Thread(new ThreadStart(reader.doOpen));
                        mWorkingThread.Start();
                        /*int r = 0;
                        while(excelReader.Read())
                        {
                            for (int c = 0; c < fieldCount; c++)
                            {
                                Int32 value = excelReader.GetInt32(c);
                                if (value < 0)
                                {
                                    continue;
                                }
                                if (r == 0)
                                {
                                    mainGridView.Columns[c].HeaderText = value + "";
                                }
                                else
                                {
                                    mainGridView.Rows[r - 1].Cells[c].Value = value;
                                }

                                Console.WriteLine("excel[" + r + "][" + c + "]=" + value);
                            }
                            r++;
                        }
                        
                        excelReader.Close();
                        
                        //resetMainGridView();
                        
                        for (int i = 0; i < mainGridView.Columns.Count; i++)
                        {
                            DataGridViewColumn column = mainGridView.Columns[i];
                            column.HeaderText = (i + 1) + "";
                            column.Width = 30;
                            column.SortMode = DataGridViewColumnSortMode.NotSortable;
                            column.Frozen = false;
                            column.Resizable = DataGridViewTriState.False;
                        }*/
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
                }
                
            }
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                MessageBox.Show("有任务正在运行中，稍后再试");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();  
            saveFileDialog.Filter = "Excel File|*.xlsx";  
            saveFileDialog.Title = "Save Grid Data";  
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                //MessageBox.Show("toolStripButtonSave_Click file=" + saveFileDialog1.FileName);
                //mWorkingThread = new Thread(() => save(saveFileDialog.FileName));
                DataSaver ds = new DataSaver(mainGridView, tipLable, saveFileDialog.FileName);
                mWorkingThread = new Thread(new ThreadStart(ds.doSave));
                mWorkingThread.Start();
            }
        }

        private void toolStripButtonRun_Click(object sender, EventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                MessageBox.Show("有正在运行的任务，暂时无法进行计算。");
                return;
            }

            mWorkingThread = new Thread(new ThreadStart(runInThread));
            mWorkingThread.Start();
        }

        private void runInThread ()
        {
            tipLable.Text = "开始计算";
            int rowCount = mainGridView.Rows.Count;
            int columnCount = mainGridView.Columns.Count;
            for (int i = rowCount - 2; i >= 0; i--)
            {
                tipLable.Text = "正在计算第" + i + "行";
                for (int j = rowCount - 1 - i; j < rowCount; j++)
                {
                    int rowIndex = i;
                    int rowPlusIndex = rowIndex + 1;
                    int columnIndex = j;
                    int columnMinusIndex = columnIndex - 1;

                    DataGridViewCell cellTarget = mainGridView.Rows[rowIndex].Cells[columnIndex];

                    DataGridViewCell cellA = mainGridView.Rows[rowPlusIndex].Cells[columnMinusIndex];
                    DataGridViewCell cellB = mainGridView.Rows[rowPlusIndex].Cells[columnIndex];

                    object objA = cellA.Value;
                    object objB = cellB.Value;

                    if (objA == null || objB == null)
                    {
                        break;
                    }

                    int valueA = -1, valueB = -1;
                    try
                    {
                        valueA = Convert.ToInt32(objA);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(rowIndex + "行" + columnMinusIndex + "列数据有问题,请进行检查");
                        cellA.Selected = true;
                        return;
                    }
                    try
                    {
                        valueB = Convert.ToInt32(objB);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(rowIndex + "行" + columnIndex + "列数据有问题,请进行检查");
                        cellB.Selected = true;
                        return;
                    }
                    cellA.Selected = false;
                    cellB.Selected = false;
                    if (valueA >= 0 && valueB >= 0)
                    {
                        int value = valueB - valueA;
                        if (value < 0)
                        {
                            value = 10 + value;
                        }
                        cellTarget.Value = value;
                    }

                    Console.WriteLine("toolStripButtonRun_Click objA=" + objA + " objB=" + objB);
                }

            }
            tipLable.Text = "计算结束";
        }

        private void save(string file)
        {
            Console.WriteLine("save start");
            tipLable.Text = "开始保存";
            mOutPutFilePath = file;
            // creating Excel Application
            Excel._Application app = new Excel.Application();


            // creating new WorkBook within Excel application
            Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);


            // creating new Excelsheet in workbook
            Excel._Worksheet worksheet = null;

            // see the excel sheet behind the program
            app.Visible = true;

            // get the reference of first sheet. By default its name is Sheet1.
            // store its reference to worksheet
            worksheet = workbook.Sheets["Sheet1"];
            worksheet = workbook.ActiveSheet;

            // changing the name of active sheet
            worksheet.Name = "Exported from gridview";


            // storing header part in Excel
            for (int i = 1; i < mainGridView.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = mainGridView.Columns[i - 1].HeaderText;
            }



            // storing Each row and column value to excel sheet
            for (int i = 0; i < mainGridView.Rows.Count; i++)
            {
                tipLable.Text = "正在保存第" + i + "行";
                for (int j = 0; j < mainGridView.Columns.Count; j++)
                {
                    object value = mainGridView.Rows[i].Cells[j].Value;
                    if (value == null)
                    {
                        continue;
                    }
                    worksheet.Cells[i + 2, j + 1] = value.ToString();
                }
            }


            // save the application
            workbook.SaveAs(file, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            // Exit from the application
            app.Quit();
            tipLable.Text = "保存完成";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                mWorkingThread.Interrupt();
            }
        }

        private void toolStripButtonAddColumn_Click(object sender, EventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                MessageBox.Show("有任务正在运行中,请稍后再试");
                return;
            }
            decimal result = Prompt.ShowDialog("请填写添加列数", "添加列");
            int columnCount = Convert.ToInt32(result);
            if (columnCount <= 0)
            {
                return;
            }
            if (columnCount >= 1000)
            {
                MessageBox.Show("输入数字过大,暂时支持1000以内");
                return;
            }
            tipLable.Text = "开始添加";
            int count = mainGridView.ColumnCount;
            for (int i = 0; i < columnCount; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = (count + i + 1) + "";
                column.Width = 30;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                //column.Frozen = false;
                column.Resizable = DataGridViewTriState.False;
                
                mainGridView.Columns.Add(column);

                DataGridViewRow row = new DataGridViewRow();

                row.Resizable = DataGridViewTriState.False;
                mainGridView.Rows.Insert(0, row);
            }
            for (int i = 0; i < mainGridView.RowCount; i++)
            {
                mainGridView.Rows[i].HeaderCell.Value = (i + 1) + "";
            }
            
            tipLable.Text = "添加完成";

        }

        private void mainGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            
            int[] array = new int[10];
            
            for (int i = mainGridView.RowCount - 1; i >= 0; i--)
            {
                DataGridViewCell cell = mainGridView.Rows[i].Cells[e.ColumnIndex];
                object obj = cell.Value;
                if (obj == null)
                {
                    break;
                }
                int value = -1;
                try
                {
                    value = Convert.ToInt32(obj);
                    array[value]++;
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            string text = "第" + (e.ColumnIndex + 1) + "数据统计为:\n";
            for (int i = 0; i < array.Length; i++)
            {
                text += i + "     " + array[i] + "个\n";
            }
            dataLabel.Text = text;
        }

    }

    public static class Prompt
    {
        public static decimal ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 240,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 20, Top = 10, Text = text };
            NumericUpDown numUpDown = new NumericUpDown() { Left = 20, Top = 36 };
            numUpDown.Maximum = 999;

            //TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 200, MaxLength = 4 };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 150, Top = 90, Width = 60, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            //prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(numUpDown);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? numUpDown.Value : 0;
        }
    }
}
