using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Transactions
{
    public partial class Form1 : Form
    {
        private string connectionString = "";

        private string[] monthNames = new string[]
            {
                "January", "February", "March", "April",
                "May", "June", "July", "August",
                "September", "October", "November", "December"
            };
        private string month = "";
        private string year = "";
        private string strOutput = "";


        public Form1()
        {
            InitializeComponent();
            changeUIData();

            backgroundWorker.WorkerReportsProgress = true;

            btnCopy.Enabled = false;
            btnSave.Enabled = false;
        }

        //button clicks
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (cbProject.Text == "210")
            {
                connectionString = "Server=10.10.2.39;Database=WelkinATP;User Id=sa;Password=Wb&210$0919@KiosK(3)welKIN#;";
            }
            else if (cbProject.Text == "150")
            {
                connectionString = "Server=10.10.2.39;Database=WelkinATP_150Machine;User Id=sa;Password=Wb&210$0919@KiosK(3)welKIN#;";
                //MessageBox.Show("Under Devevlopment. Contact Developer\nContact: bablushaikh0000@gmail.com", "Sorry! Can't Generate", MessageBoxButtons.OK,MessageBoxIcon.Hand);
                //return;
            }
            else
            {
                MessageBox.Show("Under Development");
            }


            month = (Array.IndexOf(monthNames, cbMonths.Text) + 1).ToString("d2");
            year = cbYear.Text;

            if(int.Parse(year) > 2023)
            {
                MessageBox.Show("Under Devevlopment. Contact Developer\nContact: bablushaikh0000@gmail.com", "Sorry! Can't Generate", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            strOutput = string.Empty;
            txtOut.Text = string.Empty;
            progressBar.Value = 0;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FileName = cbMonths.Text+".txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    SaveToTxtFile(filePath, txtOut.Text);
                }
            }
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if(txtOut.Text != "")
            {
                try
                {
                    Clipboard.SetText(txtOut.Text);
                    btnCopy.Enabled = false;
                    btnCopy.Text = "Copied";
                }
                catch { }
            }
        }

        //functions
        //ui year and month
        private void changeUIData()
        {
            DateTime currentDate = DateTime.Now;
            DateTime prevMonth = currentDate.AddMonths(-1);
            int month = prevMonth.Month;
            int year = prevMonth.Year;
            cbYear.Items.Clear();

            cbYear.Items.Add(year-3);
            cbYear.Items.Add(year-2);
            cbYear.Items.Add(year-1);
            cbYear.Items.Add(year);
            cbYear.Items.Add(year+1);
            cbMonths.DataSource = monthNames;

            cbYear.SelectedItem = year;
            cbMonths.SelectedItem = monthNames[month - 1];
            cbProject.SelectedIndex = 0;
        }

        //save to txt method
        private void SaveToTxtFile(string filePath, string text)
        {
            try
            {
                // Write the text to the file
                File.WriteAllText(filePath, text);
                MessageBox.Show("Text saved to file successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving the file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //background worker
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int noOfDays = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));

            string firstDate = year+"-"+month+"-01";
            string lastDate = year+"-"+month+"-"+noOfDays.ToString("d2");

            List<string> kioskIDs = new List<string>();
            



            try
            {
                //geting kiosk ids
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = $"select DISTINCT KioskID from Payments where PaymentDate >='{firstDate} 00:00:00.000' and PaymentDate <='{lastDate} 00:00:00.000'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        kioskIDs.Clear();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            kioskIDs.Add(reader.GetString(0));
                        }
                    }
                    conn.Close();
                }

                int counter = 0;
                foreach (string kioskID in kioskIDs)
                {
                    counter++;
                    List<string> cspNames = new List<string>();
                    List<string> cspTrans = new List<string>();
                    string kioskName = "";
                    string totalMonthlyTrans = "";

                    
                    //geting csp names in kiosk id
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = $"select DISTINCT CSPName from Payments where PaymentDate >='{firstDate} 00:00:00.000' and PaymentDate <='{lastDate} 00:00:00.000' and kioskID='{kioskID}' order by CSPName desc";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cspNames.Clear();
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                cspNames.Add(reader.GetString(0));
                            }
                        }
                        conn.Close();
                    }


                    //geting transactions by csp name in kiosk id
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        cspTrans.Clear();
                        foreach (string cspName in cspNames)
                        {
                            string query = $"Select * from Payments where PaymentDate >='{firstDate} 00:00:00.000' and PaymentDate <='{lastDate} 00:00:00.000' and CSPName='{cspName}' and kioskID='{kioskID}'";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    int rows = 0;
                                    while (reader.Read())
                                    {
                                        rows++;
                                    }
                                    cspTrans.Add(rows.ToString());
                                }
                            }
                        }
                        conn.Close();
                    }

                    //geting total monthly transactions by kiosk id
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = $"Select * from Payments where PaymentDate >='{firstDate} 00:00:00.000' and PaymentDate <='{lastDate} 00:00:00.000' and kioskID='{kioskID}'";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                totalMonthlyTrans = "";
                                int rows = 0;
                                while (reader.Read())
                                {
                                    rows++;
                                }
                                totalMonthlyTrans = rows.ToString();
                            }
                        }
                        conn.Close();
                    }


                    //arrange csp name and trans
                    int numOfCsp = 0;
                    string cspNameWithTrans = "";
                    foreach (string cspName in cspNames)
                    {
                        cspNameWithTrans += cspName + "\t" + cspTrans[numOfCsp] +"\t";
                        numOfCsp++;
                    }


                    //find site name
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = $"Select kioskName from kioskMapping where kioskID='{kioskID}'";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    kioskName = reader.GetString(0);
                                }
                            }
                        }
                        conn.Close();
                    }

                    //output string
                    strOutput += kioskID + "\t" + kioskName + "\t" + totalMonthlyTrans + "\t" + cspNameWithTrans + Environment.NewLine;


                    
                    System.Threading.Thread.Sleep(1);
                    // Report progress                
                    int progressPercentage = (int)((double)counter / kioskIDs.Count * 100);
                    backgroundWorker.ReportProgress(progressPercentage);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtOut.Text = strOutput;

            btnCopy.Enabled = true;
            btnCopy.Text = "Copy";
            btnSave.Enabled = true;
        }
    }
}
