using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APIGateway.Models
{
    public class WaterPumpStatusPredictions
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime timestamp { get; set; }
        public double Status { get; set; }
    }
}
