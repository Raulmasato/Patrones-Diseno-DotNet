// ============================================================
// BLL/Patrones.cs
// PATRONES: Singleton, Factory, Observer, Decorator, Strategy
// ============================================================
using System;
using System.Collections.Generic;
using RepoEstudio.Domain;
using RepoEstudio.DAL;

namespace RepoEstudio.BLL
{
    // ════════════════════════════════════════════════════════
    // 1. SINGLETON
    //    Garantiza UNA sola instancia. Ej: Impresora, Config,
    //    Logger, ConexionDB.
    //    Cómo aplicarlo: constructor privado + propiedad static.
    // ════════════════════════════════════════════════════════
    public sealed class Impresora
    {
        private static Impresora _instancia;
        private static readonly object _lock = new object();

        // Cola de trabajos (acceso sincronizado con Monitor)
        private readonly Queue<TrabajoPrint> _cola = new Queue<TrabajoPrint>();
        private int _contadorId = 1;

        private Impresora()
        {
            Console.WriteLine("[Impresora] Instancia creada (Singleton)");
        }

        public static Impresora Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    lock (_lock)
                    {
                        if (_instancia == null)
                            _instancia = new Impresora();
                    }
                }
                return _instancia;
            }
        }

        // ── Encolar trabajo (thread-safe con Monitor) ────────
        public void EnviarTrabajo(string descripcion, string usuario)
        {
            lock (_cola)
            {
                var trabajo = new TrabajoPrint
                {
                    Id          = _contadorId++,
                    Descripcion = descripcion,
                    Usuario     = usuario
                };
                _cola.Enqueue(trabajo);
                Console.WriteLine($"[Impresora] Trabajo #{trabajo.Id} encolado por {usuario}");
                System.Threading.Monitor.Pulse(_cola); // notifica al consumidor
            }
        }

        // ── Obtener siguiente trabajo ────────────────────────
        public TrabajoPrint ObtenerSiguienteTrabajo()
        {
            lock (_cola)
            {
                while (_cola.Count == 0)
                    System.Threading.Monitor.Wait(_cola); // espera hasta que haya trabajo
                return _cola.Dequeue();
            }
        }

        // ── Imprimir ─────────────────────────────────────────
        public void Imprimir(TrabajoPrint trabajo)
        {
            Console.WriteLine($"[Impresora] Imprimiendo trabajo #{trabajo.Id}: {trabajo.Descripcion}");
            System.Threading.Thread.Sleep(500); // simula tiempo de impresión
            Console.WriteLine($"[Impresora] Trabajo #{trabajo.Id} COMPLETADO");
        }
    }


    // ════════════════════════════════════════════════════════
    // 2. FACTORY METHOD
    //    Crea objetos sin exponer la lógica de instanciación.
    //    Ej: diferentes tipos de notificaciones, reportes,
    //    productos, conexiones.
    // ════════════════════════════════════════════════════════
    public interface INotificacion
    {
        void Enviar(string destinatario, string mensaje);
    }

    public class NotificacionEmail : INotificacion
    {
        public void Enviar(string dest, string msg) =>
            Console.WriteLine($"[EMAIL] Para: {dest} | Msg: {msg}");
    }

    public class NotificacionSMS : INotificacion
    {
        public void Enviar(string dest, string msg) =>
            Console.WriteLine($"[SMS] Para: {dest} | Msg: {msg}");
    }

    public class NotificacionPush : INotificacion
    {
        public void Enviar(string dest, string msg) =>
            Console.WriteLine($"[PUSH] Para: {dest} | Msg: {msg}");
    }

    // Factory
    public static class NotificacionFactory
    {
        public static INotificacion Crear(string tipo)
        {
            switch (tipo.ToLower())
            {
                case "email": return new NotificacionEmail();
                case "sms":   return new NotificacionSMS();
                case "push":  return new NotificacionPush();
                default: throw new ArgumentException($"Tipo desconocido: {tipo}");
            }
        }
    }
    // USO: var n = NotificacionFactory.Crear("email");
    //      n.Enviar("user@mail.com", "Tu pedido fue enviado");


    // ════════════════════════════════════════════════════════
    // 3. OBSERVER (Publicador - Suscriptor)
    //    Notifica a múltiples objetos cuando cambia el estado
    //    de otro. Ej: eventos de UI, cambio de stock, alertas.
    // ════════════════════════════════════════════════════════
    public interface IObserver
    {
        void Actualizar(string evento, object datos);
    }

    public interface IPublicador
    {
        void Suscribir(IObserver obs);
        void Desuscribir(IObserver obs);
        void Notificar(string evento, object datos);
    }

    public class GestorPedidos : IPublicador
    {
        private readonly List<IObserver> _suscriptores = new List<IObserver>();
        private readonly PedidoRepository _repo = new PedidoRepository();

        public void Suscribir(IObserver obs)   => _suscriptores.Add(obs);
        public void Desuscribir(IObserver obs) => _suscriptores.Remove(obs);

        public void Notificar(string evento, object datos)
        {
            foreach (var obs in _suscriptores)
                obs.Actualizar(evento, datos);
        }

        public void CrearPedido(Pedido pedido)
        {
            _repo.Agregar(pedido);
            Notificar("PedidoCreado", pedido);
        }

        public void CambiarEstado(int pedidoId, EstadoPedido nuevoEstado)
        {
            var p = _repo.ObtenerPorId(pedidoId);
            if (p == null) throw new Exception($"Pedido {pedidoId} no existe");
            p.Estado = nuevoEstado;
            _repo.Actualizar(p);
            Notificar("EstadoCambiado", p);
        }
    }

    // Suscriptores concretos
    public class LogObserver : IObserver
    {
        public void Actualizar(string evento, object datos) =>
            Console.WriteLine($"[LOG] Evento: {evento} | Datos: {datos}");
    }

    public class EmailObserver : IObserver
    {
        public void Actualizar(string evento, object datos) =>
            Console.WriteLine($"[EMAIL_OBS] Notificando por email - Evento: {evento}");
    }
    // USO:
    //   var gestor = new GestorPedidos();
    //   gestor.Suscribir(new LogObserver());
    //   gestor.Suscribir(new EmailObserver());
    //   gestor.CrearPedido(new Pedido { Descripcion = "Cables x10" });


    // ════════════════════════════════════════════════════════
    // 4. DECORATOR
    //    Agrega comportamiento a un objeto dinámicamente,
    //    sin modificar su clase. Ej: loggers con cadena,
    //    validaciones extra, cache.
    // ════════════════════════════════════════════════════════
    public interface IServicioPedido
    {
        void ProcesarPedido(Pedido pedido);
    }

    public class ServicioPedidoBase : IServicioPedido
    {
        public void ProcesarPedido(Pedido pedido) =>
            Console.WriteLine($"[BASE] Procesando pedido: {pedido.Descripcion}");
    }

    // Decorador abstracto
    public abstract class PedidoDecorador : IServicioPedido
    {
        protected readonly IServicioPedido _inner;
        protected PedidoDecorador(IServicioPedido inner) { _inner = inner; }
        public virtual void ProcesarPedido(Pedido pedido) => _inner.ProcesarPedido(pedido);
    }

    // Decorador de log
    public class LogDecorador : PedidoDecorador
    {
        public LogDecorador(IServicioPedido inner) : base(inner) { }
        public override void ProcesarPedido(Pedido pedido)
        {
            Console.WriteLine($"[LOG_DEC] INICIO - {DateTime.Now:HH:mm:ss}");
            base.ProcesarPedido(pedido);
            Console.WriteLine($"[LOG_DEC] FIN    - {DateTime.Now:HH:mm:ss}");
        }
    }

    // Decorador de validación
    public class ValidacionDecorador : PedidoDecorador
    {
        public ValidacionDecorador(IServicioPedido inner) : base(inner) { }
        public override void ProcesarPedido(Pedido pedido)
        {
            if (string.IsNullOrEmpty(pedido.Descripcion))
                throw new Exception("El pedido no tiene descripción");
            Console.WriteLine("[VALID_DEC] Validación OK");
            base.ProcesarPedido(pedido);
        }
    }
    // USO:
    //   IServicioPedido svc = new ServicioPedidoBase();
    //   svc = new ValidacionDecorador(svc);
    //   svc = new LogDecorador(svc);
    //   svc.ProcesarPedido(new Pedido { Descripcion = "Resistencias" });


    // ════════════════════════════════════════════════════════
    // 5. STRATEGY
    //    Define familia de algoritmos intercambiables.
    //    Ej: distintos métodos de pago, ordenamiento,
    //    cálculo de precio, exportación.
    // ════════════════════════════════════════════════════════
    public interface IEstrategiaPago
    {
        void Pagar(decimal monto);
    }

    public class PagoEfectivo : IEstrategiaPago
    {
        public void Pagar(decimal monto) =>
            Console.WriteLine($"[PAGO] Efectivo: ${monto}");
    }

    public class PagoTarjeta : IEstrategiaPago
    {
        public void Pagar(decimal monto) =>
            Console.WriteLine($"[PAGO] Tarjeta: ${monto} (+ 3% recargo = ${monto * 1.03m:F2})");
    }

    public class PagoMercadoPago : IEstrategiaPago
    {
        public void Pagar(decimal monto) =>
            Console.WriteLine($"[PAGO] MercadoPago: ${monto} (QR generado)");
    }

    // Contexto que usa la estrategia
    public class CarritoCompras
    {
        private IEstrategiaPago _estrategia;
        public void SetEstrategia(IEstrategiaPago e) => _estrategia = e;
        public void Checkout(decimal total)
        {
            if (_estrategia == null) throw new Exception("No hay estrategia de pago");
            _estrategia.Pagar(total);
        }
    }
    // USO:
    //   var carrito = new CarritoCompras();
    //   carrito.SetEstrategia(new PagoMercadoPago());
    //   carrito.Checkout(1500m);
}
