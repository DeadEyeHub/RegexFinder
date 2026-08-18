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
            if (from <= 0) return;

            var value = (int)((double)progress / from * 10000);
            if (progress < from && value == _progressBar.Value) return;

            _tbProgress.Text = $"{progress} of {from}";
            _progressBar.Value = value;
            Application.DoEvents();
        }

        private readonly TextBox _tbProgress;
        private readonly ProgressBar _progressBar;

    }
}
