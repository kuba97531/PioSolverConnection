using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Util
{
    public interface ISolverConnection
    {
        /// <summary>
        /// Sends request to Solver and gets response (both single and multiline) in the form of array.
        /// Blocks until server returns response.
        /// </summary>
        /// <param name="request">Request as a format string</param>
        /// <param name="formatArguments">List of objects to format</param>
        /// <returns> </returns>
        /// <throws> Exception if server thrown an error during processing</throws>
        /// <throws> Exception if server is actually processing other request</throws>
        /// <throws> ServerTerminatedException if server process has been terminated</throws>
        string[] GetResponseFromSolver(string request, params object[] formatArguments);

        /// <summary>
        /// Event is triggered when there is a message from the solver that is not part of
        /// the request response, e.g. solver updates about running time and exploitability
        /// </summary>
        event SolverUtils.SolverLogMessage LogEvent;

        /// <summary>
        /// Kills the underlying solver process by sending "exit" command or killing it forcefully 
        /// if it doesn't die itself in response to exit
        /// </summary>
        void Disconnect();
    }
}
