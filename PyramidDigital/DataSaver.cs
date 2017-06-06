using System;
using System.Collections.Generic;
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
    }
}
