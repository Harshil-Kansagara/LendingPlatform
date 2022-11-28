namespace LendingPlatform.Utils.ApplicationClass.Plaid.Entity
{

    /// <summary>
    /// Represents a geographical location.
    /// </summary>
    public class LocationInfoAC
    {
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>The zip.</value>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the latitude (x-coordinate).
        /// </summary>
        /// <value>The latitude.</value>
        public double? Lat { get; set; }

        /// <summary>
        /// Gets or sets the longitude (y-coordinate).
        /// </summary>
        /// <value>The longitude.</value>
        public double? Lon { get; set; }

        /// <summary>
        /// Gets or sets the store number.
        /// </summary>
        /// <value>The store number.</value>
        public string StoreNumber { get; set; }
    }
}
