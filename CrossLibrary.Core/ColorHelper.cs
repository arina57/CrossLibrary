using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Globalization;

namespace CrossLibrary {
    public static class ColorHelper {

      
        /// <summary>
        /// Genearates a gradient between two colors in the number of steps specified
        /// </summary>
        /// <param name="startColor"></param>
        /// <param name="endColor"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public static List<Color> GetGradient(Color startColor, Color endColor, int steps) {
            var gradient = new List<Color>();
            var r = (endColor.R - startColor.R) / (steps);
            var b = (endColor.B - startColor.B) / (steps);
            var g = (endColor.G - startColor.G) / (steps);
            var a = (endColor.A - startColor.A) / (steps);
            for (var i = 0; i <= steps; i++) {
                gradient.Add(Color.FromArgb(startColor.A + a * i, startColor.R + r * i, startColor.G + g * i, startColor.B + b * i));
            }

            return gradient;
        }

        /// <summary>
        /// Genearates a gradient between colors in the number of steps specified
        /// This doesn't work
        /// </summary>
        /// <param name="startColor"></param>
        /// <param name="endColor"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public static List<Color> GetGradient(int steps, params Color[] colorsToCycle ) {
            if (colorsToCycle.Length < 1 || steps < 1 || steps < colorsToCycle.Length) {
                return new List<Color>(colorsToCycle);
            }

            var cycles = (colorsToCycle.Length - 1);
            var stepsPerColor = steps / cycles;
            var remainder = steps % cycles;
            var gradient = new List<Color>();
            
            for (int colorIndex = 0; colorIndex < cycles; colorIndex++) {
                var stepsThisCycle = stepsPerColor;
                var remainingCycles = cycles - 1 - colorIndex;
                if (remainder > remainingCycles) {
                    stepsThisCycle++;
                    remainder--;
                }
                if(colorIndex >= cycles - 1) {
                    stepsThisCycle--;
                }

                var startColor = colorsToCycle[colorIndex];
                var endColor = colorsToCycle[colorIndex + 1];
                var alphaStep = (endColor.A - startColor.A) / (stepsThisCycle);
                var redStep = (endColor.R - startColor.R) / (stepsThisCycle);
                var blueStep = (endColor.B - startColor.B) / (stepsThisCycle);
                var greenStep = (endColor.G - startColor.G) / (stepsThisCycle);

                for (var i = 0; i < stepsThisCycle; i++) {
                    var newAlpha = startColor.A + alphaStep * i;
                    var newRed = startColor.R + redStep * i;
                    var newGreen = startColor.G + greenStep * i;
                    var newBlue = startColor.B + blueStep * i;
                    gradient.Add(Color.FromArgb(newAlpha, newRed, newGreen, newBlue));
                }
            }
            gradient.Add(colorsToCycle[colorsToCycle.Length -1]);
            return gradient;
        }
        


        public static List<Color> GetLightnessGradient(Color baseColor, int steps) {
            if(steps == 1) {
                return new List<Color>() { baseColor.WithNewLightness(0.5f) };
            }

            var minLightness = 1.0 / (steps + 1);
            var lightnessRange = 1 - minLightness * 2;
            var stepAmount = lightnessRange / (steps - 1);
            var gradient = new List<Color>();
            for (int step = steps - 1; step >= 0; step--) {
                var lightness = step * stepAmount + minLightness;
                gradient.Add(baseColor.WithNewLightness((float)lightness));
            }
            return gradient;
        }

        public static List<Color> GetSaturationGradient(Color baseColor, int steps) {
            if (steps == 1) {
                return new List<Color>() { baseColor.WithNewLightness(0.5f) };
            }

            var stepAmount = 0.9f / (steps - 1);
            var gradient = new List<Color>();
            for (int step = 0; step < steps; step++) {
                var saturation = step * stepAmount + 0.1f;
                gradient.Add(baseColor.WithNewSaturation(saturation));
            }
            return gradient;
        }
        public static List<Color> GetHueGradient(Color baseColor, int steps) {
            if (steps < 1) {
                return new List<Color>();
            }
            if (steps == 1) {
                return new List<Color>() { baseColor };
            }

            var hue = baseColor.GetHue() / 360;
            var stepAmount = 1f / steps;
            var gradient = new List<Color>();
            gradient.Add(baseColor);
            for (int step = 0; step < steps - 1; step++) {
                hue += stepAmount; 
                hue = hue % 1;
                gradient.Add(baseColor.WithNewHue(hue));
            }
            return gradient;
        }

        public static float GetLightness(this Color color) {
            float r = color.R / 255.0f;
            float g = color.G / 255.0f;
            float b = color.B / 255.0f;
            float v;
            float m;
            float lightness = 0;
            v = Math.Max(r, g);
            v = Math.Max(v, b);
            m = Math.Min(r, g);
            m = Math.Min(m, b);
            lightness = (m + v) / 2.0f;
            return lightness;
        }


        public static (double Hue, double Saturation, double Brightness) ToHsl(this Color color) {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double v;
            double m;
            double vm;
            double r2, g2, b2;
            double h = 0; // default to black
            double s = 0;
            double l = 0;
            v = Math.Max(r, g);
            v = Math.Max(v, b);
            m = Math.Min(r, g);
            m = Math.Min(m, b);
            l = (m + v) / 2.0;
            if (l <= 0.0) {
                return (h, s, l);
            }

            vm = v - m;
            s = vm;
            if (s > 0.0) {
                s /= (l <= 0.5) ? (v + m) : (2.0 - v - m);
            } else {
                return (h, s, l);
            }
            r2 = (v - r) / vm;
            g2 = (v - g) / vm;
            b2 = (v - b) / vm;
            if (r == v) {
                h = (g == m ? 5.0 + b2 : 1.0 - g2);
            } else if (g == v) {
                h = (b == m ? 1.0 + r2 : 3.0 - b2);
            } else {
                h = (r == m ? 3.0 + g2 : 5.0 - r2);
            }

            h /= 6.0;

            return (h, s, l);
        }
        public static Color ColorFromHsla(float hue, float saturation, float lightness, float alpha) {
            return Color.FromArgb((int)(alpha * 255), ColorFromHsl(hue, saturation, lightness));
        }


            /// <summary>
            /// Given H,S,L in range of 0-1 Returns a Color
            /// </summary>
            /// <param name="adjustedHue"></param>
            /// <param name="adjustedSaturation"></param>
            /// <param name="adjustedLightness"></param>
            /// <returns></returns>
            public static Color ColorFromHsl(float hue, float saturation, float lightness) {
            var adjustedLightness = lightness > 1 ? 1 : lightness;
            var adjustedSaturation = saturation > 1 ? 1 : saturation;
            var adjustedHue = hue % 1;
            float v;
            float r, g, b;
            r = adjustedLightness;   // default to gray
            g = adjustedLightness;
            b = adjustedLightness;
            v = (adjustedLightness <= 0.5f) ? (adjustedLightness * (1.0f + adjustedSaturation)) : (adjustedLightness + adjustedSaturation - adjustedLightness * adjustedSaturation);
            if (v > 0) {
                float m;
                float sv;
                int sextant;
                float fract, vsf, mid1, mid2;
                m = adjustedLightness + adjustedLightness - v;
                sv = (v - m) / v;
                adjustedHue *= 6.0f;
                sextant = (int)adjustedHue;
                fract = adjustedHue - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant) {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;

                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;

                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;

                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;

                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;

                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;

                }

            }
            var red = Convert.ToByte(r * 255.0f);
            var green = Convert.ToByte(g * 255.0f);
            var blue = Convert.ToByte(b * 255.0f);
            Color rgb = Color.FromArgb(red, green, blue);
            return rgb;
        }


   
        public static Color WithNewLightness(this Color color, float lightness) {
            var saturation = color.GetSaturation();
            var hue = color.GetHue() / 360;
            return Color.FromArgb(color.A, ColorFromHsl(hue, saturation, lightness));
        }
        public static Color WithNewHue(this Color color, float hue) {
            var saturation = color.GetSaturation();
            var lightness = color.GetLightness();
            return Color.FromArgb(color.A, ColorFromHsl(hue, saturation, lightness));
        }

        public static Color WithNewSaturation(this Color color, float saturation) {
            var lightness = color.GetLightness();
            var hue = color.GetHue() / 360;
            var newColor = Color.FromArgb(color.A, ColorFromHsl(hue, saturation, lightness));
            return newColor;
        }

        public static string ToHtmlWithAlpha(this Color color) {
            return "#" 
                + color.A.ToString("X2")
                + color.R.ToString("X2") 
                + color.G.ToString("X2") 
                + color.B.ToString("X2");
        }
        public static string ToHtml(this Color color) {
            return "#"
                + color.R.ToString("X2")
                + color.G.ToString("X2")
                + color.B.ToString("X2");
        }

        public static string ToRgbaString(this Color color) {
            return $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255f})";
        }

        public static string ToRgbString(this Color color) {
            return $"RGB({color.R}, {color.G}, {color.B})";
        }
    }
}
