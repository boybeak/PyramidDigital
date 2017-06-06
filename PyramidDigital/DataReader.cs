using Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 金译彩
{
    class DataReader
    {
        private IExcelDataReader excelReader;
        private DataGridView gridView;
        private ToolStripLabel tipLabel;

        private int fieldCount;

        public DataReader (IExcelDataReader reader, DataGridView gv, ToolStripLabel tip)
        {
            excelReader = reader;
            gridView = gv;
            tipLabel = tip;
            fieldCount = excelReader.FieldCount;
        }

        public void doOpen ()
        {
            tipLabel.Text = "开始读取";
            int r = 0;
            while (excelReader.Read())
            {
                tipLabel.Text = "开始读取第" + r + "行";
                for (int c = 0; c < fieldCount; c++)
                {
                    Int32 value = excelReader.GetInt32(c);
                    if (value < 0)
                    {
                        continue;
                    }
                    if (r == 0)
                    {
                        gridView.Columns[c].HeaderText = value + "";
                    }
                    else
                    {
                        gridView.Rows[r - 1].Cells[c].Value = value;
                    }

                    Console.WriteLine("excel[" + r + "][" + c + "]=" + value);
                }
                r++;
            }

            excelReader.Close();

            //resetMainGridView();

            for (int i = 0; i < gridView.Columns.Count; i++)
            {
                DataGridViewColumn column = gridView.Columns[i];
                column.HeaderText = (i + 1) + "";
                column.Width = 30;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.Frozen = false;
                column.Resizable = DataGridViewTriState.False;
            }
            tipLabel.Text = "读取成功";

            gridView = null;
            tipLabel = null;
            excelReader = null;
        }

    }
}
