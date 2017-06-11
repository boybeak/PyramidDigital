using Excel;
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

        public void doSave ()
        {
            Console.WriteLine("save start");
            tipLable.Text = "开始保存";
            
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
            for (int i = 1; i < gridView.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = gridView.Columns[i - 1].HeaderText;
            }



            // storing Each row and column value to excel sheet
            for (int i = 0; i < gridView.Rows.Count; i++)
            {
                tipLable.Text = "正在保存第" + i + "行";
                for (int j = 0; j < gridView.Columns.Count; j++)
                {
                    object value = gridView.Rows[i].Cells[j].Value;
                    if (value == null)
                    {
                        continue;
                    }
                    worksheet.Cells[i + 2, j + 1] = value.ToString();
                }
            }


            // save the application
            workbook.SaveAs(targetPath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            // Exit from the application
            app.Quit();
            tipLable.Text = "保存完成";

            gridView = null;
            tipLable = null;
        }

        public void doSave2 ()
        {
            /*建立Excel对象*/
            Excel.Application excel = new Excel.Application();
            /*try
            {*/
                excel.Application.Workbooks.Add(true);
                excel.Visible = true;
                
                /*合并标题单元格*/
                Excel.Worksheet worksheet = (Excel.Worksheet)excel.ActiveSheet;
                
                int columnIndex = 1;
                for (int i = 0; i < gridView.ColumnCount; i++)
                {
                    if (gridView.Columns[i].Visible == true)
                    {
                        excel.Cells[2, columnIndex] = gridView.Columns[i].HeaderText;
                        (excel.Cells[2, columnIndex] as Range).HorizontalAlignment = XlHAlign.xlHAlignCenter;//字段居中
                        columnIndex++;
                    }
                }
                //填充数据              
                for (int i = 0; i < gridView.RowCount; i++)
                {
                    columnIndex = 1;
                    for (int j = 0; j < gridView.ColumnCount; j++)
                    {
                        if (gridView.Columns[j].Visible == true)
                        {
                            object value = gridView[j, i].Value;
                            if (value == null)
                            {
                                continue;
                            }
                            if (gridView[j, i].ValueType == typeof(string))
                            {
                                excel.Cells[i + 3, columnIndex] = "'" + value.ToString();
                            }
                            else
                            {
                                excel.Cells[i + 3, columnIndex] = value.ToString();
                            }
                            (excel.Cells[i + 3, columnIndex] as Range).HorizontalAlignment = XlHAlign.xlHAlignLeft;//字段居中
                            columnIndex++;
                        }
                    }
                }
                worksheet.SaveAs(targetPath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                
            /*} catch (Exception ex)
            {
                
                excel.Quit();
                excel = null;
                GC.Collect();
            }*/
            
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
