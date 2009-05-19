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
        //public static string REPORT_SUBMIT_URL = "http://127.0.0.1/diagnosticsubmit";
        public static string REPORT_SUBMIT_URL = "http://opendnsupdate.appspot.com/diagnosticsubmit";
        public static string APP_VER = "0.3";
        List<TestStatus> Tests;
        LinkLabel SeeResultsLabel;
        Label FinishedCountLabel;
        string ResultsFileName;
        string ResultsUrl;

        public Form1()
        {
            InitializeComponent();
            this.Text = "OpenDNS Diagnostic v" + APP_VER;
            this.textBoxDomain.KeyDown += new KeyEventHandler(textBox_OnKeyDownHandler);
            this.textBoxUserName.KeyDown += new KeyEventHandler(textBox_OnKeyDownHandler);
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                buttonRunTests_Click(null, null);
            }
        }

        private void CleanupAfterPreviousTests()
        {
            ResultsUrl = null;
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
            ResultsFileName = Path.GetTempFileName();
            using (FileStream fs = File.OpenWrite(ResultsFileName))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    var userName = textBoxUserName.Text;
                    userName.Trim();
                    if (!string.IsNullOrEmpty(userName))
                    {
                        sw.WriteLine("OpenDNS account: " + userName);
                    }
                    foreach (var test in Tests)
                    {
                        test.WriteResult(sw);
                    }
                }
            }
        }

        private void SubmitDiagnosticReport()
        {
            try
            {
                var wc = new WebClient();
                byte[] response = wc.UploadFile(REPORT_SUBMIT_URL, ResultsFileName);
                string resp = Encoding.UTF8.GetString(response, 0, response.Length);
                ResultsUrl = resp;
                int x = this.labelDomain.Location.X;
                int maxLineDx = this.Size.Width - 2 * x;
                SeeResultsLabel.Text = "See results at " + ResultsUrl;
                Size preferredSize = SeeResultsLabel.GetPreferredSize(new Size(maxLineDx, 13));
                SeeResultsLabel.Size = preferredSize;
            }
            catch
            {
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
            SubmitDiagnosticReport();
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
                MultiProcessStatus mps = test as MultiProcessStatus;
                if (mps != null)
                {
                    foreach (var tmp in mps.Processes)
                    {
                        if (tmp.Process == proc)
                        {
                            tmp.Stop();
                            if (mps.AllFinished())
                                mps.Stop();
                        }
                    }
                    continue;
                }
                ProcessStatus ps = test as ProcessStatus;
                if (null == ps)
                    continue;
                if (proc != ps.Process)
                    continue;
                ps.Stop();
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

        private void StartProcess(ProcessStatus ps, bool visible)
        {
            Process p = ps.Process;
            p.Exited += new EventHandler(process_Exited);
            if (visible)
                ps.ProgressIndicator.Start();
            try
            {
                ps.Start();
            }
            catch (Win32Exception)
            {
                ps.FailedToStart = true;
                ps.Finished = true;
            }
        }

        private void StartMultiPorcess(MultiProcessStatus mps)
        {
            foreach (var ps in mps.Processes)
                StartProcess(ps, false);
            mps.ProgressIndicator.Start();
        }

        private void StartTest(TestStatus test)
        {
            ProcessStatus ps = test as ProcessStatus;
            if (ps != null)
            {
                StartProcess(ps, true);
                return;
            }
            DnsResolveStatus rs = test as DnsResolveStatus;
            if (rs != null)
            {
                StartDnsResolve(rs);
                return;
            }
            MultiProcessStatus mps = test as MultiProcessStatus;
            if (mps != null)
            {
                StartMultiPorcess(mps);
                return;
            }
            Debug.Assert(false);

        }

        private void UiEnable()
        {
            this.buttonRunTests.Enabled = true;
            this.textBoxUserName.Enabled = true;
            this.textBoxDomain.Enabled = true;
        }

        private void UiDisable()
        {
            this.buttonRunTests.Enabled = false;
            this.textBoxUserName.Enabled = false;
            this.textBoxDomain.Enabled = false;
        }

        private void LayoutProcessesInfo()
        {
            int x = this.labelUserName.Location.X;
            const int progressDx = 20;
            int y = labelDomainExample.Location.Y + labelDomainExample.Size.Height + 6;
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
            y += (preferredSize.Height + buttonExit.Size.Height + 30);
            this.Controls.Add(SeeResultsLabel);
            if (ClientSize.Height < y)
                ClientSize = new Size(this.ClientSize.Width, y);
        }

        private void LinkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (null != ResultsUrl)
                System.Diagnostics.Process.Start(ResultsUrl);
            else
                System.Diagnostics.Process.Start("notepad.exe", ResultsFileName);
        }

        private void RunAllTests()
        {
            CleanupAfterPreviousTests();
            string hostname = textBoxDomain.Text;
            Tests = new List<TestStatus>();
            Tests.Add(new DnsResolveStatus("myip.opendns.com"));
            if (hostname.Contains(".")) // a weak test for a valid hostname
                Tests.Add(new ProcessStatus("tracert", hostname));

            Tests.Add(new ProcessStatus("tracert", "208.67.222.222"));
            Tests.Add(new ProcessStatus("tracert", "208.67.220.220"));
            Tests.Add(new ProcessStatus("nslookup", "myip.opendns.com"));
            Tests.Add(new ProcessStatus("nslookup", "-type=txt which.opendns.com. 208.67.222.222"));
            Tests.Add(new ProcessStatus("nslookup", "-type=txt -port=5353 which.opendns.com. 208.67.222.222"));
            Tests.Add(new ProcessStatus("nslookup", "-class=chaos -type=txt hostname.bind. 4.2.2.1"));
            Tests.Add(new ProcessStatus("nslookup", "-class=chaos -type=txt hostname.bind. 192.33.4.12"));
            Tests.Add(new ProcessStatus("nslookup", "-class=chaos -type=txt hostname.bind. 204.61.216.4"));
            Tests.Add(new ProcessStatus("nslookup", "whoami.ultradns.net udns1.ultradns.net"));
            Tests.Add(new ProcessStatus("nslookup", "-debug debug.opendns.com"));
            var mps = new MultiProcessStatus("pings");
            Tests.Add(mps);
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.67.219.99", "(www.opendns.com)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.67.219.1", "(palo alto router)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.67.216.1", "(seattle router)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.69.36.1", "(chicago router)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.67.217.1", "(new york router)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.69.32.1", "(ashburn router)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 208.69.34.1", "(london router)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 209.244.5.114", "(level3 west coast)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 209.244.7.33", "(level3 east coast)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 192.153.156.3", "(att west coast)"));
            mps.Processes.Add(new ProcessStatus("ping", "-n 5 207.252.96.3", "(att east coast)"));
            Tests.Add(new ProcessStatus("ipconfig", "/all"));
            Tests.Add(new ProcessStatus("systeminfo", null, true));
            Tests.Add(new ProcessStatus("tasklist", null));
            LayoutProcessesInfo();
            foreach (var test in Tests)
                StartTest(test);

            UiDisable();
        }

        private void buttonRunTests_Click(object sender, EventArgs e)
        {
            RunAllTests();
        }
    }
}
