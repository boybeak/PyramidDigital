using Excel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 金译彩
{
    class DataReader
    {
        private DataGridView gridView;
        private ToolStripLabel tipLabel;

        private String targetFilePath;

        public DataReader (String path, DataGridView gv, ToolStripLabel tip)
        {
            targetFilePath = path;
            gridView = gv;
            tipLabel = tip;
        }

        public void doOpen2 ()
        {
            FileStream fs = new FileStream(targetFilePath, FileMode.Open, FileAccess.Read);

            IWorkbook workbook = new XSSFWorkbook(fs);//从流内容创建Workbook对象
            ISheet sheet = workbook.GetSheetAt(0);//获取第一个工作表
            IRow headerRow = sheet.GetRow(0);//获取工作表第一行
            int columnCount = headerRow.Cells.Count;
            for (int i = 0; i < columnCount; i++)
            {
                gridView.Columns[i].HeaderText = headerRow.Cells[i].StringCellValue;
            }
            for (int i = 0; i < columnCount; i++)
            {
                IRow row = sheet.GetRow(i + 1);
                for (int j = columnCount - 1 - i; j >= 0; j--)
                {
                    ICell cell = row.GetCell(j);//获取行的第一列
                    string value = cell.ToString();//获取列的值
                    gridView.Rows[i].Cells[j].Value = value;
                }
                
                
            }
            
        }

    }
}
