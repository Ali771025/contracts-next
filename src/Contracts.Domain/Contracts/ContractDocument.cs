namespace Contracts.Domain.Contracts;

public sealed class ContractDocument : AuditedEntity
{
    private ContractDocument()
    {
        OriginalFileName=StoredFileName=RelativePath=string.Empty;
    }

    public ContractDocument(long contractId,long documentTypeId,string originalFileName,string storedFileName,
        string relativePath,long fileSize,string? contentHashSha256,AuditStamp stamp):base(stamp)
    {
        ContractId=Guard.RequiredId(contractId,nameof(contractId));DocumentTypeId=Guard.RequiredId(documentTypeId,nameof(documentTypeId));
        OriginalFileName=FileName(originalFileName,nameof(originalFileName));StoredFileName=FileName(storedFileName,nameof(storedFileName));
        RelativePath=ValidateRelativePath(relativePath);FileSize=NonnegativeSize(fileSize);
        ContentHashSha256=ValidateHash(contentHashSha256);
    }

    public long ContractId{get;private set;} public long DocumentTypeId{get;private set;}
    public string OriginalFileName{get;private set;} public string StoredFileName{get;private set;}
    public string RelativePath{get;private set;} public long FileSize{get;private set;}
    public string? ContentHashSha256{get;private set;}

    public void Replace(long documentTypeId,string originalFileName,string storedFileName,string relativePath,
        long fileSize,string hash,AuditStamp stamp)
    {DocumentTypeId=Guard.RequiredId(documentTypeId,nameof(documentTypeId));OriginalFileName=FileName(originalFileName,nameof(originalFileName));StoredFileName=FileName(storedFileName,nameof(storedFileName));RelativePath=ValidateRelativePath(relativePath);FileSize=NonnegativeSize(fileSize);ContentHashSha256=ValidateHash(hash);Touch(stamp);}

    private static string FileName(string value,string parameter){string name=Guard.RequiredText(value,parameter,260);if(name!=Path.GetFileName(name))throw new ArgumentException("A file name cannot contain a path.",parameter);return name;}
    private static string ValidateRelativePath(string value){string path=Guard.RequiredText(value,nameof(value),1000).Replace('\\','/');if(path.StartsWith('/')||path.Contains(':',StringComparison.Ordinal)||path.Split('/').Any(x=>x is "" or "." or ".."))throw new ArgumentException("Document path must be a safe relative path.",nameof(value));return path;}
    private static long NonnegativeSize(long value)=>value>=0?value:throw new ArgumentOutOfRangeException(nameof(value),"File size cannot be negative.");
    private static string? ValidateHash(string? value){if(string.IsNullOrWhiteSpace(value))return null;string hash=Guard.RequiredText(value,nameof(value),64).ToUpperInvariant();if(hash.Length!=64||hash.Any(c=>!Uri.IsHexDigit(c)))throw new ArgumentException("SHA-256 must be 64 hexadecimal characters.",nameof(value));return hash;}
}
