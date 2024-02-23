using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MemoryEditor
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Program running");

            string readLine = "";

            while (readLine != "0")
            {
                Console.WriteLine("Choose the mode of operation of the program:");
                Console.WriteLine("Type \"0\" to stop program");
                Console.WriteLine("Type \"1\" to show");
                Console.WriteLine("Type \"2\" to search");
                Console.WriteLine("Type \"3\" to clear");
                Console.WriteLine("Type \"4\" to show unsafe processes in red");
                readLine = Console.ReadLine();

                switch (readLine)
                {
                    case "0":
                        Console.Clear();
                        Console.WriteLine("Stop");
                        break;

                    case "1":
                        Console.Clear();
                        Console.WriteLine("Show");
                        ShowAllProcesses();
                        break;

                    case "2":
                        Console.Clear();
                        Console.WriteLine("Search");
                        SearchProcess();
                        break;

                    case "3":
                        Console.Clear();
                        Console.WriteLine("Clear");
                        KillNonSystemProcesses();
                        break;

                    case "4":
                        Console.Clear();
                        Console.WriteLine("Unsafe show");
                        ShowUnsafeInRedProcesses();
                        break;

                    default:
                        Console.Clear();
                        Console.WriteLine("!!!Invalid input!!!");
                        break;
                }
            }
            Console.ReadLine();
        }

        static void ShowAllProcesses()
        {
            int count = 0;
            long totalMemory = 0;

            Process[] processes = Process.GetProcesses();

            var sortedProcesses = processes.OrderByDescending(process => process.WorkingSet64).ToList();

            foreach (var process in sortedProcesses)
            {
                Console.WriteLine($"{count + 1}) {process.ProcessName} with ID {process.Id} found. MEMORY: {process.WorkingSet64 / (1024 * 1024)} MB");
                count++;
                totalMemory += process.WorkingSet64 / (1024 * 1024);
            }
            Console.WriteLine($"\nTotal memory: {totalMemory}\nProcess count: {count}");
            Console.WriteLine("Write something to continue");
            Console.ReadLine();
            Console.Clear();
        }

        static void SearchProcess()
        {
            Console.WriteLine("Write something to search:");

            int count = 0;
            string processNameToSearch = Console.ReadLine().ToLower();
            Process[] processes = Process.GetProcesses();
            var sortedProcesses = processes.OrderByDescending(process => process.WorkingSet64).ToList();

            Console.Clear();
            Console.WriteLine("Search for the word \"{0}\":", processNameToSearch);
            foreach (var process in sortedProcesses)
            {
                try
                {
                    if (process.ProcessName.ToLower().Contains(processNameToSearch))
                    {
                        Console.WriteLine($"{count + 1}) {process.ProcessName} with ID {process.Id} found. MEMORY: {process.WorkingSet64 / (1024 * 1024)} MB");
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving information for process {process.ProcessName} with ID {process.Id}: {ex.Message}");
                }
            }

            if (processes.All(process => !process.ProcessName.ToLower().Contains(processNameToSearch)))
            {
                Console.WriteLine($"No processes found with the name: {processNameToSearch}");
            }
            else
            {
                string readLine = "";
                while (readLine != "y" && readLine != "n")
                {
                    Console.WriteLine("\nClear it? (y or n)");
                    readLine = Console.ReadLine();

                    if (readLine == "y")
                    {
                        foreach (var process in processes)
                        {
                            try
                            {
                                if (!IsSystemProcess(process) && !IsImportantProcess(process) && process.ProcessName.ToLower().Contains(processNameToSearch))
                                {
                                    process.Kill();
                                    Console.WriteLine($"Process {process.ProcessName} with ID {process.Id} terminated.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error terminating process {process.ProcessName} with ID {process.Id}: {ex.Message}");
                            }
                        }
                    }
                    else if (readLine == "n")
                    {
                        Console.WriteLine("Well, no, no");
                    }
                }
            }

            Console.WriteLine("Write something to continue...");
            Console.ReadLine();
            Console.Clear();
        }


        static void KillNonSystemProcesses()
        {
            Process[] processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (!IsSystemProcess(process) && !IsImportantProcess(process))
                    {
                        process.Kill();
                        Console.WriteLine($"Process {process.ProcessName} with ID {process.Id} terminated.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error terminating process {process.ProcessName} with ID {process.Id}: {ex.Message}");
                }
            }
            Console.WriteLine("Write something to continue");
            Console.ReadLine();
            Console.Clear();
        }

        static void ShowUnsafeInRedProcesses()
        {
            int count = 0;
            long totalMemory = 0;

            Process[] processes = Process.GetProcesses();

            var sortedProcesses = processes.OrderByDescending(process => process.WorkingSet64).ToList();

            foreach (var process in sortedProcesses)
            {
                if (IsSystemProcess(process))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{count + 1}) {process.ProcessName} with ID {process.Id} found. MEMORY: {process.WorkingSet64 / (1024 * 1024)} MB");
                    Console.ResetColor();
                    count++;
                    totalMemory += process.WorkingSet64 / (1024 * 1024);
                }
            }

            foreach (var process in sortedProcesses)
            {
                if (IsImportantProcess(process))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{count + 1}) {process.ProcessName} with ID {process.Id} found. MEMORY: {process.WorkingSet64 / (1024 * 1024)} MB");
                    Console.ResetColor();
                    count++;
                    totalMemory += process.WorkingSet64 / (1024 * 1024);
                }
            }

            foreach (var process in sortedProcesses)
            {
                if (IsSystemProcess(process) || IsImportantProcess(process))
                {
                    continue;
                }

                Console.WriteLine($"{count + 1}) {process.ProcessName} with ID {process.Id} found. MEMORY: {process.WorkingSet64 / (1024 * 1024)} MB");
                count++;
                totalMemory += process.WorkingSet64 / (1024 * 1024);
            }

            Console.WriteLine($"\nTotal memory: {totalMemory}\nProcess count: {count}");
            Console.WriteLine("Write something to continue");
            Console.ReadLine();
            Console.Clear();
        }

        static bool IsSystemProcess(Process process)
        {
            List<string> processNames = new List<string>
            {
                "system",
                "msvsmon",
                "MsMpEng",
                "conhost",
                "useroobebroker",
                "SearchApp",
                "msedgewebview2",
                "smss",
                "Idle",
                "fontdrvhost",
                "sqlwriter",
                "csrss",
                "svchost"
            };

            return processNames.Any(l_process => process.ProcessName.ToLower().Contains(l_process));
        }

        static bool IsImportantProcess(Process process)
        {
            List<string> processNames = new List<string>
            {
                "service",
                "memoryeditor",
                "devenv",
            };

            return processNames.Any(l_process => process.ProcessName.ToLower().Contains(l_process));
        }
    }
}