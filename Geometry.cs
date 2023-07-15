// Geometry.cs - Contains some basic Geometry structs (Complex numbers, Points, Vectors)
// ---------------------------------------------------------------------------------------

using static System.Math;
namespace GrayBMP;

/// <summary>A number in the complex plane of the form (X + iY)</summary>
readonly struct Complex {
   public Complex (double x, double y) => (X, Y) = (x, y);
   public override string ToString () => $"{X} + i{Y}";

   public double Norm => Math.Sqrt (X * X + Y * Y);
   public double NormSq => X * X + Y * Y;

   public static readonly Complex Zero = new (0, 0);

   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);

   public readonly double X, Y;
}

/// <summary>A point in 2D space, with double-precision coordinates (X, Y)</summary>
readonly record struct Point2 (double X, double Y) {
   public (int X, int Y) Round () => ((int)(X + 0.5), (int)(Y + 0.5));
   /// <summary>Makes a radial move with the given radius and angle</summary>
   public Point2 RadialMove (double r, double ang) => new (X + r * Math.Cos (ang), Y + r * Math.Sin (ang));
}

/// <summary>A Line in 2 dimensions (A -> B)</summary>
readonly record struct Line (Point2 A, Point2 B);

/// <summary>A drawing is a collection of lines</summary>
class Drawing {
   public void AddLine (Line line) => mLines.Add (line);

   public IReadOnlyList<Line> Lines => mLines;
   List<Line> mLines = new ();
}

public static class Geo {
   /// <summary>The 60 degreee constant (in radians)</summary>
   public const double OneThirdPI = PI / 3;
   /// <summary>The 90 degree constant (in radians)</summary>
   public const double HalfPI = PI / 2;
}
