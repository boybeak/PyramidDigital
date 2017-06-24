using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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

            Control.CheckForIllegalCrossThreadCalls = false;
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
                        tipLable.Text = "开始打开";
                        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                        IWorkbook workbook = new XSSFWorkbook(fs);//从流内容创建Workbook对象
                        ISheet sheet = workbook.GetSheetAt(0);//获取第一个工作表
                        IRow headerRow = sheet.GetRow(0);//获取工作表第一行
                        int columnCount = headerRow.Cells.Count;
                        newGrid(columnCount);
                        for (int i = 0; i < columnCount; i++)
                        {
                            mainGridView.Columns[i].HeaderText = headerRow.Cells[i].StringCellValue;
                        }
                        for (int i = 0; i < columnCount; i++)
                        {
                            IRow row = sheet.GetRow(i + 1);
                            for (int j = columnCount - 1 - i; j < columnCount; j++)
                            {
                                ICell cell = row.GetCell(j);//获取行的第一列
                                string value = cell.ToString();//获取列的值
                                mainGridView.Rows[i].Cells[j].Value = value;
                            }


                        }
                        tipLable.Text = "打开完成";

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                        tipLable.Text = "打开失败";
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
                mWorkingThread = new Thread(new ThreadStart(ds.doSave3));
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            
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

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mWorkingThread != null && mWorkingThread.IsAlive)
            {
                if (MessageBox.Show("有任务运行中,是否确认退出程序？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    mWorkingThread.Interrupt();
                    // 关闭所有的线程
                    this.Dispose();
                    this.Close();
                }
                else
                    e.Cancel = true;
                
            }
            
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
