using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new Image () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);

      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;
      MouseDown += OnMouseDown;
      //Octagon
      DrawLine (new Point (300, 100), new Point (400,100));
      DrawLine (new Point (400, 100), new Point (500,200));
      DrawLine (new Point (500, 200), new Point (500, 300));
      DrawLine (new Point (500, 300), new Point (400, 400));
      DrawLine (new Point (400, 400), new Point (300, 400));
      DrawLine (new Point (300, 400), new Point (200, 300));
      DrawLine (new Point (200, 300), new Point (200, 200));
      DrawLine (new Point (200, 200), new Point (300, 100));
      //DrawMandelbrot (-0.5, 0, 1);
   }

   void OnMouseDown (object sender, MouseButtonEventArgs e) {
      var pos = e.GetPosition (this);
      if (mPrev == null) { mPrev = pos; return; }
      DrawLine (mPrev.Value, pos);
      mPrev = null;
   }
   Point? mPrev;

   void DrawLine (Point a, Point b) {
      int x0 = (int)a.X, y0 = (int)a.Y, x1 = (int)b.X, y1 = (int)b.Y;
      int dx = x1 - x0, dy = y1 - y0, add = 1;
      bool swap = Math.Abs (dy) > Math.Abs (dx);
      if (swap) (x0, y0, x1, y1) = (y0, x0, y1, x1);
      if (x1 < x0) (x0, y0, x1, y1) = (x1, y1, x0, y0);
      (dx, dy) = (x1 - x0, y1 - y0);
      if (y1 < y0) { add = -1; dy = Math.Abs (dy); }
      int y = y0, d = 2 * dy - dx;
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = x0; x <= x1; x++) {
            if (swap) SetPixel (y, x, 255);
            else SetPixel (x, y, 255);
            if (d >= 0) { y = y + add; d += 2 * (dy - dx); } else d += 2 * dy;
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, mBmp.PixelWidth, mBmp.PixelHeight));
      } finally {
         mBmp.Unlock ();
      }
   }

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new Complex (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      } finally {
         mBmp.Unlock ();
      }
   }

   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            var pt = e.GetPosition (this);
            int x = (int)pt.X, y = (int)pt.Y;
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         } finally {
            mBmp.Unlock ();
         }
      }
   }

   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      } finally {
         mBmp.Unlock ();
      }
   }

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
