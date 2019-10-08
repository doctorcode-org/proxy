using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DoctorProxy
{
    public class ProcessManager
    {
        private int _MaxWait;

        public Action OnStart = null;
        public Action OnEnd = null;
        public Action<Exception> OnError = null;

        public ProcessManager()
        {
            _MaxWait = 15000;
        }

        public ProcessManager(int maxWait)
        {
            _MaxWait = maxWait;
        }

        public async Task<bool> Start(string processPath, string arguments)
        {
            var execute = new Process();
            try
            {
                var processInfo = new ProcessStartInfo(processPath, arguments);
                processInfo.CreateNoWindow = true;
                processInfo.ErrorDialog = false;
                processInfo.UseShellExecute = false;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;

                execute.EnableRaisingEvents = true;
                execute.Exited += new EventHandler(Execute_Exited);
                execute.StartInfo = processInfo;
                execute.ErrorDataReceived += new DataReceivedEventHandler(Execute_ErrorDataReceived);
                if (execute.Start())
                {
                    OnStart?.Invoke();
                }

                // var output = await execute.StandardOutput.ReadToEndAsync();
                var error = await execute.StandardError.ReadToEndAsync();
                execute.WaitForExit(_MaxWait);

                if (execute.ExitCode != 0)
                {
                    OnError?.Invoke(new Exception("OpenSsl process ended with internal error."));
                }

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                if (execute != null)
                    execute.Close();
            }
            return false;
        }

        void Execute_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnError?.Invoke(new Exception(e.Data));
        }

        private void Execute_Exited(object sender, EventArgs e)
        {
            OnEnd?.Invoke();
        }


    }
}
