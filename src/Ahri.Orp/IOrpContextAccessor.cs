namespace Ahri.Orp
{
    public interface IOrpContextAccessor
    {
        /// <summary>
        /// Gets the <see cref="IOrpContext"/> instance.
        /// </summary>
        IOrpContext Instance { get; }
    }
}
