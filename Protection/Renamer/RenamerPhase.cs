using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MindLated.Protection.Renamer;

internal class RenamerPhase
{
    public enum RenameMode
    {
        Ascii,
        Key,
        Normal
    }

    private const string Ascii = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private static readonly Random Random = new();

    private static readonly string[] NormalNameStrings =
    {
        "HasPermission", "HasPermissions", "GetPermissions", "GetOpenWindows", "EnumWindows", "GetWindowText",
        "GetWindowTextLength", "IsWindowVisible", "GetShellWindow", "Awake", "FixedUpdate", "add_OnRockedInitialized",
        "remove_OnRockedInitialized", "Awake", "Initialize", "Translate", "Reload", "<Initialize>b__13_0", "Initialize",
        "FixedUpdate", "Start", "checkTimerRestart", "QueueOnMainThread", "QueueOnMainThread", "RunAsync", "RunAction",
        "Awake", "FixedUpdate", "IsUri", "GetTypes", "GetTypesFromParentClass", "GetTypesFromParentClass",
        "GetTypesFromInterface", "GetTypesFromInterface", "get_Timeout", "set_Timeout", "GetWebRequest",
        "get_SteamID64", "set_SteamID64", "get_SteamID", "set_SteamID", "get_OnlineState", "set_OnlineState",
        "get_StateMessage", "set_StateMessage", "get_PrivacyState", "set_PrivacyState", "get_VisibilityState",
        "set_VisibilityState", "get_AvatarIcon", "set_AvatarIcon", "get_AvatarMedium", "set_AvatarMedium",
        "get_AvatarFull", "set_AvatarFull", "get_IsVacBanned", "set_IsVacBanned", "get_TradeBanState",
        "set_TradeBanState", "get_IsLimitedAccount", "set_IsLimitedAccount", "get_CustomURL", "set_CustomURL",
        "get_MemberSince", "set_MemberSince", "get_HoursPlayedLastTwoWeeks", "set_HoursPlayedLastTwoWeeks",
        "get_Headline", "set_Headline", "get_Location", "set_Location", "get_RealName", "set_RealName", "get_Summary",
        "set_Summary", "get_MostPlayedGames", "set_MostPlayedGames", "get_Groups", "set_Groups", "Reload",
        "ParseString", "ParseDateTime", "ParseDouble", "ParseUInt16", "ParseUInt32", "ParseUInt64", "ParseBool",
        "ParseUri", "IsValidCSteamID", "LoadDefaults", "LoadDefaults", "get_Clients", "Awake", "handleConnection",
        "FixedUpdate", "Broadcast", "OnDestroy", "Read", "Send", "<Awake>b__8_0", "get_InstanceID", "set_InstanceID",
        "get_ConnectedTime", "set_ConnectedTime", "Send", "Read", "Close", "get_Address", "get_Instance",
        "set_Instance", "Save", "Load", "Unload", "Load", "Save", "Load", "get_Configuration", "LoadPlugin",
        "<.ctor>b__3_0", "<LoadPlugin>b__4_0", "add_OnPluginUnloading", "remove_OnPluginUnloading",
        "add_OnPluginLoading", "remove_OnPluginLoading", "get_Translations", "get_State", "get_Assembly",
        "set_Assembly", "get_Directory", "set_Directory", "get_Name", "set_Name", "get_DefaultTranslations",
        "IsDependencyLoaded", "ExecuteDependencyCode", "Translate", "ReloadPlugin", "LoadPlugin", "UnloadPlugin",
        "OnEnable", "OnDisable", "Load", "Unload", "TryAddComponent", "TryRemoveComponent", "add_OnPluginsLoaded",
        "remove_OnPluginsLoaded", "get_Plugins", "GetPlugins", "GetPlugin", "GetPlugin", "Awake", "Start",
        "GetMainTypeFromAssembly", "loadPlugins", "unloadPlugins", "Reload", "GetAssembliesFromDirectory",
        "LoadAssembliesFromDirectory", "<Awake>b__12_0", "GetGroupsByIds", "GetParentGroups", "HasPermission",
        "GetGroup", "RemovePlayerFromGroup", "AddPlayerToGroup", "DeleteGroup", "SaveGroup", "AddGroup", "GetGroups",
        "GetPermissions", "GetPermissions", "<GetGroups>b__11_3", "Start", "FixedUpdate", "Reload", "HasPermission",
        "GetGroups", "GetPermissions", "GetPermissions", "AddPlayerToGroup", "RemovePlayerFromGroup", "GetGroup",
        "SaveGroup", "AddGroup", "DeleteGroup", "DeleteGroup", "<FixedUpdate>b__4_0", "Enqueue", "_Logger_DoWork",
        "processLog", "Log", "Log", "var_dump", "LogWarning", "LogError", "LogError", "Log", "LogException",
        "ProcessInternalLog", "logRCON", "writeToConsole", "ProcessLog", "ExternalLog", "Invoke", "_invoke",
        "TryInvoke", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax",
        "Execute", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax",
        "Execute", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax",
        "Execute", "get_Name", "set_Name", "get_Name", "set_Name", "get_Name", "get_Help", "get_Syntax",
        "get_AllowedCaller", "get_Commands", "set_Commands", "add_OnExecuteCommand", "remove_OnExecuteCommand",
        "Reload", "Awake", "checkCommandMappings", "checkDuplicateCommandMappings", "Plugins_OnPluginsLoaded",
        "GetCommand", "GetCommand", "getCommandIdentity", "getCommandType", "Register", "Register", "Register",
        "DeregisterFromAssembly", "GetCooldown", "SetCooldown", "Execute", "RegisterFromAssembly"
    };

    private static readonly Dictionary<string, string> Names = new();

    private static string RandomString(int length, string chars)
    {
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    private static string GetRandomName()
    {
        return NormalNameStrings[Random.Next(NormalNameStrings.Length)];
    }

    public static string GenerateString(RenameMode mode)
    {
        return mode switch
        {
            RenameMode.Ascii => RandomString(Random.Next(1, 7), Ascii),
            RenameMode.Key => RandomString(16, Ascii),
            RenameMode.Normal => GetRandomName(),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }


    public static void ExecuteClassRenaming(ModuleDefMD module)
    {
        foreach (var type in module.GetTypes())
        {
            if (type.IsGlobalModuleType) continue;

            if (type.Name == "GeneratedInternalTypeHelper" || type.Name == "Resources" || type.Name == "Settings")
                continue;
            if (Names.TryGetValue(type.Name, out var nameValue))
            {
                type.Name = nameValue;
            }
            else
            {
                var newName = GenerateString(RenameMode.Ascii);

                Names.Add(type.Name, newName);
                type.Name = newName;
            }
        }

        ApplyChangesToResourcesClasses(module);
    }


    private static void ApplyChangesToResourcesClasses(ModuleDefMD module)
    {
        var moduleToRename = module;

        foreach (var resource in moduleToRename.Resources)
        foreach (var item in Names)
            if (resource.Name.Contains(item.Key))
                resource.Name = resource.Name.Replace(item.Key, item.Value);

        foreach (var type in moduleToRename.GetTypes())
        foreach (var property in type.Properties)
        {
            if (property.Name != "ResourceManager")
                continue;

            var instr = property.GetMethod.Body.Instructions;

            for (var i = 0; i < instr.Count - 3; i++)
                if (instr[i].OpCode == OpCodes.Ldstr)
                    foreach (var item in Names.Where(item =>
                                 item.Key == instr[i].Operand.ToString()))
                        instr[i].Operand = item.Value;
        }
    }


    public static void ExecuteFieldRenaming(ModuleDefMD module)
    {
        foreach (var type in module.GetTypes())
        {
            if (type.IsGlobalModuleType) continue;

            foreach (var field in type.Fields)
                if (Names.TryGetValue(field.Name, out var nameValue))
                {
                    field.Name = nameValue;
                }
                else
                {
                    var newName = GenerateString(RenameMode.Ascii);

                    Names.Add(field.Name, newName);
                    field.Name = newName;
                }
        }

        ApplyChangesToResourcesField(module);
    }

    private static ModuleDefMD ApplyChangesToResourcesField(ModuleDefMD module)
    {
        foreach (var typeDef in module.GetTypes())
            if (!typeDef.IsGlobalModuleType)
                foreach (var methodDef in typeDef.Methods)
                    if (methodDef.Name != "InitializeComponent")
                    {
                        if (!methodDef.HasBody) continue;
                        IList<Instruction> instructions = methodDef.Body.Instructions;
                        for (var i = 0; i < instructions.Count - 3; i++)
                            if (instructions[i].OpCode == OpCodes.Ldstr)
                                foreach (var keyValuePair in Names)
                                    if (keyValuePair.Key == instructions[i].Operand.ToString())
                                        instructions[i].Operand = keyValuePair.Value;
                    }

        return module;
    }

    public static void ExecuteMethodRenaming(ModuleDefMD module)
    {
        foreach (var type in module.GetTypes())
        {
            if (type.IsGlobalModuleType) continue;

            if (type.Name == "GeneratedInternalTypeHelper") continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;

                if (method.Name == ".ctor" || method.Name == ".cctor") continue;

                method.Name = GenerateString(RenameMode.Ascii);
            }
        }
    }

    public static void ExecuteModuleRenaming(ModuleDefMD mod)
    {
        foreach (var module in mod.Assembly.Modules)
        {
            var isWpf = false;
            foreach (var asmRef in module.GetAssemblyRefs())
                if (asmRef.Name == "WindowsBase" || asmRef.Name == "PresentationCore" ||
                    asmRef.Name == "PresentationFramework" || asmRef.Name == "System.Xaml")
                    isWpf = true;

            if (!isWpf)
            {
                module.Name = GenerateString(RenameMode.Ascii);

                module.Assembly.CustomAttributes.Clear();
                module.Mvid = Guid.NewGuid();
                module.Assembly.Name = GenerateString(RenameMode.Ascii);
                module.Assembly.Version = new Version(Random.Next(1, 9), Random.Next(1, 9), Random.Next(1, 9),
                    Random.Next(1, 9));
            }
        }
    }

    public static void ExecuteNamespaceRenaming(ModuleDefMD module)
    {
        foreach (var type in module.GetTypes())
        {
            if (type.IsGlobalModuleType) continue;

            if (type.Namespace == "") continue;

            if (Names.TryGetValue(type.Namespace, out var nameValue))
            {
                type.Namespace = nameValue;
            }
            else
            {
                var newName = GenerateString(RenameMode.Ascii);

                Names.Add(type.Namespace, newName);
                type.Namespace = newName;
            }
        }

        ApplyChangesToResourcesNamespace(module);
    }

    private static void ApplyChangesToResourcesNamespace(ModuleDefMD module)
    {
        foreach (var resource in module.Resources)
        foreach (var item in Names.Where(item => resource.Name.Contains(item.Key)))
            resource.Name = resource.Name.Replace(item.Key, item.Value);

        foreach (var type in module.GetTypes())
        foreach (var property in type.Properties)
        {
            if (property.Name != "ResourceManager")
                continue;

            var instr = property.GetMethod.Body.Instructions;

            for (var i = 0; i < instr.Count - 3; i++)
                if (instr[i].OpCode == OpCodes.Ldstr)
                    foreach (var item in Names.Where(item =>
                                 item.Key == instr[i].Operand.ToString()))
                        instr[i].Operand = item.Value;
        }
    }

    public static void ExecutePropertiesRenaming(ModuleDefMD module)
    {
        foreach (var type in module.GetTypes())
        {
            if (type.IsGlobalModuleType) continue;

            foreach (var property in type.Properties) property.Name = GenerateString(RenameMode.Ascii);
        }
    }
}