// ============================================================
// DOMAIN/Entities.cs
// Entidades base del dominio — sin dependencias externas
// ============================================================
using System;
using System.Collections.Generic;

namespace RepoEstudio.Domain
{
    // ── Entidad base con Id genérico ────────────────────────
    public abstract class EntityBase
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    // ── Producto ────────────────────────────────────────────
    public class Producto : EntityBase
    {
        public string Nombre      { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio     { get; set; }
        public int Stock          { get; set; }
        public string Categoria   { get; set; }
    }

    // ── Pedido / Trabajo genérico ───────────────────────────
    public class Pedido : EntityBase
    {
        public string Descripcion { get; set; }
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
        public int UsuarioId      { get; set; }
    }

    public enum EstadoPedido { Pendiente, EnProceso, Completado, Cancelado }

    // ── Usuario ─────────────────────────────────────────────
    public class Usuario : EntityBase
    {
        public string Nombre   { get; set; }
        public string Email    { get; set; }
        public string Password { get; set; }
        public string Rol      { get; set; } = "Cliente";
    }

    // ── Trabajo de impresión (para el ejercicio del parcial) ─
    public class TrabajoPrint
    {
        public int    Id          { get; set; }
        public string Descripcion { get; set; }
        public string Usuario     { get; set; }
        public DateTime Enviado   { get; set; } = DateTime.Now;
    }
}
