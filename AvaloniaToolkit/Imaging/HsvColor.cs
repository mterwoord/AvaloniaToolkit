namespace AvaloniaToolkit.Imaging
{
    /// <summary>
    /// Defines a color in Hue/Saturation/Value (HSV) space.
    /// </summary>
    public struct HsvColor
    {
        /// <summary>
        /// The Hue in 0..360 range.
        /// </summary>
        public double H;
        /// <summary>
        /// The Saturation in 0..1 range.
        /// </summary>
        public double S;
        /// <summary>
        /// The Value in 0..1 range.
        /// </summary>
        public double V;
        /// <summary>
        /// The Alpha/opacity in 0..1 range.
        /// </summary>
        public double A;
    }
}