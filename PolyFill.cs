using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GrayBMP {
   class PolyFill {
      /// <summary>
      /// Adding the lines atart point and end point with positive slopes and rejecting horizontal lines.
      /// </summary>
      /// <param name="x0"></param>
      /// <param name="y0"></param>
      /// <param name="x1"></param>
      /// <param name="y1"></param>
      public void AddLine (int x0, int y0, int x1, int y1) {
         if (y1 > y0) mLines.Add ((x0, y0, x1, y1));
         else if (y0 > y1) mLines.Add ((x1, y1, x0, y0));
      }
      /// <summary>
      /// Sorting the line list with incrementing values of Y
      /// </summary>
      public void Sort () {
         mLines = mLines.OrderBy (a => a.Y0).ToList();
      }

      public void Fill (GrayBMP bmp, int color) {
         int LineCount = mLines.Count;
         int Startindex = 0;
         for (double i = 0.5; i < bmp.Height; i++) {
            //Adding the lines with in range for intersection and storing the index for next start point. 
            for (int j = Startindex; j < LineCount; j++) {
               if (mLines[j].Y0 < i) {
                  mCK.Add (mLines[j]);
                  Startindex++;
               } else break;
            }
            //Removing the lines out of range and calculating the intersection point for lines in range.
            var x = new List<int> ();
            for (int k = mCK.Count - 1; k >= 0; k--) {
               if (mCK[k].Y1 < i) mCK.RemoveAt (k);
               else x.Add (mCK[k].X0 + (((mCK[k].X1 - mCK[k].X0) * ((int)i - mCK[k].Y0)) / (mCK[k].Y1 - mCK[k].Y0)));
            }
            // sorting the points of intersection and drawing line
            x = x.Order ().ToList ();
            for (int l = 0; l < x.Count; l += 2) bmp.DrawLine (x[l], (int)i, x[l + 1], (int)i, 225);
         }
      }
      List<(int X0, int Y0, int X1, int Y1)> mLines = new (), mCK = new();
   }


}
