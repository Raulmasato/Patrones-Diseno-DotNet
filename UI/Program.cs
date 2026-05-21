// ============================================================
// UI/Program.cs
// Punto de entrada: demos de TODOS los patrones
// Compilar en .NET Framework 4.7.2 — Console Application
// ============================================================
using System;
using RepoEstudio.BLL;
using RepoEstudio.DAL;
using RepoEstudio.Domain;
using RepoEstudio.Services;

namespace RepoEstudio.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════════╗");
                Console.WriteLine("║   REPO DE ESTUDIO — PATRONES DE DISEÑO  ║");
                Console.WriteLine("╠══════════════════════════════════════════╣");
                Console.WriteLine("║  1. Singleton                            ║");
                Console.WriteLine("║  2. Factory Method                       ║");
                Console.WriteLine("║  3. Observer                             ║");
                Console.WriteLine("║  4. Decorator                            ║");
                Console.WriteLine("║  5. Strategy                             ║");
                Console.WriteLine("║  6. Adapter                              ║");
                Console.WriteLine("║  7. Template Method                      ║");
                Console.WriteLine("║  8. Command (+ Undo)                     ║");
                Console.WriteLine("║  9. Builder                              ║");
                Console.WriteLine("║ 10. State                                ║");
                Console.WriteLine("║ 11. Proxy                                ║");
                Console.WriteLine("║ 12. Facade                               ║");
                Console.WriteLine("║ 13. Repository                           ║");
                Console.WriteLine("║ 14. Productor-Consumidor (PARCIAL)       ║");
                Console.WriteLine("║  0. Salir                                ║");
                Console.WriteLine("╚══════════════════════════════════════════╝");
                Console.Write("Opción: ");
                var op = Console.ReadLine();
                Console.WriteLine();

                switch (op)
                {
                    case "1": DemoSingleton();         break;
                    case "2": DemoFactory();            break;
                    case "3": DemoObserver();           break;
                    case "4": DemoDecorator();          break;
                    case "5": DemoStrategy();           break;
                    case "6": DemoAdapter();            break;
                    case "7": DemoTemplate();           break;
                    case "8": DemoCommand();            break;
                    case "9": DemoBuilder();            break;
                    case "10": DemoState();             break;
                    case "11": DemoProxy();             break;
                    case "12": DemoFacade();            break;
                    case "13": DemoRepository();        break;
                    case "14": SistemaPrintService.Ejecutar(); break;
                    case "0": salir = true;             break;
                    default: Console.WriteLine("Opción inválida"); break;
                }

                if (!salir)
                {
                    Console.WriteLine("\nPresione ENTER para continuar...");
                    Console.ReadLine();
                }
            }
        }

        // ── 1. SINGLETON ─────────────────────────────────────
        static void DemoSingleton()
        {
            Console.WriteLine("─── SINGLETON ───");
            var a = Impresora.Instancia;
            var b = Impresora.Instancia;
            Console.WriteLine($"¿Misma instancia? {ReferenceEquals(a, b)}");
            a.EnviarTrabajo("Informe mensual", "Demo");
        }

        // ── 2. FACTORY ───────────────────────────────────────
        static void DemoFactory()
        {
            Console.WriteLine("─── FACTORY METHOD ───");
            string[] tipos = { "email", "sms", "push" };
            foreach (var t in tipos)
            {
                var n = NotificacionFactory.Crear(t);
                n.Enviar("usuario@uai.edu", $"Prueba de {t}");
            }
        }

        // ── 3. OBSERVER ──────────────────────────────────────
        static void DemoObserver()
        {
            Console.WriteLine("─── OBSERVER ───");
            var gestor = new GestorPedidos();
            gestor.Suscribir(new LogObserver());
            gestor.Suscribir(new EmailObserver());
            gestor.CrearPedido(new Pedido { Descripcion = "Resistencias 1k x100" });
            gestor.CambiarEstado(1, EstadoPedido.EnProceso);
        }

        // ── 4. DECORATOR ─────────────────────────────────────
        static void DemoDecorator()
        {
            Console.WriteLine("─── DECORATOR ───");
            IServicioPedido svc = new ServicioPedidoBase();
            svc = new ValidacionDecorador(svc);
            svc = new LogDecorador(svc);
            svc.ProcesarPedido(new Pedido { Descripcion = "Condensadores 100uF" });
        }

        // ── 5. STRATEGY ──────────────────────────────────────
        static void DemoStrategy()
        {
            Console.WriteLine("─── STRATEGY ───");
            var carrito = new CarritoCompras();
            carrito.SetEstrategia(new PagoEfectivo());    carrito.Checkout(1000m);
            carrito.SetEstrategia(new PagoTarjeta());     carrito.Checkout(1000m);
            carrito.SetEstrategia(new PagoMercadoPago()); carrito.Checkout(1000m);
        }

        // ── 6. ADAPTER ───────────────────────────────────────
        static void DemoAdapter()
        {
            Console.WriteLine("─── ADAPTER ───");
            IEnviadorCorreo correo = new SmtpAdapter();
            correo.Enviar("admin@uai.edu", "Sistema listo", "El sistema fue configurado.");
        }

        // ── 7. TEMPLATE METHOD ───────────────────────────────
        static void DemoTemplate()
        {
            Console.WriteLine("─── TEMPLATE METHOD ───");
            Console.WriteLine(">> Reporte PDF:");
            GeneradorReporte rep1 = new ReportePDF();
            rep1.Generar();
            Console.WriteLine("\n>> Reporte Excel:");
            GeneradorReporte rep2 = new ReporteExcel();
            rep2.Generar();
        }

        // ── 8. COMMAND ───────────────────────────────────────
        static void DemoCommand()
        {
            Console.WriteLine("─── COMMAND ───");
            var repo    = new PedidoRepository();
            var gestor  = new GestorComandos();
            var pedido  = new Pedido { Descripcion = "Transistores NPN" };
            gestor.Ejecutar(new ComandoCrearPedido(repo, pedido));
            Console.WriteLine(">> Deshaciendo...");
            gestor.Deshacer();
            gestor.Deshacer(); // nada que deshacer
        }

        // ── 9. BUILDER ───────────────────────────────────────
        static void DemoBuilder()
        {
            Console.WriteLine("─── BUILDER ───");
            var cfg = new ConfiguracionBuilder()
                .ConNombre("ElectroJoule")
                .ConConexionDB("Server=.;Database=EJ;Trusted_Connection=True;")
                .ConMaxHilos(4)
                .EnModoDebug()
                .ConSmtp("smtp.gmail.com")
                .Build();
            Console.WriteLine(cfg);
        }

        // ── 10. STATE ────────────────────────────────────────
        static void DemoState()
        {
            Console.WriteLine("─── STATE ───");
            var pedido = new ContextoPedido("Cable HDMI x5");
            pedido.Procesar(); // Pendiente → En Proceso
            pedido.Procesar(); // En Proceso → Completado
            pedido.Cancelar(); // No se puede cancelar
            pedido.Procesar(); // Ya está completado

            var pedido2 = new ContextoPedido("Fusibles 5A");
            pedido2.Cancelar(); // Cancelado desde Pendiente
            pedido2.Procesar(); // No se puede procesar
        }

        // ── 11. PROXY ────────────────────────────────────────
        static void DemoProxy()
        {
            Console.WriteLine("─── PROXY ───");
            IServicioArchivo svcAdmin = new ProxyArchivoSeguro("admin");
            svcAdmin.LeerArchivo("C:/secreto.txt");

            IServicioArchivo svcUser = new ProxyArchivoSeguro("invitado");
            svcUser.LeerArchivo("C:/secreto.txt");
        }

        // ── 12. FACADE ───────────────────────────────────────
        static void DemoFacade()
        {
            Console.WriteLine("─── FACADE ───");
            // Primero agregamos un producto con stock
            var prodRepo = new ProductoRepository();
            prodRepo.Agregar(new Producto
            {
                Nombre    = "Arduino Uno",
                Precio    = 3500m,
                Stock     = 5,
                Categoria = "Microcontroladores"
            });

            var facade = new FacadeProcesoPedido();
            facade.ProcesarCompra(1, 42, "cliente@electrojoule.com");
        }

        // ── 13. REPOSITORY ───────────────────────────────────
        static void DemoRepository()
        {
            Console.WriteLine("─── REPOSITORY ───");
            var repo = new ProductoRepository();
            repo.Agregar(new Producto { Nombre = "Resistencia 1k", Precio = 10, Stock = 100, Categoria = "Pasivos" });
            repo.Agregar(new Producto { Nombre = "LED Rojo",        Precio = 15, Stock = 200, Categoria = "Activos" });
            repo.Agregar(new Producto { Nombre = "Capacitor 100uF", Precio = 20, Stock = 50,  Categoria = "Pasivos" });

            Console.WriteLine(">> Todos:");
            foreach (var p in repo.ObtenerTodos())
                Console.WriteLine($"   [{p.Id}] {p.Nombre} - ${p.Precio}");

            Console.WriteLine(">> Por categoría 'Pasivos':");
            foreach (var p in repo.ObtenerPorCategoria("Pasivos"))
                Console.WriteLine($"   {p.Nombre}");

            Console.WriteLine($">> ¿Stock producto 1? {repo.HayStock(1)}");
        }
    }
}
