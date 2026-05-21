// ============================================================
// SERVICES/ConcurrenciaService.cs
// PATRÓN: Productor-Consumidor con Thread + Monitor
// Incluye también: Proxy, Facade
// ============================================================
using System;
using System.Collections.Generic;
using System.Threading;
using RepoEstudio.BLL;
using RepoEstudio.Domain;

namespace RepoEstudio.Services
{
    // ════════════════════════════════════════════════════════
    // PRODUCTOR-CONSUMIDOR  (resolución del parcial completa)
    // ════════════════════════════════════════════════════════

    // ── Clase Usuario (productor — corre en su propio Thread) ─
    public class UsuarioProductor
    {
        private readonly string _nombre;
        private readonly int    _cantTrabajos;
        private readonly Random _rnd = new Random();

        public UsuarioProductor(string nombre, int cantTrabajos)
        {
            _nombre       = nombre;
            _cantTrabajos = cantTrabajos;
        }

        // Este método se pasa a new Thread(...)
        public void Producir()
        {
            var impresora = Impresora.Instancia; // Singleton
            for (int i = 1; i <= _cantTrabajos; i++)
            {
                string desc = $"Doc-{_nombre}-{i}";
                impresora.EnviarTrabajo(desc, _nombre);

                // Control de velocidad: espera aleatoria entre envíos
                int espera = _rnd.Next(200, 800);
                Thread.Sleep(espera);
            }
            Console.WriteLine($"[{_nombre}] Finalizó de enviar {_cantTrabajos} trabajos.");
        }
    }

    // ── Hilo consumidor (procesa la cola de la impresora) ────
    public class ImpresoraConsumidor
    {
        private bool _activa = true;

        public void Detener() => _activa = false;

        public void Consumir()
        {
            var impresora = Impresora.Instancia;
            while (_activa)
            {
                try
                {
                    var trabajo = impresora.ObtenerSiguienteTrabajo();
                    impresora.Imprimir(trabajo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Consumidor] Error: {ex.Message}");
                }
            }
        }
    }

    // ── Orquestador: levanta hilos y los coordina ────────────
    public static class SistemaPrintService
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== SISTEMA DE IMPRESIÓN CONCURRENTE ===\n");

            var consumidor = new ImpresoraConsumidor();
            var hiloConsumidor = new Thread(consumidor.Consumir) { IsBackground = true };
            hiloConsumidor.Start();

            // Crear usuarios productores
            var usuarios = new[]
            {
                new UsuarioProductor("Ana",    3),
                new UsuarioProductor("Carlos", 2),
                new UsuarioProductor("Marta",  4),
            };

            var hilosProductores = new List<Thread>();
            foreach (var u in usuarios)
            {
                var hilo = new Thread(u.Producir);
                hilosProductores.Add(hilo);
                hilo.Start();
            }

            // Esperar que terminen todos los productores
            foreach (var h in hilosProductores)
                h.Join();

            Thread.Sleep(2000); // dar tiempo al consumidor
            consumidor.Detener();

            Console.WriteLine("\n=== FIN DEL SISTEMA DE IMPRESIÓN ===");
        }
    }


    // ════════════════════════════════════════════════════════
    // 11. PROXY
    //     Controla el acceso a otro objeto. Ej: autenticación,
    //     cache, acceso remoto, lazy loading.
    // ════════════════════════════════════════════════════════
    public interface IServicioArchivo
    {
        string LeerArchivo(string ruta);
    }

    public class ServicioArchivoReal : IServicioArchivo
    {
        public string LeerArchivo(string ruta)
        {
            Console.WriteLine($"[REAL] Leyendo archivo: {ruta}");
            return "contenido_del_archivo";
        }
    }

    // Proxy con control de acceso
    public class ProxyArchivoSeguro : IServicioArchivo
    {
        private readonly ServicioArchivoReal _real = new ServicioArchivoReal();
        private readonly string _usuarioActual;

        public ProxyArchivoSeguro(string usuario) { _usuarioActual = usuario; }

        public string LeerArchivo(string ruta)
        {
            if (_usuarioActual != "admin")
            {
                Console.WriteLine($"[PROXY] Acceso denegado a {_usuarioActual}");
                return null;
            }
            Console.WriteLine($"[PROXY] Acceso permitido a {_usuarioActual}");
            return _real.LeerArchivo(ruta);
        }
    }
    // USO:
    //   IServicioArchivo svc = new ProxyArchivoSeguro("admin");
    //   svc.LeerArchivo("C:/datos.txt");


    // ════════════════════════════════════════════════════════
    // 12. FACADE
    //     Provee interfaz simplificada para un subsistema
    //     complejo. Ej: API unificada para múltiples servicios,
    //     proceso de checkout, envío de notificaciones.
    // ════════════════════════════════════════════════════════
    public class FacadeProcesoPedido
    {
        private readonly DAL.ProductoRepository  _prodRepo  = new DAL.ProductoRepository();
        private readonly DAL.PedidoRepository    _pedRepo   = new DAL.PedidoRepository();
        private readonly IEnviadorCorreo         _correo    = new SmtpAdapter();

        // Un solo método público que orquesta todo
        public bool ProcesarCompra(int productoId, int usuarioId, string emailUsuario)
        {
            // 1. Verificar stock
            if (!_prodRepo.HayStock(productoId))
            {
                Console.WriteLine("[FACADE] Sin stock");
                return false;
            }

            // 2. Crear pedido
            var pedido = new Pedido
            {
                Descripcion = $"Compra producto #{productoId}",
                UsuarioId   = usuarioId,
                Estado      = EstadoPedido.Pendiente
            };
            _pedRepo.Agregar(pedido);

            // 3. Enviar confirmación
            _correo.Enviar(emailUsuario, "Compra confirmada", $"Pedido #{pedido.Id} recibido.");

            Console.WriteLine($"[FACADE] Compra exitosa. Pedido #{pedido.Id}");
            return true;
        }
    }
    // USO:
    //   var facade = new FacadeProcesoPedido();
    //   facade.ProcesarCompra(1, 5, "cliente@mail.com");
}
