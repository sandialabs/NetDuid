namespace NetDuid
{
    /// <summary>
    ///     DUID type identifier as specified by RFC8415 Section 11
    /// </summary>
    public enum DuidType
    {
        /// <summary>
        ///     An unspecified DUID type not specified in RFC8415 of RFC6355
        /// </summary>
        /// <remarks>DUID type code <c>0x0000</c> or within range <c>0x0005</c>-<c>0xFFFF</c> inclusive</remarks>
        Undefined = 0,

        /// <summary>
        /// Link-layer address plus time
        /// </summary>
        /// <remarks>DUID type code of <c>0x0001</c></remarks>
        LinkLayerPlusTime = 1,

        /// <summary>
        ///  Vendor-assigned unique ID based on Enterprise Number
        /// </summary>
        /// <remarks>DUID type code of <c>0x0002</c></remarks>
        VendorAssigned = 2,

        /// <summary>
        ///  Link-layer address
        /// </summary>
        /// <remarks>DUID type code of <c>0x0003</c></remarks>
        LinkLayer = 3,

        /// <summary>
        ///  Universally Unique Identifier (UUID) (RFC6355)
        /// </summary>
        /// <remarks>DUID type code of <c>0x0004</c></remarks>
        Uuid = 4,
    }
}
