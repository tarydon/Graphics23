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
      //DrawMandelbrot (-0.5, 0, 1);
   }


    void OnMouseDown (object s, MouseEventArgs e) {
        var pt = e.GetPosition (this);
        if (mStart.X == -1) {
            mStart = pt;
            return;
        }
        DrawLine ((int)mStart.X, (int)mStart.Y, (int)pt.X, (int)pt.Y);
        mStart = new Point (-1, -1);
    }
    Point mStart = new(-1, -1);

    public void DrawLine (int x1, int y1, int x2, int y2) {
        try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            int width = x2 - x1, height = y2 - y1;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (width < 0) dx1 = -1; else if (width > 0) dx1 = 1;
            if (height < 0) dy1 = -1; else if (height > 0) dy1 = 1;
            if (width < 0) dx2 = -1; else if (width > 0) dx2 = 1;
            int width2 = Math.Abs (width);
            int height2 = Math.Abs (height);
            var rect = new Int32Rect (width > 0 ? x1 : x2, height > 0 ? y1 : y2, width2, height2);
            if (!(width2 > height2)) {
                width2 = Math.Abs (height);
                height2 = Math.Abs (width);
                if (height < 0) dy2 = -1; else if (height > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = width2 / 2;
            for (int i = 0; i <= width2; i++) {
                SetPixel (x1, y1, 255);
                numerator += height2;
                if (!(numerator < width2)) {
                    numerator -= width2;
                    x1 += dx1;
                    y1 += dy1;
                } else {
                    x1 += dx2;
                    y1 += dy2;
                }
            }
            mBmp.AddDirtyRect (rect);
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
