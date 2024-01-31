using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
//K-means optimization using gaussian kernel input and an ideal centroid with the lowest sum of squared error

namespace WindowsFormsTesis1
{
    public partial class FMain : Form
    {
        //BackgroundWorker bgWorker;
        public FMain()
        {
            InitializeComponent();
            RtbxProses.SelectionFont = new System.Drawing.Font(RtbxProses.SelectionFont.FontFamily, 14.0F);
        }
        public string str1 = "";
        public string strStatistic = "";
        private Random rnd = new Random();
        private void FTugas2_Load(object sender, EventArgs e)
        {
            CboCountCluster.Items.Clear();
            for (int i = 1; i <= 100; i++)
            {
                CboCountCluster.Items.Add(i.ToString());
            }
            CboCountIteration.Items.Clear();
            for (int i = 1; i <= 20; i++)
            {
                if ((i * 50) % 2 == 0)
                {
                    CboCountIteration.Items.Add((i * 50).ToString());
                }
            }
            RbSetting1.Checked = true;
            RbSetting1_CheckedChanged(sender, e);
            BtnProcess.Enabled = false;
        }
        private void SetDefault()
        {
            SetEnabled();
            CboCountCluster.SelectedIndex = 2;
            CboCountIteration.SelectedIndex = 3;
            CbZScore.Checked = true;
            CbKernelGRBF.Checked = false;
            RbCentroidRandom.Checked = true;
            CbEvaluationDBI.Checked = true;
            CbEvaluationSC.Checked = true;
            SetDisabled();
        }
        private void SetEnabled()
        {
            Gb1.Enabled = true;
            Gb2.Enabled = true;
            Gb3.Enabled = true;
            Gb4.Enabled = true;
            CboCountCluster.Enabled = true;
            CboCountIteration.Enabled = true;
        }
        private void SetDisabled()
        {
            Gb1.Enabled = false;
            Gb2.Enabled = false;
            Gb3.Enabled = false;
            Gb4.Enabled = false;
            CboCountCluster.Enabled = false;
            CboCountIteration.Enabled = false;
        }
        private void DgvRawData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }
        private void DgvRawData_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.CellStyle.BackColor = Color.Red;
            if (e.Control != null)
            {
                e.Control.KeyPress += new KeyPressEventHandler(Column1_KeyPress2);
                Console.WriteLine(e.Control.Text);
            }
        }
        private void Column1_KeyPress2(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != '.')
                e.Handled = true;
            if (e.KeyChar == '.')
            {
                if (((DataGridViewTextBoxEditingControl)sender).Text.Length == 0)
                    e.Handled = true;
                if (((DataGridViewTextBoxEditingControl)sender).Text.Contains('.'))
                    e.Handled = true;
            }
            if (e.KeyChar == '-')
            {
                if (((DataGridViewTextBoxEditingControl)sender).Text.Length != 0)
                    e.Handled = true;
                if (((DataGridViewTextBoxEditingControl)sender).Text.Contains('-'))
                    e.Handled = true;
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg_im = new OpenFileDialog
                {
                    Filter = "Excel File|*.xls;*.xlsx;*.xlsm"
                };
                string file = "";
                System.Data.DataTable dt = new System.Data.DataTable();
                DataRow row;
                BtnProcess.Enabled = true;
                if (dlg_im.ShowDialog() == DialogResult.OK)
                {
                    BtnProcess.Enabled = false;
                    RtbxProses.Text = "";
                    DgvRawData.DataSource = null;
                    DgvResultData.DataSource = null;
                    file = dlg_im.FileName;
                    Microsoft.Office.Interop.Excel.Application excelApp0 = new Microsoft.Office.Interop.Excel.Application();
                    Workbook excelWorkbook0 = excelApp0.Workbooks.Open(file);
                    _Worksheet excelWorksheet0 = excelWorkbook0.Sheets[1];
                    Range excelRange0 = excelWorksheet0.UsedRange;
                    int rowCount0 = excelRange0.Rows.Count;
                    progressBar1.Maximum = rowCount0;

                    BackgroundWorker bgWorker2 = new BackgroundWorker();
                    bgWorker2.WorkerReportsProgress = true;
                    bgWorker2.DoWork += (o, eventArgs) =>
                    {
                        Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                        Workbook excelWorkbook = excelApp.Workbooks.Open(file);
                        _Worksheet excelWorksheet = excelWorkbook.Sheets[1];
                        Range excelRange = excelWorksheet.UsedRange;

                        int rowCount = excelRange.Rows.Count;
                        int colCount = excelRange.Columns.Count;

                        for (int i = 1; i <= rowCount; i++)
                        {
                            for (int k = 1; k <= colCount; k++)
                            {
                                dt.Columns.Add(excelRange.Cells[1, k].Value2.ToString());
                            }
                            break;
                        }
                        int rowCounter;
                        for (int i = 2; i <= rowCount; i++)
                        {
                            row = dt.NewRow();
                            rowCounter = 0;
                            for (int j = 1; j <= colCount; j++)
                            {
                                if (excelRange.Cells[i, j] != null && excelRange.Cells[i, j].Value2 != null)
                                {
                                    row[rowCounter] = excelRange.Cells[i, j].Value2.ToString();
                                }
                                else
                                {
                                    //row[i] = "";
                                    row[rowCounter] = "";
                                }
                                rowCounter++;
                            }
                            dt.Rows.Add(row);
                            ((BackgroundWorker)o).ReportProgress(i);
                        }
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Marshal.ReleaseComObject(excelRange);
                        Marshal.ReleaseComObject(excelWorksheet);
                        excelWorkbook.Close();
                        Marshal.ReleaseComObject(excelWorkbook);
                        excelApp.Quit();
                        Marshal.ReleaseComObject(excelApp);
                    };
                    bgWorker2.ProgressChanged += (o, eventArgsProgressChanged) =>
                    {
                        progressBar1.Value = eventArgsProgressChanged.ProgressPercentage;
                    };
                    bgWorker2.RunWorkerCompleted += (o, eventArgsProgressChanged) =>
                    {
                        DgvRawData.DataSource = dt;
                        foreach (var series0 in ChartRawData.Series)
                        {
                            series0.Points.Clear();
                        }
                        ChartRawData.Series.Clear();
                        foreach (var series0 in ChartResultData.Series)
                        {
                            series0.Points.Clear();
                        }
                        ChartResultData.Series.Clear();

                        double[][] dataRaw2 = new double[DgvRawData.Rows.Count - 1][];
                        try
                        {
                            for (int i = 0; i < DgvRawData.Rows.Count - 1; ++i)
                            {
                                dataRaw2[i] = new double[DgvRawData.Rows[0].Cells.Count];
                            }
                            for (int i = 0; i < DgvRawData.Rows.Count - 1; i++)
                            {
                                for (int j = 0; j < DgvRawData.Rows[i].Cells.Count; j++)
                                {
                                    double OutVal = ConvertToDouble(DgvRawData.Rows[i].Cells[j].Value.ToString());
                                    dataRaw2[i][j] = OutVal;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Data conversion error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                            return;
                        }

                        ChartRawData.Series.Clear();
                        for (int i = 1; i < dataRaw2[0].Length; i++)
                        {
                            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
                            double[] x1 = new double[dataRaw2.Length];
                            double[] y1 = new double[dataRaw2.Length];
                            for (int j = 0; j < dataRaw2.Length; j++)
                            {
                                x1[j] = Convert.ToDouble(dataRaw2[j][0].ToString());
                                y1[j] = Convert.ToDouble(dataRaw2[j][i].ToString());
                                series1.Points.DataBindXY(x1, y1);
                            }
                            //Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                            series1.ChartType = SeriesChartType.Point;
                            series1.MarkerSize = 10;
                            series1.MarkerStyle = MarkerStyle.Circle;
                            series1.Color = Color.Blue;//randomColor
                            ChartRawData.Series.Add(series1);
                        }
                        ChartRawData.ResetAutoValues();
                        ChartRawData.Titles.Clear();
                        ChartRawData.Titles.Add($"Scatter Plot");
                        ChartRawData.ChartAreas[0].AxisX.Title = "X";
                        ChartRawData.ChartAreas[0].AxisY.Title = "Y";
                        ChartRawData.Legends.Clear();

                        progressBar1.Value = 0;
                        BtnProcess.Enabled = true;
                        MessageBox.Show("Finish Import", "Attention");
                    };
                    bgWorker2.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show("Data error atau dibatalkan!", "Attention");
                    if (DgvRawData.Rows.Count == 0)
                    {
                        BtnProcess.Enabled = false;
                    }
                    else
                    {
                        BtnProcess.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error import Data: "+ex.Message, "Attention");
                if (DgvRawData.Rows.Count == 0)
                {
                    BtnProcess.Enabled = false;
                }
                else
                {
                    BtnProcess.Enabled = true;
                }
            }
        }
        public void MakeChart0(System.Data.DataTable dt20, System.Windows.Forms.DataVisualization.Charting.Chart chart20)
        {
            foreach (var series01 in chart20.Series)
            {
                series01.Points.Clear();
            }
            chart20.Series.Clear();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            double[] x1 = new double[dt20.Rows.Count];
            for (int i = 1; i < dt20.Columns.Count; i++)
            {

                double[] y1 = new double[dt20.Rows.Count];
                for (int j = 0; j < dt20.Rows.Count; j++)
                {
                    x1[j] = Convert.ToDouble(dt20.Rows[j][0].ToString());
                    y1[j] = Convert.ToDouble(dt20.Rows[j][i].ToString());
                    series1.Points.DataBindXY(x1, y1);

                }
                
            }
            series1.ChartType = SeriesChartType.Bubble;
            series1.MarkerStyle = MarkerStyle.Circle;
            series1.Color = Color.Red;

            chart20.Series.Add(series1);
            chart20.ResetAutoValues();
            chart20.Titles.Clear();
            chart20.Titles.Add($"Scatter Plot");
            chart20.ChartAreas[0].AxisX.Title = "X";
            chart20.ChartAreas[0].AxisY.Title = "Y";
        }
        public void MakeChart1(System.Data.DataTable dt10, System.Windows.Forms.DataVisualization.Charting.Chart chart10)
        {
            foreach (var series0 in chart10.Series)
            {
                series0.Points.Clear();
            }
            chart10.Series.Clear();

            double[] x1 = new double[dt10.Rows.Count];
            for (int j = 0; j < dt10.Rows.Count; j++)
            {
                x1[j] = Convert.ToDouble(dt10.Rows[j][0].ToString());
            }
            for (int i = 1; i < dt10.Columns.Count - 1; i++)
            {

                double[] y1 = new double[dt10.Rows.Count];
                for (int j = 0; j < dt10.Rows.Count; j++)
                {
                    y1[j] = Convert.ToDouble(dt10.Rows[j][i].ToString());

                }
                System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
                series1.Points.DataBindXY(x1, y1);
                series1.ChartType = SeriesChartType.Point;
                series1.MarkerStyle = MarkerStyle.Circle;
                series1.Points[0].Color = Color.Red;
                chart10.Series.Add(series1);
            }
            chart10.ResetAutoValues();
            chart10.Titles.Clear();
            chart10.Titles.Add($"Scatter Plot");
            chart10.ChartAreas[0].AxisX.Title = "X";
            chart10.ChartAreas[0].AxisY.Title = "Y";
        }
        private void RbSetting1_CheckedChanged(object sender, EventArgs e)
        {
            SetDefault();
        }

        private void RbSetting2_CheckedChanged(object sender, EventArgs e)
        {
            SetEnabled();
        }

        private void BtnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                str1 = "";
                strStatistic = "";
                RtbxProses.Text = "";
                RtbxStatistik.Text = "";
                foreach (var series0 in ChartResultData.Series)
                {
                    series0.Points.Clear();
                }
                ChartResultData.Series.Clear();
                if (DgvRawData.Rows.Count == 0 || DgvRawData.Columns.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk di proses" + Environment.NewLine + "silahkan import atau tambah data!", "Attention");
                    return;
                }
                try
                {
                    for (int i = 0; i < DgvRawData.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < DgvRawData.Rows[i].Cells.Count; j++)
                        {
                            string strValue = DgvRawData.Rows[i].Cells[j].Value.ToString();
                            double OutVal = ConvertToDouble(strValue);
                            if (strValue == null && strValue.Length == 0 && double.IsNaN(OutVal) && double.IsInfinity(OutVal))
                            {
                                MessageBox.Show("There is empty or null data or text" + Environment.NewLine + "please check and adjust!", "Attention");
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Empty data check error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                    return;
                }
                int countcluster = Convert.ToInt32(CboCountCluster.SelectedItem.ToString());
                int maxIteration = Convert.ToInt32(CboCountIteration.SelectedItem.ToString());
                bool isZscore = (CbZScore.Checked == true) ? true : false;
                bool isKernel = (CbKernelGRBF.Checked == true) ? true : false;
                int centroidType = 0;
                string centroidTypeStr = "Random";
                if (RbCentroidRandomCombination.Checked == true)
                {
                    centroidType = 1;
                    centroidTypeStr = "Random Combination";
                }
                bool isEvaluationDBI = (CbEvaluationDBI.Checked == true) ? true : false;
                bool isEvaluationSilhouette = (CbEvaluationSC.Checked == true) ? true : false;

                double[][] dataRaw = new double[DgvRawData.Rows.Count - 1][];
                try
                {
                    for (int i = 0; i < DgvRawData.Rows.Count - 1; ++i)
                    {
                        dataRaw[i] = new double[DgvRawData.Rows[0].Cells.Count];
                    }
                    for (int i = 0; i < DgvRawData.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < DgvRawData.Rows[i].Cells.Count; j++)
                        {
                            double OutVal = ConvertToDouble(DgvRawData.Rows[i].Cells[j].Value.ToString());
                            dataRaw[i][j] = OutVal;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Data conversion error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                    return;
                }
                str1 += Environment.NewLine + "//======================================//";
                str1 += Environment.NewLine + "*PROCESS OF DATA SETTING*";
                str1 += Environment.NewLine + "Row count : " + dataRaw.Length;
                str1 += Environment.NewLine + "Cluster count : " + countcluster;
                str1 += Environment.NewLine + "Max. iteration : " + maxIteration;
                str1 += Environment.NewLine + "Z-Score : " + isZscore;
                str1 += Environment.NewLine + "Kernel GRBF : " + isKernel;
                str1 += Environment.NewLine + "Centroid type : " + centroidTypeStr;
                str1 += Environment.NewLine + "Evaluation DBI : " + isEvaluationDBI;
                str1 += Environment.NewLine + "Evaluation SC : " + isEvaluationSilhouette;
                strStatistic += Environment.NewLine + "Row count : " + dataRaw.Length;
                strStatistic += Environment.NewLine + "Cluster count : " + countcluster;
                strStatistic += Environment.NewLine + "Max. iteration : " + maxIteration;
                strStatistic += Environment.NewLine + "Z-Score : " + isZscore;
                strStatistic += Environment.NewLine + "Kernel GRBF : " + isKernel;
                strStatistic += Environment.NewLine + "Centroid type : " + centroidTypeStr;
                strStatistic += Environment.NewLine + "Evaluation DBI : " + isEvaluationDBI;
                strStatistic += Environment.NewLine + "Evaluation SC : " + isEvaluationSilhouette;

                double[][] dataNormalized = new double[dataRaw.Length][];
                Array.Copy(dataRaw, dataNormalized, dataRaw.Length);
                if (isZscore == true)
                {
                    try
                    {
                        str1 += Environment.NewLine;
                        str1 += Environment.NewLine + "//======================================//";
                        str1 += Environment.NewLine + "*PROCESS OF DATA NORMALIZATION*";
                        str1 += Environment.NewLine + "Z-Score: " + isZscore;
                        BtnProcess.Enabled = false;
                        dataNormalized = MakeZscore1(dataRaw);
                        str1 += Environment.NewLine + "Z-Score: success";
                        BtnProcess.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        BtnProcess.Enabled = (DgvRawData.Rows.Count > 0) ? true : false;
                        MessageBox.Show("Z-Score error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                        return;
                    }
                }
                double[][] dataKernel = new double[dataNormalized.Length][];
                Array.Copy(dataNormalized, dataKernel, dataNormalized.Length);
                if (isKernel == true)
                {
                    try
                    {
                        str1 += Environment.NewLine;
                        str1 += Environment.NewLine + "//======================================//";
                        str1 += Environment.NewLine + "*PROCESS OF DATA TRANSFORMATION*";
                        str1 += Environment.NewLine + "Kernel GRBF : " + isKernel;
                        BtnProcess.Enabled = false;
                        dataKernel = MakeKernelGRBF1(dataNormalized, isKernel);
                        str1 += Environment.NewLine + "Kernel GRBF : success";
                        BtnProcess.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        BtnProcess.Enabled = (DgvRawData.Rows.Count > 0) ? true : false;
                        MessageBox.Show("GRBF Kernel Error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                        return;
                    }
                }
                str1 += Environment.NewLine;
                str1 += Environment.NewLine + "//======================================//";
                str1 += Environment.NewLine + "*PROCESS OF INITIAL CENTROID*";

                double[][] meanInisial = new double[countcluster][];
                for (int k = 0; k < countcluster; ++k)
                {
                    meanInisial[k] = new double[dataKernel[0].Length];
                }
                if (centroidType == 0)
                {
                    try
                    {
                        BtnProcess.Enabled = false;
                        str1 += Environment.NewLine + "Centroid type: " + centroidTypeStr;
                        double[][] Kombinasi1 = new double[1][];
                        Kombinasi1 = MakeRandom1(dataKernel.Length, countcluster);
                        meanInisial = GetDataMakeRandom1(dataKernel,Kombinasi1, countcluster);
                        str1 += Environment.NewLine + "Random data row";
                        str1 += Environment.NewLine + "===============";
                        MakeTableDouble("No.", "Col", Kombinasi1, "");

                        str1 += Environment.NewLine + "Step-2: Search for centroid data";
                        str1 += Environment.NewLine + "===============";
                        MakeTableDouble("cluster", "Col", meanInisial, "");
                        BtnProcess.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        BtnProcess.Enabled = (DgvRawData.Rows.Count > 0) ? true : false;
                        MessageBox.Show("Error Centroid type: " + centroidTypeStr + Environment.NewLine + ex.Message, "Attention");
                        return;
                    }
                }
                else if (centroidType == 1)
                {
                    try
                    {
                        BtnProcess.Enabled = false;
                        str1 += Environment.NewLine + "Centroid type: " + centroidTypeStr;
                        double[][] kombinasi1 = new double[1][];
                        double[][] kombinasiTemporer = new double[10][];
                        double[] nilaiSSETemporer = new double[10];
                        for (int i = 0; i < 10; ++i)
                        {
                            kombinasi1 = MakeRandom1(dataKernel.Length, countcluster);
                            kombinasiTemporer[i] = kombinasi1[0];
                            meanInisial = GetDataMakeRandom1(dataKernel, kombinasi1, countcluster);
                            nilaiSSETemporer[i] = CariSSE(dataKernel, meanInisial);          
                            str1 += Environment.NewLine + "Step-["+i+"]: Random data row";                           
                            MakeTableDouble("No.", "Col", kombinasi1, "");
                            str1 += Environment.NewLine + "nilaiSSE["+i+"]: "+ nilaiSSETemporer[i];
                            str1 += Environment.NewLine + "===============";
                            Thread.Sleep(10);
                        }
                        str1 += Environment.NewLine + "===============";
                        kombinasi1[0] = kombinasiTemporer[0];
                        double nilaiSSEMinimum = nilaiSSETemporer[0];
                        for (int i = 0; i < nilaiSSETemporer.Length; ++i)
                        {
                            if (nilaiSSEMinimum > nilaiSSETemporer[i])
                            {
                                nilaiSSEMinimum = nilaiSSETemporer[i];
                                kombinasi1[0] = kombinasiTemporer[i];
                            }
                        }
                        str1 += Environment.NewLine + "nilaiSSEMinimum: " + nilaiSSEMinimum;
                        str1 += Environment.NewLine + "data row";
                        MakeTableDouble("No.", "Col", kombinasi1, "");
                        meanInisial = GetDataMakeRandom1(dataKernel, kombinasi1, countcluster);
                        str1 += Environment.NewLine + "Data centorid";
                        MakeTableDouble("cluster", "Col", meanInisial, ""); 
                        BtnProcess.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        BtnProcess.Enabled = (DgvRawData.Rows.Count > 0) ? true : false;
                        MessageBox.Show("Error Centroid type: " + centroidTypeStr + Environment.NewLine + ex.Message, "Attention");
                        return;
                    }
                }
                str1 += Environment.NewLine;
                str1 += Environment.NewLine + "//======================================//";
                str1 += Environment.NewLine + "*PROCESS OF K-MEANS*";
                str1 += Environment.NewLine + "//======================================//";
                str1 += Environment.NewLine;

                str1 += Environment.NewLine + "======================================";
                str1 += Environment.NewLine + "Step : Initialization cluster";
                str1 += Environment.NewLine + "======================================";
                double[][] meanFix = new double[countcluster][];
                double[] dataSSW = new double[countcluster];
                for (int k = 0; k < countcluster; ++k)
                {
                    meanFix[k] = meanInisial[k];
                    dataSSW[k] = 0;
                }

                int[] clusterInisial = new int[dataKernel.Length];
                for (int i = 0; i < dataKernel.Length; ++i)
                {
                    clusterInisial[i] = 0;
                };
                int[] countclustersInisial = new int[meanInisial.Length];

                for (int i = 0; i < dataKernel.Length; ++i)
                {
                    double[] jarak = new double[meanInisial.Length]; // distances8 from curr tuple to each mean
                    for (int k = 0; k < meanInisial.Length; ++k)
                    {
                        double[] tuple = dataKernel[i];
                        double[] mean = meanInisial[k];
                        double sumSquaredDiffs = 0.0;
                        for (int j = 0; j < tuple.Length; ++j)
                        {
                            sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
                        }
                        jarak[k] = Math.Sqrt(sumSquaredDiffs);
                    }
                    int indexOfMin = 0;
                    double smallDist = jarak[0];
                    for (int k = 0; k < jarak.Length; ++k)
                    {
                        if (jarak[k] < smallDist)
                        {
                            smallDist = jarak[k];
                            indexOfMin = k;

                        }

                    }
                    int newclusterID8 = indexOfMin;
                    if (newclusterID8 != clusterInisial[i])
                    {
                        clusterInisial[i] = newclusterID8;
                    }

                    int cluster = clusterInisial[i];
                    ++countclustersInisial[cluster];
                    dataSSW[clusterInisial[i]] += smallDist;
                }
                MakeTableInt("cluster", "Count", countclustersInisial);


                for (int i = 0; i < countcluster; ++i)
                {
                    dataSSW[i] = dataSSW[i] / countclustersInisial[i];
                }

                str1 += Environment.NewLine + "======================================";
                str1 += Environment.NewLine + "Step : Iteration K-mean";
                str1 += Environment.NewLine + "======================================";
                str1 += Environment.NewLine + "----START K-Means!----";
                int Iteration = 0;

                int[] clusterFix = new int[clusterInisial.Length]; // proposed result
                Array.Copy(clusterInisial, clusterFix, clusterInisial.Length);
                bool updatedcluster = true;
                bool updatedMean = true;
                while (updatedcluster == true && updatedMean == true && Iteration < maxIteration)
                {
                    ++Iteration;
                    str1 += Environment.NewLine + "====================";
                    str1 += Environment.NewLine + "Iteration: " + Iteration + " | calculation mean";
                    str1 += Environment.NewLine + "====================";
                    str1 += Environment.NewLine + "[" + Iteration + "] START calculation mean";
                    double[][] meanUpdate = new double[countcluster][];
                    for (int k = 0; k < countcluster; ++k)
                    {
                        meanUpdate[k] = new double[dataKernel[0].Length];
                    }
                    int[] countclustersInisial2 = new int[meanUpdate.Length];
                    for (int i = 0; i < dataKernel.Length; ++i)
                    {
                        int cluster = clusterInisial[i];
                        ++countclustersInisial2[cluster];
                    }
                    MakeTableInt("[" + Iteration + "] cluster", "Count", countclustersInisial2);
                    for (int k = 0; k < meanUpdate.Length; ++k)
                    {
                        if (countclustersInisial2[k] == 0)
                        {
                            updatedMean = false; // bad clustering4. no change to means7[][]
                            str1 += Environment.NewLine + "----END K-Means mean does not change / bad clustering----";
                            break;
                        }
                    }
                    str1 += Environment.NewLine + "[" + Iteration + "] mean new";
                    for (int i = 0; i < dataKernel.Length; ++i)
                    {
                        int cluster = clusterInisial[i];
                        for (int j = 0; j < dataKernel[i].Length; ++j)
                        {
                            meanUpdate[cluster][j] += dataKernel[i][j]; // akumulasi calculation

                        }
                    }
                    for (int k = 0; k < meanUpdate.Length; ++k)
                    {
                        for (int j = 0; j < meanUpdate[k].Length; ++j)
                        {
                            meanUpdate[k][j] /= countclustersInisial2[k]; // berbahaya jika div by 0
                        }
                    }
                    for (int k = 0; k < countcluster; ++k)
                    {
                        meanFix[k] = meanUpdate[k];
                    }
                    updatedMean = true;
                    str1 += Environment.NewLine + "[" + Iteration + "] mean new: success";
                    str1 += Environment.NewLine;
                    str1 += Environment.NewLine + "[" + Iteration + "] END calculation mean";
                    str1 += Environment.NewLine + "====================";
                    str1 += Environment.NewLine + "iteration: " + Iteration + " | calculation cluster";
                    str1 += Environment.NewLine + "====================";
                    str1 += Environment.NewLine + "[" + Iteration + "] START calculation cluster";
                    updatedcluster = false;
                    string[] clusternewStr = new string[clusterInisial.Length]; // proposed result
                    string[][] jarakStr = new string[dataRaw.Length][];
                    for (int i = 0; i < dataRaw.Length; ++i)
                    {
                        jarakStr[i] = new string[countcluster];
                    }
                    //--number version
                    int[] clusternew = new int[clusterInisial.Length]; // proposed result
                    Array.Copy(clusterInisial, clusternew, clusterInisial.Length);
                    double[] jarak = new double[meanUpdate.Length]; // distances8 from curr tuple to each mean
                    int[] countclustersUpdate = new int[meanUpdate.Length];

                    for (int i = 0; i < countcluster; ++i)
                    {
                        dataSSW[i] = 0.0;
                    }
                    for (int i = 0; i < dataKernel.Length; ++i)
                    {
                        //string str2 = "";
                        for (int k = 0; k < meanUpdate.Length; ++k)
                        {
                            //manual hitung jarak
                            double[] tuple = dataKernel[i];
                            double[] mean = meanUpdate[k];
                            double sumSquaredDiffs = 0.0;
                            for (int j = 0; j < tuple.Length; ++j)
                            {
                                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
                            }
                            jarak[k] = Math.Sqrt(sumSquaredDiffs);
                            jarakStr[i][k] = jarak[k].ToString();
                        }
                        //manual hitung MinIndex
                        int indexOfMin = 0;
                        double smallDist = jarak[0];
                        for (int k = 0; k < jarak.Length; ++k)
                        {
                            if (jarak[k] < smallDist)
                            {
                                smallDist = jarak[k];
                                indexOfMin = k;
                            }
                        }
                        int newclusterID8 = indexOfMin;
                        if (newclusterID8 != clusternew[i])
                        {
                            updatedcluster = true;
                            clusternew[i] = newclusterID8; // update
                            clusternewStr[i] = clusterInisial[i] + " => " + clusternew[i] + " true";
                        }
                        else
                        {
                            clusternewStr[i] = clusterInisial[i] + " => " + clusternew[i] + " false";
                        }
                        int cluster = clusternew[i];
                        ++countclustersUpdate[cluster];
                        dataSSW[clusternew[i]] += smallDist;
                    }
                    //PENYEBAB LAMA
                    //MakeTableClusterString("[" + Iteration + "] No", "Dist", jarakStr, clusternewStr, "A=>B Change?");
                    MakeTableInt("[" + Iteration + "] cluster", "Count", countclustersUpdate);

                    for (int i = 0; i < countcluster; ++i)
                    {
                        dataSSW[i] = dataSSW[i] / countclustersUpdate[i];
                    }
                    clusterFix = clusternew;
                    str1 += Environment.NewLine + "[" + Iteration + "] END calculation cluster";
                    for (int k = 0; k < meanUpdate.Length; ++k)
                    {
                        if (countclustersUpdate[k] == 0)
                        {
                            updatedcluster = false; //tidak ada perubahan tapi calculation tidak bagus
                            str1 += Environment.NewLine + "----END K-Means bad clustering----";
                            break;
                        }
                    }
                    //clusterFix = clusternew;
                    if (updatedcluster == false)
                    {
                        updatedcluster = false; //tidak ada perubahan sehingga konvergen
                        str1 += Environment.NewLine + "----END K-Means cluster konvergen----";
                        str1 += Environment.NewLine + "======================================";
                        str1 += Environment.NewLine;
                        str1 += Environment.NewLine + "*RESULT OF K-MEANS*";
                         str1 += Environment.NewLine + "//======================================//";
                        str1 += Environment.NewLine + "Result by dataRaw";
                        str1 += Environment.NewLine + "======================================";
                        MakeTableClusterResult("No", "Col", dataRaw, clusternew, countcluster, "cluster");

                        //Statistic start
                        strStatistic += Environment.NewLine + "======================================";
                        strStatistic += Environment.NewLine + "Result by dataRaw";
                        strStatistic += Environment.NewLine + "======================================";
                        MakeTableClusterResultStatistic("No", "Col", dataRaw, clusternew, countcluster, "cluster");
                        //Statistic end
                        foreach (var series0 in ChartResultData.Series)
                        {
                            series0.Points.Clear();
                        }
                        ChartResultData.Series.Clear();
                        for (int i = 1; i < dataRaw[0].Length; i++)
                        {
                            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
                            double[] x1 = new double[dataRaw.Length];
                            double[] y1 = new double[dataRaw.Length];
                            for (int j = 0; j < dataRaw.Length; j++)
                            {
                                x1[j] = Convert.ToDouble(dataRaw[j][0].ToString());
                                y1[j] = Convert.ToDouble(dataRaw[j][i].ToString());
                                series1.Points.DataBindXY(x1, y1);
                            }
                            series1.ChartType = SeriesChartType.Point;
                            series1.MarkerSize = 10;
                            series1.MarkerStyle = MarkerStyle.Circle;
                            ChartResultData.Series.Add(series1);
                        }
                        ChartResultData.ResetAutoValues();
                        ChartResultData.Titles.Clear();
                        ChartResultData.Titles.Add($"Scatter Plot");
                        ChartResultData.ChartAreas[0].AxisX.Title = "X";
                        ChartResultData.ChartAreas[0].AxisY.Title = "Y";
                        ChartResultData.Legends.Clear();
                        for (int k = 0; k < countcluster; ++k)
                        {
                            Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                            for (int i = 1; i < dataRaw[0].Length; i++)
                            {
                                for (int j = 0; j < dataRaw.Length; j++)
                                {
                                    int clusterID = clusternew[j];
                                    if (clusterID == k)
                                    {
                                        ChartResultData.Series["Series"+i].Points[j].Color = randomColor;
                                    }
                                }
                            }
                        }
                        System.Data.DataTable dt1 = new System.Data.DataTable();
                        DataRow row;
                        int rowCount = dataRaw.Length;
                        int colCount = dataRaw[0].Length + 1;
                        for (int i2 = 0; i2 < rowCount; i2++)
                        {
                            for (int k2 = 0; k2 < colCount; k2++)
                            {
                                if (k2 != (colCount - 1))
                                {
                                    dt1.Columns.Add("X" + (k2 + 1));
                                }
                                else
                                {
                                    dt1.Columns.Add("Cluster");
                                }
                            }
                            break;
                        }
                        int rowCounter;
                        for (int i = 0; i < dataRaw.Length; i++)
                        {
                            row = dt1.NewRow();
                            rowCounter = 0;
                            for (int j = 0; j < dataRaw[i].Length + 1; j++)
                            {
                                if (j < dataRaw[i].Length)
                                {
                                    row[rowCounter] = dataRaw[i][j].ToString();
                                }
                                else
                                {
                                    row[rowCounter] = clusternew[i].ToString();
                                }
                                rowCounter++;
                            }
                            dt1.Rows.Add(row);
                        }
                        DgvResultData.DataSource = dt1;
                        break;
                    }
                    else
                    {
                        Array.Copy(clusternew, clusterInisial, clusternew.Length); // update
                        updatedcluster = true; // clustering berlanjut peling tidak ada 1 berubah
                        str1 += Environment.NewLine + "----CONTINUE K-Means perubahan cluster----";
                    }
                }

                str1 += Environment.NewLine;
                str1 += Environment.NewLine + "//======================================//";
                str1 += Environment.NewLine + "*PROCESS OF EVALUATION*";

                if (isEvaluationDBI == true)
                {
                    str1 += Environment.NewLine + "======================================";
                    str1 += Environment.NewLine + "Step Evaluation Davies-Bouldin Index (DBI)";
                    double[][] dataSSB = MakeEvaluationDBI(meanFix  , dataSSW, 2, 1);
                    double[][] dataRasio = MakeEvaluationDBI(meanFix, dataSSW, 2, 2);
                    double[][] dataMatrixRasio = MakeEvaluationDBI(meanFix, dataSSW, 2, 3);
                    double[][] dataMaxRasio = MakeEvaluationDBI(meanFix, dataSSW, 2, 4);
                    double avgRatio = 0;
                    for (int i = 0; i < dataMaxRasio.Length; ++i)
                    {
                        avgRatio += dataMaxRasio[i][1];
                    }
                    avgRatio /= dataMaxRasio.Length;
                    str1 += Environment.NewLine + "Value DBI: " + avgRatio;

                    strStatistic += Environment.NewLine + "======================================";
                    strStatistic += Environment.NewLine + "Evaluation Davies-Bouldin Index (DBI)";
                    strStatistic += Environment.NewLine + "Value DBI : " + avgRatio;
                }

                if (isEvaluationSilhouette == true)
                {
                    str1 += Environment.NewLine + "======================================";
                    str1 += Environment.NewLine + "Step Evaluation Silhouette Coefficient(SC)";
                    double[][] dataSihloutteStep9 = MakeEvaluationSihloutte(countcluster, dataKernel, clusterFix, 9);

                    double avgSilhouette = 0;
                    for (int i = 0; i < dataSihloutteStep9.Length; ++i)
                    {
                        avgSilhouette += dataSihloutteStep9[i][2];
                    }
                    avgSilhouette /= dataSihloutteStep9.Length;
                    str1 += Environment.NewLine + "Value SC : " + avgSilhouette;
                    str1 += Environment.NewLine;
                    //Statistic start
                    strStatistic += Environment.NewLine + "======================================";
                    strStatistic += Environment.NewLine + "Evaluation Silhouette Coefficient(SC)";
                    ///strStatistic += Environment.NewLine + "======================================";
                    strStatistic += Environment.NewLine + "Value SC : " + avgSilhouette;
                }
                RtbxProses.Text = str1;
                RtbxStatistik.Text = strStatistic;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Process: "+ex.Message,"Attention");
            }

        }
        private static double[][] MakeEvaluationDBI(double[][] dataRaw, double[] dataRaw2, int countkomb, int options)
        {
            string[] dataKombinasi01 = new string[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                dataKombinasi01[i] = i.ToString();
            }
            int countKombinasi = MakeCombinationRaw(dataKombinasi01, countkomb).ToList().Count();
            double[][] dataKombinasi02 = new double[countKombinasi][];
            double[][] resultSSB = new double[countKombinasi][];
            double[][] resultRasio = new double[countKombinasi][];
            double[][] matrixRasio = new double[dataRaw2.Length][];
            double[][] resultMax = new double[dataRaw2.Length][];

            for (int i = 0; i < countKombinasi; ++i)
            {
                dataKombinasi02[i] = new double[countkomb];
                resultSSB[i] = new double[countkomb + 1];
                resultRasio[i] = new double[countkomb + 1];
            }
            for (int i = 0; i < dataRaw2.Length; ++i)
            {
                matrixRasio[i] = new double[dataRaw2.Length];
                resultMax[i] = new double[countkomb];
            }
            int a1 = 0;
            foreach (IEnumerable<string> data1 in MakeCombinationRaw(dataKombinasi01, countkomb))
            {
                for (int k = 0; k < data1.ToArray().Length; ++k)
                {
                    dataKombinasi02[a1][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                    resultSSB[a1][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                    resultRasio[a1][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                }
                ++a1;
            }
            double[][] meanTemporer = new double[countkomb][];
            for (int k = 0; k < countkomb; ++k)
            {
                meanTemporer[k] = new double[dataRaw[0].Length];
            }
            for (int i = 0; i < dataKombinasi02.Length; ++i)
            {
                double sumRatio = 0.0;
                for (int j = 0; j < dataKombinasi02[i].Length; ++j)
                {
                    meanTemporer[j] = dataRaw[(int)dataKombinasi02[i][j]];
                    sumRatio += dataRaw2[(int)dataKombinasi02[i][j]];
                }
                double[] jarakTemporer = new double[meanTemporer.Length];
                double sumSSB = 0.0;
                for (int l = 0; l < meanTemporer.Length; ++l)
                {
                    for (int m = 0; m < meanTemporer.Length; ++m)
                    {
                        double[] tuple = meanTemporer[l];
                        double[] mean = meanTemporer[m];
                        double sumSquaredDiffs = 0.0;
                        for (int n = 0; n < tuple.Length; ++n)
                        {
                            sumSquaredDiffs += Math.Pow((tuple[n] - mean[n]), 2);
                        }
                        jarakTemporer[m] = Math.Sqrt(sumSquaredDiffs);
                    }
                    sumSSB += jarakTemporer[0];
                    resultSSB[i][countkomb] = sumSSB;
                }
                sumRatio /= sumSSB;
                resultRasio[i][countkomb] = sumRatio;
            }
            for (int l = 0; l < resultRasio.Length; ++l)
            {
                int a = (int)resultRasio[l][0];
                int b = (int)resultRasio[l][1];
                matrixRasio[a][b] = resultRasio[l][countkomb];
                matrixRasio[b][a] = resultRasio[l][countkomb];
            }
            for (int l = 0; l < matrixRasio.Length; ++l)
            {
                double vMax = matrixRasio[l][0];
                for (int m = 0; m < matrixRasio[l].Length; ++m)
                {
                    if (matrixRasio[l][m] > vMax)
                    {
                        vMax = matrixRasio[l][m];
                    }
                    resultMax[l][1] = vMax;
                }
            }
            if (options == 1)
            {
                return resultSSB;
            }
            else if (options == 2)
            {
                return resultRasio;
            }
            else if (options == 3)
            {
                return matrixRasio;
            }
            else if (options == 4)
            {
                return resultMax;
            }
            else
            {
                return resultSSB;
            }
        }

        private static double[][] MakeEvaluationSihloutte(int countcluster, double[][] dataKernel, int[] clusternew, int options)
        {       
            double[][] sortArray = new double[dataKernel.Length][];
            for (int i = 0; i < dataKernel.Length; ++i)
            {
                sortArray[i] = new double[dataKernel[i].Length + 1];
                for (int j = 0; j < dataKernel[i].Length; ++j)
                {
                    sortArray[i][j] = dataKernel[i][j];
                }
                sortArray[i][sortArray[0].Length - 1] = clusternew[i];
            }
            double[][] sortArrayAll1 = new double[sortArray.Length][];
            Array.Copy(sortArray, sortArrayAll1, sortArray.Length);
            int[] sortcluster = new int[clusternew.Length];
            Array.Copy(clusternew, sortcluster, clusternew.Length);
            for (int i = 0; i < sortArray[0].Length; ++i)
            {
                sortArrayAll1 = sortArray.OrderBy(x => x[i]).ToArray();
            }

            for (int i = 0; i < sortArrayAll1.Length; ++i)
            {
                sortArray[i] = new double[dataKernel[i].Length];
                for (int j = 0; j < (sortArrayAll1[i].Length - 1); ++j)
                {
                    sortArray[i][j] = sortArrayAll1[i][j];

                }
                sortcluster[i] = (int)sortArrayAll1[i][(sortArrayAll1[0].Length - 1)];
            }


            double[][] resultStep1 = new double[sortArray.Length][];
            for (int i = 0; i < sortArray.Length; ++i)
            {
                resultStep1[i] = new double[sortArray.Length];
            }
            double[] jarakTemporer = new double[sortArray.Length];
            for (int i = 0; i < sortArray.Length; ++i)
            {
                double[] mean = sortArray[i];
                for (int k = 0; k < sortArray.Length; ++k)
                {

                    double[] tuple = sortArray[k];
                    double sumSquaredDiffs = 0.0;
                    for (int n = 0; n < tuple.Length; ++n)
                    {
                        sumSquaredDiffs += Math.Pow((tuple[n] - mean[n]), 2);
                    }
                    resultStep1[i][k] = Math.Sqrt(sumSquaredDiffs);
                }
            }
            double[][] resultStep2fake = new double[resultStep1.Length][];
            double[] resultStep2 = new double[resultStep1.Length];
            for (int i = 0; i < resultStep2fake.Length; ++i)
            {
                resultStep2fake[i] = new double[3];
                for (int j = 0; j < resultStep2fake[i].Length; ++j)
                {
                    resultStep2fake[i][j] = 0.0;
                }
                resultStep2[i] = 0;
            }

            for (int i = 0; i < sortcluster.Length; ++i)
            {
                double sum1 = 0.0;
                double sum2 = 0.0;
                int count1 = 0;
                for (int k = 0; k < sortcluster.Length; ++k)
                {
                    if (sortcluster[i] == sortcluster[k])
                    {
                        sum1 += resultStep1[i][k];
                        ++count1;
                    }
                }
                if (count1 - 1 <= 0)
                {
                    count1 = 1;
                }
                sum2 = sum1 / (count1 - 1);
                if (double.IsNaN(sum2))
                {
                    sum2 = 0;
                }
                resultStep2[i] = sum2;
                resultStep2fake[i][0] = sum2;
            }

            double[][] resultStep3 = new double[sortArray.Length][];
            for (int i = 0; i < sortArray.Length; ++i)
            {
                resultStep3[i] = new double[countcluster];
            }
            for (int h = 0; h < sortcluster.Length; ++h)
            {
                for (int l = 0; l < resultStep3[h].Length; ++l)
                {
                    double sum1 = 0.0;
                    double sum2 = 0.0;
                    int count1 = 0;
                    for (int k = 0; k < sortcluster.Length; ++k)
                    {
                        if (sortcluster[k] == l)
                        {
                            sum1 += resultStep1[h][k];
                            ++count1;
                        }
                    }
                    if (sortcluster[h] == l)
                    {
                        sum2 = 0;
                    }
                    else
                    {
                        sum2 = sum1 / count1;
                        if (double.IsNaN(sum2))
                        {
                            sum2 = 0;
                        }
                    }
                    resultStep3[h][l] = sum2;
                }
            }
            for (int h = 0; h < sortcluster.Length; ++h)
            {
                double MinValue = 0;
                for (int l = 0; l < resultStep3[h].Length; ++l)
                {
                    if (sortcluster[h] != l)
                    {
                        MinValue = resultStep3[h][l];
                    }
                    for (int j = 0; j < resultStep3[h].Length; ++j)
                    {
                        if (resultStep3[h][j] < MinValue && sortcluster[h] != j)
                        {
                            MinValue = resultStep3[h][j];
                        }
                    }
                }
                resultStep2fake[h][1] = MinValue;
            }
            for (int i = 0; i < resultStep2fake.Length; ++i)
            {
                double MaxValue = resultStep2fake[i][0];
                for (int j = 0; j < resultStep2fake[i].Length; ++j)
                {
                    if (MaxValue < resultStep2fake[i][j])
                    {
                        MaxValue = resultStep2fake[i][j];
                    }
                }
                resultStep2fake[i][2] = (resultStep2fake[i][1] - resultStep2fake[i][0]) / MaxValue;
            }
            if (options == 1)
            {
                return dataKernel;
            }
            else if (options == 3)
            {
                return sortArrayAll1;
            }
            else if (options == 4)
            {
                return sortArray;
            }
            else if (options == 5)
            {
                return resultStep1;
            }
            else if (options == 6)
            {
                return resultStep2fake;
            }
            else if (options == 7)
            {
                return resultStep3;
            }
            else if (options == 8)
            {
                return resultStep2fake;
            }
            else if (options == 9)
            {
                return resultStep2fake;
            }
            else
            {
                return dataKernel;
            }
        }
        public void MakeTableDouble1(string strA, string strB, double[] data)
        {
            MakeHeader2(strA, strB);
            str1 += Environment.NewLine;
            for (int i = 0; i < data.Length; ++i)
            {
                str1 += i.ToString().PadLeft(3) + " | " + data[i].ToString() + " | ";
                str1 += Environment.NewLine + "";
            }
        } // ShowData
        private static double[][] MakeRandomCentroid1x(double[][] dataRaw, int countcluster)
        {
            Random random = new Random(0);
            double[][] means = new double[countcluster][];
            for (int j = 0; j < means.Length; ++j)
            {
                means[j] = dataRaw[j];
            }
            for (int j = countcluster; j < means.Length; ++j)
            {
                means[j] = dataRaw[random.Next(0, countcluster)];
            }
            return means;
        }
        private static double[][] MakeRandom1(int dataLength, int countcluster)
        {
            double[][] resultKombinasi1 = new double[1][];
            resultKombinasi1[0] = new double[countcluster];
            _ = new List<int>();
            List<int> uniqueNoList = UniqueRandomNoList(dataLength, countcluster);
            int a1 = 0;
            foreach (int no in uniqueNoList)
            {
                resultKombinasi1[0][a1] = no;
                a1++;
            }
            Thread.Sleep(10); 
            return resultKombinasi1;
        }
        private static double[][] GetDataMakeRandom1(double[][] dataRaw, double[][] dataKombinasi, int countcluster)
        {

            double[][] means = new double[countcluster][];
            for (int i = 0; i < dataKombinasi.Length; ++i)
            {
                for (int j = 0; j < dataKombinasi[i].Length; ++j)
                {
                    double a1 = dataKombinasi[i][j];
                    means[j] = dataRaw[Convert.ToInt16(a1)];
                }
            }
            return means;
        }
        private static double[][] MakeRandomCentroid2(double[][] dataRaw, int countcluster, string tipe)
        {
            double[][] resultKombinasi1 = new double[1][];
            resultKombinasi1[0] = new double[countcluster];

            double[][] means = new double[countcluster][];
            _ = new List<int>();
            List<int> uniqueNoList = UniqueRandomNoList(dataRaw.Length, countcluster);
            int a1 = 0;
            foreach (int no in uniqueNoList)
            {
                means[a1] = dataRaw[no];
                resultKombinasi1[0][a1] = no;
                a1++;
            }
            Thread.Sleep(10);
            if (tipe == "meanData")
            {
                return means;
            }
            else
            {
                return resultKombinasi1;
            }
        }
        private static double[][] MakeRandomCentroid1(double[][] dataRaw, int countcluster)
        {
            double[][] means = new double[countcluster][];
            _ = new List<int>();
            List<int> uniqueNoList = UniqueRandomNoList(dataRaw.Length, countcluster);
            int a1 = 0;
            foreach (int no in uniqueNoList)
            {
                means[a1] = dataRaw[no];
                a1++;
            }
            return means;
        }
        public static List<int> UniqueRandomNoList(int maxRange, int totalRandomnoCount)
        {

            List<int> noList = new List<int>();
            int count = 0;
            Random r = new Random();
            List<int> listRange = new List<int>();
            for (int i = 0; i < totalRandomnoCount; i++)
            {
                listRange.Add(i);
            }
            while (listRange.Count > 0)
            {
                int item = r.Next(maxRange);// listRange[];    
                if (!noList.Contains(item) && listRange.Count > 0)
                {
                    noList.Add(item);
                    listRange.Remove(count);
                    count++;
                }
            }
            return noList;
        }
        public void MakeTableClusterResult(string strA, string col, double[][] data, int[] data2, int numClusters, string strB)
        {
            MakeHeader1(strA, col, ConvertDoubleToArrayString(data[0]), strB);
            // str1 += Environment.NewLine;
            for (int k = 0; k < numClusters; ++k)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    int clusterID = data2[i];
                    if (clusterID != k) continue;
                    str1 += Environment.NewLine + i.ToString().PadLeft(3) + " | ";
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] >= 0.0) str1 += " ";
                        str1 += data[i][j].ToString() + " | ";
                    }
                    str1 += "[" + k + "]";
                }
                str1 += Environment.NewLine + "-----------------------";
            } // k
        } // ShowData
        public void MakeTableClusterResultStatistic(string strA, string col, double[][] data, int[] data2, int numClusters, string strB)
        {
            MakeHeaderResult1(strA, col, ConvertDoubleToArrayString(data[0]), strB);
            // str1 += Environment.NewLine;
            for (int k = 0; k < numClusters; ++k)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    int clusterID = data2[i];
                    if (clusterID != k) continue;
                    strStatistic += Environment.NewLine + i.ToString().PadLeft(3) + " | ";
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] >= 0.0) str1 += " ";
                        strStatistic += data[i][j].ToString() + " | ";
                    }
                    strStatistic += "[" + k + "]";
                }
                strStatistic += Environment.NewLine + "-----------------------";
            } // k
        } // ShowData
        public void MakeHeaderResult1(string strA, string col, string[] data, string strB)
        {
            var sb = new System.Text.StringBuilder();
            if (strA.Length > 0)
            {
                sb.Append(strA + " | ");
            }
            for (int j = 0; j < data.Length; ++j)
            {
                sb.Append(" " + col + +j + " | ");
            }
            if (strB.Length > 0)
            {
                sb.Append(" " + strB + " | ");
            }
            strStatistic += Environment.NewLine + sb;// + Environment.NewLine;
        }
        public void MakeTableClusterString(string strA, string col, string[][] data, string[] data2, string strB)
        {
            MakeHeader1(strA, col, data[0], strB);
            str1 += Environment.NewLine;
            for (int i = 0; i < data.Length; ++i)
            {
                str1 += i.ToString().PadLeft(3) + " | ";
                for (int j = 0; j < data[i].Length; ++j)
                {
                    str1 += data[i][j].ToString() + " | ";
                }
                str1 += "[" + data2[i].ToString() + "]";
                str1 += Environment.NewLine + "";
            }
        }
        public void MakeHeader2(string strA, string strB)
        {
            var sb = new System.Text.StringBuilder();
            if (strA.Length > 0)
            {
                sb.Append(strA + " | ");
            }
            if (strB.Length > 0)
            {
                sb.Append(" " + strB + " | ");
            }
            str1 += Environment.NewLine + sb;// + Environment.NewLine;
        }
        public void MakeTableInt(string strA, string strB, int[] data)
        {
            MakeHeader2(strA, strB);
            str1 += Environment.NewLine;
            for (int i = 0; i < data.Length; ++i)
            {
                // str1 += i.ToString().PadLeft(3) + " | ";
                str1 += i.ToString().PadLeft(3) + " | " + data[i].ToString() + " | ";
                // }
                str1 += Environment.NewLine + "";
            }
        } // ShowData
        public void MakeTableCluster(string strA, string col, double[][] data, int[] data2, string strB)
        {
            MakeHeader1(strA, col, ConvertDoubleToArrayString(data[0]), strB);
            str1 += Environment.NewLine;
            for (int i = 0; i < data.Length; ++i)
            {
                str1 += i.ToString().PadLeft(3) + " | ";
                for (int j = 0; j < data[i].Length; ++j)
                {
                    str1 += data[i][j].ToString() + " | ";
                }
                str1 += "[" + data2[i].ToString() + "]";
                str1 += Environment.NewLine + "";
            }
        }

        static IEnumerable<IEnumerable<T>>
        MakeCombinationRaw<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return MakeCombinationRaw(list, length - 1).SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0), (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        static IEnumerable<IEnumerable<Tx>>
        MakeCombinationRawRandom<Tx>(IEnumerable<Tx> list, int length) where Tx : IComparable
        {
            if (length == 1) return list.Select(t => new Tx[] { t
            });
            return MakeCombinationRaw(list, length - 1).SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0), (t1, t2) => t1.Concat(new Tx[] { t2 }));
        }
        private static double[][] MakeCombinationOptions1(double[][] dataRaw, int countcluster, int options)
        {
            string[] dataKombinasi01 = new string[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                dataKombinasi01[i] = i.ToString();
            }
            int countKombinasi = MakeCombinationRaw(dataKombinasi01, countcluster).ToList().Count();
            double[][] dataKombinasi02 = new double[countKombinasi][];
            double[][] result = new double[countKombinasi][]; //array kombinasi
            double[][] result2 = new double[countKombinasi][];//hasil SSE
            double[][] result3 = new double[countKombinasi][];//array kombinasi dan hasil SSE
            double[][] result4 = new double[1][];//array kombinasi dan hasil SSE
            for (int i = 0; i < countKombinasi; ++i)
            {
                dataKombinasi02[i] = new double[countcluster];
                result[i] = new double[countcluster];
                result2[i] = new double[1];
                result3[i] = new double[countcluster + 1];

            }
            result4[0] = new double[countcluster + 1];

            int a1 = 0;
            foreach (IEnumerable<string> data1 in MakeCombinationRaw(dataKombinasi01, countcluster))
            {
                for (int k = 0; k < data1.ToArray().Length; ++k)
                {
                    dataKombinasi02[a1][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                    result[a1][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                    result2[a1][0] = 0.0;
                    result3[a1][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                }
                ++a1;
            }
            double[][] meanTemporer = new double[countcluster][];
            for (int k = 0; k < countcluster; ++k)
            {
                meanTemporer[k] = new double[dataRaw[0].Length];
            }

            double[] jarakSSEMin = new double[dataKombinasi02.Length];
            //str1 += Environment.NewLine + "No. | CENTROID | SSE ";
            for (int i = 0; i < dataKombinasi02.Length; ++i)
            {
                string str2 = "";
                for (int j = 0; j < dataKombinasi02[i].Length; ++j)
                {
                    str2 += dataKombinasi02[i][j].ToString() + " | ";
                    int x1 = (int)dataKombinasi02[i][j];
                    meanTemporer[j] = dataRaw[x1];

                }
                double[] jarakTemporer = new double[meanTemporer.Length];
                double sumSSE = 0.0;
                for (int l = 0; l < dataRaw.Length; ++l)
                {
                    for (int m = 0; m < meanTemporer.Length; ++m)
                    {
                        //manual hitung jarak
                        double[] tuple = dataRaw[l];
                        double[] mean = meanTemporer[m];
                        double sumSquaredDiffs = 0.0;
                        for (int n = 0; n < tuple.Length; ++n)
                        {
                            sumSquaredDiffs += Math.Pow((tuple[n] - mean[n]), 2);
                        }
                        jarakTemporer[m] = sumSquaredDiffs;
                    }
                    //manual hitung MinIndex
                    double smallDist = jarakTemporer[0];
                    for (int m = 0; m < jarakTemporer.Length; ++m)
                    {
                        if (jarakTemporer[m] < smallDist)
                        {
                            smallDist = jarakTemporer[m];
                        }
                    }
                    sumSSE += smallDist;
                    jarakSSEMin[i] = sumSSE;
                    result2[i][0] = jarakSSEMin[i];
                    result3[i][countcluster] = jarakSSEMin[i];


                }
            }
            double smallDist2 = jarakSSEMin[0];
            int indexOfSSE = 0;
            for (int m = 0; m < jarakSSEMin.Length; ++m)
            {
                if (jarakSSEMin[m] < smallDist2)
                {
                    smallDist2 = jarakSSEMin[m];
                    indexOfSSE = m;
                }
            }
            result4[0][countcluster] = smallDist2; ;
            string str3 = "";
            double[][] meanInisial = new double[countcluster][];
            for (int j = 0; j < dataKombinasi02[indexOfSSE].Length; ++j)
            {
                result4[0][j] = dataKombinasi02[indexOfSSE][j];
                str3 += dataKombinasi02[indexOfSSE][j].ToString() + " | ";
                int x1 = (int)dataKombinasi02[indexOfSSE][j];
                meanInisial[j] = dataRaw[x1];
            }

            if (options == 1)
            {
                return result;
            }
            else if (options == 2)
            {
                return result2;
            }
            else if (options == 3)
            {
                return result3;
            }
            else if (options == 4)
            {
                return result4;
            }
            else
            {
                return meanInisial;
            }
        }
        private static double CariSSE(double[][] dataRaw, double[][] meanTemporer)
        {
            double[] jarakTemporer = new double[meanTemporer.Length];
            double sumSSE = 0.0;
            for (int l = 0; l < dataRaw.Length; ++l)
            {
                for (int m = 0; m < meanTemporer.Length; ++m)
                {
                    //manual hitung jarak
                    double[] tuple = dataRaw[l];
                    double[] mean = meanTemporer[m];
                    double sumSquaredDiffs = 0.0;
                    for (int n = 0; n < tuple.Length; ++n)
                    {
                        sumSquaredDiffs += Math.Pow((tuple[n] - mean[n]), 2);
                    }
                    jarakTemporer[m] = sumSquaredDiffs;
                }
                //manual hitung MinIndex
                double smallDist = jarakTemporer[0];
                for (int m = 0; m < jarakTemporer.Length; ++m)
                {
                    if (jarakTemporer[m] < smallDist)
                    {
                        smallDist = jarakTemporer[m];
                    }
                }
                sumSSE += smallDist;
                //result1[i][countcluster] = sumSSE;
            }
            return Math.Round(sumSSE,4);
        }
        private static double[][] MakeRandomCombination10(double[][] dataRaw, int countcluster, int LengthOfData)
        {
            string[] dataKombinasi01 = new string[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                dataKombinasi01[i] = i.ToString();
            }
            int countKombinasi = MakeCombinationRaw(dataKombinasi01, countcluster).ToList().Count();
            double[] resultInt = new double[LengthOfData];
            for (int i = 0; i < LengthOfData; ++i)
            {
                resultInt[i] = 0;
            }
            int[] intNumbers = new int[LengthOfData];
            int maxNumber = countKombinasi;
            Random intRandom = new Random();
            for (int intCounter = 0; intCounter <= intNumbers.GetUpperBound(0); intCounter++)
            {
                intNumbers[intCounter] = intRandom.Next(0, maxNumber - 1);
                resultInt[intCounter] = intNumbers[intCounter];
            }
            double[][] dataKombinasi02 = new double[LengthOfData][];
            double[][] result1 = new double[LengthOfData][];

            for (int i = 0; i < LengthOfData; ++i)
            {
                dataKombinasi02[i] = new double[countcluster];
                result1[i] = new double[countcluster + 1];
            }

            int a1 = 0;
            foreach (IEnumerable<string> data1 in MakeCombinationRaw(dataKombinasi01, countcluster))
            {
                for (int i = 0; i < resultInt.Length; ++i)
                {
                    if (a1 == resultInt[i])
                    {
                        for (int k = 0; k < data1.ToArray().Length; ++k)
                        {
                            dataKombinasi02[i][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                            result1[i][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                        }
                    }
                }
                ++a1;
            }
            double[][] meanTemporer = new double[countcluster][];
            for (int k = 0; k < countcluster; ++k)
            {
                meanTemporer[k] = new double[dataRaw[0].Length];
            }
            for (int i = 0; i < dataKombinasi02.Length; ++i)
            {
                string str2 = "";
                for (int j = 0; j < dataKombinasi02[i].Length; ++j)
                {
                    str2 += dataKombinasi02[i][j].ToString() + " | ";
                    int x1 = (int)dataKombinasi02[i][j];
                    meanTemporer[j] = dataRaw[x1];

                }
                double[] jarakTemporer = new double[meanTemporer.Length];
                double sumSSE = 0.0;
                for (int l = 0; l < dataRaw.Length; ++l)
                {
                    for (int m = 0; m < meanTemporer.Length; ++m)
                    {
                        //manual hitung jarak
                        double[] tuple = dataRaw[l];
                        double[] mean = meanTemporer[m];
                        double sumSquaredDiffs = 0.0;
                        for (int n = 0; n < tuple.Length; ++n)
                        {
                            sumSquaredDiffs += Math.Pow((tuple[n] - mean[n]), 2);
                        }
                        jarakTemporer[m] = sumSquaredDiffs;
                    }
                    //manual hitung MinIndex
                    double smallDist = jarakTemporer[0];
                    for (int m = 0; m < jarakTemporer.Length; ++m)
                    {
                        if (jarakTemporer[m] < smallDist)
                        {
                            smallDist = jarakTemporer[m];
                        }
                    }
                    sumSSE += smallDist;
                    result1[i][countcluster] = sumSSE;
                }
            }
            double[][] sortResult1 = new double[result1.Length][];
            Array.Copy(result1, sortResult1, result1.Length);
            result1 = sortResult1.OrderBy(x => x[countcluster]).ToArray();
            return result1;
        }

        private static double[][] MakeCombinationOptions3(double[][] dataRaw, int countcluster, int options)
        {
            string[] dataKombinasi01 = new string[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                dataKombinasi01[i] = i.ToString();
                ///result5[i]= new double[1];
            }
            int countKombinasi = MakeCombinationRaw(dataKombinasi01, countcluster).ToList().Count();
            double[][] dataKombinasi02 = new double[dataRaw.Length][];
            double[][] result = new double[dataRaw.Length][]; //array kombinasi
            double[][] result2 = new double[dataRaw.Length][];//hasil SSE
            double[][] result3 = new double[dataRaw.Length][];//array kombinasi dan hasil SSE
            double[][] result4 = new double[1][];//array kombinasi dan hasil SSE
            //double[][] result6 = new double[1][];//array kombinasi dan hasil SSE
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                dataKombinasi02[i] = new double[countcluster];
                result[i] = new double[countcluster];
                result2[i] = new double[1];
                result3[i] = new double[countcluster + 1];

            }
            result4[0] = new double[countcluster + 1];
            //==
            double[] resultInt = new double[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                resultInt[i] = 0;
            }
            int[] intNumbers = new int[dataRaw.Length];
            int maxNumber = countKombinasi;
            Random intRandom = new Random();
            for (int intCounter = 0; intCounter <= intNumbers.GetUpperBound(0); intCounter++)
            {
                intNumbers[intCounter] = intRandom.Next(0, maxNumber - 1);
                //result5[intCounter][0]= intNumbers[intCounter];
                resultInt[intCounter] = intNumbers[intCounter];
            }
            //==
            int a1 = 0;
            foreach (IEnumerable<string> data1 in MakeCombinationRaw(dataKombinasi01, countcluster))
            {
                for (int k = 0; k < data1.ToArray().Length; ++k)
                {
                    for (int i = 0; i < resultInt.Length; ++i)
                    {
                        // dataKombinasi01[i] = i.ToString();
                        if (a1 == resultInt[i])
                        {
                            dataKombinasi02[i][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                            result[i][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                            result2[i][0] = 0.0;
                            result3[i][k] = Convert.ToDouble(data1.ToArray()[k].ToString());
                        }
                    }
                }
                ++a1;
            }

            double[][] meanTemporer = new double[countcluster][];
            for (int k = 0; k < countcluster; ++k)
            {
                meanTemporer[k] = new double[dataRaw[0].Length];
            }

            double[] jarakSSEMin = new double[dataKombinasi02.Length];
            //str1 += Environment.NewLine + "No. | CENTROID | SSE ";
            for (int i = 0; i < dataKombinasi02.Length; ++i)
            {
                string str2 = "";
                for (int j = 0; j < dataKombinasi02[i].Length; ++j)
                {
                    str2 += dataKombinasi02[i][j].ToString() + " | ";
                    int x1 = (int)dataKombinasi02[i][j];
                    meanTemporer[j] = dataRaw[x1];

                }
                double[] jarakTemporer = new double[meanTemporer.Length];
                double sumSSE = 0.0;
                for (int l = 0; l < dataRaw.Length; ++l)
                {
                    for (int m = 0; m < meanTemporer.Length; ++m)
                    {
                        //manual hitung jarak
                        double[] tuple = dataRaw[l];
                        double[] mean = meanTemporer[m];
                        double sumSquaredDiffs = 0.0;
                        for (int n = 0; n < tuple.Length; ++n)
                        {
                            sumSquaredDiffs += Math.Pow((tuple[n] - mean[n]), 2);
                        }
                        jarakTemporer[m] = sumSquaredDiffs;
                    }
                    //manual hitung MinIndex
                    double smallDist = jarakTemporer[0];
                    for (int m = 0; m < jarakTemporer.Length; ++m)
                    {
                        if (jarakTemporer[m] < smallDist)
                        {
                            smallDist = jarakTemporer[m];
                        }
                    }
                    sumSSE += smallDist;
                    jarakSSEMin[i] = sumSSE;
                    result2[i][0] = jarakSSEMin[i];
                    result3[i][countcluster] = jarakSSEMin[i];
                }
            }
            double smallDist2 = jarakSSEMin[0];
            int indexOfSSE = 0;
            for (int m = 0; m < jarakSSEMin.Length; ++m)
            {
                if (jarakSSEMin[m] < smallDist2)
                {
                    smallDist2 = jarakSSEMin[m];
                    indexOfSSE = m;
                }
            }
            result4[0][countcluster] = smallDist2; ;
            string str3 = "";
            double[][] meanInisial = new double[countcluster][];
            for (int j = 0; j < dataKombinasi02[indexOfSSE].Length; ++j)
            {
                result4[0][j] = dataKombinasi02[indexOfSSE][j];
                str3 += dataKombinasi02[indexOfSSE][j].ToString() + " | ";
                int x1 = (int)dataKombinasi02[indexOfSSE][j];
                meanInisial[j] = dataRaw[x1];
            }

            if (options == 1)
            {
                return result;
            }
            else if (options == 2)
            {
                return result2;
            }
            else if (options == 3)
            {
                return result3;
            }
            else if (options == 4)
            {
                return result4;
            }
            else
            {
                return meanInisial;
            }
        }
        private static double[][] MakeZscore1(double[][] dataRaw)
        {
            double[][] result = new double[dataRaw.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[dataRaw[0].Length]; ;
            }
            for (int j = 0; j < dataRaw[0].Length; ++j) // each col
            {
                double colSum = 0.0;
                for (int i = 0; i < dataRaw.Length; ++i)
                {
                    colSum += dataRaw[i][j];
                }
                double mean = colSum / dataRaw.Length;
                double sum = 0.0;
                for (int i = 0; i < dataRaw.Length; ++i)
                {
                    sum += (dataRaw[i][j] - mean) * (dataRaw[i][j] - mean);
                }
                double sd = sum / (dataRaw.Length - 1);
                sd = Math.Sqrt(sd);
                for (int i = 0; i < dataRaw.Length; ++i)
                {
                    result[i][j] = Math.Round(((dataRaw[i][j] - mean) / sd),4);
                }
            }
            return result;
        }
        private static double[][] MakeKernelGRBF1(double[][] dataRaw, bool isKernel)
        {
            // double[][] dataKernel = MakeKernelGRBF1(dataNormalized, isKernel);
            //MakeTableDouble("No.", "Col", dataKernel, "");
            // normalize raw data by computing (x - mean) / stddev
            // primary alternative is min-max:
            // v' = (v - min) / (max - min)
            double[][] result = new double[dataRaw.Length][];
            if (isKernel != true)
            {
                Array.Copy(dataRaw, result, dataRaw.Length);
            }
            else
            {
                try
                {
                    for (int i = 0; i < dataRaw.Length; ++i)
                    {
                        result[i] = new double[dataRaw.Length];
                    }

                    double[] jarak2 = new double[dataRaw.Length];
                    for (int i = 0; i < dataRaw.Length; ++i)
                    { // walk thru each tuple
                        for (int k = 0; k < dataRaw.Length; ++k)
                        {
                            //manual hitung jarak
                            double[] tuple = dataRaw[i];
                            double[] mean = dataRaw[k];
                            double sumSquaredDiffs = 0.0;
                            for (int j = 0; j < tuple.Length; ++j)
                            {
                                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
                            }
                            jarak2[k] = Math.Exp(-(sumSquaredDiffs) / (2 * Math.Pow(1.5, 2)));
                            result[i][k] = Math.Round(jarak2[k],4);

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return result;
                }
            }
            return result;
        }
        private static string[] ConvertDoubleToArrayString(double[] dataRaw)
        {
            string[] result = new string[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                result[i] = dataRaw[i].ToString();
            }
            return result;
        }
        public void MakeHeader1(string strA, string col, string[] data, string strB)
        {
            var sb = new System.Text.StringBuilder();
            if (strA.Length > 0)
            {
                sb.Append(strA + " | ");
            }
            for (int j = 0; j < data.Length; ++j)
            {
                sb.Append(" " + col + +j + " | ");
            }
            if (strB.Length > 0)
            {
                sb.Append(" " + strB + " | ");
            }
            str1 += Environment.NewLine + sb;// + Environment.NewLine;
        }
        public void MakeHeader1Statistic(string strA, string col, string[] data, string strB)
        {
            var sb = new System.Text.StringBuilder();
            if (strA.Length > 0)
            {
                sb.Append(strA + " | ");
            }
            for (int j = 0; j < data.Length; ++j)
            {
                sb.Append(" " + col + +j + " | ");
            }
            if (strB.Length > 0)
            {
                sb.Append(" " + strB + " | ");
            }
            strStatistic += Environment.NewLine + sb;// + Environment.NewLine;
        }
        public void MakeTableDouble(string strA, string col, double[][] data, string strB)
        {
            MakeHeader1(strA, col, ConvertDoubleToArrayString(data[0]), strB);
            str1 += Environment.NewLine;
            for (int i = 0; i < data.Length; ++i)
            {
                str1 += "[" + i.ToString().PadLeft(0) + "] ";
                for (int j = 0; j < data[i].Length; ++j)
                {
                    str1 += data[i][j].ToString() + " | ";
                }
                str1 += Environment.NewLine + "";
            }
        }


        private double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception e)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        //throw new Exception("Wrong string-to-double format");
                        double OutVal;
                        double.TryParse(s, out OutVal);

                        if (double.IsNaN(OutVal) || double.IsInfinity(OutVal))
                        {
                            result = 0;
                        }
                        else
                        {
                            result = OutVal;
                        }
                        return result;
                    }
                }
            }
            return result;
        }

        private void CboCountCluster_SelectedIndexChanged(object sender, EventArgs e)
        {
            //label9.Text = cboCountCluster.SelectedItem.ToString();
        }

        private void gb2_Enter(object sender, EventArgs e)
        {

        }

        private void DgvRawData_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new System.Drawing.Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void DgvRawData_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DgvResultData_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new System.Drawing.Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void BtnElbow_Click(object sender, EventArgs e)
        {
            //=>cek jumlah data tidak boleh kosong
            if (DgvRawData.Rows.Count == 0 || DgvRawData.Columns.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk di proses" + Environment.NewLine + "silahkan import atau tambah data!", "Attention");
                return;
            }
            //=>cek data kosong
            try
            {
                for (int i = 0; i < DgvRawData.Rows.Count - 1; i++)
                {
                    for (int j = 0; j < DgvRawData.Rows[i].Cells.Count; j++)
                    {
                        string strValue = DgvRawData.Rows[i].Cells[j].Value.ToString();
                        double OutVal = ConvertToDouble(strValue);
                        if (strValue == null && strValue.Length == 0 && double.IsNaN(OutVal) && double.IsInfinity(OutVal))
                        {
                            MessageBox.Show("There is empty or null data or text" + Environment.NewLine + "please check and adjust!", "Attention");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Empty data check error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                return;
            }
            double[][] dataRaw = new double[DgvRawData.Rows.Count - 1][];
            //=>konversi data dari datagrid ke array dengan type double
            try
            {
                for (int i = 0; i < DgvRawData.Rows.Count - 1; ++i)
                {
                    dataRaw[i] = new double[DgvRawData.Rows[0].Cells.Count];
                }
                for (int i = 0; i < DgvRawData.Rows.Count - 1; i++)
                {
                    for (int j = 0; j < DgvRawData.Rows[i].Cells.Count; j++)
                    {
                        double OutVal = ConvertToDouble(DgvRawData.Rows[i].Cells[j].Value.ToString());
                        dataRaw[i][j] = OutVal;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data conversion error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                return;
            }
            dt0.Clear();
            dt0.Rows.Clear();
            dt0.Columns.Clear();
            DataRow row;
            int rowCount = dataRaw.Length;
            int colCount = dataRaw[0].Length;
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    dt0.Columns.Add("X" + (j + 1));
                }
                break;
            }
            int rowCounter;
            for (int i = 0; i < dataRaw.Length; i++)
            {
                row = dt0.NewRow();
                rowCounter = 0;
                for (int j = 0; j < dataRaw[i].Length; j++)
                {
                    row[rowCounter] = dataRaw[i][j].ToString();
                    rowCounter++;
                }
                dt0.Rows.Add(row);
            }
            FElbow f2 = new FElbow();
            f2.ShowDialog();
        }
        public static System.Data.DataTable dt0 = new System.Data.DataTable();

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CboCountIteration_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void CbZScore_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CbKernelGRBF_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RbCentroidRandom_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RbCentroidRandomCombination_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CbEvaluationDBI_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CbEvaluationSC_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

}
