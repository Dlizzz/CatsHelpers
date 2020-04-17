using System;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.ApplicationModel.Resources;

namespace CatsHelpers.ColorMaps
{
#pragma warning disable CS0419 // La référence de l'attribut cref est ambiguë
    /// <summary>
    /// Implement a 256 elements color scale with static data as readonly array of sRGB truples 
    /// and interpolation method to get color from scale as a double.
    /// </summary>
    public class ColorMap
    {
        // Get resource loader for the library
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("CatsHelpers/ErrorMessages");
        // Backing store for the color data
        private readonly (double, double, double)[] _colorData;
        private readonly int maxIndexColorData;

        /// <summary>
        /// Get color data as a read only collection of truples of double (sRGB) 
        /// </summary>
        /// <value>The collection</value>
        public ReadOnlyCollection<(double, double, double)> ColorData => Array.AsReadOnly(_colorData);

        /// <summary>
        /// Create the color scale from color data
        /// </summary>
        /// <param name="colorData">An array of truples of double, with each component being between 0 and 1</param>
        /// <exception cref="ArgumentNullException">colorData can't be null</exception>
        public ColorMap((double, double, double)[] colorData)
        {
            if (colorData == null) throw new ArgumentNullException(nameof(colorData));
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
            maxIndexColorData = _colorData.GetUpperBound(0);
        }

        /// <summary>
        /// Get the sRGB data (truples of double) corresponding to the given position on the colormap through interpolation 
        /// </summary>
        /// <param name="position">The position on the scale</param>
        /// <param name="inverse">If true, return the sRGB data corresponding to the inversed colormap (Optional ; Default : false)</param>
        /// <remarks><paramref name="position"/> is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> Must be in the [0 , 1] range</exception>
        /// <returns>The sRGB data as a truples of double</returns>
        public (double, double, double) GetInterpolatedsRGB(double position, bool inverse = false)
        {
            if (position < 0 || position > 1) throw new ArgumentOutOfRangeException(nameof(position), resourceLoader.GetString("ValueNotPercentage"));

            // Inverse the position if needed
            if (inverse) position = 1 - position;

            // Get the colors just before and after the position
            double index = position * maxIndexColorData;
            int lowIndex = (int)Math.Truncate(index);
            int highIndex = lowIndex + 1;

            // Get sRGB data for low and high color
            var (lowR, lowG, lowB) = _colorData[lowIndex];
            var (highR, highG, highB) = _colorData[highIndex];

            // Linear interpolation of the components values
            double factor = index - lowIndex;
            double newR = lowR + (highR - lowR) * factor;
            double newG = lowG + (highG - lowG) * factor;
            double newB = lowB + (highB - lowB) * factor;

            // Return fully opaque color
            return (newR, newG, newB);
        }

        /// <summary>
        /// Get the <see cref="Color"/> corresponding to the given position on the colormap through interpolation 
        /// </summary>
        /// <param name="position">The position on the scale</param>
        /// <param name="inverse">If true, return the <see cref="Color"/> corresponding to the inversed color scale (Optional ; Default : false)</param>
        /// <remarks><paramref name="position"/> is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> Must be in the [0 , 1] range</exception>
        /// <returns><see cref="Color"/> The color</returns>
        public Color GetInterpolatedColor(double position, bool inverse = false)
        {
            var (r, g, b) = GetInterpolatedsRGB(position, inverse);
            
            // Return fully opaque color
            return ColorFromsRGB(r, g, b);
        }

        /// <summary>
        /// Create an indexed colormap, as an array of bytes of bytes (4 bytes by color), by interpolating colors data
        /// </summary>
        /// <param name="count">Number of colors in the new colormap</param>
        /// <returns>The array of byte of byte (4 bytes per color)</returns>
        public byte[][] CreateIndexedBytesColorMap(int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), resourceLoader.GetString("ValueNotStrictlyPositive"));
            
            var colorMap = new byte[count][];
            double position;

            for (int index = 0; index < count; index++)
            {
                position = (double)index / count;
                
                var (r, g, b) = GetInterpolatedsRGB(position);
                colorMap[index] = BytesFromsRGB(r, g, b);
            }

            return colorMap;
        }

        /// <summary>
        /// Create an indexed colormap, as an array of <see cref="Color"/>, by interpolating colors data
        /// </summary>
        /// <param name="count">Number of colors in the new colormap</param>
        /// <returns>The array of <see cref="Color"/></returns>
        public Color[] CreateIndexedColorsColorMap(int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), resourceLoader.GetString("ValueNotStrictlyPositive"));

            var colorMap = new Color[count];
            double position;

            for (int index = 0; index < count; index++)
            {
                position = (double)index / count;

                var (r, g, b) = GetInterpolatedsRGB(position);
                colorMap[index] = ColorFromsRGB(r, g, b);
            }

            return colorMap;
        }

        // Convert sRGB components to bytes array (4 bytes)
        private static byte[] BytesFromsRGB(double r, double g, double b)
        {
            var color = new byte[4];

            // Fully opaque color
            color[0] = byte.MaxValue;
            color[1] = Convert.ToByte(r * byte.MaxValue);
            color[2] = Convert.ToByte(g * byte.MaxValue);
            color[3] = Convert.ToByte(b * byte.MaxValue);

            return color;
        }

        // Convert sRGB components to Color struct
        private static Color ColorFromsRGB(double r, double g, double b)
        {
            byte[] color = BytesFromsRGB(r, g, b);

            // Return fully opaque color
            return Color.FromArgb(color[0], color[1], color[2], color[3]);
        }
    }
#pragma warning restore CS0419 // La référence de l'attribut cref est ambiguë
}

