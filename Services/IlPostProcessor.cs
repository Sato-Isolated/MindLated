using dnlib.DotNet;
using System;

namespace MindLated.Services;

internal static class IlPostProcessor
{
    internal readonly record struct PostProcessReport(int ProcessedMethods, int SkippedMethods);

    public static PostProcessReport ProcessModule(ModuleDefMD module)
    {
        var processedMethods = 0;
        var skippedMethods = 0;

        foreach (var type in module.GetTypes())
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody || !method.Body.HasInstructions)
                    continue;

                try
                {
                    var body = method.Body;
                    body.SimplifyBranches();
                    body.SimplifyMacros(method.Parameters);
                    body.UpdateInstructionOffsets();
                    body.OptimizeBranches();
                    body.OptimizeMacros();
                    body.UpdateInstructionOffsets();
                    processedMethods++;
                }
                catch
                {
                    skippedMethods++;
                }
            }
        }

        return new PostProcessReport(processedMethods, skippedMethods);
    }
}