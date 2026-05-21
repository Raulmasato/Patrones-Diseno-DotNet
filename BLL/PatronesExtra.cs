// ============================================================
// BLL/PatronesExtra.cs
// PATRONES: Adapter, Template Method, Command, Builder, State
// ============================================================
using System;
using System.Collections.Generic;
using System.Text;
using RepoEstudio.Domain;

namespace RepoEstudio.BLL
{
    // ════════════════════════════════════════════════════════
    // 6. ADAPTER
    //    Convierte la interfaz de una clase en otra que el
    //    cliente espera. Ej: integrar librería externa, API
    //    vieja, servicio de terceros.
    // ════════════════════════════════════════════════════════

    // Interfaz que NOSOTROS queremos usar
    public interface IEnviadorCorreo
    {
        void Enviar(string para, string asunto, string cuerpo);
    }

    // Clase externa/vieja que NO podemos modificar
    public class SmtpLegacy
    {
        public void SendMail(string[] recipients, string subject, string body, bool isHtml)
        {
            Console.WriteLine($"[SMTP_LEGACY] Enviando a {string.Join(",", recipients)}: {subject}");
        }
    }

    // Adapter: adapta SmtpLegacy a IEnviadorCorreo
    public class SmtpAdapter : IEnviadorCorreo
    {
        private readonly SmtpLegacy _smtp = new SmtpLegacy();

        public void Enviar(string para, string asunto, string cuerpo)
        {
            // Adaptamos la firma
            _smtp.SendMail(new[] { para }, asunto, cuerpo, false);
        }
    }
    // USO:
    //   IEnviadorCorreo correo = new SmtpAdapter();
    //   correo.Enviar("admin@uai.edu", "Pedido recibido", "Tu pedido fue procesado.");


    // ════════════════════════════════════════════════════════
    // 7. TEMPLATE METHOD
    //    Define el esqueleto de un algoritmo en una clase base
    //    y deja que las subclases implementen ciertos pasos.
    //    Ej: distintos tipos de reportes, exportaciones,
    //    procesos con pasos fijos pero variantes.
    // ════════════════════════════════════════════════════════
    public abstract class GeneradorReporte
    {
        // Método plantilla: define el algoritmo
        public void Generar()
        {
            ObtenerDatos();
            ProcesarDatos();
            FormatearSalida();
            Guardar();
        }

        protected abstract void ObtenerDatos();
        protected abstract void ProcesarDatos();
        protected abstract void FormatearSalida();

        // Hook con implementación por defecto (puede sobreescribirse)
        protected virtual void Guardar() =>
            Console.WriteLine("[TEMPLATE] Reporte guardado en disco");
    }

    public class ReportePDF : GeneradorReporte
    {
        protected override void ObtenerDatos()   => Console.WriteLine("[PDF] Consultando BD");
        protected override void ProcesarDatos()  => Console.WriteLine("[PDF] Agrupando datos");
        protected override void FormatearSalida()=> Console.WriteLine("[PDF] Generando PDF con iTextSharp");
    }

    public class ReporteExcel : GeneradorReporte
    {
        protected override void ObtenerDatos()   => Console.WriteLine("[EXCEL] Consultando BD");
        protected override void ProcesarDatos()  => Console.WriteLine("[EXCEL] Calculando totales");
        protected override void FormatearSalida()=> Console.WriteLine("[EXCEL] Generando .xlsx");
        protected override void Guardar()        => Console.WriteLine("[EXCEL] Enviando por email");
    }
    // USO:
    //   GeneradorReporte rep = new ReportePDF();
    //   rep.Generar();


    // ════════════════════════════════════════════════════════
    // 8. COMMAND
    //    Encapsula una solicitud como objeto. Permite
    //    deshacer/rehacer, colas de operaciones, logging.
    //    Ej: botones de UI, sistema de transacciones,
    //    macros, operaciones reversibles.
    // ════════════════════════════════════════════════════════
    public interface IComando
    {
        void Ejecutar();
        void Deshacer();
    }

    public class ComandoCrearPedido : IComando
    {
        private readonly PedidoRepository _repo;
        private readonly Pedido _pedido;
        public ComandoCrearPedido(PedidoRepository repo, Pedido pedido)
        { _repo = repo; _pedido = pedido; }

        public void Ejecutar()
        {
            _repo.Agregar(_pedido);
            Console.WriteLine($"[CMD] Pedido creado: {_pedido.Descripcion}");
        }

        public void Deshacer()
        {
            _repo.Eliminar(_pedido.Id);
            Console.WriteLine($"[CMD] Pedido eliminado (deshacer): {_pedido.Descripcion}");
        }
    }

    // Invocador: mantiene historial de comandos
    public class GestorComandos
    {
        private readonly Stack<IComando> _historial = new Stack<IComando>();

        public void Ejecutar(IComando cmd)
        {
            cmd.Ejecutar();
            _historial.Push(cmd);
        }

        public void Deshacer()
        {
            if (_historial.Count == 0) { Console.WriteLine("[CMD] Nada que deshacer"); return; }
            _historial.Pop().Deshacer();
        }
    }
    // USO:
    //   var repo = new PedidoRepository();
    //   var gestor = new GestorComandos();
    //   gestor.Ejecutar(new ComandoCrearPedido(repo, new Pedido { Descripcion = "Cable USB" }));
    //   gestor.Deshacer();


    // ════════════════════════════════════════════════════════
    // 9. BUILDER
    //    Construye objetos complejos paso a paso.
    //    Ej: configuración de objetos con muchos parámetros,
    //    query builders, constructores de reportes, emails.
    // ════════════════════════════════════════════════════════
    public class ConfiguracionSistema
    {
        public string NombreApp   { get; set; }
        public string ConexionDB  { get; set; }
        public int    MaxHilos    { get; set; }
        public bool   ModoDebug   { get; set; }
        public string SmtpServer  { get; set; }

        public override string ToString() =>
            $"App={NombreApp} | DB={ConexionDB} | Hilos={MaxHilos} | Debug={ModoDebug}";
    }

    public class ConfiguracionBuilder
    {
        private readonly ConfiguracionSistema _cfg = new ConfiguracionSistema();

        public ConfiguracionBuilder ConNombre(string nombre)
            { _cfg.NombreApp = nombre; return this; }

        public ConfiguracionBuilder ConConexionDB(string conn)
            { _cfg.ConexionDB = conn; return this; }

        public ConfiguracionBuilder ConMaxHilos(int n)
            { _cfg.MaxHilos = n; return this; }

        public ConfiguracionBuilder EnModoDebug(bool debug = true)
            { _cfg.ModoDebug = debug; return this; }

        public ConfiguracionBuilder ConSmtp(string smtp)
            { _cfg.SmtpServer = smtp; return this; }

        public ConfiguracionSistema Build() => _cfg;
    }
    // USO:
    //   var cfg = new ConfiguracionBuilder()
    //       .ConNombre("ElectroJoule")
    //       .ConConexionDB("Server=.;Database=EJ;Trusted_Connection=True;")
    //       .ConMaxHilos(4)
    //       .EnModoDebug()
    //       .Build();
    //   Console.WriteLine(cfg);


    // ════════════════════════════════════════════════════════
    // 10. STATE
    //     Permite a un objeto cambiar su comportamiento
    //     cuando su estado interno cambia.
    //     Ej: estados de un pedido, semáforo, conexión,
    //     maqueta de estados de UI.
    // ════════════════════════════════════════════════════════
    public interface IEstadoPedidoHandler
    {
        void Procesar(ContextoPedido ctx);
        void Cancelar(ContextoPedido ctx);
    }

    public class ContextoPedido
    {
        public IEstadoPedidoHandler Estado { get; set; }
        public string Descripcion { get; set; }

        public ContextoPedido(string desc)
        {
            Descripcion = desc;
            Estado = new EstadoPendiente(); // estado inicial
        }

        public void Procesar() => Estado.Procesar(this);
        public void Cancelar() => Estado.Cancelar(this);
    }

    public class EstadoPendiente : IEstadoPedidoHandler
    {
        public void Procesar(ContextoPedido ctx)
        {
            Console.WriteLine($"[STATE] '{ctx.Descripcion}': Pendiente → En Proceso");
            ctx.Estado = new EstadoEnProceso();
        }
        public void Cancelar(ContextoPedido ctx)
        {
            Console.WriteLine($"[STATE] '{ctx.Descripcion}': Cancelado desde Pendiente");
            ctx.Estado = new EstadoCancelado();
        }
    }

    public class EstadoEnProceso : IEstadoPedidoHandler
    {
        public void Procesar(ContextoPedido ctx)
        {
            Console.WriteLine($"[STATE] '{ctx.Descripcion}': En Proceso → Completado");
            ctx.Estado = new EstadoCompletado();
        }
        public void Cancelar(ContextoPedido ctx) =>
            Console.WriteLine("[STATE] No se puede cancelar un pedido en proceso");
    }

    public class EstadoCompletado : IEstadoPedidoHandler
    {
        public void Procesar(ContextoPedido ctx) =>
            Console.WriteLine("[STATE] Pedido ya está completado");
        public void Cancelar(ContextoPedido ctx) =>
            Console.WriteLine("[STATE] No se puede cancelar un pedido completado");
    }

    public class EstadoCancelado : IEstadoPedidoHandler
    {
        public void Procesar(ContextoPedido ctx) =>
            Console.WriteLine("[STATE] Pedido cancelado, no se puede procesar");
        public void Cancelar(ContextoPedido ctx) =>
            Console.WriteLine("[STATE] Ya está cancelado");
    }
    // USO:
    //   var pedido = new ContextoPedido("Resistencias 1k");
    //   pedido.Procesar(); // Pendiente → En Proceso
    //   pedido.Procesar(); // En Proceso → Completado
    //   pedido.Cancelar(); // "No se puede cancelar"
}
