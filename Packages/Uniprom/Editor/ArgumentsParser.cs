
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Uniprom.Editor
{
  public class ArgumentsParser
  {
    static readonly string[] s_secrets = { "androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass" };
    static readonly string s_eol = Environment.NewLine;
    
    public static Dictionary<string, string> GetValidatedOptions(string[] secrets = default)
    {
      ParseCommandLineArguments(secrets ?? Array.Empty<string>(), out var validatedOptions);
      
      if (!validatedOptions.TryGetValue("projectPath", out var projectPath))
      {
        Console.WriteLine("Missing argument -projectPath");
        EditorApplication.Exit(110);
      }
      
      if (!validatedOptions.TryGetValue("buildTarget", out var buildTarget))
      {
        Console.WriteLine("Missing argument -buildTarget");
        EditorApplication.Exit(120);
      }

      if (!Enum.IsDefined(typeof(BuildTarget), buildTarget))
      {
        Console.WriteLine($"{buildTarget} is not a defined {nameof(BuildTarget)}");
        EditorApplication.Exit(121);
      }
      
      if (!validatedOptions.TryGetValue("customBuildPath", out var customBuildPath))
      {
        Console.WriteLine("Missing argument -customBuildPath");
        EditorApplication.Exit(130);
      }

      const string defaultCustomBuildName = "TestBuild";
      if (!validatedOptions.TryGetValue("customBuildName", out var customBuildName))
      {
        Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
        validatedOptions.Add("customBuildName", defaultCustomBuildName);
      }
      else if (customBuildName == "")
      {
        Console.WriteLine($"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}.");
        validatedOptions.Add("customBuildName", defaultCustomBuildName);
      }

      return validatedOptions;
    }

    static void ParseCommandLineArguments(string[] secrets, out Dictionary<string, string> providedArguments)
    {
      providedArguments = new Dictionary<string, string>();
      var args = Environment.GetCommandLineArgs();

      Console.WriteLine(
        $"{s_eol}" +
        $"###########################{s_eol}" +
        $"#    Parsing settings     #{s_eol}" +
        $"###########################{s_eol}" +
        $"{s_eol}"
      );
      
      for (int current = 0, next = 1; current < args.Length; current++, next++)
      {
        var isFlag = args[current].StartsWith("-");
        if (!isFlag)
        {
          continue;
        }
        var flag = args[current].TrimStart('-');
        var flagHasValue = next < args.Length && !args[next].StartsWith("-");
        var value = flagHasValue ? args[next].TrimStart('-') : "";
        var isSecret = s_secrets.Contains(flag) || secrets.Contains(flag);
        var displayValue = isSecret ? "*HIDDEN*" : "\"" + value + "\"";
        Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
        providedArguments.Add(flag, value);
      }
    }
  }
}