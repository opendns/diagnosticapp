using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ProgressControls;

namespace OpenDnsDiagnostic
{
    public class TestStatus
    {
        public string _displayName;
        public virtual string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        public bool Finished;
        public bool FailedToStart;
        public Label Label;
        public ProgressIndicator ProgressIndicator;

        public TestStatus()
        {
            _displayName = null;
            FailedToStart = false;
            Finished = false;
            Label = null;
            ProgressIndicator = null;
        }

        public void Stop()
        {
            Finished = true;
            ProgressIndicator.Stop();
            ProgressIndicator.Visible = false;
            Label.Text = "Finished: " + Label.Text;
            Label.ForeColor = System.Drawing.Color.Gray;
        }

        public void WriteSeparatorLine(StreamWriter sw)
        {
            sw.WriteLine("---------------------------------------------");
        }

        public virtual void WriteResult(StreamWriter sw)
        {
        }
    }

    public class ProcessStatus : TestStatus
    {
        public override string DisplayName
        {
            get
            {
                if (null != _displayName)
                    return _displayName;
                if (null == Args)
                    return Exe;
                return Exe + " " + Args;
            }
            set
            {
                _displayName = value;
            }
        }
        public string Exe;
        public string Args;
        public Process Process;
        public string StdOut;
        public string StdErr;

        public ProcessStatus(string exe, string args)
            : base()
        {
            Exe = exe;
            Debug.Assert(null != exe);
            Args = args;
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.EnableRaisingEvents = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = Exe;
            p.StartInfo.Arguments = Args;
            Process = p;
        }

        public override void WriteResult(StreamWriter sw)
        {
            WriteSeparatorLine(sw);
            sw.WriteLine("Results for: " + DisplayName);
            if (StdOut != null && StdOut.Length > 0)
            {
                sw.WriteLine("stdout:");
                sw.WriteLine(StdOut);
            }

            if (StdErr != null && StdErr.Length > 0)
            {
                sw.WriteLine("stderr:");
                sw.WriteLine(StdErr);
            }
        }
    }

    public class DnsResolveStatus : TestStatus
    {
        public string Hostname;
        public IPAddress[] IPAddresses;
        public DnsResolveStatus(string hostname)
            : base()
        {
            Hostname = hostname;
        }

        public override string DisplayName
        {
            get
            {
                if (null != _displayName)
                    return _displayName;
                return "DNS lookup of " + Hostname;
            }
            set
            {
                base.DisplayName = value;
            }
        }

        public override void WriteResult(StreamWriter sw)
        {
            WriteSeparatorLine(sw);
            sw.WriteLine("Results of DNS lookup of: " + Hostname);
            if (null == IPAddresses)
            {
                sw.WriteLine();
                return;
            }
            foreach (var ip in IPAddresses)
            {
                var s = "  " + ip.ToString();
                sw.WriteLine(s);
            }
            sw.WriteLine();
        }
    }
}
