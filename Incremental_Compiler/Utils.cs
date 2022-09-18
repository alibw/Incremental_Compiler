using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
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

    public static List<MetadataReference> AddDependencies(List<DependentClass> dependencies)
    {
        List<MetadataReference> dependenciesReferences = new List<MetadataReference>(DefaultReferences);
        for (int i = 0; i < dependencies.Count; i++)
        {
            CompileClassToDll(dependencies[i]);
            string compiledDllsPath = "C:\\dlls";
            DirectoryInfo di = new DirectoryInfo(compiledDllsPath);
            var referencedDlls = Directory.GetFiles(compiledDllsPath, "*.dll");

            if (File.Exists(Path.Combine(compiledDllsPath, dependencies[i].dependentClass.Name + ".dll")))
            {
                dependenciesReferences.Add(
                    MetadataReference.CreateFromFile(Path.Combine(compiledDllsPath,
                        dependencies[i].dependentClass.Name + ".dll")));
                //compiledUsings = compiledUsings + $"using {file.Name.Trim('_').First()};";
            }
            else
            {
                if (!CompileClassToDll(dependencies[i]))
                {
                    throw new Exception($"Compilation Of {dependencies[i].dependentClass.Name} Failed!");
                }
                else
                {
                    dependenciesReferences.Add(
                        MetadataReference.CreateFromFile(Path.Combine(compiledDllsPath,
                            dependencies[i].dependentClass.Name + ".dll")));
                }
            }
        }
        return dependenciesReferences;
    }

    public static bool CompileClassToDll(DependentClass classType)
    {
        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
        var projectName = classType.dependentClass.Namespace;
        var classPath = di.Parent.Parent.Parent.Parent.FullName;
        var source = File.ReadAllText(Path.Combine(classPath, projectName, classType.dependentClass.Name + ".cs"));
        var parsedSyntaxTree = CSharpSyntaxTree.ParseText(usings + source);

        var options = new CSharpCompilationOptions
        (
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release);

        var fileName = classType.dependentClass.Name;
        var compilation = CSharpCompilation.Create(fileName + ".dll",
            new SyntaxTree[] { parsedSyntaxTree },
            AddDependencies(classType.dependencies), options);

        var result = compilation.Emit($"C:\\dlls\\{fileName}.dll");
        if (!result.Success)
        {
            foreach (var item in result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Distinct())
            {
                Console.WriteLine("Diagnostic:" + item.GetMessage());
            }

            File.Delete($"C:\\dlls\\{classType.dependentClass.Name}.dll");

            return false;
        }
        else
        {
            Assembly compiledDll;
            compiledDll = Assembly.LoadFrom($"C:\\dlls\\{classType.dependentClass.Name}.dll");
            foreach (var reference in compiledDll.GetReferencedAssemblies())
            {
                Console.WriteLine($"Reference of {classType.dependentClass.Name} : {reference.Name}");
            }

            return true;
        }
    }

    public static Type CompileDependency(Type type, params Type[] dependencies)
    {
        return type;
    }

    public static void FindOutDependencies(string csProjPath)
    {
        XDocument projDefinition = XDocument.Load(csProjPath);
        var references = projDefinition.Element("Project").Elements("ItemGroup").Elements("ProjectReference")
            .Select(x => x.FirstAttribute.Value);

        foreach (string reference in references)
        {
            Console.WriteLine("Project Dependencies : " + reference);
        }

        DependentClass t = new DependentClass()
        {
            dependentClass = typeof(ThirdClass),
            dependencies = new List<DependentClass>()
            {
                new DependentClass()
                {
                    dependentClass = typeof(TobereferredClass),
                    dependencies = new List<DependentClass>()
                    {
                        new DependentClass()
                        {
                            dependentClass = typeof(NotToBeReferredClass)
                        }
                    }
                }
            }
        };
        Utils.CompileClassToDll(t);
        
        
        
        //Utils.CompileClassToDll(typeof(NotToBeReferredClass));
        //Utils.CompileClassToDll(typeof(TobereferredClass));
        //Utils.CompileClassToDll(typeof(ReferredTest),typeof(TobereferredClass),typeof(NotToBeReferredClass));
    }
}