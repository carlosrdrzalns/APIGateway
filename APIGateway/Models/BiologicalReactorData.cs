using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APIGateway.Models
{
    public class BiologicalReactorData
    {
		[Key]
		public Guid ID { get; set; }
		public DateTime timestamp { get; set; }
		public double SI0 { get; set; }
		public double SS0 { get; set; }
		public double XI0 { get; set; }
		public double XS0 { get; set; }
		public double XBH0 { get; set; }
		public double XBA0 { get; set; }
		public double XP0 { get; set; }
		public double SO0 { get; set; }
		public double SNO0 { get; set; }
		public double SNH0 { get; set; }
		public double SND0 { get; set; }
		public double XND0 { get; set; }
		public double SALK0 { get; set; }
		public double SI_ini { get; set; }
		public double SS_ini { get; set; }
		public double XI_ini { get; set; }
		public double XS_ini { get; set; }
		public double XBH_ini { get; set; }
		public double XBA_ini { get; set; }
		public double XP_ini { get; set; }
		public double SO_ini { get; set; }
		public double SNO_ini { get; set; }
		public double SNH_ini { get; set; }
		public double SND_ini { get; set; }
		public double XND_ini { get; set; }
		public double SALK_ini { get; set; }
		public double SI_ini2 { get; set; }
		public double SS_ini2 { get; set; }
		public double XI_ini2 { get; set; }
		public double XS_ini2 { get; set; }
		public double XBH_ini2 { get; set; }
		public double XBA_ini2 { get; set; }
		public double XP_ini2 { get; set; }
		public double SO_ini2 { get; set; }
		public double SNO_ini2 { get; set; }
		public double SNH_ini2 { get; set; }
		public double SND_ini2 { get; set; }
		public double XND_ini2 { get; set; }
		public double SALK_ini2 { get; set; }
	}
}
