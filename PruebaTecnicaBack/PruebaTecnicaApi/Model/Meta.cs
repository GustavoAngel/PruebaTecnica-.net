using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PruebaTecnicaApi.Model
{
    public class Meta
    {
        public Guid id { get; set; }
        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int TotalTareas { get; set; }

        public int Porciento { get; set; }

        public int Completadas { get; set; }
        
    }
}
