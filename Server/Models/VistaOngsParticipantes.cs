using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TransparencyServer.Models 
{
    // FIX: Quitamos [Key] e Id.
    public class VistaOngsParticipantes
    {
        [Column("TotalONGsInscritas")]
        public int TotalONGsInscritas { get; set; }
    }
}