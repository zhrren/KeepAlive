using LitJson;
using log4net;
using Mark.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace KeepAlive
{
    public class Program : ServiceBase
    {
        private static ILog _log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
            {
                _log.Error(e);
            };
            Program service = new Program();
            if (Environment.UserInteractive)
            {
                service.OnStart(args);
                Console.CancelKeyPress += (sender, e) => service.OnStop();
                Console.Read();
                service.OnStop();
            }
            else
            {
                service.AutoLog = true;
                Run(service);
            }
        }
        
        private SettingsManager _settings;
        private List<TaskWorker> workers;
        private bool isStarting;

        public Program()
        {
            workers = new List<TaskWorker>();

            _settings = new SettingsManager();
            _settings.Changed += _settings_Changed;
            _settings_Changed(null, null);

            InitializeComponent();
        }
        private void _settings_Changed(object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(_settings.Combine("log4net.config")));
            Apps.Settings = _settings.Renew<Settings>();
            _log.InfoFormat("settings: {0}", JsonMapper.ToJson(Apps.Settings));


            _log.Info(String.Format("Settings Changed {0} at {1}.", ServiceName, DateTime.Now));

            if (isStarting)
            {
                foreach (var worker in workers)
                    worker.Cancel();

                foreach (var task in Apps.Settings.Tasks)
                {
                    var worker = new TaskWorker(task);
                    workers.Add(worker);
                }
            }

            _log.Info(String.Format("Restarting {0} at {1}.", ServiceName, DateTime.Now));
        }

        protected override void OnStart(string[] args)
        {
            isStarting = true;

            if (Environment.UserInteractive)
            {
                _log.Info("Press any key to stop the program.");
            }

            foreach (var task in Apps.Settings.Tasks)
            {
                var worker = new TaskWorker(task);
                workers.Add(worker);
            }
            _log.Info(String.Format("Starting {0} at {1}.", ServiceName, DateTime.Now));
        }

        protected override void OnStop()
        {
            isStarting = false;

            foreach (var worker in workers)
                worker.Cancel();
            workers.Clear();

            _log.Info(String.Format("Stopping {0} at {1}.", ServiceName, DateTime.Now));
        }

        private void InitializeComponent()
        {
            ServiceName = "KeepAlive";
        }

    }
}
