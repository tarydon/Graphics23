// LinesWin.cs - Demo window for testing the DrawLine and related functions
// ---------------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.Reflection;

namespace GrayBMP;

class LinesWin : Window {
   public LinesWin () {
      Width = 900; Height = 600;
      Left = 200; Top = 50;
      WindowStyle = WindowStyle.None;
      mBmp = new GrayBMP (Width, Height);

      Image image = new () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
         Source = mBmp.Bitmap
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);
      Content = image;
      mDX = mBmp.Width; mDY = mBmp.Height;
      LoadPolyFill ();
      // Start a timer to repaint a new frame every 33 milliseconds
      //DispatcherTimer timer = new () {
      //   Interval = TimeSpan.FromMilliseconds (100), IsEnabled = true,
      //};
      //timer.Tick += NextFrame;
   }
   readonly GrayBMP mBmp;
   readonly int mDX, mDY;

   void LoadPolyFill () {
      var leafLines = new List<int> ();
      using (new BlockTimer ("Fill Poly")) {
         using (var sr = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GrayBMP.Res.leaf-fill.txt")))
            leafLines = sr.ReadToEnd ().Replace ("\r\n", " ").TrimEnd ().Split (' ').Select (int.Parse).ToList ();
         for (int i = 0; i < leafLines.Count;) {
            if (leafLines[i + 1] != leafLines[i + 3]) // ignore horizontal lines (y1 == y2)
               Plines.Add ((leafLines[i++], leafLines[i++], leafLines[i++], leafLines[i++]));
            else i += 4;
         }
         Plines.OrderBy (p => p.Y1);
         Fill (255);
      }
   }

   public void Fill (int color) {
      double yMin = Plines.Min (p => p.Y1) + 0.5, yMax = Plines.Max (p => p.Y1) - 0.5;
      List<int> mInterSect = new ();
      for (var yScan = yMin; yScan <= yMax; yScan++) {
         mInterSect.Clear ();
         foreach (var (X1, Y1, X2, Y2) in Plines) {
            if ((Y1 < yScan && Y2 >= yScan) || (Y2 < yScan && Y1 >= yScan))
               mInterSect.Add ((int)(X1 + (yScan - Y1) / (Y2 - Y1) * (X2 - X1)));
         }
         mInterSect.Order ().ToList ();
         for (int j = 0; j < mInterSect.Count; j++)
            mBmp.DrawHorizontalLine (mInterSect[j], mInterSect[++j], (int)yScan, color);
      }
   }

   void NextFrame (object sender, EventArgs e) {
      using (new BlockTimer ("Lines")) {
         mBmp.Begin ();
         mBmp.Clear (0);
         for (int i = 0; i < 100000; i++) {
            int x0 = R.Next (mDX), y0 = R.Next (mDY),
                x1 = R.Next (mDX), y1 = R.Next (mDY),
                color = R.Next (256);
            mBmp.DrawLine (x0, y0, x1, y1, color);
         }
         mBmp.End ();
      }
   }
   Random R = new ();
   List<(int X1, int Y1, int X2, int Y2)> Plines = new ();
}

class BlockTimer : IDisposable {
   public BlockTimer (string message) {
      mStart = DateTime.Now;
      mMessage = message;
   }
   readonly DateTime mStart;
   readonly string mMessage;

   public void Dispose () {
      int elapsed = (int)((DateTime.Now - mStart).TotalMilliseconds + 0.5);
      Console.WriteLine ($"{mMessage}: {elapsed}ms");
   }
}