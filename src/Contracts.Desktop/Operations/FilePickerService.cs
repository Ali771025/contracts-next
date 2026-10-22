namespace Contracts.Desktop.Operations;

public interface IFilePickerService
{
    public string? PickDocument();
}

public sealed class WpfFilePickerService : IFilePickerService
{
    public string? PickDocument()
    {
        Microsoft.Win32.OpenFileDialog dialog = new()
        {
            CheckFileExists = true,
            Multiselect = false,
            Filter = "اسناد مجاز|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.png;*.jpg;*.jpeg|همه فایل‌ها|*.*",
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
