using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PruebaTecnicaApi.Model
{
    public class Tarea
    {
        public Guid Id { get; set; }

        public Guid IdMeta { get; set; }
        public string Nombre { get; set; }

        public DateTime FechaCreada { get; set; }

        public string Estado { get; set; }
        public bool IsImportant { get; set; }
    }
}
