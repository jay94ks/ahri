namespace Ahri
{
    /// <summary>
    /// The service that creates a child scope.
    /// </summary>
    public interface IServiceScopeFactory
    {
        /// <summary>
        /// Creates a child scope.
        /// </summary>
        /// <returns></returns>
        IServiceScope CreateScope();

        /// <summary>
        /// Creates a child scope with scope-specific service registrations.
        /// </summary>
        /// <param name="Services"></param>
        /// <returns></returns>
        IServiceScope CreateScope(IServiceCollection Services);
    }
}
