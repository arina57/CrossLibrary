using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossLibrary {
    public static class MathAndGeometry {


        public static List<int> GetRandomNumbers(int minInclusive, int maxExclusive, int number) {
            if (maxExclusive - minInclusive <= number) {
                throw new Exception($"Cannot not generate more than possible combinations. Min: {minInclusive}, Max: {maxExclusive} Combinations: {maxExclusive - minInclusive}, Number: {number}");
            }
            Random random = new Random();
            var randomNumbers = new List<int>();
            for (int i = 0; i < number; i++) {
                var randomNumber = random.Next(minInclusive, maxExclusive);
                while (randomNumbers.Contains(randomNumber)) {
                    randomNumber = random.Next(minInclusive, maxExclusive);
                }
                randomNumbers.Add(randomNumber);
            }

            return randomNumbers;
        }


        public static List<int> GetRandomNumbers(Random random, int minInclusive, int maxExclusive, int number, params int[] numberToExclude) {
            if (maxExclusive - minInclusive - numberToExclude.Length < number) {
                throw new Exception($"Cannot not generate more than possible combinations. Min: {minInclusive}, Max: {maxExclusive} Combinations: {maxExclusive - minInclusive}, Number: {number}");
            }
            var randomNumbers = new List<int>();
            for (int i = 0; i < number; i++) {
                var randomNumber = random.Next(minInclusive, maxExclusive);
                while (randomNumbers.Contains(randomNumber) || numberToExclude.Contains(randomNumber)) {
                    randomNumber = random.Next(minInclusive, maxExclusive);
                }
                randomNumbers.Add(randomNumber);
            }

            return randomNumbers;
        }

        public enum Direction {
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            None = 0
        }

        public enum VerticalPostion {
            Unspecified = -1,
            Top = 0,
            Center = 1,
            Bottom = 2

        }

        public enum HorizontalPosition {
            Unspecified = -1,
            Left = 0,
            Center = 1,
            Right = 2
        }

        public enum Corner {
            TopLeft = 0,
            TopRight = 1,
            BottomRight = 2,
            BottomLeft = 3
        }

        public enum Side {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }


        public static VerticalPostion GetVerticalPostion(FloatRect innerRect, FloatRect outerRect) {
            var rectVertCenterPoint = GetPoint(innerRect, VerticalPostion.Center, HorizontalPosition.Center).Y;
            var screenHeight = outerRect.Height;
            if (rectVertCenterPoint < screenHeight / 3.0) {
                return VerticalPostion.Top;
            } else if (rectVertCenterPoint > screenHeight * 2.0 / 3.0) {
                return VerticalPostion.Bottom;
            } else {
                return VerticalPostion.Center;
            }

        }
        public static HorizontalPosition GetHorizontalPosition(FloatRect innerRect, FloatRect outerRect) {
            var rectHorzCenterPoint = GetPoint(innerRect, VerticalPostion.Center, HorizontalPosition.Center).X;
            var screenWidth = outerRect.Width;
            if (rectHorzCenterPoint < screenWidth / 3.0) {
                return HorizontalPosition.Left;
            } else if (rectHorzCenterPoint > screenWidth * 2.0 / 3.0) {
                return HorizontalPosition.Right;
            } else {
                return HorizontalPosition.Center;
            }
        }

        public static Position GetPosition(FloatRect innerRect, FloatRect outerRectt) {
            Position position;
            position.Horizontal = GetHorizontalPosition(innerRect, outerRectt);
            position.Vertical = GetVerticalPostion(innerRect, outerRectt);
            return position;
        }



        public struct Position {

            public Position(VerticalPostion vertical, HorizontalPosition horizontal) {
                Vertical = vertical;
                Horizontal = horizontal;
            }

            public VerticalPostion Vertical;
            public HorizontalPosition Horizontal;
        }


        public static DPoint GetPoint(FloatRect rect, VerticalPostion verticalPostion = VerticalPostion.Center, HorizontalPosition horizontalPosition = HorizontalPosition.Center) {
            double x = 0;
            double y = 0;

            switch (verticalPostion) {
                case VerticalPostion.Top:
                    y = rect.Y;
                    break;
                case VerticalPostion.Center:
                case VerticalPostion.Unspecified:
                    y = rect.Y + rect.Height / 2.0;
                    break;
                case VerticalPostion.Bottom:
                    y = rect.Y + rect.Height;
                    break;
            }

            switch (horizontalPosition) {
                case HorizontalPosition.Left:
                    x = rect.X;
                    break;
                case HorizontalPosition.Center:
                case HorizontalPosition.Unspecified:
                    x = rect.X + rect.Width / 2.0;
                    break;
                case HorizontalPosition.Right:
                    x = rect.X + rect.Width;
                    break;
            }

            var point = new DPoint(x, y);
            return point;
        }

        public static int SecondsToMillis(this double seconds) => Convert.ToInt32(seconds * 1000);
            

        public static TimeSpan SecondsToTimeSpan(this double seconds) {
            var wholeSeconds = (int)seconds;
            var remainingMiliSeconds = (int)((seconds - wholeSeconds) * 100);
            return new TimeSpan(0, 0, wholeSeconds, remainingMiliSeconds);
        }

        public struct DPoint : IEquatable<DPoint>, IEquatable<object> {
            public double X;
            public double Y;
            public DPoint(double x, double y) {
                X = x;
                Y = y;
            }

            public static DPoint Origin { get => new DPoint(0, 0); }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + X.GetHashCode();
                    hash = hash * 23 + Y.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return obj is DPoint && this == (DPoint)obj;
            }

            public bool Equals(DPoint other) {
                return this == other;
            }

            public static bool operator ==(DPoint point1, DPoint point2) {
                return (point1.X == point2.X && point1.Y == point2.Y);
            }

            public static bool operator !=(DPoint point1, DPoint point2) {
                return !(point1 == point2);
            }


            public static DPoint operator +(DPoint point1, DPoint point2) {
                return new DPoint(point1.X + point2.X, point1.Y + point2.Y);
            }

            public static DPoint operator -(DPoint point1, DPoint point2) {
                return new DPoint(point1.X - point2.X, point1.Y - point2.Y);
            }

        }

        public struct FPoint : IEquatable<FPoint>, IEquatable<object> {
            public float X;
            public float Y;
            public FPoint(float x, float y) {
                X = x;
                Y = y;
            }

            public static FPoint Origin { get => new FPoint(0, 0); }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + X.GetHashCode();
                    hash = hash * 23 + Y.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return obj is FPoint && this == (FPoint)obj;
            }

            public bool Equals(FPoint other) {
                return this == other;
            }

            public static bool operator ==(FPoint point1, FPoint point2) {
                return (point1.X == point2.X && point1.Y == point2.Y);
            }

            public static bool operator !=(FPoint point1, FPoint point2) {
                return !(point1 == point2);
            }


            public static FPoint operator +(FPoint point1, FPoint point2) {
                return new FPoint(point1.X + point2.X, point1.Y + point2.Y);
            }

            public static FPoint operator -(FPoint point1, FPoint point2) {
                return new FPoint(point1.X - point2.X, point1.Y - point2.Y);
            }

        }

        public struct IntPoint : IEquatable<IntPoint>, IEquatable<object> {
            public int X;
            public int Y;
            public IntPoint(int x, int y) {
                X = x;
                Y = y;
            }

            public static IntPoint Origin { get => new IntPoint(0, 0); }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + X.GetHashCode();
                    hash = hash * 23 + Y.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return obj is IntPoint && this == (IntPoint)obj;
            }

            public bool Equals(IntPoint other) {
                return this == other;
            }

            public static bool operator ==(IntPoint point1, IntPoint point2) {
                return (point1.X == point2.X && point1.Y == point2.Y);
            }

            public static bool operator !=(IntPoint point1, IntPoint point2) {
                return !(point1 == point2);
            }


            public static IntPoint operator +(IntPoint point1, IntPoint point2) {
                return new IntPoint(point1.X + point2.X, point1.Y + point2.Y);
            }

            public static IntPoint operator -(IntPoint point1, IntPoint point2) {
                return new IntPoint(point1.X - point2.X, point1.Y - point2.Y);
            }
        }

        /// <summary>
        /// wraps a number back round from the min to max
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static double Wrap(double value, double max, double min) {
            value -= min;
            max -= min;
            if (max == 0) {
                return min;
            }
            value = value % max;
            value += min;
            while (value < min) {
                value += max;
            }
            return value;
        }

        public struct LineF : IEquatable<LineF>, IEquatable<object> {
            public FPoint Start { get; set; }
            public FPoint Stop { get; set; }

            public LineF(float startX, float startY, float stopX, float stopY) {
                Start = new FPoint(startX, startY);
                Stop = new FPoint(stopX, stopY);
            }

            public LineF(FPoint start, FPoint stop) {
                Start = start;
                Stop = stop;
            }


            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + Start.X.GetHashCode();
                    hash = hash * 23 + Start.Y.GetHashCode();
                    hash = hash * 23 + Start.X.GetHashCode();
                    hash = hash * 23 + Start.Y.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return obj is LineF && this == (LineF)obj;
            }

            public bool Equals(LineF other) {
                return this == other;
            }

            public static bool operator ==(LineF point1, LineF point2) {
                return (point1.Start == point2.Start && point1.Stop == point2.Stop);
            }

            public static bool operator !=(LineF point1, LineF point2) {
                return !(point1 == point2);
            }
        }

        public static double GetAngleRad(DPoint point1, DPoint point2, DPoint point3) {
            var p12 = LengthBetweenPoints(point1, point2);
            var p13 = LengthBetweenPoints(point1, point3);
            var p23 = LengthBetweenPoints(point2, point3);
            var theta = Math.Acos(Math.Pow(p12, 2) + Math.Pow(p13, 2) - Math.Pow(p23, 2) / (2 * p12 * p13));
            return theta;
        }

        public static double GetAngleRad(DPoint origin, DPoint destination) {
            var theta = GetAngleRad(origin.X, origin.Y, destination.X, destination.Y);
            return theta;
        }

        public static double GetAngleRad(double originX, double originY, double destinationX, double destinationY) {
            var deltaX = destinationX - originX;
            var deltaY = originY - destinationY;
            var theta = Math.Atan2(deltaX, deltaY);
            return theta;
        }

        public static double GetAngleDeg(double originX, double originY, double destinationX, double destinationY) {
            return GetAngleRad(originX, originY, destinationX, destinationY).ToDegrees();
        }

        public static double GetAngleDeg(DPoint origin, DPoint destination) {
            return GetAngleRad(origin.X, origin.Y, destination.X, destination.Y).ToDegrees();
        }

        public static double ToDegrees(this double radians) {
            return radians * 180 / Math.PI;
        }

        public static double ToRadians(this double degrees) {
            return degrees * Math.PI / 180;
        }


        public static double ToDegrees(this float radians) {
            return radians * 180 / Math.PI;
        }

        public static double ToRadians(this float degrees) {
            return degrees * Math.PI / 180;
        }

        public static double Hypot(double side1, double side2) {
            return Math.Sqrt(Math.Pow(side1, 2.0) + Math.Pow(side2, 2.0));
        }

        public static double LengthBetweenPoints(DPoint point1, DPoint point2) {
            return LengthBetweenPoints(point1.X, point1.Y, point2.X, point2.Y);
        }

        public static double LengthBetweenPoints(double x1, double y1, double x2,double y2) {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static DPoint RadialIntersection(double radians, double Width, double height) {
            //method seems to be off by 90 degrees, so rotating 90 degrees.
            radians -= Math.PI / 2;
            radians = Wrap(radians, Math.PI * 2, 0);
            var xRadius = Width / 2;
            var yRadius = height / 2;
            DPoint pointRelativeToCenter;
            var tangent = Math.Tan(radians);
            var y = xRadius * tangent;
            // An infinite line passing through the center at angle `radians`
            // intersects the right edge at Y coordinate `y` and the left edge
            // at Y coordinate `-y`.
            if (Math.Abs(y) <= yRadius) {
                // The line intersects the left and right edges before it intersects
                // the top and bottom edges.
                if (radians < Math.PI / 2 || radians > Math.PI + Math.PI / 2) {
                    // The ray at angle `radians` intersects the right edge.
                    pointRelativeToCenter = new DPoint(xRadius, y);
                } else {
                    // The ray intersects the left edge.
                    pointRelativeToCenter = new DPoint(-xRadius, -y);
                }
            } else {
                // The line intersects the top and bottom edges before it intersects
                // the left and right edges.
                var x = yRadius / tangent;
                if (radians < Math.PI) {
                    // The ray at angle `radians` intersects the bottom edge.
                    pointRelativeToCenter = new DPoint(x, yRadius);
                } else {
                    // The ray intersects the top edge.
                    pointRelativeToCenter = new DPoint(-x, -yRadius);
                }
            }
            return pointRelativeToCenter;
        }

        public static DPoint RadialIntersection(double radians, FloatRect frame) {
            var point = RadialIntersection(radians, frame.Width, frame.Height);
            return new DPoint(point.X + frame.MidX, point.Y + frame.MidY);
        }




        public struct FloatRect : IEquatable<FloatRect>, IEquatable<object> {
            public float X { get; set; }
            public float Y { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + X.GetHashCode();
                    hash = hash * 23 + Y.GetHashCode();
                    hash = hash * 23 + Width.GetHashCode();
                    hash = hash * 23 + Height.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return obj is FloatRect && this == (FloatRect)obj;
            }

            public bool Equals(FloatRect other) {
                return this == other;
            }

            public static bool operator ==(FloatRect floatRect1, FloatRect floatRect2) {
                return (floatRect1.X == floatRect2.X && floatRect1.Y == floatRect2.Y && floatRect1.Width == floatRect2.Width && floatRect1.Height == floatRect2.Height);
            }

            public static bool operator !=(FloatRect floatRect1, FloatRect floatRect2) {
                return !(floatRect1 == floatRect2);
            }

            public void Offset(FPoint point) {
                X += point.X;
                Y += point.Y;
            }

            public void MoveTo(FPoint point) {
                X = point.X;
                Y = point.Y;
            }

            public void Offset(float x, float y) {
                X += x;
                Y += y;
            }

            public void MoveTo(float x, float y) {
                X = x;
                Y = y;
            }

        public FloatRect(float x, float y, float width, float height) {
                if (width < 0) {
                    Width = Math.Abs(width);
                    X = x - Width;
                } else {
                    X = x;
                    Width = width;
                }
                if (height < 0) {
                    Height = Math.Abs(height);
                    Y = y - Height;
                } else {
                    Y = y;
                    Height = height;
                }
            }

            public float MidX => X + Width / 2; 
            public float MidY => Y + Height / 2; 
            public float MaxX { get => X + Width; set => Width = value - X; }
            public float MaxY { get => Y + Height; set => Height  = value - Y ; }

            public float MinX { get => X; set => X = value; }
            public float MinY { get => Y; set => Y = value; }

            public float Top { get => Y; set => Y = value; }
            public float Left { get => X; set => X = value; }
            public float Bottom { get => MaxY; set => MaxY = value; }
            public float Right { get => MaxX; set => MaxX = value; }

            public FPoint Center {
                get {
                    return new FPoint(MidX, MidY);
                }
            }

            public static readonly FloatRect Empty = new FloatRect(0, 0, -1, -1);
            public bool IsEmpty { get => (Width < 0 || Height < 0); }
            public IntRect ToIntRect() {
                return new IntRect(Convert.ToInt32(X), Convert.ToInt32(Y), Convert.ToInt32(Width), Convert.ToInt32(Height));
            }
        }




        public struct IntRect : IEquatable<IntRect>, IEquatable<object> {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public override int GetHashCode() {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + X.GetHashCode();
                    hash = hash * 23 + Y.GetHashCode();
                    hash = hash * 23 + Width.GetHashCode();
                    hash = hash * 23 + Height.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj) {
                return obj is IntRect && this == (IntRect)obj;
            }

            public bool Equals(IntRect other) {
                return this == other;
            }

            public static bool operator ==(IntRect floatRect1, IntRect floatRect2) {
                return (floatRect1.X == floatRect2.X && floatRect1.Y == floatRect2.Y && floatRect1.Width == floatRect2.Width && floatRect1.Height == floatRect2.Height);
            }

            public static bool operator !=(IntRect floatRect1, IntRect floatRect2) {
                return !(floatRect1 == floatRect2);
            }

            public void Offset(int x, int y) {
                X += x;
                Y += y;
            }


            public IntRect(int x, int y, int width, int height) {
                if (width < 0) {
                    Width = Math.Abs(width);
                    X = x - Width;
                } else {
                    X = x;
                    Width = width;
                }
                if (height < 0) {
                    Height = Math.Abs(height);
                    Y = y - Height;
                } else {
                    Y = y;
                    Height = height;
                }
            }

            public int MidX => X + Width / 2;
            public int MidY => Y + Height / 2;
            public int MaxX { get => X + Width; set => Width = value - X; }
            public int MaxY { get => Y + Height; set => Height = value - Y; }

            public int MinX { get => X; set => X = value; }
            public int MinY { get => Y; set => Y = value; }

            public int Top { get => Y; set => Y = value; }
            public int Left { get => X; set => X = value; }
            public int Bottom { get => MaxY; set => MaxY = value; }
            public int Right { get => MaxX; set => MaxX = value; }

            public IntPoint Center {
                get {
                    return new IntPoint(MidX, MidY);
                }
            }

            public static readonly IntRect Empty = new IntRect(0, 0, -1, -1);
            public bool IsEmpty { get => (Width < 0 || Height < 0); }
            public FloatRect ToFloatRect() {
                return new FloatRect(X, Y, Width, Height);
            }
        }

        public static FloatRect GetCornerRect(FloatRect rect, Corner corner, float radius) {
            var cornerRect = new FloatRect();
            switch (corner) {
                case Corner.TopLeft:
                    cornerRect.MinX = rect.X;
                    cornerRect.MaxX = rect.X + radius;
                    cornerRect.MinY = rect.MinY;
                    cornerRect.MaxY = rect.MinY + radius;
                    break;
                case Corner.TopRight:
                    cornerRect.MinX = rect.MaxX - radius;
                    cornerRect.MaxX = rect.MaxX;
                    cornerRect.MinY = rect.MinY;
                    cornerRect.MaxY = rect.MinY + radius;
                    break;
                case Corner.BottomRight:
                    cornerRect.MinX = rect.MaxX - radius;
                    cornerRect.MaxX = rect.MaxX;
                    cornerRect.MinY = rect.MaxY - radius;
                    cornerRect.MaxY = rect.MaxY;
                    break;
                case Corner.BottomLeft:
                    cornerRect.MinX = rect.X;
                    cornerRect.MaxX = rect.X + radius;
                    cornerRect.MinY = rect.MaxY - radius;
                    cornerRect.MaxY = rect.MaxY;
                    break;
            }
            return cornerRect;
        }

        public static FloatRect GetRectFromPoints(DPoint point1, DPoint point2) {
            var rect = new FloatRect();
            rect.X = (float)Math.Min(point1.X, point2.X);
            rect.Y = (float)Math.Min(point1.Y, point2.Y);
            rect.MaxX = (float)Math.Max(point1.X, point2.X);
            rect.MaxY = (float)Math.Max(point1.Y, point2.Y);
            return rect;
        }
        public static FloatRect GetSquareWithCenterPoint(DPoint point1, float width) {
            var halfWidth = width / 2;
            var rect = new FloatRect((float)point1.X - halfWidth, (float)point1.Y - halfWidth, width, width);
            return rect;
        }

        public static string ToOrdinalString(int num) {
            if (num <= 0) {
                return num.ToString();
            }
            switch (num % 100) {
                case 11:
                case 12:
                case 13:
                    return num + "ᵗʰ";
            }

            switch (num % 10) {
                case 1:
                    return num + "ˢᵗ";
                case 2:
                    return num + "ⁿᵈ";
                case 3:
                    return num + "ʳᵈ";
                default:
                    return num + "ᵗʰ";
            }
        }

        
    }
}
