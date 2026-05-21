# REPOSITORIO DE ESTUDIO — PATRONES DE DISEÑO
## Trabajo de Diploma · UAI · .NET Framework 4.7.2

---

## ARQUITECTURA EN CAPAS

```
UI (Program.cs / WinForms)
    ↓  llama a
SERVICES (orquestación, concurrencia)
    ↓  llama a
BLL (lógica de negocio, patrones de comportamiento)
    ↓  llama a
DAL (acceso a datos, Repository)
    ↓  usa
DOMAIN (entidades puras, sin dependencias)
```

---

## PATRONES — RESUMEN RÁPIDO

### 1. SINGLETON
- **Qué resuelve:** Garantizar una única instancia en toda la aplicación.
- **Cuándo usarlo:** Impresora, Logger, Configuración, Conexión DB.
- **Clave:** Constructor privado + propiedad `static` con double-checked locking.

```csharp
public sealed class Impresora {
    private static Impresora _inst;
    private static readonly object _lock = new object();
    private Impresora() { }
    public static Impresora Instancia {
        get {
            if (_inst == null) lock(_lock) if (_inst == null) _inst = new Impresora();
            return _inst;
        }
    }
}
```

---

### 2. FACTORY METHOD
- **Qué resuelve:** Crear objetos sin exponer la lógica de instanciación al cliente.
- **Cuándo usarlo:** Distintos tipos de notificaciones, reportes, conexiones.
- **Clave:** Interfaz + switch/if en la Factory.

```csharp
public static INotificacion Crear(string tipo) {
    switch(tipo) {
        case "email": return new NotificacionEmail();
        case "sms":   return new NotificacionSMS();
        default: throw new ArgumentException("Tipo desconocido");
    }
}
```

---

### 3. OBSERVER (Publicador-Suscriptor)
- **Qué resuelve:** Notificar automáticamente a múltiples objetos ante un cambio.
- **Cuándo usarlo:** Cambio de stock, estados de pedido, eventos de UI, alertas.
- **Clave:** `List<IObserver>` + método `Notificar()`.

```csharp
public void CrearPedido(Pedido p) {
    _repo.Agregar(p);
    Notificar("PedidoCreado", p);  // avisa a todos los suscriptores
}
```

---

### 4. DECORATOR
- **Qué resuelve:** Agregar comportamiento sin modificar la clase original.
- **Cuándo usarlo:** Logger en cadena, validaciones extra, caché.
- **Clave:** Implementar la misma interfaz y envolver el objeto original.

```csharp
IServicioPedido svc = new ServicioPedidoBase();
svc = new ValidacionDecorador(svc);  // agrega validación
svc = new LogDecorador(svc);         // agrega log
svc.ProcesarPedido(pedido);          // ejecuta toda la cadena
```

---

### 5. STRATEGY
- **Qué resuelve:** Intercambiar algoritmos en tiempo de ejecución.
- **Cuándo usarlo:** Métodos de pago, tipos de ordenamiento, formatos de exportación.
- **Clave:** Interfaz de estrategia + setter en el contexto.

```csharp
carrito.SetEstrategia(new PagoMercadoPago());
carrito.Checkout(1500m);
```

---

### 6. ADAPTER
- **Qué resuelve:** Hace compatible una clase con una interfaz que no implementa.
- **Cuándo usarlo:** Integrar librerías externas, APIs viejas, servicios de terceros.
- **Clave:** Clase Adapter implementa la interfaz nueva y usa el objeto viejo internamente.

```csharp
// SmtpLegacy tiene SendMail(...) — no podemos cambiarla
// SmtpAdapter adapta SendMail a Enviar(...)
IEnviadorCorreo correo = new SmtpAdapter();
correo.Enviar("user@mail.com", "Asunto", "Cuerpo");
```

---

### 7. TEMPLATE METHOD
- **Qué resuelve:** Definir el esqueleto de un algoritmo, dejando pasos a subclases.
- **Cuándo usarlo:** Distintos tipos de reportes, procesos con pasos fijos.
- **Clave:** Método `Generar()` en clase base llama métodos `abstract` en subclases.

```csharp
public void Generar() {        // template method — no se sobreescribe
    ObtenerDatos();            // abstract — cada subclase lo implementa
    ProcesarDatos();           // abstract
    FormatearSalida();         // abstract
    Guardar();                 // virtual — tiene implementación por defecto
}
```

---

### 8. COMMAND
- **Qué resuelve:** Encapsula una operación como objeto; permite deshacer/rehacer.
- **Cuándo usarlo:** Botones de UI, transacciones, macros, operaciones reversibles.
- **Clave:** Interfaz `IComando` con `Ejecutar()` y `Deshacer()` + Stack de historial.

```csharp
gestor.Ejecutar(new ComandoCrearPedido(repo, pedido));  // crea
gestor.Deshacer();                                       // borra
```

---

### 9. BUILDER
- **Qué resuelve:** Construir objetos complejos paso a paso con método fluido.
- **Cuándo usarlo:** Objetos con muchos parámetros opcionales, configuraciones.
- **Clave:** Cada método del Builder retorna `this` para encadenar.

```csharp
var cfg = new ConfiguracionBuilder()
    .ConNombre("App")
    .ConMaxHilos(4)
    .EnModoDebug()
    .Build();
```

---

### 10. STATE
- **Qué resuelve:** El comportamiento cambia según el estado interno del objeto.
- **Cuándo usarlo:** Estados de pedido, semáforo, conexión de red, UI con fases.
- **Clave:** Cada estado es una clase; el contexto delega el comportamiento al estado actual.

```csharp
pedido.Procesar();  // Pendiente → En Proceso
pedido.Procesar();  // En Proceso → Completado
pedido.Cancelar();  // "No se puede cancelar un pedido completado"
```

---

### 11. PROXY
- **Qué resuelve:** Controla el acceso a otro objeto (seguridad, caché, lazy loading).
- **Cuándo usarlo:** Autenticación, logging de acceso, acceso remoto.
- **Clave:** Implementa la misma interfaz que el objeto real y decide si delegar.

```csharp
IServicioArchivo svc = new ProxyArchivoSeguro("invitado");
svc.LeerArchivo("datos.txt");  // → "Acceso denegado"
```

---

### 12. FACADE
- **Qué resuelve:** Simplifica el acceso a un subsistema complejo con una interfaz unificada.
- **Cuándo usarlo:** Proceso de checkout, notificaciones múltiples, operaciones compuestas.
- **Clave:** Un método público que orquesta múltiples subsistemas internamente.

```csharp
var facade = new FacadeProcesoPedido();
facade.ProcesarCompra(productoId, usuarioId, email);
// Internamente: verifica stock, crea pedido, envía email
```

---

### 13. REPOSITORY
- **Qué resuelve:** Abstrae el acceso a datos; separa la lógica de negocio del almacenamiento.
- **Cuándo usarlo:** Siempre en la capa DAL. Permite cambiar SQL por en-memoria para tests.
- **Clave:** Interfaz `IRepository<T>` genérica.

```csharp
public interface IRepository<T> where T : EntityBase {
    void   Agregar(T e);
    T      ObtenerPorId(int id);
    IEnumerable<T> ObtenerTodos();
    void   Actualizar(T e);
    void   Eliminar(int id);
}
```

---

### 14. PRODUCTOR-CONSUMIDOR (con Thread + Monitor)
- **Qué resuelve:** Desacopla la producción del consumo; coordina múltiples hilos.
- **Cuándo usarlo:** Colas de impresión, procesamiento asíncrono, pipelines.
- **Clave:**
  - `Monitor.Pulse()` para notificar al consumidor que hay trabajo.
  - `Monitor.Wait()` para que el consumidor espere cuando la cola está vacía.
  - `lock(objeto)` para que solo un hilo acceda a la cola a la vez.

```csharp
// PRODUCTOR
lock (_cola) {
    _cola.Enqueue(trabajo);
    Monitor.Pulse(_cola);  // despierta al consumidor
}

// CONSUMIDOR
lock (_cola) {
    while (_cola.Count == 0)
        Monitor.Wait(_cola);  // espera hasta que haya trabajo
    return _cola.Dequeue();
}
```

---

## GUÍA RÁPIDA: ¿QUÉ PATRÓN USAR?

| Situación del enunciado                          | Patrón         |
|--------------------------------------------------|----------------|
| "solo debe haber una instancia"                  | Singleton      |
| "crear distintos tipos de X"                     | Factory        |
| "notificar cuando cambia el estado"              | Observer       |
| "agregar funcionalidad sin modificar la clase"   | Decorator      |
| "distintos algoritmos intercambiables"           | Strategy       |
| "integrar librería/sistema externo"              | Adapter        |
| "pasos fijos, implementación variable"           | Template Method|
| "operaciones reversibles / historial"            | Command        |
| "objeto complejo con muchos parámetros"          | Builder        |
| "comportamiento cambia según estado"             | State          |
| "controlar acceso a un objeto"                   | Proxy          |
| "simplificar subsistema complejo"                | Facade         |
| "abstraer acceso a datos"                        | Repository     |
| "múltiples hilos productores/consumidores"       | Prod-Consumidor|

---

## ESTRUCTURA DEL PROYECTO (.NET Framework 4.7.2)

```
RepoEstudio/
├── DOMAIN/
│   └── Entities.cs          ← EntityBase, Producto, Pedido, Usuario, TrabajoPrint
├── DAL/
│   └── Repository.cs        ← IRepository<T>, repos concretos
├── BLL/
│   ├── Patrones.cs          ← Singleton, Factory, Observer, Decorator, Strategy
│   └── PatronesExtra.cs     ← Adapter, Template, Command, Builder, State
├── SERVICES/
│   └── ConcurrenciaService.cs ← Productor-Consumidor, Proxy, Facade
└── UI/
    └── Program.cs           ← Menú interactivo con demo de todos los patrones
```

---

## NOTAS PARA EL EXAMEN

1. **Siempre identificar el patrón pedido** antes de empezar a codear.
2. En concurrencia: `lock` + `Monitor.Pulse/Wait` = productor-consumidor.
3. El Singleton del parcial es la `Impresora` — constructor privado obligatorio.
4. Los `Thread` se crean con `new Thread(metodo)` y se arrancan con `.Start()`.
5. `.Join()` espera que el hilo termine antes de continuar.
6. `IsBackground = true` en el hilo consumidor hace que termine cuando termine el programa principal.
