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
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace WpfApplication1
{

    public partial class ColorWindow : Window
    {
        KinectSensor kinect;
        public ColorWindow(KinectSensor sensor) : this()
        {
            kinect = sensor;
        }

        public ColorWindow()
        {
            InitializeComponent();
            Loaded += ColorWindow_Loaded;
            Unloaded += ColorWindow_Unloaded;
        }
        void ColorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                kinect.ColorStream.Disable();
                kinect.DepthStream.Disable();
                kinect.Stop();
                kinect.ColorFrameReady -= myKinect_ColorFrameReady;
                kinect.DepthFrameReady -= mykinect_DepthFrameReady;
            }
        }
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;

        private WriteableBitmap _DepthImageBitmap;
        private Int32Rect _DepthImageBitmapRect;
        private int _DepthImageStride;
        void ColorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                #region 處理彩色影像
                ColorImageStream colorStream = kinect.ColorStream;
                kinect.ColorStream.Enable();
                _ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,colorStream.FrameHeight, 96, 96,PixelFormats.Bgr32, null);
                _ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,colorStream.FrameHeight);
                _ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorData.Source = _ColorImageBitmap;
                kinect.ColorFrameReady += myKinect_ColorFrameReady;
                #endregion

                DepthImageStream depthStream = kinect.DepthStream;
                kinect.DepthStream.Enable();
                _DepthImageBitmap = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 
                    96, 96, PixelFormats.Gray16, null);
                _DepthImageBitmapRect = new Int32Rect(0, 0, depthStream.FrameWidth, depthStream.FrameHeight);
                _DepthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                DepthData.Source = _DepthImageBitmap;              
                kinect.DepthFrameReady += mykinect_DepthFrameReady;

                kinect.Start();
            }
        }

        short[] pixelData;
        void mykinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;

                pixelData = new short[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixelData);
                _DepthImageBitmap.WritePixels(_DepthImageBitmapRect, pixelData, _DepthImageStride, 0);
            }
        }

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return ;

                byte[] colorpixelData = new byte[frame.PixelDataLength];
                frame.CopyPixelDataTo(colorpixelData);
                _ColorImageBitmap.WritePixels(_ColorImageBitmapRect, colorpixelData,_ColorImageStride, 0);
            }
        }

        private void ColorData_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(ColorData);

            if (pixelData != null && pixelData.Length > 0)
            {
                int pixelIndex = (int)(p.X + ((int)p.Y * kinect.DepthStream.FrameWidth));
                int depth = pixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                Title = "影像深度 : " + depth + " 公厘(mm)";
            }
        }
    }
}
