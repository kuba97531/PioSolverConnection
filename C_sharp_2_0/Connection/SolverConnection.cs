using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Util
{

    public class SolverConnection : ISolverConnection
    {
        public event SolverUtils.SolverLogMessage LogEvent;

        public int SolverConnectionTimeoutInMiliseconds = 1000;

        public SolverConnection(string solverExecutablePath)
        {
            StartSolverForExecutable(solverExecutablePath);
        }

        public string[] GetResponseFromSolver(string request, params object[] formatArguments)
        {
            var r = new RequestResponse(string.Format(request, formatArguments));
            SetRequest(r);
            r.WaitForResponse();
            if (r.IsError)
            {
                throw new SolverException(r.Request + r.ErrorMessage);
            }
            return r.Response;
        }

        public void Disconnect()
        {
            SetRequest(new RequestResponse("exit"));

            _solverOutupuInLoopThread.Join(SolverConnectionTimeoutInMiliseconds);
            if (_solverOutupuInLoopThread.IsAlive)
            {
                _solverOutupuInLoopThread.Abort();
            }

            _processMessageThread.Join(SolverConnectionTimeoutInMiliseconds);
            if (_processMessageThread.IsAlive)
            {
                _processMessageThread.Abort();
            }

            _solverProcess.WaitForExit(SolverConnectionTimeoutInMiliseconds);
            _solverProcess.Kill();
        }

        #region solver start / monitor
        private Process _solverProcess;
        private Thread _solverOutupuInLoopThread;
        private Thread _processMessageThread;
        private Thread _checkForServerDeathThread;

        private void StartSolverForExecutable(string executablePath)
        {
            _solverProcess = new Process
            {
                StartInfo = new ProcessStartInfo(executablePath)
                {
                    WorkingDirectory = Directory.GetParent(executablePath).FullName,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };
            _solverProcess.Start();

            var headerResult = SolverProcessStandardOutputReadLineAsync();
            headerResult.Wait(SolverConnectionTimeoutInMiliseconds);

            if (_solverProcess.HasExited)
            {
                throw new SolverException($"{executablePath} has not started. It could be missing dependencies.");
            }
            if (!headerResult.IsCompleted || headerResult.Result == null)
            {
                _solverProcess.Kill();
                throw new SolverException("Couldn't connect to solver.");
            }

            _solverProcess.StandardInput.WriteLine("set_end_string END");

            string ReadSingleLineOrKillWithError(string message)
            {
                var r = SolverProcessStandardOutputReadLineAsync();
                r.Wait(SolverConnectionTimeoutInMiliseconds);
                if (!r.IsCompleted)
                {
                    _solverProcess.Kill();
                    throw new SolverException(message);
                }
                return r.Result;
            }

            string firstResponse;
            do
            {
                var errorMessage = @"PioSolver is malfunctioning. It didn't reply to set_end_string END";
                firstResponse = ReadSingleLineOrKillWithError(errorMessage);
            }
            while (firstResponse != "set_end_string ok!");

            var result = ReadSingleLineOrKillWithError("PioSolver is malfunctioning. It didn't reply with an END after 'set_end_string ok!'.");
            if (result != "END")
            {
                throw new SolverException(
                    $"PioSolver is malfunctioning. It replied with a '{result}' instead of END after 'set_end_string ok!'.");
            }


            _solverOutupuInLoopThread = new Thread(ProcessSolverOutputInLoop) { IsBackground = true, Name = "BGT: raw solver i/o" };
            _solverOutupuInLoopThread.Start();

            _processMessageThread = new Thread(ProcessMessagesInLoop) { IsBackground = true, Name = "BGT: Deque, Process, Pulse" };
            _processMessageThread.Start();

            _checkForServerDeathThread = new Thread(WaitForSolverToDie)
            {
                IsBackground = true,
                Name = "BGT: Wait for solver to die."
            };

            _checkForServerDeathThread.Start();
        }
        private void WaitForSolverToDie()
        {
            while (!_solverProcess.HasExited)
            {
                Thread.Sleep(100);
            }
            if (_solverProcess.ExitCode != 0)
            {
                LogEvent(SolverMessageType.SolverInfo, "Solver process died with error code " + _solverProcess.ExitCode);
            }
            lock (_requestsLocker)
            {
                foreach (var r in _liveRequestResponses.ToArray())
                {
                    SetErrorMessageSolverTerminated(r);
                }
            }
        }
        #endregion

        #region I/O processing
        private Task<string> SolverProcessStandardOutputReadLineAsync()
        {
            return Task.Run((Func<string>)SolverProcessStandardOutputReadLine);
        }

        private string SolverProcessStandardOutputReadLine()
        {
            while (true)
            {
                string line = _solverProcess.StandardOutput.ReadLine();
                if (line == null)
                {
                    return null;
                }
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line == "SOLVER:")
                {
                    var solverLines = new List<string>();
                    while (line != "END")
                    {
                        solverLines.Add(line);
                        line = _solverProcess.StandardOutput.ReadLine();
                        if (line == null)
                        {
                            return null;
                        }
                        line = line.Trim();
                    }
                    LogEvent?.Invoke(SolverMessageType.SolverInfo, string.Join(Environment.NewLine, solverLines));
                }
                else if (line.StartsWith("SOLVER:"))
                {
                    LogEvent?.Invoke(SolverMessageType.SolverInfo, line);
                }
                else if (line.StartsWith("ALERT:"))
                {
                    LogEvent?.Invoke(SolverMessageType.SolverWarning, line.Substring("ALERT:".Length));
                }
                else if (line.StartsWith("LOG:"))
                {
                    LogEvent?.Invoke(SolverMessageType.AlwaysLog, line.Substring("LOG:".Length));
                }
                else if (line.StartsWith("[LOG]"))
                {
                    LogEvent?.Invoke(SolverMessageType.AlwaysLog, line.Substring("[LOG]".Length));
                }
                else if (line.StartsWith("[IGNORE]"))
                {
                    LogEvent?.Invoke(SolverMessageType.SolverIgnore, line.Substring("[IGNORE]".Length));
                }
                else
                {
                    LogEvent?.Invoke(SolverMessageType.SolverResponse, line);
                    return line;
                }
            }
        }

        private void ProcessSolverOutputInLoop()
        {
            while (!_solverProcess.HasExited)
            {
                var line = SolverProcessStandardOutputReadLine();
                if (line == null)
                {
                    return;
                }
                this.RecordSingleLineFromSolver(line);
            }
        }

        private void RecordSingleLineFromSolver(string line)
        {
            lock (_solverScanningLocker)
            {
                _scannedLines.Add(line);
                Monitor.Pulse(_solverScanningLocker);
            }
        }

        private string ReadSingleLineFromSolver()
        {
            lock (_solverScanningLocker)
            {
                while (_scannedLines.Count == 0)
                {
                    bool threadAlive = _solverOutupuInLoopThread.IsAlive;
                    if (threadAlive)
                    {
                        Monitor.Wait(_solverScanningLocker, 100);
                    }
                    else
                    {
                        return null;
                    }
                }
                var item = _scannedLines[0];
                _scannedLines.RemoveAt(0);
                return item;
            }
        }

        private void WriteLineToStandardInput(string line)
        {
            LogEvent?.Invoke(SolverMessageType.Request, line);
            _solverProcess.StandardInput.WriteLine(line);
        }

        #endregion

        #region process requests
        private void ProcessMessagesInLoop()
        {
            while (!_solverProcess.HasExited)
            {
                ProcessMessage();
            }
        }

        private object _requestsLocker = new object();

        // this list is used only at the end - to tell all waiting in the queue that solver has died
        private ConcurrentQueue<RequestResponse> _liveRequestResponses = new ConcurrentQueue<RequestResponse>();

        private List<string> _scannedLines = new List<string>();

        private object _solverScanningLocker = new object();

        private void ProcessMessage()
        {
            RequestResponse request;
            lock (_requestsLocker)
            {
                while (!_liveRequestResponses.TryDequeue(out request))
                {
                    Monitor.Wait(_requestsLocker);
                }
            }

            string line = ReadSingleLineFromSolver();
            if (line == null)
            {
                SetErrorMessageSolverTerminated(request);
                return;
            }
            if (line.StartsWith("ERROR"))
            {
                request.IsError = true;
                request.ErrorMessage = line;
            }
            var lines = new List<string>();

            while (line != "END")
            {
                if (line == null)
                {
                    SetErrorMessageSolverTerminated(request);
                    return;
                }
                lines.Add(line);
                line = ReadSingleLineFromSolver();
            }
            request.Response = lines.ToArray();

            request.PulseResponse();
            lock (_requestsLocker)
            {
                if (_liveRequestResponses.Count == 0 && _scannedLines.Count > 0)
                {
                    var msg = string.Join("\n", _scannedLines);
                    LogEvent?.Invoke(SolverMessageType.AlwaysLog, ("Unexpected problems with the solver. The following communication from solver was ignored:\n" + msg));

                    _scannedLines.Clear();
                }
            }
        }

        private void SetRequest(RequestResponse item)
        {
            if (_solverProcess.HasExited)
            {
                SetErrorMessageSolverTerminated(item);
                return;
            }

            lock (_requestsLocker)
            {
                _liveRequestResponses.Enqueue(item);
                StartProcessingMessage(item);
                Monitor.Pulse(_requestsLocker);
                if (item.Request == "exit")
                {
                    return;
                }
            }
        }
        private void StartProcessingMessage(RequestResponse item)
        {
            WriteLineToStandardInput(item.Request);
        }

        private void SetErrorMessageSolverTerminated(RequestResponse item)
        {
            item.IsError = true;
            item.ErrorMessage = "Solver process has terminated";
            item.PulseResponse();
        }

        private class RequestResponse
        {
            public string[] Response { get; set; }
            public string Request { get; set; }
            public bool IsError { get; set; }
            public string ErrorMessage { get; set; }

            public RequestResponse(string request)
            {
                Request = request;
            }

            private readonly object Locker = new object();

            private bool Resolved { get; set; }

            public void WaitForResponse()
            {
                lock (this.Locker)
                {
                    if (!this.Resolved)
                    {
                        Monitor.Wait(this.Locker);
                    }
                }
            }

            public void PulseResponse()
            {
                lock (this.Locker)
                {
                    this.Resolved = true;
                    Monitor.PulseAll(this.Locker);
                }
            }
        }
        #endregion
    }
}
