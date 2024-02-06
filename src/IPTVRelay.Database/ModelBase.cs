using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database
{
    public class ModelBase
    {
        [Key]
        public long Id { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Modified { get; set; } = DateTime.UtcNow;

        public override bool Equals(object? obj)
        {
            if (obj?.GetType() == GetType())
            {
                if (((ModelBase)obj).Id == Id)
                {
                    return true;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode() + GetType().GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Id}";
        }
    }
}
