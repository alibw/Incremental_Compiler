using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NotTobereferredProject;
using TobereferredProject;

namespace Incremental_Compiler;

public static class Utils
{
    //private static List<string> referencedDlls = Directory.GetFiles(@"C:\dlls")
    private static string usings =
        @"using System.Linq.Expressions;
                using System;                   
                using System.Reflection;";

    //private static string compiledUsings;
    static string path = Path.GetDirectoryName(typeof(object).Assembly.Location);

    private static IEnumerable<MetadataReference> DefaultReferences =
        new[]
        {
            MetadataReference.CreateFromFile(Path.Combine(path, "mscorlib.dll")),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Core.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "Microsoft.CSharp.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Private.CoreLib.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Runtime.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Console.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Linq.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Linq.Expressions.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.Text.RegularExpressions.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.IO.dll"))),
            MetadataReference.CreateFromFile((Path.Combine(path, "System.ComponentModel.Primitives.dll")))
        };

    public static List<MetadataReference> AddDependencies(params Type[] dependencies)
    {
        List<MetadataReference> dependenciesReferences = new List<MetadataReference>(DefaultReferences);
        for (int i = 0; i < dependencies.Length; i++)
        {
            string compiledDllsPath = "C:\\dlls";
            DirectoryInfo di = new DirectoryInfo(compiledDllsPath);
            var referencedDlls = Directory.GetFiles(compiledDllsPath, "*.dll");
            dependenciesReferences.Add(
                MetadataReference.CreateFromFile(Path.Combine(compiledDllsPath, dependencies[i].Name + ".dll")));
            //compiledUsings = compiledUsings + $"using {file.Name.Trim('_').First()};";
        }
        return dependenciesReferences;
    }

    public static void CompileClassToDll(Type classType, string projectName, params Type[]? dependencies)
    {
        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
        var classPath = di.Parent.Parent.Parent.Parent.FullName;
        var source = File.ReadAllText(Path.Combine(classPath, projectName, classType.Name + ".cs"));
        var parsedSyntaxTree = CSharpSyntaxTree.ParseText(usings + source);

        var options = new CSharpCompilationOptions
        (
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release);

        var fileName = classType.Name;
        var compilation = CSharpCompilation.Create( fileName + ".dll",
            new SyntaxTree[] { parsedSyntaxTree },
            AddDependencies(dependencies), options);

        var result = compilation.Emit($"C:\\dlls\\{fileName}.dll");
        if (!result.Success)
        {
            foreach (var item in result.Diagnostics)
            {
                Console.WriteLine("Diagnostic:" + item.GetMessage());
            }
        }
        else
        {
            Assembly compiledDll;
            compiledDll = Assembly.LoadFrom($"C:\\dlls\\{classType.Name}.dll");
            foreach (var reference in compiledDll.GetReferencedAssemblies())
            {
                Console.WriteLine($"Reference of {classType.Name} : {reference.Name}");
            }
        }
    }

    public static void FindOutDependencies(string csProjPath)
    {
        XDocument projDefinition = XDocument.Load(csProjPath);
        var references = projDefinition.Element("Project").Elements("ItemGroup").Elements("ProjectReference")
            .Select(x => x.FirstAttribute.Value);

        foreach (string reference in references)
        {
            Console.WriteLine(reference);
        }

        Utils.CompileClassToDll(typeof(NotToBeReferredClass), "TobereferredProject");
        Utils.CompileClassToDll(typeof(TobereferredClass), "TobereferredProject");
        Utils.CompileClassToDll(typeof(ReferredTest), "Incremental_Compiler",typeof(TobereferredClass),typeof(NotToBeReferredClass));
    }
}