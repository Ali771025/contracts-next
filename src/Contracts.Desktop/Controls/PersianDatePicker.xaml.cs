namespace Contracts.Desktop.Controls;

using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

public partial class PersianDatePicker : UserControl
{
    private static readonly PersianCalendar Calendar = new();
    private static readonly string[] MonthNames = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"];
    private int visibleYear;
    private int visibleMonth;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(PersianDatePicker), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public PersianDatePicker()
    {
        InitializeComponent();
        SetVisibleMonth(DateTime.Today);
    }

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    private void OpenCalendar(object sender, RoutedEventArgs e)
    {
        if (TryParse(Text, out DateTime selected)) SetVisibleMonth(selected);
        else SetVisibleMonth(DateTime.Today);
        RenderMonth(); CalendarPopup.IsOpen = true;
    }

    private void PreviousMonth(object sender, RoutedEventArgs e) { MoveMonth(-1); RenderMonth(); }
    private void NextMonth(object sender, RoutedEventArgs e) { MoveMonth(1); RenderMonth(); }
    private void SelectToday(object sender, RoutedEventArgs e) { SelectDate(DateTime.Today); }
    private void ClearDate(object sender, RoutedEventArgs e) { Text = ""; CalendarPopup.IsOpen = false; }

    private void MoveMonth(int delta)
    {
        int index = checked(visibleYear * 12 + visibleMonth - 1 + delta);
        visibleYear = Math.DivRem(index, 12, out int monthIndex); visibleMonth = monthIndex + 1;
    }

    private void SetVisibleMonth(DateTime date) { visibleYear = Calendar.GetYear(date); visibleMonth = Calendar.GetMonth(date); }

    private void RenderMonth()
    {
        MonthTitle.Text = $"{MonthNames[visibleMonth - 1]} {visibleYear}";
        DaysGrid.Children.Clear();
        DateTime first = Calendar.ToDateTime(visibleYear, visibleMonth, 1, 0, 0, 0, 0);
        int offset = ((int)first.DayOfWeek + 1) % 7;
        for (int index = 0; index < offset; index++) DaysGrid.Children.Add(new Border());
        int days = Calendar.GetDaysInMonth(visibleYear, visibleMonth);
        for (int day = 1; day <= days; day++)
        {
            DateTime date = Calendar.ToDateTime(visibleYear, visibleMonth, day, 0, 0, 0, 0);
            Button button = new() { Content = day.ToString(CultureInfo.InvariantCulture), MinWidth = 34, MinHeight = 32, Padding = new(2), Margin = new(1), Tag = date };
            button.Click += SelectDay; DaysGrid.Children.Add(button);
        }
        while (DaysGrid.Children.Count < 42) DaysGrid.Children.Add(new Border());
    }

    private void SelectDay(object sender, RoutedEventArgs e) { if (sender is Button { Tag: DateTime date }) SelectDate(date); }
    private void SelectDate(DateTime date)
    {
        Text = $"{Calendar.GetYear(date):0000}/{Calendar.GetMonth(date):00}/{Calendar.GetDayOfMonth(date):00}";
        CalendarPopup.IsOpen = false;
    }

    private static bool TryParse(string? value, out DateTime result)
    {
        result = default; if (string.IsNullOrWhiteSpace(value)) return false;
        string[] parts = value.Split('/');
        try { if (parts.Length == 3) { result = Calendar.ToDateTime(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture), int.Parse(parts[2], CultureInfo.InvariantCulture), 0, 0, 0, 0); return true; } }
        catch (ArgumentOutOfRangeException) { }
        catch (FormatException) { }
        return false;
    }
}
