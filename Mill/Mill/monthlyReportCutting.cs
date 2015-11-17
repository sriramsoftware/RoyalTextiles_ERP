﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Mill
{
    public partial class monthlyReportCutting : Form
    {
        public monthlyReportCutting()
        {
            InitializeComponent();
        }
        //===============================================
        //===============================================
        // this doc is having exception of connection is not closed
        //===============================================
        //===============================================
        SqlConnection con = new SqlConnection("Data Source=.\\SQLEXPRESS;AttachDbFilename=D:\\Training\\Mill\\Mill\\database.mdf;Integrated Security=True;User Instance=True");
        void dataBind()
        {
            try
            {
                con.Open();
                int month = int.Parse(cbMonth.SelectedIndex.ToString()) + 1;
                string querry = "SELECT  c.CId 'Sr. No', p.ProductName, p.Size, c.Quantity FROM tblCutting AS c INNER JOIN tblProduct AS p ON c.PId = p.PId INNER JOIN tblWorker AS w ON c.WId = w.WId where Month(c.Date)='" + month + "' and w.FirstName='" + cbWorkerName.Text + "'";
                SqlCommand cmd = new SqlCommand(querry, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                //DataGridViewColumn col = new DataGridViewColumn();
                //col.Name = "SrNo";
                //col.HeaderText = "Sr No";

                dgvCuttingMonthlyReport.DataSource = dt;
                this.dgvCuttingMonthlyReport.AllowUserToOrderColumns = false;
                this.dgvCuttingMonthlyReport.AllowUserToResizeColumns = false;

                this.dgvCuttingMonthlyReport.Columns[1].Width = 250;
                this.dgvCuttingMonthlyReport.ReadOnly = true;
                con.Close();
            }
            catch (Exception ex)
            {
                MyMessageBox.ShowBox(ex.Message,"Exception");
            }
            finally
            {
                con.Close();
            }
        }
        void populate()
        {
            try
            {

                con.Open();
                string strQuerry;
                strQuerry = "select FirstName from tblWorker";
                SqlCommand cmd = new SqlCommand(strQuerry, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.SelectCommand.CommandText = cmd.CommandText.ToString();
                DataTable dt = new DataTable();
                da.Fill(dt);
                cbWorkerName.DataSource = dt;
                cbWorkerName.DisplayMember = "FirstName";
                cbWorkerName.ValueMember = "FirstName";
                con.Close();
            }
            catch (Exception ex)
            {
                MyMessageBox.ShowBox(ex.Message, "Exception");
            }
            finally
            {
                con.Close();
            }

        }
        

        private void monthlyReportCutting_Load(object sender, EventArgs e)
        {
            populate();
         
           // dataBind();
        }
        void totalQuantity()
        {
            try
            {
                long totalQuantity = 0;

                for (int i = 0; i < dgvCuttingMonthlyReport.Rows.Count - 1; i++)
                {
                    long quantity = long.Parse(dgvCuttingMonthlyReport.Rows[i].Cells["Quantity"].Value.ToString());
                    totalQuantity = totalQuantity + quantity;
                }
                lblQuantityShow.Text = totalQuantity.ToString();
            }
            catch (Exception e)
            {
                MyMessageBox.ShowBox(e.Message, "Exception");
            }
        }
        private void cbWorkerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataBind();
            totalQuantity();
        }

        private void cbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataBind();
            totalQuantity();
        }
        public static void ExportToExcel(DataGridView dgView)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            try
            {
                // instantiating the excel application class
                object misValue = System.Reflection.Missing.Value;
                excelApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook currentWorkbook = excelApp.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel.Worksheet currentWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)currentWorkbook.ActiveSheet;
                currentWorksheet.Columns.ColumnWidth = 18;
                if (dgView.Rows.Count > 0)
                {
                    currentWorksheet.Cells[1, 1] = DateTime.Now.ToString("s");
                    int i = 1;
                    foreach (DataGridViewColumn dgviewColumn in dgView.Columns)
                    {
                        // Excel work sheet indexing starts with 1
                        currentWorksheet.Cells[2, i] = dgviewColumn.Name;
                        ++i;
                    }
                    Microsoft.Office.Interop.Excel.Range headerColumnRange = currentWorksheet.get_Range("A2", "G2");
                    headerColumnRange.Font.Bold = true;
                    headerColumnRange.Font.Color = 0xFF0000;
                    //headerColumnRange.EntireColumn.AutoFit();
                    int rowIndex = 0;
                    for (rowIndex = 0; rowIndex < dgView.Rows.Count; rowIndex++)
                    {
                        DataGridViewRow dgRow = dgView.Rows[rowIndex];
                        for (int cellIndex = 0; cellIndex < dgRow.Cells.Count; cellIndex++)
                        {
                            currentWorksheet.Cells[rowIndex + 3, cellIndex + 1] = dgRow.Cells[cellIndex].Value;
                        }
                    }
                    Microsoft.Office.Interop.Excel.Range fullTextRange = currentWorksheet.get_Range("A1", "G" + (rowIndex + 1).ToString());
                    fullTextRange.WrapText = true;
                    fullTextRange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                }
                else
                {
                    string timeStamp = DateTime.Now.ToString("s");
                    timeStamp = timeStamp.Replace(':', '-');
                    timeStamp = timeStamp.Replace("T", "__");
                    currentWorksheet.Cells[1, 1] = timeStamp;
                    currentWorksheet.Cells[1, 2] = "No error occured";
                }
                using (SaveFileDialog exportSaveFileDialog = new SaveFileDialog())
                {
                    exportSaveFileDialog.Title = "Select Excel File";
                    exportSaveFileDialog.Filter = "Microsoft Office Excel Workbook(*.xlsx)|*.xlsx";

                    if (DialogResult.OK == exportSaveFileDialog.ShowDialog())
                    {
                        string fullFileName = exportSaveFileDialog.FileName;
                        // currentWorkbook.SaveCopyAs(fullFileName);
                        // indicating that we already saved the workbook, otherwise call to Quit() will pop up
                        // the save file dialogue box

                        currentWorkbook.SaveAs(fullFileName, Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook, System.Reflection.Missing.Value, misValue, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlUserResolution, true, misValue, misValue, misValue);
                        currentWorkbook.Saved = true;
                        MyMessageBox.ShowBox("Exported successfully", "Exported to Excel");
                    }
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.ShowBox(ex.Message, "Exception");
            }
            finally
            {
                if (excelApp != null)
                {
                    excelApp.Quit();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExportToExcel(dgvCuttingMonthlyReport);
        }
    }
}
