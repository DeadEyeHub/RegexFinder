using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace regexFinder
{
    public class NotificationProgress
    {
        public NotificationProgress(TextBox tbProgress, ProgressBar progressBar) {
            _tbProgress = tbProgress;
            _progressBar = progressBar;
        }
       

        public void SetProgress(int progress, int from) { 
            _tbProgress.Text = $"{progress} of {from}";
            _progressBar.Value = (int)((double)progress / from * 10000);
            Application.DoEvents();
        }

        private readonly TextBox _tbProgress;
        private readonly ProgressBar _progressBar;

    }
}
