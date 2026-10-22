namespace Contracts.Infrastructure.Administration;

using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.Versioning;

public sealed record ApplicationPaths(string RootPath,string DatabasePath,string DocumentsPath,string ConfigurationPath,string DefaultBackupPath)
{
    public string MaskedRoot=>$"{Path.GetPathRoot(RootPath)}…{Path.DirectorySeparatorChar}{Path.GetFileName(RootPath)}";
}

public static class ApplicationPathPolicy
{
    public static ApplicationPaths Resolve(IConfiguration configuration)
    {
        string? configured=configuration["Paths:Root"];
        string root=string.IsNullOrWhiteSpace(configured)?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ContractsNext"):configured;
        if(!Path.IsPathFullyQualified(root))throw new InvalidOperationException("Paths:Root must be an absolute local path.");
        root=Path.GetFullPath(root);EnsureLocal(root,"application data");
        string data=Child(root,"Data"),documents=Child(root,"Documents"),config=Child(root,"Config"),backups=Child(root,"Backups");
        return new(root,Child(data,"contracts.db"),documents,Child(config,"settings.json"),backups);
    }
    public static string ValidateBackupPath(string path,string documentsPath)
    {
        if(!Path.IsPathFullyQualified(path))throw new InvalidOperationException("Backup path must be absolute.");string full=Path.GetFullPath(path);EnsureLocal(full,"backup");
        string docs=Path.TrimEndingDirectorySeparator(Path.GetFullPath(documentsPath))+Path.DirectorySeparatorChar;
        if((Path.TrimEndingDirectorySeparator(full)+Path.DirectorySeparatorChar).StartsWith(docs,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Backup path cannot be inside document storage.");return full;
    }
    public static string ValidateDatabasePath(string path)
    {if(!Path.IsPathFullyQualified(path))throw new InvalidOperationException("SQLite DataSource must be absolute (or :memory: for tests).");string full=Path.GetFullPath(path);EnsureLocal(full,"SQLite database");return full;}
    public static string ValidateLocalFilePath(string path,string label)
    {if(!Path.IsPathFullyQualified(path))throw new InvalidOperationException($"{label} path must be absolute.");string full=Path.GetFullPath(path);EnsureLocal(full,label);return full;}
    public static void EnsurePrivateDirectory(string path)
    {
        Directory.CreateDirectory(path);if(!OperatingSystem.IsWindows())return;SecurityIdentifier user=WindowsIdentity.GetCurrent().User??throw new InvalidOperationException("Current Windows identity has no SID.");
        DirectorySecurity acl=new();acl.SetAccessRuleProtection(true,false);InheritanceFlags inherit=InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit;
        acl.AddAccessRule(new(user,FileSystemRights.FullControl,inherit,PropagationFlags.None,AccessControlType.Allow));acl.AddAccessRule(new(new SecurityIdentifier(WellKnownSidType.LocalSystemSid,null),FileSystemRights.FullControl,inherit,PropagationFlags.None,AccessControlType.Allow));acl.AddAccessRule(new(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid,null),FileSystemRights.FullControl,inherit,PropagationFlags.None,AccessControlType.Allow));new DirectoryInfo(path).SetAccessControl(acl);
    }
    public static bool HasBroadWriteAccess(string path)
    {
        return OperatingSystem.IsWindows()&&HasBroadWriteAccessWindows(path);
    }
    [SupportedOSPlatform("windows")]
    private static bool HasBroadWriteAccessWindows(string path)
    {
        SecurityIdentifier everyone=new(WellKnownSidType.WorldSid,null),users=new(WellKnownSidType.BuiltinUsersSid,null),authenticated=new(WellKnownSidType.AuthenticatedUserSid,null);
        FileSystemRights write=FileSystemRights.Write|FileSystemRights.Modify|FileSystemRights.FullControl;return new DirectoryInfo(path).GetAccessControl().GetAccessRules(true,true,typeof(SecurityIdentifier)).OfType<FileSystemAccessRule>().Any(rule=>rule.AccessControlType==AccessControlType.Allow&&(rule.FileSystemRights&write)!=0&&rule.IdentityReference is SecurityIdentifier sid&&(sid==everyone||sid==users||sid==authenticated));
    }
    private static string Child(string root,string name){string value=Path.GetFullPath(Path.Combine(root,name));string prefix=Path.TrimEndingDirectorySeparator(root)+Path.DirectorySeparatorChar;if(!value.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Application path escaped its root.");return value;}
    private static void EnsureLocal(string path,string label)
    {if(new Uri(path).IsUnc)throw new NotSupportedException($"Network-share {label} is unsupported.");string? drive=Path.GetPathRoot(path);if(!string.IsNullOrEmpty(drive)&&new DriveInfo(drive).DriveType==DriveType.Network)throw new NotSupportedException($"Network-drive {label} is unsupported.");}
}
