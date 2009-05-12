using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OpenDnsDiagnostic
{
    class ProcessStatus
    {
        private string _displayName;
        public string DisplayName {
            get {
                if (null != _displayName)
                    return _displayName;
                if (null == Args)
                    return Exe;
                return Exe + " " + Args;
            }
            set {
                _displayName = value;
            }
        }
        public string Exe;
        public string Args;
        public bool Finished;
        public bool FailedToStart;
        public Process Process;
        public string StdOut;
        public string StdErr;

        public ProcessStatus(string exe, string args)
        {
            Exe = exe;
            Debug.Assert(null != exe);
            Args = args;
            FailedToStart = false;
            Finished = false;
            _displayName = null;
            StdErr = null;
            StdOut = null;
            Process = null;
        }
    }

    public partial class Form1 : Form
    {
        List<ProcessStatus> Processes;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NotifyUiAllProcessesFinished()
        {
            UiEnable();
        }

        delegate void NotifyUiProcessFinishedDelegate();
        private void NotifyUiProcessFinished()
        {
            if (this.InvokeRequired)
            {
                var myDelegate = new NotifyUiProcessFinishedDelegate(NotifyUiProcessFinished);
                this.BeginInvoke(myDelegate);
                return;
            }

            foreach (var ps in Processes)
            {
                if (!ps.Finished)
                    return;
            }

            NotifyUiAllProcessesFinished();
        }

        private void process_Exited(object sender, System.EventArgs e)
        {
            var proc = sender as Process;
            Debug.Assert(proc != null);
            foreach (var ps in Processes) 
            {
                if (proc != ps.Process)
                    continue;

                ps.StdOut = proc.StandardOutput.ReadToEnd();
                ps.StdErr = proc.StandardError.ReadToEnd();
                ps.Finished = true;
                NotifyUiProcessFinished();
                return;
            }
            Debug.Assert(false);
        }

        private void StartProcess(ProcessStatus ps)
        {
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(process_Exited);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = ps.Exe;
            p.StartInfo.Arguments = ps.Args;
            ps.Process = p;
            try
            {
                p.Start();
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

        private void runAllTests()
        {
            Processes = new List<ProcessStatus>();
            Processes.Add(new ProcessStatus("tracert", "208.67.222.222"));
            Processes.Add(new ProcessStatus("tracert", "208.67.220.220"));

            foreach (var ps in Processes)
            {
                StartProcess(ps);
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
