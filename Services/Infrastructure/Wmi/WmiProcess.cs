using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;

namespace Services.Infrastructure.Wmi
{
    public class WmiProcess
    {
        private const string ClassName = "Win32_Process";
        private const int SleepInterval = 500;
        private const int MaxMinutesToWait = 5;

        public static ProcessReturnCode Run(string machineName, string commandLine, string args, string currentDirectory)
        {
            var scope = new ManagementScope($@"\\{machineName}\root\cimv2",
                new ConnectionOptions { EnablePrivileges = true });

            scope.Connect();

            using (var processClass = new ManagementClass(scope, new ManagementPath(ClassName), new ObjectGetOptions()))
            {
                var inParams = processClass.GetMethodParameters("Create");
                commandLine = Path.Combine(currentDirectory, commandLine);
                inParams["CommandLine"] = $"{commandLine} {args}";
                var outParams = processClass.InvokeMethod("Create", inParams, null);

                if (outParams == null)
                {
                    return ProcessReturnCode.UnknownFailure;
                }

                var pid = Convert.ToUInt32(outParams["processId"]);

                if (pid != 0)
                {
                    WaitForPidToDie(machineName, pid);
                }

                return (ProcessReturnCode) Convert.ToUInt32(outParams["returnValue"]);
            }
        }

        private static void WaitForPidToDie(string machineName, uint pid)
        {
            var deadline = DateTime.UtcNow.AddMinutes(MaxMinutesToWait);
            var numberOfAttempts = 0;

            while (PidExists(machineName, pid))
            {
                numberOfAttempts++;

                var sleep = SleepInterval + numberOfAttempts;

                Thread.Sleep(sleep);

                if (DateTime.UtcNow <= deadline)
                {
                    continue;
                }

                break;
            }
        }

        private static bool PidExists(string machineName, uint pid)
        {
            try
            {
                if (machineName == "127.0.0.1")
                {
                    Process.GetProcessById((int) pid);
                }
                else
                {
                    Process.GetProcessById((int) pid, machineName);
                }

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}