using System;
using Windows.UI;
using Windows.ApplicationModel.Resources;

namespace CatsHelpers.ColorMaps
{
    /// <summary>
    /// Define a color key to build the color scale. 
    /// It defines a Color at a relative position within the scale (% from first color in scale).
    /// </summary>
    /// <remarks>Equatable structure</remarks>
    public struct ColorKey : IEquatable<ColorKey>
    {
        // Position property internal back store
        private double _position;

        /// <value>Get or set Color value for the key.</value>
        public Color ARGBValue { get; set; }

        /// <summary>Key position</summary>
        /// <value>Get or set relative position of the key within the scale.</value>
        /// <remarks>Value is a percentage and must be between 0 and 1.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException"><see cref="Position"/> Must be in the [0 - 1] range</exception>
        public double Position 
        { 
            get => _position; 
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(Position), ResourceLoader.GetForCurrentView("CatsHelpers/ErrorMessages").GetString("ValueNotPercentage"));
                }
                _position = value;
            }
        }

        /// <summary>
        /// Allow a comparison of two color keys based on their relative position withtin the scale.
        /// <paramref name="x"/> is greater than <paramref name="y"/> if its relative position is greater than <paramref name="y"/> relative position.
        /// Compliant with <a href="https://docs.microsoft.com/en-us/dotnet/api/system.comparison-1?view=netframework-4.8">Comparison&lt;T></a> delegate for list sorting.
        /// </summary>
        /// <remarks>Static method</remarks>
        /// <param name="x">First key</param>
        /// <param name="y">Second key</param>
        /// <returns>-1 : x &lt; y ; 0 : x = y ; 1 x > y</returns>
        public static int Compare(ColorKey x, ColorKey y)
        {
            if (x.Position == y.Position) return 0;
            return (x.Position < y.Position) ? -1 : 1;
        }

        /// <summary>
        /// Return the hash code for this instance
        /// </summary>
        /// <returns><see cref="int"/> The hash code</returns>
        public override int GetHashCode() => Convert.ToInt32(_position) ^ ARGBValue.GetHashCode();

        /// <summary>
        /// Equality operator 
        /// </summary>
        /// <param name="left"><see cref="ColorKey"/></param>
        /// <param name="right"><see cref="ColorKey"/></param>
        /// <returns>True if <paramref name="left"/> is equal to <paramref name="right"/>, false otherwise</returns>
        public static bool operator ==(ColorKey left, ColorKey right) => left.Equals(right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left"><see cref="ColorKey"/></param>
        /// <param name="right"><see cref="ColorKey"/></param>
        /// <returns>True if <paramref name="left"/> is not equals to <paramref name="right"/>, false otherwise</returns>
        public static bool operator !=(ColorKey left, ColorKey right) => !(left == right);

        /// <summary>
        /// Indicates whether this instance and another one are equal
        /// </summary>
        /// <param name="other">The other <see cref="ColorKey"/></param>
        /// <remarks>Keys are equal if their position and color are equal.</remarks>
        /// <returns>True if equal</returns>
        public bool Equals(ColorKey other) => other.Position == _position && other.ARGBValue == ARGBValue;

        /// <summary>
        /// Indicates whether this instance and another object are equal
        /// </summary>
        /// <param name="obj">The other <see cref="object"/></param>
        /// <remarks>Keys are equal if their position and color are equal.</remarks>
        /// <returns>True if object is a <see cref="ColorKey"/> and if keys are equal</returns>
        public override bool Equals(object obj) => obj is ColorKey && Equals((ColorKey)obj);
    }  
}