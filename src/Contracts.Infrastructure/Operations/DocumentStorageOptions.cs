namespace Contracts.Infrastructure.Operations;

public sealed class DocumentStorageOptions
{
    public string RootPath{get;set;}=Path.Combine(AppContext.BaseDirectory,"Data","Documents");
    public long MaxFileSizeBytes{get;set;}=25L*1024*1024;
    public HashSet<string> AllowedExtensions{get;set;}=new(StringComparer.OrdinalIgnoreCase){".pdf",".doc",".docx",".xls",".xlsx",".png",".jpg",".jpeg"};
}
