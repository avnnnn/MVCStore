using System.ComponentModel.DataAnnotations.Schema;

namespace MVCStore.Models.Data
{
    [Table("tblRoles")]
    public class RoleDTO
    {
        public int id { get; set; }
        public string Name { get; set; }
    }
}