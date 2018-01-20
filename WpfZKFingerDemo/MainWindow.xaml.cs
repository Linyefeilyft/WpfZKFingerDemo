using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AxZKFPEngXControl;
using System.Drawing;

namespace WpfZKFingerDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private AxZKFPEngX ZKFPEngX1;
        private int fpcHandle;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initFinger();
        }

        private void initFinger()
        {
            if (null == ZKFPEngX1)
            {
                //实例化OCX控件
                ZKFPEngX1 = new AxZKFPEngXControl.AxZKFPEngX();
                //显式调用该实例的CreateControl方法才能调用其它方法，否则在调用时系统将报AxHost.InvalidActiveXStateException异常
                ZKFPEngX1.CreateControl();
                //连接多个指纹采集器时，选择指纹头的序号，从0 开始；小于零时指纹采集器不工作
                ZKFPEngX1.SensorIndex = 0;
                //取得指纹初始特征
                ZKFPEngX1.OnFeatureInfo += new IZKFPEngXEvents_OnFeatureInfoEventHandler(ZKFPEngX1_OnFeatureInfo);
                ZKFPEngX1.OnEnroll += new IZKFPEngXEvents_OnEnrollEventHandler(ZKFPEngX1_OnEnroll);
                //指纹图片接收
                ZKFPEngX1.OnImageReceived += new IZKFPEngXEvents_OnImageReceivedEventHandler(ZKFPEngX1_OnImageReceived);
                //指纹识别系统算法引擎版本号
                ZKFPEngX1.FPEngineVersion = "9";
                ZKFPEngX1.Threshold = 5;
            }
            if (!ZKFPEngX1.EngineValid)
            {
                //初始化指纹识别系统
                int result = ZKFPEngX1.InitEngine();

                if (-2 == result)
                {
                    result = ZKFPEngX1.InitEngine();
                }
                //初始化成功
                if (0 == result)
                {
                    //创建指纹识别高速缓冲空间 并返回其句柄
                    CreateFPCacheDBEx();
                }
                else if (1 == result)
                {
                    endFinger();
                    throw new Exception("指纹识别驱动程序加载失败");
                }
                else if (2 == result)
                {
                    endFinger();
                    throw new Exception("没有连接指纹识别仪");
                }
                else
                {
                    endFinger();
                    throw new Exception("指定的指纹仪不存在");
                }
            }
            ZKFPEngX1.FlushFPImages();
        }

        void ZKFPEngX1_OnEnroll(object sender, IZKFPEngXEvents_OnEnrollEvent e)
        {
            
        }

        private void ZKFPEngX1_OnFeatureInfo(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnFeatureInfoEvent e)
        {
            String strTemp = "Fingerprint quality";
            if (e.aQuality != 0)
            {
                strTemp = strTemp + " not good";
            }
            else
            {
                strTemp = strTemp + " good";
            }
            if (ZKFPEngX1.EnrollIndex != 1)
            {
                if (ZKFPEngX1.IsRegister)
                {
                    if (ZKFPEngX1.EnrollIndex - 1 > 0)
                    {
                        strTemp = strTemp + '\n' + " Register status: still press finger " + Convert.ToString(ZKFPEngX1.EnrollIndex - 1) + " times!";
                    }
                }
            }
            ShowHintInfo(strTemp);
        }

        //获取指纹图像并在窗口中实时显示
        private void ZKFPEngX1_OnImageReceived(object sender, AxZKFPEngXControl.IZKFPEngXEvents_OnImageReceivedEvent e)
        {
            Bitmap bmp = new Bitmap(150, 138);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                int dc = g.GetHdc().ToInt32();
                ZKFPEngX1.PrintImageAt(dc, 0, 0, bmp.Width, bmp.Height);
            }
            image1.Source = BitmapToBitmapSource(bmp);
            bmp.Dispose();
            bmp = null;    
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //release resource  
            DeleteObject(ptr);

            return result;
        }  

        private void CreateFPCacheDBEx()
        {
            fpcHandle = ZKFPEngX1.CreateFPCacheDB();
        }

        private void endFinger()
        {
            ZKFPEngX1.EndEngine();
        }

        private void ShowHintInfo(String s)
        {
            if (s != "")
            {
                listBox1.Items.Add(s);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            initFinger();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ZKFPEngX1.CancelEnroll();
            ZKFPEngX1.EnrollCount = 3;
            ZKFPEngX1.BeginEnroll();
            ShowHintInfo("Begin Register");
        }
    }
}
