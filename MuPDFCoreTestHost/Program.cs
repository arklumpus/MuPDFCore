using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tests;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace TestHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleColor defaultBackground = Console.BackgroundColor;
            ConsoleColor defaultForeground = Console.ForegroundColor;

            Console.WriteLine();
            Console.WriteLine("MuPDFCore test suite");
            Console.WriteLine();
            Console.WriteLine("Loading tests...");

            Assembly testAssembly = Assembly.GetAssembly(typeof(MuPDFWrapperTests));

            Type[] types = testAssembly.GetTypes();

            List<(string, Func<Task>)> tests = new List<(string, Func<Task>)>();
            HashSet<string> filesToDeploy = new HashSet<string>();

            for (int i = 0; i < types.Length; i++)
            {
                bool found = false;

                Attribute[] typeAttributes = Attribute.GetCustomAttributes(types[i]);

                foreach (Attribute attr in typeAttributes)
                {
                    if (attr is TestClassAttribute)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    MethodInfo[] methods = types[i].GetMethods(BindingFlags.Public | BindingFlags.Instance);

                    foreach (MethodInfo method in methods)
                    {
                        bool foundMethod = false;

                        foreach (Attribute attr in method.GetCustomAttributes())
                        {
                            if (attr is TestMethodAttribute)
                            {
                                foundMethod = true;
                            }

                            if (attr is DeploymentItemAttribute dia)
                            {
                                filesToDeploy.Add(dia.Path);
                            }
                        }

                        if (foundMethod)
                        {
                            Type type = types[i];

                            tests.Add((types[i].Name + "." + method.Name, async () =>
                            {
                                object obj = Activator.CreateInstance(type);
                                object returnValue = method.Invoke(obj, null);

                                if (returnValue is Task task)
                                {
                                    await task;
                                }
                            }
                            ));
                        }
                    }
                }
            }

            int longestTestNameLength = (from el in tests select el.Item1.Length).Max();

            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write("  Found {0} tests.  ", tests.Count.ToString());
            Console.BackgroundColor = defaultBackground;
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("{0} files will be deployed:", filesToDeploy.Count.ToString());

            foreach (string sr in filesToDeploy)
            {
                Console.WriteLine("    " + sr);
            }

            Console.WriteLine();

            string deploymentPath = Path.GetTempFileName();
            File.Delete(deploymentPath);
            Directory.CreateDirectory(deploymentPath);

            foreach (string sr in filesToDeploy)
            {
                File.Copy(sr, Path.Combine(deploymentPath, Path.GetFileName(sr)));
            }

            string originalCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(deploymentPath);

            Console.WriteLine("Deployment completed.");
            Console.WriteLine();
            Console.WriteLine("Running tests...");
            Console.WriteLine();

            List<(string, Func<Task>)> failedTests = new List<(string, Func<Task>)>();

            for (int i = 0; i < tests.Count; i++)
            {
                Console.Write(tests[i].Item1 + new string(' ', longestTestNameLength - tests[i].Item1.Length + 1));

                bool failed = false;

                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    await tests[i].Item2();
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    failed = true;
                    failedTests.Add(tests[i]);

                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("   Failed    ");
                    Console.BackgroundColor = defaultBackground;

                    Console.Write("    {0}ms", sw.ElapsedMilliseconds.ToString());

                    Console.WriteLine();

                    while (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.Message);
                        ex = ex.InnerException;
                    }

                    Console.WriteLine(ex.Message);
                }

                if (!failed)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.Write("  Succeeded  ");
                    Console.BackgroundColor = defaultBackground;
                    Console.Write("    {0}ms", sw.ElapsedMilliseconds.ToString());
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            if (failedTests.Count < tests.Count)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write("  {0} tests succeeded  ", tests.Count - failedTests.Count);
                Console.BackgroundColor = defaultBackground;
                Console.WriteLine();
            }

            if (failedTests.Count > 0)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("  {0} tests failed  ", failedTests.Count);
                Console.BackgroundColor = defaultBackground;
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("Failed tests:");
                Console.ForegroundColor = ConsoleColor.Red;

                for (int i= 0; i < failedTests.Count; i++)
                {
                    Console.WriteLine("    {0}", failedTests[i].Item1);
                }

                Console.WriteLine();
                Console.ForegroundColor = defaultForeground;

                Console.WriteLine();
                Console.WriteLine("The failed tests will now be repeated one by one.");

                for (int i = 0; i < failedTests.Count; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue...");
                    Console.WriteLine();
                    Console.ReadKey();
                    Console.Write(failedTests[i].Item1 + new string(' ', longestTestNameLength - failedTests[i].Item1.Length + 1));

                    bool failed = false;

                    try
                    {
                        await failedTests[i].Item2();
                    }
                    catch (Exception ex)
                    {
                        failed = true;

                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.Write("   Failed    ");
                        Console.BackgroundColor = defaultBackground;

                        Console.WriteLine();

                        while (ex.InnerException != null)
                        {
                            Console.WriteLine(ex.Message);
                            ex = ex.InnerException;
                        }

                        Console.WriteLine(ex.Message);
                    }

                    if (!failed)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.Write("  Succeeded  ");
                        Console.BackgroundColor = defaultBackground;
                    }

                    Console.WriteLine();
                }
            }

            Directory.SetCurrentDirectory(originalCurrentDirectory);
            Directory.Delete(deploymentPath, true);
        }
    }
}
