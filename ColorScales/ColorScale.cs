using System;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.ApplicationModel.Resources;

namespace CatsHelpers.ColorScales
{
#pragma warning disable CS0419 // La référence de l'attribut cref est ambiguë
    /// <summary>
    /// Implement a 256 elements color scale with static data as readonly array of sRGB truples 
    /// and interpolation method to get color from scale as a double.
    /// </summary>
    public class ColorScale
    {
        // Get resource loadre for the library
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("CatsHelpers/ErrorMessages");
        // backing store for the color data
        private readonly (double, double, double)[] _colorData;


        /// <summary>
        /// Get color data as a read only collection of truples of double (sRGB) 
        /// </summary>
        /// <value>The collection</value>
        public ReadOnlyCollection<(double, double, double)> ColorData => Array.AsReadOnly(_colorData);

        /// <summary>
        /// Create the color scale from color data
        /// </summary>
        /// <param name="colorData">An array of 256 truples of double, with each component being between 0 and 1</param>
        /// <exception cref="ArgumentNullException">colorData can't be null</exception>
        /// <exception cref="ArgumentOutOfRangeException">colorData must have exactly 256 elements, with all elements components being between 0 and 1</exception>
        public ColorScale((double, double, double)[] colorData)
        {
            if (colorData == null) throw new ArgumentNullException(nameof(colorData));
            if (colorData.Length != 256) throw new ArgumentOutOfRangeException(nameof(colorData), resourceLoader.GetString("WrongColorDataArraySize"));
            foreach ((double, double, double) data in colorData)
            {
                if (data.Item1 < 0 || data.Item1 > 1
                    || data.Item2 < 0 || data.Item2 > 1
                    || data.Item3 < 0 || data.Item1 > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(colorData), resourceLoader.GetString("ValueNotPercentage"));
                }
            }

            _colorData = colorData;
        }

        /// <summary>
        /// Get the color corresponding to the given position on the scale through interpolation 
        /// </summary>
        /// <param name="position">The position on the scale</param>
        /// <param name="inverse">If true, return the <see cref="Windows.UI.Color"/> corresponding to the inversed color scale (Optional ; Default : false)</param>
        /// <remarks><paramref name="position"/> is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> Must be in the [0 , 1] range</exception>
        /// <returns>The color</returns>
        public Color GetColor(double position, bool inverse = false)
        {
            if (position < 0 || position > 1) throw new ArgumentOutOfRangeException(nameof(position), resourceLoader.GetString("ValueNotPercentage"));

            // Inverse the position if needed
            if (inverse) position = 1 - position;
            // Return the last color if we are at the end of the scale
            if (position == 1) return SRGBToColor(_colorData[255]); 

            // Get the colors just before and after the position
            double index = position * 255.0;
            double lowIndex = Math.Truncate(index);
            double highIndex = lowIndex + 1.0;

            (double lowR, double lowG, double lowB) = _colorData[Convert.ToInt32(lowIndex)];
            (double highR, double highG, double highB) = _colorData[Convert.ToInt32(highIndex)];

            // Linear interpolation of the components values
            double factor = index - lowIndex;
            double newR = lowR + (highR - lowR) * factor;
            double newG = lowG + (highG - lowG) * factor;
            double newB = lowB + (highB - lowB) * factor;

            // Return fully opaque color
            return SRGBToColor(newR, newG, newB);
        }

        // Convert sRGB components to Color struct
        private Color SRGBToColor(double r, double g, double b) => Color.FromArgb(255, Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
        // Convert sRGB truples to Color struct
        private Color SRGBToColor((double, double, double) srgb) => Color.FromArgb(255, Convert.ToByte(srgb.Item1 * 255), Convert.ToByte(srgb.Item2 * 255), Convert.ToByte(srgb.Item3 * 255));
    }
#pragma warning restore CS0419 // La référence de l'attribut cref est ambiguë
}

