using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsTesis1
{
    public partial class FElbow : Form
    {
        //BackgroundWorker bgWorker;
        private Series _kMeansSeries;
        private BindingList<DPoint> _kmeansPointList;

        public FElbow()
        {
            InitializeComponent();
            dynamicRichTextBox.SelectionFont = new Font(dynamicRichTextBox.SelectionFont.FontFamily, 14.0F);
        }
        public string str1 = "";
        private void FElbow_Load(object sender, EventArgs e)
        {
            _kmeansPointList = new BindingList<DPoint>();
            dataGridView1.DataSource = FMain.dt0;
            cboCountIteration.Items.Clear();
            for (int i = 1; i <= 20; i++)
            {
                if ((i * 50) % 2 == 0)
                {
                    cboCountIteration.Items.Add((i * 50).ToString());
                }
            }
            cboCountClusterFrom.Items.Clear();
            cboCountClusterTo.Items.Clear();
            cboDataCount.Items.Clear();
            for (int i = 1; i <= dataGridView1.Rows.Count; i++)
            {
                cboDataCount.Items.Add((i).ToString());
                cboCountClusterFrom.Items.Add(i.ToString());
                cboCountClusterTo.Items.Add(i.ToString());
            }
            cboDataCount.SelectedIndex = 0;
            cboCountClusterFrom.SelectedIndex = 0;
            cboCountClusterTo.SelectedIndex = 0;

            ////btnProcess.Enabled = false;
            cbKernelGRBF.Checked = false;
            SetDefault();
        }
        private void SetDefault()
        {
            cboDataCount.SelectedIndex = 0;
            cboCountClusterFrom.SelectedIndex = 0;
            cboCountClusterTo.SelectedIndex = 0;
            cboCountIteration.SelectedIndex = 0;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {

        }
        private void Column1_KeyPress2(object sender, KeyPressEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }


        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                str1 = "";
                dynamicRichTextBox.Text = "";
                if (dataGridView1.Rows.Count == 0 || dataGridView1.Columns.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk di proses" + Environment.NewLine + "silahkan import atau tambah data!", "Attention");
                    return;
                }
                try
                {
                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++)
                        {
                            string strValue = dataGridView1.Rows[i].Cells[j].Value.ToString();
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
                int dataCount = Convert.ToInt32(cboDataCount.SelectedItem.ToString());
                int countClusterFrom = Convert.ToInt32(cboCountClusterFrom.SelectedItem.ToString());
                int countClusterTo = Convert.ToInt32(cboCountClusterTo.SelectedItem.ToString());
                int maxIteration = Convert.ToInt32(cboCountIteration.SelectedItem.ToString());
                bool isKernel = (cbKernelGRBF.Checked == true) ? true : false;
                try
                {
                    if (cboDataCount.Items.Count > 0 && cboCountClusterFrom.Items.Count > 0 && cboCountClusterTo.Items.Count > 0)
                    {
                        if (dataCount == 0 || countClusterFrom > dataCount)
                        {
                            cboCountClusterFrom.SelectedIndex = 0;
                            cboCountClusterTo.SelectedIndex = 0;
                            MessageBox.Show("Count data is not allowed 0 !" + Environment.NewLine + "or Count Cluster From cannot be > Data Count", "Attention");
                            return;
                        }
                        else if (countClusterTo > countClusterFrom && countClusterTo > dataCount)
                        {
                            cboCountClusterTo.SelectedIndex = cboDataCount.SelectedIndex;
                            MessageBox.Show("Count Cluster to cannot be > Data Count !", "Attention");
                            return;
                        }
                        else if (countClusterTo < countClusterFrom)
                        {
                            cboCountClusterTo.SelectedIndex = cboCountClusterFrom.SelectedIndex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                double[][] dataRaw = new double[dataCount][];
                try
                {
                    for (int i = 0; i < dataCount; ++i)
                    {
                        dataRaw[i] = new double[dataGridView1.Rows[0].Cells.Count];
                    }
                    for (int i = 0; i < dataCount; i++)
                    {
                        for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++)
                        {
                            double OutVal = ConvertToDouble(dataGridView1.Rows[i].Cells[j].Value.ToString());
                            dataRaw[i][j] = OutVal;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Data conversion error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                    return;
                }


                MakeTableDouble("No.", "Col", dataRaw, "");
                str1 += Environment.NewLine;
                str1 += Environment.NewLine + "//======================================//";
                str1 += Environment.NewLine + "*PROCESS OF DATA SETTING*";
                str1 += Environment.NewLine + "Data count : " + dataCount;
                str1 += Environment.NewLine + "Cluster count from : " + countClusterFrom + " to " + countClusterTo;
                str1 += Environment.NewLine + "Max. iteration : " + maxIteration;
                str1 += Environment.NewLine + "Z-Score : true";
                str1 += Environment.NewLine + "Kernel GRBF : " + isKernel;
                str1 += Environment.NewLine + "Centroid type : Random";
                str1 += Environment.NewLine;

                double[][] dataNormalized = new double[dataRaw.Length][];
                Array.Copy(dataRaw, dataNormalized, dataRaw.Length);
                try
                {
                    str1 += Environment.NewLine + "//======================================//";
                    str1 += Environment.NewLine + "*PROCESS OF DATA NORMALIZATION*";
                    str1 += Environment.NewLine + "Z-Score: true";
                    dataNormalized = MakeZscore1(dataRaw);
                    str1 += Environment.NewLine + "Z-Score: success";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Z-Score error: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                    return;
                }
                double[][] dataKernel = new double[dataNormalized.Length][];
                Array.Copy(dataNormalized, dataKernel, dataNormalized.Length);
                if (isKernel == true)
                {
                    try
                    {
                        str1 += Environment.NewLine + "//======================================//";
                        str1 += Environment.NewLine + "*PROCESS OF DATA TRANSFORMATION*";
                        str1 += Environment.NewLine + "Kernel GRBF : " + isKernel;
                        dataKernel = MakeKernelGRBF1(dataNormalized, isKernel);
                        str1 += Environment.NewLine + "Kernel GRBF : success";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error Kernel GRBF: Please check your data!" + Environment.NewLine + ex.Message, "Attention");
                        return;
                    }
                }

                int countcluster = 0;
                int lengthcluster = countClusterTo - countClusterFrom + 1;
                double[] dataWCSS = new double[lengthcluster];
                int count1 = 0;
                for (int count0 = countClusterFrom; count0 <= countClusterTo; ++count0)
                {
                    countcluster = count0;
                           str1 += Environment.NewLine + "** PROCESS K = " + count0 + " **";


                    double[][] meanInisial = new double[countcluster][];
                    for (int k = 0; k < countcluster; ++k)
                    {
                        meanInisial[k] = new double[dataKernel[0].Length];
                    }
                    try
                    {
                        meanInisial = MakeRandomCentroid1(dataKernel, countcluster);

                        double[][] resultKombinasi1 = new double[1][];
                        resultKombinasi1[0] = new double[countcluster];
                        for (int j = 0; j < (dataKernel.Length); ++j)
                        {
                            for (int l = 0; l < (meanInisial.Length); ++l)
                            {
                                int a1 = 0;
                                for (int k = 0; k < (dataKernel[j].Length); ++k)
                                {
                                    if (dataKernel[j][k] == meanInisial[l][k])
                                    {
                                        a1++;
                                        if (a1 == countcluster)
                                        {
                                            resultKombinasi1[0][l] = j;
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ////btnProcess.Enabled = (dataGridView1.Rows.Count > 0) ? true : false;
                        MessageBox.Show("Error Searching Random Mode Centroid" + Environment.NewLine + ex.Message, "Attention");
                        return;
                    }
                    double[][] meanFix = new double[countcluster][];
                    for (int k = 0; k < countcluster; ++k)
                    {
                        meanFix[k] = meanInisial[k];
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
                            //manual hitung jarak
                            double[] tuple = dataKernel[i];
                            double[] mean = meanInisial[k];
                            double sumSquaredDiffs = 0.0;
                            for (int j = 0; j < tuple.Length; ++j)
                            {
                                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
                            }
                            jarak[k] = Math.Sqrt(sumSquaredDiffs);
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
                        if (newclusterID8 != clusterInisial[i])
                        {
                            clusterInisial[i] = newclusterID8;
                        }

                        int cluster = clusterInisial[i];
                        ++countclustersInisial[cluster];
                    }
                    int Iteration = 0;

                    int[] clusterFix = new int[clusterInisial.Length]; // proposed result
                    Array.Copy(clusterInisial, clusterFix, clusterInisial.Length);
                    bool updatedcluster = true;
                    bool updatedMean = true;
                    while (updatedcluster == true && updatedMean == true && Iteration < maxIteration)
                    {
                        ++Iteration;
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
                        for (int k = 0; k < meanUpdate.Length; ++k)
                        {
                            if (countclustersInisial2[k] == 0)
                            {
                                updatedMean = false; // bad clustering4. no change to means7[][]
                                 break;
                            }
                        }
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
                        updatedcluster = false;

                        //--string version
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
                        //double[] jarakWCSS1 = new double[countcluster];

                        //untk mencari WCSS
                        double[] jarakWCSS = new double[meanUpdate.Length];
                        //string[] clusternewStrWCSS = new string[clusterInisial.Length]; // proposed result
                        double[][] jarakStrWCSS = new double[dataRaw.Length][];
                        for (int i = 0; i < dataRaw.Length; ++i)
                        {
                            jarakStrWCSS[i] = new double[countcluster];
                        }
                        for (int i = 0; i < dataKernel.Length; ++i)
                        {
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

                                jarakWCSS[k] = sumSquaredDiffs;
                                jarakStrWCSS[i][k] = jarakWCSS[k];

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
                        }

                        clusterFix = clusternew;
            
                        for (int k = 0; k < meanUpdate.Length; ++k)
                        {
                            if (countclustersUpdate[k] == 0)
                            {
                                updatedcluster = false;
                                break;
                            }
                        }
                        //clusterFix = clusternew;
                        if (updatedcluster == false)
                        {
                            updatedcluster = false;
                            double[] jarakWCSSsum = new double[countcluster];
                            double jarakWCSSresult = 0;
                            for (int k = 0; k < countcluster; ++k)
                            {
                                for (int i = 0; i < jarakStrWCSS.Length; ++i)
                                {
                                    for (int j = 0; j < jarakStrWCSS[i].Length; ++j)
                                    {
                                        if (j == k)
                                        {
                                            if (clusternew[i] == k)
                                            {
                                                jarakWCSSsum[k] += jarakStrWCSS[i][j];
                                            }
                                        }
                                    }
                                }
                                jarakWCSSresult += jarakWCSSsum[k];
                            }
                            str1 += Environment.NewLine + "Hasil SSE : " + jarakWCSSresult;
                            dataWCSS[count1] = jarakWCSSresult;
                            str1 += Environment.NewLine + "-----------------------";
                            break;
                        }
                        else
                        {
                            Array.Copy(clusternew, clusterInisial, clusternew.Length); // update
                            updatedcluster = true; 
                        }
                    }
                    ++count1;
                }
                str1 += Environment.NewLine + ">>======================================<<";
                str1 += Environment.NewLine + "All Result SSE";
                str1 += Environment.NewLine + "======================================";
                int count2 = countClusterFrom;

                _kmeansPointList.Clear();
                for (int j = 0; j < count1; ++j)
                {
                    str1 += Environment.NewLine + "[" + count2 + "] " + dataWCSS[j];
                    _kmeansPointList.Add(new DPoint(count2, dataWCSS[j]));
                    count2++;
                }

                _kMeansSeries = new Series("KMeans");
                _kMeansSeries.ChartType = SeriesChartType.Line;

                // chart2.Series.Points.Clear();
                chart2.Series.Clear();
                chart2.Series.Add(_kMeansSeries);
                _kMeansSeries.XValueMember = "X";
                _kMeansSeries.YValueMembers = "Y";
                chart2.DataSource = _kmeansPointList;
                chart2.DataBind();
                dynamicRichTextBox.Text = str1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private static double[][] MakeKernelGRBF1(double[][] dataRaw, bool isKernel)
        {
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
                            result[i][k] = jarak2[k];

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
        public void MakeTableDouble1(string strA, string strB, double[] data)
        {
            MakeHeader2(strA, strB);
            str1 += Environment.NewLine;
            for (int i = 0; i < data.Length; ++i)
            {
                str1 += i.ToString().PadLeft(3) + " | " + data[i].ToString() + " | ";
                // }
                str1 += Environment.NewLine + "";
            }
        } // ShowData
        private static double[][] MakeRandomCentroid1(double[][] dataRaw, int countcluster)
        {
            double[][] means = new double[countcluster][];
            List<int> uniqueNoList = new List<int>();
            uniqueNoList = UniqueRandomNoList(dataRaw.Length, countcluster);
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
        public void MakeTableClusterString2(string strA, string col, double[][] data, string[] data2, string strB)
        {
            MakeHeader3(strA, col, data[0], strB);
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
        public void MakeHeader3(string strA, string col, double[] data, string strB)
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
        private static double[][] MakeCombinationOptions2(double[][] dataRaw, int countcluster, int LengthOfData)
        {
            //int LengthOfData = 10; //dataRaw.Length;
            string[] dataKombinasi01 = new string[dataRaw.Length];
            for (int i = 0; i < dataRaw.Length; ++i)
            {
                dataKombinasi01[i] = i.ToString();
            }
            int countKombinasi = MakeCombinationRaw(dataKombinasi01, countcluster).ToList().Count();
            //==
            //double[] resultInt = new double[dataRaw.Length];
            double[] resultInt = new double[LengthOfData];
            for (int i = 0; i < LengthOfData; ++i)
            {
                resultInt[i] = 0;
            }
            //int[] intNumbers = new int[dataRaw.Length];
            int[] intNumbers = new int[LengthOfData];
            int maxNumber = countKombinasi;
            Random intRandom = new Random();
            for (int intCounter = 0; intCounter <= intNumbers.GetUpperBound(0); intCounter++)
            {
                intNumbers[intCounter] = intRandom.Next(0, maxNumber - 1);
                resultInt[intCounter] = intNumbers[intCounter];
            }
            //==
            double[][] dataKombinasi02 = new double[LengthOfData][];
            double[][] result1 = new double[LengthOfData][];
            //double[][] dataKombinasi02 = new double[10][];
            //double[][] result1 = new double[10][];//array kombinasi dan hasil SSE  

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
                    result[i][j] = (dataRaw[i][j] - mean) / sd;
                    // result[i][j] = dataRaw[i][j];
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

        private void cboCountCluster_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cboDataCount_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cboCountClusterFrom_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cboCountClusterTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboDataCount.Items.Count > 0 && cboCountClusterFrom.Items.Count > 0 && cboCountClusterTo.Items.Count > 0)
                {
                    int dataCount = Convert.ToInt32(cboDataCount.SelectedItem.ToString());
                    int countClusterFrom = Convert.ToInt32(cboCountClusterFrom.SelectedItem.ToString());
                    int countClusterTo = Convert.ToInt32(cboCountClusterTo.SelectedItem.ToString());
                    if (dataCount == 0 || countClusterFrom > dataCount)
                    {
                        cboCountClusterFrom.SelectedIndex = 0;
                        cboCountClusterTo.SelectedIndex = 0;
                    }
                    else if (countClusterTo > countClusterFrom && countClusterTo > dataCount)
                    {
                        cboCountClusterTo.SelectedIndex = cboDataCount.SelectedIndex;
                    }
                    else if (countClusterTo < countClusterFrom)
                    {
                        cboCountClusterTo.SelectedIndex = cboCountClusterFrom.SelectedIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

}
