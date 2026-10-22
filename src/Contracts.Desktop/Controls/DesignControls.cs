namespace Contracts.Desktop.Controls;

using System.Windows.Controls;
using System.Windows.Input;

public class PageHeader : HeaderedContentControl { }
public class FilterPanel : HeaderedContentControl { }
public class FormField : HeaderedContentControl { }
public class SearchBox : TextBox { }
public class MoneyField : TextBox { }
public class StatusBadge : ContentControl { }
public class EmptyState : ContentControl { }
public class ErrorBanner : ContentControl { }
public class Toast : ContentControl { }

public class LoadingOverlay : ContentControl
{
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
        nameof(IsLoading), typeof(bool), typeof(LoadingOverlay), new PropertyMetadata(false));
    public bool IsLoading { get => (bool)GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
}

public class RecordNavigator : Control
{
    public static readonly DependencyProperty PreviousCommandProperty = DependencyProperty.Register(
        nameof(PreviousCommand), typeof(ICommand), typeof(RecordNavigator));
    public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
        nameof(NextCommand), typeof(ICommand), typeof(RecordNavigator));
    public static readonly DependencyProperty CanMovePreviousProperty = DependencyProperty.Register(
        nameof(CanMovePrevious), typeof(bool), typeof(RecordNavigator), new PropertyMetadata(true));
    public static readonly DependencyProperty CanMoveNextProperty = DependencyProperty.Register(
        nameof(CanMoveNext), typeof(bool), typeof(RecordNavigator), new PropertyMetadata(true));
    public ICommand? PreviousCommand { get => (ICommand?)GetValue(PreviousCommandProperty); set => SetValue(PreviousCommandProperty, value); }
    public ICommand? NextCommand { get => (ICommand?)GetValue(NextCommandProperty); set => SetValue(NextCommandProperty, value); }
    public bool CanMovePrevious { get => (bool)GetValue(CanMovePreviousProperty); set => SetValue(CanMovePreviousProperty, value); }
    public bool CanMoveNext { get => (bool)GetValue(CanMoveNextProperty); set => SetValue(CanMoveNextProperty, value); }
}

