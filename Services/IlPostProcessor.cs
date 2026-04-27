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
                    var instructionCount = body.Instructions.Count;
                    var conservativeMinStack = (ushort)Math.Min(ushort.MaxValue, Math.Max(8, instructionCount + 8));
                    body.MaxStack = conservativeMinStack;
                    body.KeepOldMaxStack = true;
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