namespace KioskBrains.Clients.TecDocWs.Models
{
    public class Category
    {
        /// <summary>
        /// <see cref="Category"/> Id.
        /// </summary>
        public int AssemblyGroupNodeId { get; set; }

        /// <summary>
        /// <see cref="Category"/> Name.
        /// </summary>
        public string AssemblyGroupName { get; set; }

        public bool HasChilds { get; set; }

        public int? ParentNodeId { get; set; }

        public override string ToString()
        {
            return $"{AssemblyGroupName} ({AssemblyGroupNodeId}) {HasChilds} {ParentNodeId}";
        }
    }
}