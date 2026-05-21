// ============================================================
// DAL/IRepository.cs  +  implementaciones
// PATRÓN: Repository  (abstrae el acceso a datos)
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using RepoEstudio.Domain;

namespace RepoEstudio.DAL
{
    // ── Interfaz genérica ────────────────────────────────────
    public interface IRepository<T> where T : EntityBase
    {
        void   Agregar(T entidad);
        T      ObtenerPorId(int id);
        IEnumerable<T> ObtenerTodos();
        void   Actualizar(T entidad);
        void   Eliminar(int id);
    }

    // ── Implementación en memoria (útil para exámenes/tests) ─
    public class RepositorioEnMemoria<T> : IRepository<T> where T : EntityBase
    {
        protected readonly List<T> _datos = new List<T>();
        private static int _nextId = 1;

        public void Agregar(T entidad)
        {
            entidad.Id = _nextId++;
            _datos.Add(entidad);
            Console.WriteLine($"[Repo] Agregado: {typeof(T).Name} Id={entidad.Id}");
        }

        public T ObtenerPorId(int id) => _datos.FirstOrDefault(x => x.Id == id);

        public IEnumerable<T> ObtenerTodos() => _datos;

        public void Actualizar(T entidad)
        {
            var idx = _datos.FindIndex(x => x.Id == entidad.Id);
            if (idx >= 0) _datos[idx] = entidad;
        }

        public void Eliminar(int id)
        {
            var item = ObtenerPorId(id);
            if (item != null) _datos.Remove(item);
        }
    }

    // ── Repo concreto de Producto con método extra ───────────
    public class ProductoRepository : RepositorioEnMemoria<Producto>
    {
        public IEnumerable<Producto> ObtenerPorCategoria(string cat) =>
            _datos.Where(p => p.Categoria == cat);

        public bool HayStock(int productoId)
        {
            var p = ObtenerPorId(productoId);
            return p != null && p.Stock > 0;
        }
    }

    // ── Repo concreto de Pedido ─────────────────────────────
    public class PedidoRepository : RepositorioEnMemoria<Pedido>
    {
        public IEnumerable<Pedido> ObtenerPorUsuario(int usuarioId) =>
            _datos.Where(p => p.UsuarioId == usuarioId);
    }

    // ── Repo concreto de Usuario ────────────────────────────
    public class UsuarioRepository : RepositorioEnMemoria<Usuario>
    {
        public Usuario ObtenerPorEmail(string email) =>
            _datos.FirstOrDefault(u => u.Email == email);
    }
}
