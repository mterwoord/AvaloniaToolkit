using System;
using System.Threading.Tasks;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvaloniaToolkit.Imaging
{
    /// <summary>
    /// Extension methods for WriteableBitmap used for generating color picker bitmaps.
    /// </summary>
    public static class WriteableBitmapColorPickerExtensions
    {
        #region RenderColorPickerHueLightness()

        /// <summary>
        /// Renders the color picker rectangle based on hue and lightness.
        /// </summary>
        /// <param name="target">The target bitmap.</param>
        /// <param name="saturation">The saturation.</param>
        public static void RenderColorPickerHueLightness(this WritableBitmap target, double saturation)
        {
            var pw = target.PixelWidth;
            var ph = target.PixelHeight;

            using (var xLocked = target.Lock())
            {
                if (xLocked.Format != PixelFormat.Rgba8888)
                {
                    throw new InvalidOperationException("Only Rgba8888 supported!");
                }
                RenderColorPickerHueLightnessCore(saturation, pw, ph, xLocked);
            }
        }
        
        /// <summary>
        /// Renders the color picker rectangle based on hue and lightness asynchronously.
        /// </summary>
        /// <param name="target">The target bitmap.</param>
        /// <param name="saturation">The saturation.</param>
        /// <returns></returns>
        public static async Task RenderColorPickerHueLightnessAsync(this WritableBitmap target, double saturation)
        {
            var pw = target.PixelWidth;
            var ph = target.PixelHeight;

            using (var xLocked = target.Lock())
            {
                if (xLocked.Format != PixelFormat.Rgba8888)
                {
                    throw new InvalidOperationException("Only Rgba8888 supported!");
                }
                await Task.Run(() => RenderColorPickerHueLightnessCore(saturation, pw, ph, xLocked));
            }

        }

        private static unsafe void RenderColorPickerHueLightnessCore(
            double saturation,
            int pw,
            int ph,
            ILockedFramebuffer pixels)
        {
            var xmax = pw - 1;
            var ymax = ph - 1;

            var xPtr = (int*)pixels.Address.ToPointer();
            for (int y = 0; y < ph; y++)
            {
                double lightness = 1.0 * (ph - 1 - y) / ymax;

                for (int x = 0; x < pw; x++)
                {
                    double hue = 360.0 * x / xmax;
                    var c = ColorExtensions.FromHsl(hue, saturation, lightness);

                    xPtr[pw * y + x] = c.GetBgra8888Value();
                }
            }
        }

        #endregion

        /// <summary>
        /// Renders the color picker hue ring asynchronously.
        /// </summary>
        /// <param name="target">The target bitmap.</param>
        /// <param name="innerRingRadius">The inner ring radius.</param>
        /// <param name="outerRingRadius">The outer ring radius.</param>
        public static async Task RenderColorPickerHueRingAsync(this WritableBitmap target, int innerRingRadius = 0, int outerRingRadius = 0)
        {
            var pw = target.PixelWidth;
            var ph = target.PixelHeight;
            using (var xLocked = target.Lock())
            {
                if (xLocked.Format != PixelFormat.Bgra8888)
                {
                    throw new InvalidOperationException("Only Bgra8888 supported!");
                }

                await Task.Run(() => RenderColorPickerHueRingCore(innerRingRadius, outerRingRadius, pw, ph, xLocked));
            }
        }

        private static unsafe void RenderColorPickerHueRingCore(
            int innerRingRadius, int outerRingRadius, int pw, int ph, ILockedFramebuffer pixels)
        {
            var pch = pw / 2;
            var pcv = ph / 2;

            if (outerRingRadius == 0)
            {
                outerRingRadius = Math.Min(pw, ph) / 2;
            }
            if (innerRingRadius == 0)
            {
                innerRingRadius = outerRingRadius * 2 / 3;
            }

            // Outer ring radius square
            var orr2 = outerRingRadius * outerRingRadius;

            // Inner ring radius square
            var irr2 = innerRingRadius * innerRingRadius;

            //var orr22 = (outerRingRadius - 5) * (outerRingRadius - 5);
            //var irr22 = (innerRingRadius + 5) * (innerRingRadius + 5);
            const double piInv = 1.0 / Math.PI;
            var xPtr = (int*)pixels.Address.ToPointer();
            for (int y = 0; y < ph; y++)
            {
                for (int x = 0; x < pw; x++)
                {
                    // Radius square
                    var r2 = (x - pch) * (x - pch) + (y - pcv) * (y - pcv);

                    if (r2 >= irr2 &&
                        r2 <= orr2)
                    {
                        var angleRadians = Math.Atan2(y - pcv, x - pch);
                        var angleDegrees = (angleRadians * 180 * piInv + 90 + 360) % 360;
                        //var alpha = (r2 - irr22 < 5) || (orr22 - r2 < 5) ? 0.5 : 1;
                        //var c = ColorExtensions.FromHsl(angleDegrees, 1.0 * alpha, 0.5 * alpha, alpha);
                        var c = ColorExtensions.FromHsl(angleDegrees, 1.0, 0.5);
                        xPtr[pw * y + x] = c.GetBgra8888Value();
                    }
                    //else
                    //{
                    //    pixels[pw * y + x] = int.MaxValue;
                    //}
                }
            }
        }
    
    }

}