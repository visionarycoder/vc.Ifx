using Microsoft.EntityFrameworkCore;
using vc.Ifx.Data;

namespace Example.Filtering.Gamma.Orm.Models
{

    [PrimaryKey(nameof(Id))]
    [Index(nameof(Value), IsUnique = true)]
    public class PartType : ReferenceEntity
    {
    
    }

}