using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace 水平仪_WP_Runtime
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 

    public sealed partial class MainPage : Page
    {
        private ApplicationDataContainer appSetting;
        private DisplayRequest dispRequest = null;
        Accelerometer accelerometer;

        double oldAccX = 0;
        double oldAccY = 0;
        double oldAccZ = 0;

        double pcAccX = 0;
        double pcAccY = 0;
        double pcAccZ = 0;

        public MainPage()
        {
            appSetting = ApplicationData.Current.LocalSettings; //本地存储
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            setUI();
            // 页面加载触发加速度控制小球的运动
            Loaded += MainPage_Loaded;
        }

        private void setUI()
        {
            BackGroundCanvas.Width = Window.Current.Bounds.Width * 0.8;
            BackGroundCanvas.Margin = new Thickness(0, (Window.Current.Bounds.Height - BackGroundCanvas.Width) / 4, 0, 0);

            BackGroundImage.Width = Window.Current.Bounds.Width * 0.8;
            BallImage.Width = BackGroundCanvas.Width * 0.1388;

            if (!(bool)appSetting.Values["showPF"])
            {
                Favorite.Visibility = Visibility.Collapsed;
            }
            if (dispRequest == null)
            {
                dispRequest = new DisplayRequest();
                dispRequest.RequestActive();
            }


            BallImage.Margin = new Thickness(BackGroundCanvas.Width / 2 - BallImage.Width / 2, BackGroundCanvas.Width / 2 - BallImage.Width / 2, 0, 0);
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取加速度计对象
            accelerometer = Accelerometer.GetDefault();
            if (accelerometer == null)
            {
                await new MessageDialog("不支持加速度计传感器").ShowAsync();
            }
            // 设置获取数据的时间间隔
            accelerometer.ReportInterval = accelerometer.MinimumReportInterval;
            // 订阅加速度变化的事件
            accelerometer.ReadingChanged += accelerometer_ReadingChanged;
            Debug.WriteLine(accelerometer.MinimumReportInterval);
        }

        // 加速度变化的事件处理程序
        async void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MyReadingChanged(args.Reading);
            });
        }

        // 通过加速度控制小球的运动
        private void MyReadingChanged(AccelerometerReading e)
        {
#if DEBUG
            XTextBlock.Text = "X：" + e.AccelerationX;
            YTextBlock.Text = "Y：" + e.AccelerationY;
            ZTextBlock.Text = "Z：" + e.AccelerationZ;
#endif

            double accelerationFactor = Math.Abs(e.AccelerationZ) == 0 ? 0.1 : Math.Abs(e.AccelerationZ);

            pcAccX = e.AccelerationX;
            pcAccY = e.AccelerationY;
            pcAccZ = e.AccelerationZ;

            double AccX = e.AccelerationX - double.Parse(appSetting.Values["pcX"].ToString());
            double AccY = e.AccelerationY - double.Parse(appSetting.Values["pcY"].ToString());
            double AccZ = e.AccelerationZ - double.Parse(appSetting.Values["pcZ"].ToString());

            if (Math.Abs(AccX - oldAccX) > 0.02 || Math.Abs(AccY - oldAccY) > 0.02 || Math.Abs(AccZ - oldAccZ) > 0.02)
            {
                double SumWidth = BackGroundCanvas.Width / 2 - BallImage.Width / 2;

                double MarX;
                double MarY;
                double RenX = AccX / 1 * 90; ;
                double RenY = AccY / 1 * 90; ;

                if (AccX < 0)
                {
                    MarX = SumWidth + (-AccX) / 1 * SumWidth;
                }
                else
                {
                    MarX = SumWidth - AccX / 1 * SumWidth;
                }
                if (AccY < 0)
                {
                    MarY = SumWidth + AccY / 1 * SumWidth;
                }
                else
                {
                    MarY = SumWidth + AccY / 1 * SumWidth;
                }

                BallImage.Margin = new Thickness(MarX, MarY, 0, 0);
                phone1Image.RenderTransform.SetValue(RotateTransform.AngleProperty, RenX);
                phone2Image.RenderTransform.SetValue(RotateTransform.AngleProperty, RenY);
                Angle1TextBlock.Text = String.Format("{0:F}", RenX) + "°";
                Angle2TextBlock.Text = String.Format("{0:F}", RenY) + "°";

                oldAccX = AccX;
                oldAccY = AccY;
                oldAccZ = AccZ;

            }

            if (Math.Abs(AccX) > 0.15 || Math.Abs(AccY) > 0.15 || Math.Abs(AccZ) > 0.15)
            {
                Setting.IsEnabled = false;
            }
            else
            {
                Setting.IsEnabled = true;
            }
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: 准备此处显示的页面。

            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件

        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            accelerometer = null;
            Application.Current.Exit();
        }

        private async void Favorite_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("zune:reviewapp?appid=" + CurrentApp.AppId)); //用于商店app，自动获取ID
            appSetting.Values["showPF"] = false;
            Favorite.Visibility = Visibility.Collapsed;
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            appSetting.Values["pcX"] = pcAccX;
            appSetting.Values["pcY"] = pcAccY;
            appSetting.Values["pcZ"] = pcAccZ;
        }
    }
}
