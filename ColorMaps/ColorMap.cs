using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    /// <remarks>Implement <see cref="INotifyPropertyChanged"/></remarks>
    public class ColorMap: INotifyPropertyChanged
    {
        #region Initialization
        // Get resource loader for the library
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("CatsHelpers/ErrorMessages");
        // Back store for the color data
        private readonly (double, double, double)[] _colorData;
        // Store max index of _colordata for optimization
        private readonly int maxIndexColorData;
        // Back store for Inversed property (default to false)
        private bool _inversed = false;

        /// <summary>
        /// Create the color scale from color data
        /// </summary>
        /// <param name="name">Name of the colormap</param>
        /// <param name="colorData">An array of truples of double, with each component being between 0 and 1</param>
        /// <exception cref="ArgumentNullException">name or colorData can't be null</exception>
        public ColorMap(string name, (double, double, double)[] colorData)
        {
            if (colorData == null) throw new ArgumentNullException(nameof(colorData));
            if (name == null) throw new ArgumentNullException(nameof(name));
            foreach ((double, double, double) data in colorData)
            {
                if (data.Item1 < 0 || data.Item1 > 1
                    || data.Item2 < 0 || data.Item2 > 1
                    || data.Item3 < 0 || data.Item1 > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(colorData), resourceLoader.GetString("ValueNotPercentage"));
                }
            }

            Name = name;
            _colorData = colorData;
            maxIndexColorData = _colorData.GetUpperBound(0);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get color data as a read only collection of truples of double (sRGB) 
        /// </summary>
        /// <value>The collection</value>
        public ReadOnlyCollection<(double, double, double)> ColorData => Array.AsReadOnly(_colorData);

        /// <summary>
        /// Get or set the Inversed property.
        /// </summary>
        /// <value>When True, the colormap is inversed.</value>
        /// <remarks>OnInversedChanged event is triggered whenever the property is changed.</remarks>
        public bool Inversed
        {
            get => _inversed;
            set
            {
                _inversed = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Get the name of the colormap.
        /// </summary>
        public string Name { get; }
        #endregion

        #region Events
        /// <summary>
        /// The event for property changed 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property changed event handler
        /// </summary>
        /// <param name="propertyName"><see cref="string"/> Name of the changed property (default to caller name)</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Interpolation
        /// <summary>
        /// Get the sRGB data (truples of double) corresponding to the given position on the colormap through interpolation 
        /// </summary>
        /// <param name="position">The position on the scale</param>
        /// <remarks><paramref name="position"/> is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> Must be in the [0 , 1] range</exception>
        /// <returns>The sRGB data as a truples of double</returns>
        public (double, double, double) GetInterpolatedsRGB(double position)
        {
            if (position < 0 || position > 1) throw new ArgumentOutOfRangeException(nameof(position), resourceLoader.GetString("ValueNotPercentage"));

            // Inverse the position if needed
            if (_inversed) position = 1 - position;

            // If position equals 1, return the last color
            if (position == 1) return _colorData[maxIndexColorData];
            // If position equals 0, return the first color
            if (position == 0) return _colorData[0];

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
        /// <remarks><paramref name="position"/> is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> Must be in the [0 , 1] range</exception>
        /// <returns><see cref="Color"/> The color</returns>
        public Color GetInterpolatedColor(double position)
        {
            var (r, g, b) = GetInterpolatedsRGB(position);
            
            // Return fully opaque color
            return ColorFromsRGB(r, g, b);
        }
        #endregion

        #region Indexed colormap
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
        #endregion

        #region Conversion
        // Convert sRGB components to bytes array (4 bytes)
        private static byte[] BytesFromsRGB(double r, double g, double b)
        {
            var color = new byte[4];

            // Fully opaque color
            // Pixel format = DirectXPixelFormat.B8G8R8A8UintNormalized ; [0]: Blue ; [1]: Green ; [2]: Red ; [3]: Alpha
            color[0] = Convert.ToByte(b * byte.MaxValue);
            color[1] = Convert.ToByte(g * byte.MaxValue); 
            color[2] = Convert.ToByte(r * byte.MaxValue);
            color[3] = byte.MaxValue;

            return color;
        }

        // Convert sRGB components to Color struct
        private static Color ColorFromsRGB(double r, double g, double b)
        {
            byte[] color = BytesFromsRGB(r, g, b);

            // Return fully opaque color
            return Color.FromArgb(color[0], color[1], color[2], color[3]);
        }
        #endregion
    }
#pragma warning restore CS0419 // La référence de l'attribut cref est ambiguë
}

