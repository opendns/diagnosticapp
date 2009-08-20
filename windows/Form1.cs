using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using LitJson;
using ProgressControls;

namespace OpenDnsDiagnostic
{
    public partial class Form1 : Form
    {
        public static string APP_VER = "1.0.5";
        //public static string REPORT_SUBMIT_URL = "http://127.0.0.1/diagnosticsubmit";
        public static string REPORT_SUBMIT_URL = "http://opendnsupdate.appspot.com/diagnosticsubmit";
        public static string AUTO_UPDATE_BASE_URL = "https://www.opendns.com/upgrade/windows/diagnostic/";
        List<TestStatus> Tests;
        LinkLabel SeeResultsLabel;
        Label FinishedCountLabel;
        string ResultsFileName;
        string ResultsUrl;
        string AutoDetectedUserName;

        delegate void AutoUpgradeCheckDelegate();
        public Form1()
        {
            InitializeComponent();
            this.Icon = new Icon(GetType(), "Icon1.ico");
            this.Text = "OpenDNS Diagnostic v" + APP_VER;
            this.textBoxDomain.KeyDown += new KeyEventHandler(textBox_OnKeyDownHandler);
            this.textBoxTicket.KeyDown += new KeyEventHandler(textBox_OnKeyDownHandler);
            this.textBoxUserName.KeyDown += new KeyEventHandler(textBox_OnKeyDownHandler);
            AutoDetectOpenDnsUserName();
            FillAutoDetectedUserName();

            var myDelegate = new AutoUpgradeCheckDelegate(AutoUpgradeCheckBackground);
            myDelegate.BeginInvoke(null, null);
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
            FillAutoDetectedUserName();
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
                    var ticket = textBoxTicket.Text;
                    ticket.Trim();
                    if (!string.IsNullOrEmpty(ticket))
                    {
                        sw.WriteLine("OpenDNS ticket: " + ticket);
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
                int x = this.labelAccount.Location.X;
                int maxLineDx = this.Size.Width - 2 * x;
                SeeResultsLabel.Text = "See results at " + ResultsUrl;
                Size preferredSize = SeeResultsLabel.GetPreferredSize(new Size(maxLineDx, 13));
                SeeResultsLabel.Size = preferredSize;
            }
            catch
            {
            }
        }

        private string ImportUserNameFromNewClient()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = System.IO.Path.Combine(path, "OpenDNS Updater");
            path = System.IO.Path.Combine(path, "settings.dat");
            if (!File.Exists(path))
                return null;

            //string json = Encoding.UTF8.GetString(response, 0, response.Length);
            try
            {
                using (var sr = new StreamReader(path))
                {
                    string jsonTxt = sr.ReadToEnd();
                    JsonData json = JsonMapper.ToObject(jsonTxt);
                    JsonData nameObject = json["user_name"];
                    string name = nameObject.ToString();
                    return name;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string ImportUserNameFromOldClient()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            path = System.IO.Path.Combine(path, "OpenDNS Updater");
            path = System.IO.Path.Combine(path, "settings.ini");
            if (!File.Exists(path))
                return null;

            try
            {
                using (var sr = new StreamReader(path))
                {
                    while (true)
                    {
                        string l = sr.ReadLine();
                        if (null == l)
                            return null;
                        if (l.StartsWith("username"))
                        {
                            var parts = l.Split(new char[] { '=' });
                            if (parts.Length == 2)
                            {
                                string name = parts[1];
                                name = name.Trim();
                                return name;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        private void AutoDetectOpenDnsUserName()
        {
            AutoDetectedUserName = ImportUserNameFromNewClient();
            if (AutoDetectedUserName == null)
                AutoDetectedUserName = ImportUserNameFromOldClient();
        }

        private void FillAutoDetectedUserName()
        {
            if (AutoDetectedUserName != null)
            {
                this.textBoxUserName.Text = AutoDetectedUserName;
                this.textBoxTicket.Select();
            }
            else
            {
                this.textBoxUserName.Focus();
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

            FinishedCountLabel.Text = String.Format("Finished {0} out of {1} tests.", finishedCount, Tests.Count);
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
            this.textBoxTicket.Enabled = true;
            this.textBoxDomain.Enabled = true;
        }

        private void UiDisable()
        {
            this.buttonRunTests.Enabled = false;
            this.textBoxUserName.Enabled = false;
            this.textBoxTicket.Enabled = false;
            this.textBoxDomain.Enabled = false;
        }

        private void LayoutProcessesInfo()
        {
            int x = this.labelAccount.Location.X;
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
            // a weak test for a valid hostname
            if (hostname.Contains("."))
            {
                Uri host;
                if (Uri.TryCreate(hostname, UriKind.Absolute, out host))
                    hostname = host.DnsSafeHost;
                Tests.Add(new ProcessStatus("tracert", hostname));

            }

            Tests.Add(new ProcessStatus("tracert", "208.67.222.222"));
            Tests.Add(new ProcessStatus("tracert", "208.67.220.220"));
            Tests.Add(new ProcessStatus("nslookup", "myip.opendns.com."));
            Tests.Add(new ProcessStatus("nslookup", "-type=txt which.opendns.com. 208.67.222.222"));
            Tests.Add(new ProcessStatus("nslookup", "-type=txt -port=5353 which.opendns.com. 208.67.222.222"));
            Tests.Add(new ProcessStatus("nslookup", "-class=chaos -type=txt hostname.bind. 4.2.2.1"));
            Tests.Add(new ProcessStatus("nslookup", "-class=chaos -type=txt hostname.bind. 192.33.4.12"));
            Tests.Add(new ProcessStatus("nslookup", "-class=chaos -type=txt hostname.bind. 204.61.216.4"));
            Tests.Add(new ProcessStatus("nslookup", "whoami.ultradns.net udns1.ultradns.net"));
            Tests.Add(new ProcessStatus("nslookup", "-debug debug.opendns.com. 208.67.222.222"));
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

        struct UpgradeJsonResponse
        {
            public bool upgrade;
            public bool force;
            public string version;
            public string download;
        }

        static public void LaunchBrowser(string url)
        {
            Process.Start(url);
        }

        public void AutoUpgradeCheckBackground()
        {
            string autoUpdateArgs = "?version=" + APP_VER;
            var autoUpdateUrl = AUTO_UPDATE_BASE_URL + autoUpdateArgs;
            UpgradeJsonResponse json;
            try
            {
                WebRequest request = WebRequest.Create(autoUpdateUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responsedata = response.GetResponseStream();
                StreamReader responsereader = new StreamReader(responsedata);
                string responseStr = responsereader.ReadToEnd();
                json = JsonMapper.ToObject<UpgradeJsonResponse>(responseStr);
            }
            catch
            {
                // it's ok if we fail
                return;
            }
            if (!json.upgrade)
                return;
            ShowUpgradeAvailableThreadSafe(json.force, json.version, json.download);
        }

        delegate void ShowUpgradeAvailableDelegate(bool force, string version, string downloadUrl);
        private void ShowUpgradeAvailableThreadSafe(bool force, string version, string downloadUrl)
        {
            if (this.InvokeRequired)
            {
                var myDelegate = new ShowUpgradeAvailableDelegate(ShowUpgradeAvailableThreadSafe);
                this.BeginInvoke(myDelegate, new object[] { force, version, downloadUrl });
                return;
            }
            string s;
            if (force)
            {
                s = String.Format("You need to update to version {0}.", version);
                MessageBox.Show(s);
                LaunchBrowser(downloadUrl);
                Application.Exit();
            }
            s = String.Format("New version {0} is available. Do you want to update?", version);
            var res = MessageBox.Show(s, "Update available", MessageBoxButtons.YesNo);
            if (DialogResult.Yes == res)
                LaunchBrowser(downloadUrl);
        }

    }
}
