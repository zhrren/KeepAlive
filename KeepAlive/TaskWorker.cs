using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeepAlive
{
    public class TaskWorker
    {
        private BackgroundWorker _worker;
        private Task _task;
        private WebClient _client;
        private ILog _log = LogManager.GetLogger(typeof(TaskWorker));

        public TaskWorker(Task task)
        {
            _task = task;

            _client = new WebClient();

            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += new DoWorkEventHandler(DoWork);
            _worker.RunWorkerAsync(this);
        }

        public void Cancel()
        {
            _worker.CancelAsync();
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                try
                {
                   string content =  _client.DownloadString(_task.Url);
                    _log.Debug(content);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }
                Thread.Sleep((int)(_task.Interval * 1000));
            }
        }
    }
}
