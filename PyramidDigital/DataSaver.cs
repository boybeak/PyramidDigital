using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 金译彩
{
    class DataSaver
    {
        private DataGridView gridView;
        private ToolStripLabel tipLable;
        private String targetPath;

        public DataSaver (DataGridView gv, ToolStripLabel tl, String path)
        {
            gridView = gv;
            tipLable = tl;
            targetPath = path;
        }

        
        public void doSave3 ()
        {
            tipLable.Text = "开始保存";
            FileStream fos = new FileStream(targetPath, FileMode.Create, FileAccess.Write);

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");
            IRow headerRow = sheet.CreateRow(0);
            // storing header part in Excel
            for (int i = 0; i < gridView.Columns.Count; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellType(CellType.String);
                
                cell.SetCellValue(gridView.Columns[i].HeaderText);
                //worksheet.Cells[1, i] = gridView.Columns[i - 1].HeaderText;
            }

            // storing Each row and column value to excel sheet
            for (int i = 0; i < gridView.Rows.Count; i++)
            {
                tipLable.Text = "正在保存第" + i + "行";
                IRow row = sheet.CreateRow(i + 1);
                for (int j = 0; j < gridView.Columns.Count; j++)
                {
                    ICell cell = row.CreateCell(j);
                    object value = gridView.Rows[i].Cells[j].Value;
                    if (value == null)
                    {
                        continue;
                    }
                    cell.SetCellValue(value.ToString());
                    //worksheet.Cells[i + 2, j + 1] = ;
                }
            }
            workbook.Write(fos);
            tipLable.Text = "保存完成";
        }
    }
}
