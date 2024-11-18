namespace Eyefinity.Utilities.AuditLogging
{
    public interface IAuditLogger<T>
    {
        public Guid InstanceId { get; }
        /// <summary>
        /// method to excluded entities from auditing
        /// </summary>
        /// <param name="excludedEntities"></param>
        public void SetExcludedEntities(ICollection<string> excludedEntities);

        /// <summary>
        /// Start of audit this is where the elements are gather 
        /// </summary>
        /// <param name="context"></param>
        public void StartEntry(T? context);
        /// <summary>
        /// Finalize the audit details
        /// </summary>
        public void EndEntry();

        public void Failed();
    }
}
