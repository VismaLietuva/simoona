using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models
{
    public abstract class AbstractClassifier : BaseModelWithOrg
    {
        public virtual string Name { get; set; }

        public virtual string Value { get; set; }

        [ForeignKey("Parent")]
        public virtual int? ParentId { get; set; }

        public virtual AbstractClassifier Parent { get; set; }

        public virtual string SortOrder { get; set; }

        [InverseProperty("Parent")]
        public virtual ICollection<AbstractClassifier> Childs { get; set; }
    }
}