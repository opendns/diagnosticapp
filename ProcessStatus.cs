using System;
using System.Diagnostics;
using System.Windows.Forms;
using ProgressControls;

namespace OpenDnsDiagnostic
{
    class ProcessStatus
    {
        private string _displayName;
        public string DisplayName
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
        public bool Finished;
        public bool FailedToStart;
        public Process Process;
        public string StdOut;
        public string StdErr;
        public Label Label;
        public ProgressIndicator ProgressIndicator;

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
            Label = null;
            ProgressIndicator = null;
        }
    }
}
