using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Util
{
    public class SolverUtils
    {
        /// <summary>
        /// Event handler to show log messages from the solver
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        public delegate void SolverLogMessage(SolverMessageType messageType, string message);
    }

    public enum SolverMessageType
    {
        Request, SolverResponse, SolverInfo, SolverWarning, SolverIgnore, AlwaysLog
    }
    public class SolverException : Exception
    {
        public SolverException(string message) : base(message)
        {
        }
    }
}

