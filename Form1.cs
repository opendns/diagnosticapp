using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using ProgressControls;

namespace OpenDnsDiagnostic
{
    public partial class Form1 : Form
    {
        public static string APP_VER = "0.1";
        List<TestStatus> Tests;
        LinkLabel SeeResultsLabel;
        Label FinishedCountLabel;
        string ResultsFileName;

        public Form1()
        {
            InitializeComponent();
            this.Text = "OpenDNS Diagnostic v" + APP_VER;
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CleanupAfterPreviousTests()
        {
            this.Controls.Remove(FinishedCountLabel);
            this.Controls.Remove(SeeResultsLabel);
            if (null == Tests)
                return;
            foreach (var test in Tests)
            {
                var l = test.Label;
                if (l != null)
                    this.Controls.Remove(l);
                var pi = test.ProgressIndicator;
                if (pi != null)
                    this.Controls.Remove(pi);
            }
        }

        private void SaveResultsToTempFile()
        {
            using (FileStream fs = File.OpenWrite(ResultsFileName))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var test in Tests)
                    {
                        test.WriteResult(sw);
                    }
                }
            }
        }

        private void NotifyUiAllTestsFinished()
        {
            try
            {
                SaveResultsToTempFile();
            }
            catch (Exception)
            {
            }
            SeeResultsLabel.Visible = true;
            FinishedCountLabel.Visible = false;
            UiEnable();
        }

        private void NotifyUiTestFinished()
        {
            int finishedCount = 0;
            foreach (var test in Tests)
            {
                if (test.Finished)
                    finishedCount += 1;
            }

            //int maxLineDx = this.Size.Width;
            FinishedCountLabel.Text = String.Format("Finished {0} out of {1} tests.", finishedCount, Tests.Count);
            //Size preferredSize = FinishedCountLabel.GetPreferredSize(new Size(maxLineDx, 13));
            //FinishedCountLabel.Size = preferredSize;
            if (finishedCount == Tests.Count)
                NotifyUiAllTestsFinished();
        }

        delegate void process_ExitedDelegate(object sender, System.EventArgs e);
        public void process_Exited(object sender, System.EventArgs e)
        {
            if (this.InvokeRequired)
            {
                var myDelegate = new process_ExitedDelegate(process_Exited);
                this.BeginInvoke(myDelegate, new object[] { sender, e });
                return;
            }

            var proc = sender as Process;
            Debug.Assert(proc != null);
            foreach (var test in Tests)
            {
                ProcessStatus ps = test as ProcessStatus;
                if (null == ps)
                    continue;
                if (proc != ps.Process)
                    continue;
                test.Stop();

                ps.StdOut = proc.StandardOutput.ReadToEnd();
                ps.StdErr = proc.StandardError.ReadToEnd();
            }

            NotifyUiTestFinished();
        }
        delegate void DnsTestFinishedDelegate(DnsResolveStatus rs);
        public void DnsTestFinishedThreadSafe(DnsResolveStatus rs)
        {
            if (this.InvokeRequired)
            {
                var myDelegate = new DnsTestFinishedDelegate(DnsTestFinishedThreadSafe);
                this.BeginInvoke(myDelegate, new object[] { rs });
                return;
            }
            rs.Stop();
            NotifyUiTestFinished();
        }

        private void DnsCallback(IAsyncResult result)
        {
            DnsResolveStatus rs = result.AsyncState as DnsResolveStatus;
            try
            {
                IPAddress[] ips = Dns.EndGetHostAddresses(result);
                rs.IPAddresses = ips;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            DnsTestFinishedThreadSafe(rs);
        }

        private void StartDnsResolve(DnsResolveStatus rs)
        {
            Dns.BeginGetHostAddresses(rs.Hostname, new AsyncCallback(DnsCallback), rs);
        }

        private void StartProcess(ProcessStatus ps)
        {
            Process p = ps.Process;
            p.Exited += new EventHandler(process_Exited);
            try
            {
                p.Start();
                ps.ProgressIndicator.Start();
            }
            catch (Win32Exception)
            {
                ps.FailedToStart = true;
                ps.Finished = true;
            }
        }

        private void UiEnable()
        {
            this.buttonRunTests.Enabled = true;
            this.textBox1.Enabled = true;
        }

        private void UiDisable()
        {
            this.buttonRunTests.Enabled = false;
            this.textBox1.Enabled = false;
        }

        private void LayoutProcessesInfo()
        {
            int x = this.label1.Location.X;
            const int progressDx = 20;
            int y = this.textBox1.Location.Y + 30;
            Size preferredSize;
            int maxLineDx = this.Size.Width - 2 * x;
            foreach (var test in Tests)
            {
                var l = new Label();
                l.ForeColor = System.Drawing.Color.Black;
                l.AutoSize = true;
                l.Location = new System.Drawing.Point(x, y);
                l.Text = test.DisplayName;
                preferredSize = l.GetPreferredSize(new Size(maxLineDx, 13));
                l.Size = preferredSize;
                l.Show();
                test.Label = l;
                this.Controls.Add(l);
                int dy = preferredSize.Height;
                int dx = preferredSize.Width;

                var pi = new ProgressIndicator();
                pi.Location = new Point(x + dx + 4, y-3);
                pi.Size = new Size(progressDx, dy);
                test.ProgressIndicator = pi;
                this.Controls.Add(pi);
                pi.Start();

                y += (dy + 6);
            }
            FinishedCountLabel = new Label();
            FinishedCountLabel.Visible = true;
            FinishedCountLabel.Location = new Point(x, y);
            FinishedCountLabel.Text = String.Format("Finished 0 out {0} tests.", Tests.Count);
            FinishedCountLabel.Location = new Point(x, y);
            //preferredSize = FinishedCountLabel.GetPreferredSize(new Size(maxLineDx, 13));
            //FinishedCountLabel.Size = preferredSize;
            FinishedCountLabel.AutoSize = true;
            this.Controls.Add(FinishedCountLabel);

            SeeResultsLabel = new LinkLabel();
            SeeResultsLabel.Visible = false;
            SeeResultsLabel.Location = new Point(x, y);
            SeeResultsLabel.Text = "See results";
            preferredSize = SeeResultsLabel.GetPreferredSize(new Size(maxLineDx, 13));
            SeeResultsLabel.Size = preferredSize;
            SeeResultsLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_Clicked);
            this.Controls.Add(SeeResultsLabel);
        }

        private void LinkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", ResultsFileName);
        }

        private void runAllTests()
        {
            CleanupAfterPreviousTests();
            ResultsFileName = Path.GetTempFileName();
            Tests = new List<TestStatus>();
            Tests.Add(new DnsResolveStatus("myip.opendns.com"));
            Tests.Add(new ProcessStatus("tracert", "208.67.222.222"));
            Tests.Add(new ProcessStatus("tracert", "208.67.220.220"));
            Tests.Add(new ProcessStatus("nslookup", "myip.opendns.com"));
            Tests.Add(new ProcessStatus("nslookup", "-type=txt which.opendns.com. 208.67.222.222"));
            Tests.Add(new ProcessStatus("ipconfig", "/all"));
            LayoutProcessesInfo();
            foreach (var test in Tests)
            {
                ProcessStatus ps = test as ProcessStatus;
                if (ps != null)
                    StartProcess(ps);
                DnsResolveStatus rs = test as DnsResolveStatus;
                if (rs != null)
                    StartDnsResolve(rs);
            }
            UiDisable();
        }

        private void buttonRunTests_Click(object sender, EventArgs e)
        {
            runAllTests();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
