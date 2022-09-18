using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using JetBrains.dotMemoryUnit;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using TobereferredProject;

namespace Incremental_Compiler
{
    [TestFixture]
    public class DependencyFinder
    {

        public void SetMsBuildExePath()
        {
            try
            {
                var startInfo = new ProcessStartInfo("dotnet", "--list-sdks")
                {
                    RedirectStandardOutput = true
                };

                var process = Process.Start(startInfo);
                process.WaitForExit(1000);

                var output = process.StandardOutput.ReadToEnd();
                var sdkPaths = Regex.Matches(output, "([0-9]+.[0-9]+.[0-9]+) \\[(.*)\\]")
                    .OfType<Match>()
                    .Select(m => System.IO.Path.Combine(m.Groups[2].Value, m.Groups[1].Value, "MSBuild.dll"));

                var sdkPath = sdkPaths.Last();
                Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", sdkPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Test]
        public void FindDependency()
        {
            Utils.FindOutDependencies(
                "H:\\RiderProjects\\Incremental_Compiler\\Incremental_Compiler\\Incremental_Compiler.csproj");
        }
    }
}


// var project = new Project("H:\\RiderProjects\\Incremental_Compiler\\Incremental_Compiler\\Incremental_Compiler.csproj");
// project.IsBuildEnabled = true;
// //project.SetGlobalProperty("Configuration", "Release");
// project.Build();

// List<ILogger> loggers = new List<ILogger>();
// loggers.Add(new ConsoleLogger());
// var projectCollection = new ProjectCollection();
// projectCollection.RegisterLoggers(loggers);
// var project = projectCollection.LoadProject(
//     "H:\\RiderProjects\\Incremental_Compiler\\Incremental_Compiler\\Incremental_Compiler.csproj"); // Needs a reference to System.Xml
// try
// {
//     project.Build();
//     
//     dotMemory.Check(memory => //2
//         Assert.That(AppDomain.CurrentDomain.GetAssemblies().Count(), Is.EqualTo(4)));
// }
// finally
// {
//     projectCollection.UnregisterAllLoggers();
// }