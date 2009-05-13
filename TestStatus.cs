using System;
using System.Diagnostics;
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
    }
}
