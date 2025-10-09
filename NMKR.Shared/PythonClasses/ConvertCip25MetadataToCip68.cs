using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;

namespace NMKR.Shared.PythonClasses
{
    public static class ConvertCip25MetadataToCip68
    {
        private static readonly ScriptEngine engine = Python.CreateEngine();
        public static string ConverMetadata(string metadata, string extrafield, string policyid, string tokenname)
        {
            ScriptScope scope = engine.CreateScope();
            scope.SetVariable("policyid", policyid);
            scope.SetVariable("tokenname", tokenname);
            scope.SetVariable("cip25metadata",metadata);

            if (!string.IsNullOrEmpty(extrafield))
            {
                if (extrafield.StartsWith("0x"))
                {
                    extrafield = extrafield.Substring(2);
                    scope.SetVariable("extrafieldhex", extrafield);
                }
                else
                {
                    scope.SetVariable("extrafield", extrafield);
                }
            }

            //   scope.SetVariable("version", 1);
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
           // var path = Directory.GetCurrentDirectory();
           string script = File.ReadAllText(path + Path.DirectorySeparatorChar + "PythonClasses" +
                                            Path.DirectorySeparatorChar + "Scripts" + Path.DirectorySeparatorChar +
                                            "ConvertCip25ToCip68.py");
            engine.Execute(script, scope);
            string cip68 = scope.GetVariable<string>("cip68metadata");
            return JsonFormatter.FormatJson(cip68);
        }
    }
}
