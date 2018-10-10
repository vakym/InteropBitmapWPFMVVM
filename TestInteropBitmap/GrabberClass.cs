using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;

namespace TestInteropBitmap
{
    public class GrabberClass :BaseViewModel
    {
        /// <summary>
        /// Класс, который отвечает за захват видеопотока.
        /// </summary>
        private VideoCapture Capture;
        
        private InteropBitmap frame;
        public InteropBitmap Frame
        {
            get { return frame; }
            private set
            {
                frame = value;
                OnPropertyChanged();
            }
        }
        private Mat MatFrame = new Mat();
        private IntPtr source;
        private IntPtr map;
        private uint pcount;
        public bool StartStream(string url)
        {
            ///Оборачиваем конструктор класса VideoCapture в задачу, и ждем завершения 5 секунд.
            ///Это необходимо, т.к. у VideoCapture нет таймаута и если камера не ответила, то приложение зависнет. 
            var CaptureTask = Task.Factory.StartNew(() => Capture = new VideoCapture(url));
            if(!CaptureTask.Wait(new TimeSpan(0,0,5)))
            {
                return false;
            }
            ///Получаем первый кадр с камеры
            Capture.Grab();
            ///Если удачно записали его в переменную, то продолжаем
            if(!Capture.Retrieve(MatFrame,3))
            {
                return false;
            }
            ///Узнаем формат пикселей кадра
            System.Windows.Media.PixelFormat pixelFormat = GetPixelFormat(MatFrame);
            ///Определяем сколько места занимает один кадр
            pcount = (uint)(MatFrame.Width * MatFrame.Height * pixelFormat.BitsPerPixel / 8);
            ///Создаем объект в памяти
            source = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, pcount, null);
            ///Получаем ссылку на начальный адрес объекта
            map = MapViewOfFile(source, 0xF001F, 0, 0, pcount);
            ///Инициализируем InteropBitmap используя созданный выше объект в памяти
            Frame = Imaging.CreateBitmapSourceFromMemorySection(source, MatFrame.Width, MatFrame.Height, pixelFormat, MatFrame.Width * pixelFormat.BitsPerPixel / 8, 0) as InteropBitmap;
            Capture.ImageGrabbed += Capture_ImageGrabbed;
            Capture.Start();
            return true;
        }
        public void StopStream()
        {
            Capture.Stop();

        }
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            Capture.Retrieve(MatFrame, 3);
            CopyMemory(map, MatFrame.DataPointer, (int)pcount);
            Application.Current.Dispatcher.Invoke(() => Frame.Invalidate());
        }
        private System.Windows.Media.PixelFormat GetPixelFormat(Mat mat)
        {
            switch (mat.Bitmap.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgr32;
                default:
                    throw new Exception("Unexpected pixel format");
            }
        }
        #region Extern Libs
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
        #endregion
    }
}
