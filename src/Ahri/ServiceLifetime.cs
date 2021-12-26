namespace Ahri
{
    /// <summary>
    /// Determines the life cycle of service objects.
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// Singleton lifetime is valid in the scope where ServiceDescriptor is registered and its sub-scopes.
        /// </summary>
        Singleton,

        /// <summary>
        /// Scoped lifetime is valid in the scope that requested the service and its sub-scopes.
        /// </summary>
        Scoped,

        /// <summary>
        /// The transient lifetime creates a new service instance on every request.
        /// </summary>
        Transient
    }
}
