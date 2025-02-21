namespace NetDuid
{
    /// <content>
    ///     <see cref="Duid"/> implementation of operators
    /// </content>
    public sealed partial class Duid
    {
        #region Operators

        /// <summary>
        ///     Equality operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are logically or referentially equal.
        ///     Returns <see langword="true"/> if both are <see langword="null"/>.
        /// </returns>
        public static bool operator ==(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null || rhs is null)
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>
        ///     Inequality operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> and <paramref name="rhs"/> are logically or referentially unequal.
        ///     Returns <see langword="false"/> if both are <see langword="null"/>.
        /// </returns>
        public static bool operator !=(Duid lhs, Duid rhs) => !(lhs == rhs);

        /// <summary>
        ///     Less than operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is less than <paramref name="rhs"/>
        /// </returns>
        public static bool operator <(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return false;
            }

            if (lhs is null)
            {
                return !(rhs is null); // null is considered less than any non-null value
            }

            if (rhs is null)
            {
                return false; // non-null is not less than null
            }

            return lhs.CompareTo(rhs) < 0;
        }

        /// <summary>
        ///     Less than or equal to operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is less than or equal to <paramref name="rhs"/>.
        /// </returns>
        public static bool operator <=(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null)
            {
                return !(rhs is null); // null is considered less than any non-null value
            }

            if (rhs is null)
            {
                return false; // non-null is not less than null
            }

            return lhs.CompareTo(rhs) <= 0;
        }

        /// <summary>
        ///     Greater than operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is greater than <paramref name="rhs"/>.
        /// </returns>
        public static bool operator >(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return false;
            }

            if (lhs is null)
            {
                return false; // null is not greater than any value
            }

            if (rhs is null)
            {
                return true; // any non-null value is greater than null
            }

            return lhs.CompareTo(rhs) > 0;
        }

        /// <summary>
        ///     Greater than or equal to operator
        /// </summary>
        /// <param name="lhs">Left hand operand</param>
        /// <param name="rhs">Right hand operand</param>
        /// <remarks>null is considered less than any non-null value</remarks>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="lhs"/> is greater than or equal to <paramref name="rhs"/>.
        /// </returns>
        public static bool operator >=(Duid lhs, Duid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null)
            {
                return false; // null is not greater than any value
            }

            if (rhs is null)
            {
                return true; // any non-null value is greater than null
            }

            return lhs.CompareTo(rhs) >= 0;
        }

        #endregion
    }
}
