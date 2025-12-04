using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TransparencyServer.Models 
{
    // Al usar HasNoKey() en el Contexto, ya no necesitamos [Key] aquí.
    // Solo definimos las columnas que REALMENTE existen en la vista SQL.
    public class VistaMetricaGlobal
    {
        [Column("TotalDonado")]
        public decimal TotalDonado { get; set; }
        
        // ¡IMPORTANTE! No agregues 'public int Id { get; set; }' aquí
        // porque tu vista SQL no tiene esa columna.
    }
}