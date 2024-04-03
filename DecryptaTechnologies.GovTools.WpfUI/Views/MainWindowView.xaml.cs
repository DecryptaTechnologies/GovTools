using System.Windows;
using System.Windows.Input;

namespace DecryptaTechnologies.GovTools.WpfUI.Views;

/// <summary>
/// Interaction logic for MainWindowView.xaml
/// </summary>
public partial class MainWindowView : Window
{

    public MainWindowView()
    {
        InitializeComponent();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (sizeInfo.HeightChanged)
            Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;

        if (sizeInfo.WidthChanged)
            Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
    }

    //public void MoveToAnimated(UIElement target, double newX, double newY)
    //{
    //    var top = Canvas.GetTop(target);
    //    var left = Canvas.GetLeft(target);
    //    TranslateTransform trans = new TranslateTransform();
    //    target.RenderTransform = trans;
    //    DoubleAnimation anim1 = new DoubleAnimation(top, newY - top, TimeSpan.FromSeconds(10));
    //    DoubleAnimation anim2 = new DoubleAnimation(left, newX - left, TimeSpan.FromSeconds(10));
    //    trans.BeginAnimation(TranslateTransform.XProperty, anim1);
    //    trans.BeginAnimation(TranslateTransform.YProperty, anim2);
    //}

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void btnMinimizeWindow_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void btnMaximizeWindow_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
            WindowState = WindowState.Maximized;
        else
            WindowState = WindowState.Normal;
    }

    private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

}