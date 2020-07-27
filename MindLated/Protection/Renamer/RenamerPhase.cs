using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MindLated.Protection.Renamer
{
    public class RenamerPhase
    {
        private static readonly Dictionary<TypeDef, bool> TypeRename = new Dictionary<TypeDef, bool>();
        private static readonly List<string> TypeNewName = new List<string>();
        private static readonly Dictionary<MethodDef, bool> MethodRename = new Dictionary<MethodDef, bool>();
        private static readonly List<string> MethodNewName = new List<string>();
        private static readonly Dictionary<FieldDef, bool> FieldRename = new Dictionary<FieldDef, bool>();
        private static readonly List<string> FieldNewName = new List<string>();
        public static bool IsObfuscationActive = true;
        public static void Execute(ModuleDefMD module)
        {
            if (IsObfuscationActive)
            {
                var namespaceNewName = GenerateString(RenameMode.Normal);
                foreach (var type in module.Types)
                {
                    if (TypeRename.TryGetValue(type, out var canRenameType))
                    {
                        if (canRenameType && type.IsSerializable)
                            InternalRename(type);
                    }
                    else
                        InternalRename(type);
                    foreach (var method in type.Methods)
                    {
                        if (MethodRename.TryGetValue(method, out var canRenameMethod))
                        {
                            if (canRenameMethod && !method.IsConstructor && !method.IsSpecialName)
                                InternalRename(method);
                        }
                        else 
                            InternalRename(method);
                    }
                    MethodNewName.Clear();
                    foreach (var field in type.Fields)
                    {
                        if (FieldRename.TryGetValue(field, out var canRenameField))
                        {
                            if (canRenameField)
                                InternalRename(field);
                        }
                        else
                            InternalRename(field);
                    }
                    FieldNewName.Clear();
                }
            }
            else
            {
                foreach (var typeItem in TypeRename.Where(typeItem => typeItem.Value))
                {
                    InternalRename(typeItem.Key);
                }
                foreach (var methodItem in MethodRename.Where(methodItem => methodItem.Value))
                {
                    InternalRename(methodItem.Key);
                }
                foreach (var fieldItem in FieldRename.Where(fieldItem => fieldItem.Value))
                {
                    InternalRename(fieldItem.Key);
                }
            }
        }


        private static void InternalRename(TypeDef type)
        {
            var randString = GenerateString(RenameMode.Normal);
            while (TypeNewName.Contains(randString))
                randString = GenerateString(RenameMode.Normal);
            TypeNewName.Add(randString);
            type.Name = randString;
        }

        private static void InternalRename(MethodDef method)
        {
            var randString = GenerateString(RenameMode.Normal);
            while (MethodNewName.Contains(randString))
                randString = GenerateString(RenameMode.Normal);
            MethodNewName.Add(randString);
            method.Name = randString;
        }

        private static void InternalRename(FieldDef field)
        {
            var randString = GenerateString(RenameMode.Normal);
            while (FieldNewName.Contains(randString))
                randString = GenerateString(RenameMode.Normal);
            FieldNewName.Add(randString);
            field.Name = randString;
        }

        public static Random Random = new Random();

        private static string RandomString(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public enum RenameMode
        {
            Ascii,
            Normal
        }

        public static string[] NormalNameStrings = {
            "HasPermission", "HasPermissions", "GetPermissions", "GetOpenWindows", "EnumWindows", "GetWindowText", "GetWindowTextLength", "IsWindowVisible", "GetShellWindow", "Awake", "FixedUpdate", "add_OnRockedInitialized", "remove_OnRockedInitialized", "Awake", "Initialize", "Translate", "Reload", "<Initialize>b__13_0", "Initialize", "FixedUpdate", "Start", "checkTimerRestart", "QueueOnMainThread", "QueueOnMainThread", "RunAsync", "RunAction", "Awake", "FixedUpdate", "IsUri", "GetTypes", "GetTypesFromParentClass", "GetTypesFromParentClass", "GetTypesFromInterface", "GetTypesFromInterface", "get_Timeout", "set_Timeout", "GetWebRequest", "get_SteamID64", "set_SteamID64", "get_SteamID", "set_SteamID", "get_OnlineState", "set_OnlineState", "get_StateMessage", "set_StateMessage", "get_PrivacyState", "set_PrivacyState", "get_VisibilityState", "set_VisibilityState", "get_AvatarIcon", "set_AvatarIcon", "get_AvatarMedium", "set_AvatarMedium", "get_AvatarFull", "set_AvatarFull", "get_IsVacBanned", "set_IsVacBanned", "get_TradeBanState", "set_TradeBanState", "get_IsLimitedAccount", "set_IsLimitedAccount", "get_CustomURL", "set_CustomURL", "get_MemberSince", "set_MemberSince", "get_HoursPlayedLastTwoWeeks", "set_HoursPlayedLastTwoWeeks", "get_Headline", "set_Headline", "get_Location", "set_Location", "get_RealName", "set_RealName", "get_Summary", "set_Summary", "get_MostPlayedGames", "set_MostPlayedGames", "get_Groups", "set_Groups", "Reload", "ParseString", "ParseDateTime", "ParseDouble", "ParseUInt16", "ParseUInt32", "ParseUInt64", "ParseBool", "ParseUri", "IsValidCSteamID", "LoadDefaults", "LoadDefaults", "get_Clients", "Awake", "handleConnection", "FixedUpdate", "Broadcast", "OnDestroy", "Read", "Send", "<Awake>b__8_0", "get_InstanceID", "set_InstanceID", "get_ConnectedTime", "set_ConnectedTime", "Send", "Read", "Close", "get_Address", "get_Instance", "set_Instance", "Save", "Load", "Unload", "Load", "Save", "Load", "get_Configuration", "LoadPlugin", "<.ctor>b__3_0", "<LoadPlugin>b__4_0", "add_OnPluginUnloading", "remove_OnPluginUnloading", "add_OnPluginLoading", "remove_OnPluginLoading", "get_Translations", "get_State", "get_Assembly", "set_Assembly", "get_Directory", "set_Directory", "get_Name", "set_Name", "get_DefaultTranslations", "IsDependencyLoaded", "ExecuteDependencyCode", "Translate", "ReloadPlugin", "LoadPlugin", "UnloadPlugin", "OnEnable", "OnDisable", "Load", "Unload", "TryAddComponent", "TryRemoveComponent", "add_OnPluginsLoaded", "remove_OnPluginsLoaded", "get_Plugins", "GetPlugins", "GetPlugin", "GetPlugin", "Awake", "Start", "GetMainTypeFromAssembly", "loadPlugins", "unloadPlugins", "Reload", "GetAssembliesFromDirectory", "LoadAssembliesFromDirectory", "<Awake>b__12_0", "GetGroupsByIds", "GetParentGroups", "HasPermission", "GetGroup", "RemovePlayerFromGroup", "AddPlayerToGroup", "DeleteGroup", "SaveGroup", "AddGroup", "GetGroups", "GetPermissions", "GetPermissions", "<GetGroups>b__11_3", "Start", "FixedUpdate", "Reload", "HasPermission", "GetGroups", "GetPermissions", "GetPermissions", "AddPlayerToGroup", "RemovePlayerFromGroup", "GetGroup", "SaveGroup", "AddGroup", "DeleteGroup", "DeleteGroup", "<FixedUpdate>b__4_0", "Enqueue", "_Logger_DoWork", "processLog", "Log", "Log", "var_dump", "LogWarning", "LogError", "LogError", "Log", "LogException", "ProcessInternalLog", "logRCON", "writeToConsole", "ProcessLog", "ExternalLog", "Invoke", "_invoke", "TryInvoke", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax", "Execute", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax", "Execute", "get_Aliases", "get_AllowedCaller", "get_Help", "get_Name", "get_Permissions", "get_Syntax", "Execute", "get_Name", "set_Name", "get_Name", "set_Name", "get_Name", "get_Help", "get_Syntax", "get_AllowedCaller", "get_Commands", "set_Commands", "add_OnExecuteCommand", "remove_OnExecuteCommand", "Reload", "Awake", "checkCommandMappings", "checkDuplicateCommandMappings", "Plugins_OnPluginsLoaded", "GetCommand", "GetCommand", "getCommandIdentity", "getCommandType", "Register", "Register", "Register", "DeregisterFromAssembly", "GetCooldown", "SetCooldown", "Execute", "RegisterFromAssembly"
        };
        public static string GetRandomName()
        {
            return NormalNameStrings[Random.Next(NormalNameStrings.Length)];
        }
        public static string Ascii = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateString(RenameMode mode)
        {
            switch (mode)
            {
                case RenameMode.Ascii:
                    return RandomString(Random.Next(1, 7), Ascii);

                case RenameMode.Normal:
                    return GetRandomName();

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}