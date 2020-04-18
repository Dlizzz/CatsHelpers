using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.ApplicationModel.Resources;

namespace CatsHelpers.ColorMaps
{

#pragma warning disable CS0419 // La référence de l'attribut cref est ambiguë
    /// <summary>
    /// A collection of colors defined by linear interpolation between color keys.
    /// Implemented as a <see cref="IList{T}"/> of <see cref="ColorKey"/> and provide <see cref="ColorElements"/>, a read only list of <see cref="Windows.UI.Color"/>.
    /// </summary>
    public class ColorsCollection : IList<ColorKey>
    {
        #region Initialization
        // Get resource loadre for the library
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("CatsHelpers/ErrorMessages"); 

        // List of color keys
        private readonly List<ColorKey> colorKeys = new List<ColorKey>();
        // Back store for ColorElements property
        private readonly List<Color> _colorElements = new List<Color>();


        /// <summary>
        /// Create the collection with <paramref name="colorElementsCount"/> elements.
        /// </summary>
        /// <param name="colorElementsCount"><see cref="int"/> Number of elements</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="colorElementsCount"/> must be strictly greater than 1</exception>
        public ColorsCollection(int colorElementsCount)
        {
            ColorElementsCount = colorElementsCount;
        }
        #endregion

        #region ColorKey
        /// <summary>
        /// Get or set the <see cref="ColorKey"/> at the specified index.
        /// </summary>
        /// <param name="index"><see cref="int"/> The index.</param>
        /// <returns><see cref="ColorKey"/> The color key.</returns>
        /// <remarks>The ColorElements list is updated</remarks>
        public ColorKey this[int index]
        {
            get => colorKeys[index];
            set
            {
                colorKeys[index] = value;
                UpdateColorElements();
            }
        }

        /// <summary>
        /// Add the given <see cref="ColorKey"/> to the end of the list of color keys. 
        /// </summary>
        /// <param name="item"><see cref="ColorKey"/> The color key.</param>
        /// <remarks>The ColorElements list is updated</remarks>
        public void Add(ColorKey item)
        {
            colorKeys.Add(item);
            UpdateColorElements();
        }

        /// <summary>
        /// Remove all color keys.
        /// </summary>
        /// <remarks>The ColorElements list is updated to a transparent color scale.</remarks>
        public void Clear()
        {
            colorKeys.Clear();
            UpdateColorElements();
        }

        /// <summary>
        /// Insert the given <see cref="ColorKey"/> at the given index in the list
        /// </summary>
        /// <param name="index"><see cref="int"/> Index in the list.</param>
        /// <param name="item"><see cref="ColorKey"/> The color key.</param>
        /// <remarks>The ColorElements list is updated</remarks>
        public void Insert(int index, ColorKey item)
        {
            colorKeys.Insert(index, item);
            UpdateColorElements();
        }

        /// <summary>
        /// Remove the given <see cref="ColorKey"/> from the list
        /// </summary>
        /// <param name="item"><see cref="ColorKey"/> The color key.</param>
        /// <remarks>If the <see cref="ColorKey"/> is found, it's removed and the ColorElements list is updated</remarks>
        public bool Remove(ColorKey item)
        {
            bool removed = colorKeys.Remove(item);
            if (removed) UpdateColorElements();
            return removed;
        }

        /// <summary>
        /// Remove the <see cref="ColorKey"/> at the given index in the list
        /// </summary>
        /// <param name="index"><see cref="int"/> Index in the list.</param>
        /// <remarks>The ColorElements list is updated</remarks>
        public void RemoveAt(int index)
        {
            colorKeys.RemoveAt(index);
            UpdateColorElements();
        }

        /// <summary>
        /// Get the number of <see cref="ColorKey"/> in the list
        /// </summary>
        public int Count => colorKeys.Count;
        /// <summary>
        /// Indicates if the color key list is read only. 
        /// </summary>
        public bool IsReadOnly => false;
        /// <summary>
        /// Retrune true if the given <see cref="ColorKey"/> is in the list
        /// </summary>
        /// <param name="item"><see cref="ColorKey"/> The color key</param>
        /// <returns>True if the color key is in the list</returns>
        public bool Contains(ColorKey item) => colorKeys.Contains(item);
        /// <summary>
        /// Copy the the color key list to the given <see cref="ColorKey"/> array, starting at the given index.
        /// </summary>
        /// <param name="array"><see cref="ColorKey"/>[] The color key array to copy into.</param>
        /// <param name="arrayIndex"><see cref="int"/> Satrting index.</param>
        public void CopyTo(ColorKey[] array, int arrayIndex) => colorKeys.CopyTo(array, arrayIndex);
        /// <summary>
        /// Get an enumerator that iterates through the color key list
        /// </summary>
        /// <returns><see cref="IEnumerator&lt;ColorKey>"/> The enumerator</returns>
        public IEnumerator<ColorKey> GetEnumerator() => colorKeys.GetEnumerator();
        /// <summary>
        /// Determine the index of the given <see cref="ColorKey"/>
        /// </summary>
        /// <param name="item"><see cref="ColorKey"/> The given color key.</param>
        /// <returns>The index of the given ColorKey if found, otherwise -1.</returns>
        public int IndexOf(ColorKey item) => colorKeys.IndexOf(item);
        /// <summary>
        /// Get an enumerator that iterates through the color key list
        /// </summary>
        /// <returns><see cref="IEnumerator"/> The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => colorKeys.GetEnumerator();
        #endregion

        #region ColorElements

        /// <summary>
        /// The color scale as a read only collection of <see cref="Windows.UI.Color"/>
        /// </summary>
        /// <value>Get the collection of <see cref="Windows.UI.Color"/></value>
        public ReadOnlyCollection<Color> ColorElements { get => _colorElements.AsReadOnly(); }

        /// <summary>
        /// Number of elements in the color scale
        /// </summary>
        /// <value>Get or set the number of <see cref="Windows.UI.Color"/> in the color elements collection</value>
        /// <remarks>When the value is sat, the color elements list is cleared and recalculated</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="ColorElementsCount"/> must be strictly greater than 1</exception>
        public int ColorElementsCount 
        { 
            get => _colorElements.Count;
            set
            {
                if (value <= 1) throw new ArgumentOutOfRangeException(nameof(ColorElementsCount), resourceLoader.GetString("ValueNotStrictlyPositive"));

                _colorElements.Clear();
                Color[] colors = new Color[value];
                _colorElements.AddRange(colors);

                UpdateColorElements();
            }        
        }

        /// <summary>
        /// Convert a <paramref name="scale"/> to a <see cref="Windows.UI.Color"/> elements from the color scale. 
        /// </summary>
        /// <param name="scale">The scale to convert as a precentage</param>
        /// <param name="inverse">If true, return the <see cref="Windows.UI.Color"/> corresponding to the inversed color scale (Optional ; Default : false)</param>
        /// <remarks><paramref name="scale"/> is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="scale"/> Must be in the [0 , 1] range</exception>
        /// <returns>The color</returns>
        public Color ScaleToColor(double scale, bool inverse = false)
        {
            if (scale < 0 || scale > 1) throw new ArgumentOutOfRangeException(nameof(scale), resourceLoader.GetString("ValueNotPercentage"));

            // Interpolate the index based on scale value
            int index = Convert.ToInt32(scale * (_colorElements.Count - 1));

            // If inverse is true, return color strating from the end of the list 
            return  inverse ? _colorElements[_colorElements.Count - 1 - index] : _colorElements[index];
        }

        // Update the list of color elements from the list of color keys 
        private void UpdateColorElements()
        {
            // Sort the color key list to optimize color elements computation
            colorKeys.Sort(ColorKey.Compare);

            // Compute the color for each color element
            for (int index = 0; index < _colorElements.Count; index++)
            {
                // Calculate the color element from the relative position of the color element within the scale 
                _colorElements[index] = ComputeColor((double)index / (_colorElements.Count - 1)); 
            }
        }

        // Compute the color for the given position
        // If there is no color key, make the color transparent 
        // Find in which segment of color keys the position is
        // If there is no color key before the given position, make the color transparent
        // If there is no color key after the given position, make the color equal to last color key
        // Else calculate the relative position between the two found color keys and
        // compute the color components based on this relative position
        // Param
        //  - position : the given position. Must be between 0 and 1.
        // Return
        //  - Color : the computed color
        private Color ComputeColor(double position)
        {
            // If there is no color key, make the full color scale transparent
            if (colorKeys.Count == 0) return NamedColorMaps.TransparentColor;

            ColorKey colorSegmentStart = new ColorKey { Position = 0.0, ARGBValue = NamedColorMaps.TransparentColor };
            ColorKey colorSegmentEnd = new ColorKey { Position = 0.0, ARGBValue = NamedColorMaps.TransparentColor };
            bool startFound = false, endFound = false;

            // Find the color segment (satrt key and end key) corresponding to the position
            foreach (ColorKey colorKey in colorKeys)
            {
                // If we are at the exact position of a color key, return the color key
                if (position == colorKey.Position) return colorKey.ARGBValue;

                if (position > colorKey.Position)
                {
                    colorSegmentStart = colorKey;
                    startFound = true;
                }
                else if (position < colorKey.Position)
                {
                    colorSegmentEnd = colorKey;
                    endFound = true;
                }
            }

            // Position is before any color segment, return transparent
            if (!startFound) return NamedColorMaps.TransparentColor;
            // Position is after last color segment, return last color key
            if (!endFound) return colorSegmentStart.ARGBValue;
            
            // else calsulate relative position within the found segment
            double relativePosition = (position - colorSegmentStart.Position) / (colorSegmentEnd.Position - colorSegmentStart.Position);
            // and ompute color components through interpolation based on relative position
            Color color;
            color.R = ColorComponent(relativePosition, colorSegmentStart.ARGBValue.R, colorSegmentEnd.ARGBValue.R);
            color.G = ColorComponent(relativePosition, colorSegmentStart.ARGBValue.G, colorSegmentEnd.ARGBValue.G);
            color.B = ColorComponent(relativePosition, colorSegmentStart.ARGBValue.B, colorSegmentEnd.ARGBValue.B);
            // Make color fully opaque
            color.A = 255;

            return color;
        }

        // Color component interpolation within the [start , end] range based on position
        // Params
        //  - start , end : respectively begining and end of the range
        //  - position : relative position within the range. Must be between 0 and 1.
        // Return
        //  - byte : to be used as ARGB color component
        private static byte ColorComponent(double position, byte start, byte end) => Convert.ToByte(start + (end - start) * position);
        #endregion
#pragma warning restore CS0419 // La référence de l'attribut cref est ambiguë
    }
}