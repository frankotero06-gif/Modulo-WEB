# CONAVIHSIDA - Portal del Paciente: Agendar Citas (ASP.NET Core MVC)

Base para el módulo Web, centrada en lo que el grupo decidió priorizar:
**agendar citas**. Sigue el mismo patrón de arquitectura que ya usamos en
el login de Caja (Beckham): una interfaz de servicio única, con una
implementación mock activa hoy y una implementación real lista para
conectar a Integración (Ricardo) el día que tenga los endpoints.

No pude abrir el link de Figma que compartiste (Figma bloquea el acceso
automático a sus diseños), así que el diseño de estas pantallas reutiliza
la misma paleta institucional (`#002f6c` navy, `#ce1126` rojo) que ya
usamos en el portal de paciente en HTML/JS y en el login de Caja en C#,
para que las tres piezas se vean como un solo sistema. Si tienes capturas
o un export del Figma, lo ajusto para que calce exacto.

## Cómo correrlo

Necesitas el SDK de .NET 8 (`dotnet --version` debe mostrar 8.x). No pude
compilar esto en este entorno (no hay SDK de .NET disponible), así que
corre `dotnet build` apenas lo abras para atrapar cualquier detalle.

```bash
# desde la raíz del repositorio
dotnet run
```

Al abrir el sitio verás la portada pública. Dale a **Iniciar sesión** y
entra con la cuenta de prueba `paciente@correo.com` / `123456` (ya trae una
cita, facturas y resultados cargados) o crea una cuenta nueva desde
**"Crea tu cuenta"** en la pantalla de inicio de sesión.

## Qué incluye

- **Iniciar sesión** (`Cuenta/Login`) y **Crear cuenta** (`Cuenta/Registro`).
- **Inicio** (`Inicio/Index`) — panel de bienvenida: la próxima cita como
  "boleto" con cuenta regresiva, lo que hay por pagar y cuántos análisis
  están listos.
- **Mis citas** (`Citas/Index`) — pestañas Próximas / Historial y
  cancelación con diálogo de confirmación.
- **Agendar cita** (`Citas/Agendar`) — lo más importante de esta entrega:
  1. El paciente elige un médico (tarjetas con iniciales).
  2. Elige un día en una tira con los próximos 15 días hábiles.
  3. El formulario llama, **sin recargar la página**, al endpoint
     `GET /Citas/HorariosDisponibles?medicoId=&fecha=` (ver
     `wwwroot/js/agendar.js`) y pinta las horas libres agrupadas en
     Mañana / Tarde.
  4. Escribe el motivo (con contador de 300 caracteres).
  5. Un resumen lateral se llena en vivo y el botón "Confirmar cita" se
     activa cuando no falta nada.
  6. El servicio mock valida que el horario siga libre (evita choques si
     dos personas agendan a la vez) antes de guardar.

## Lo que pidió el profesor para la WEB

1. **Portal para que el cliente acceda a los servicios**: citas, resultados,
   facturas y pagos.
2. **Acceso por usuario**: el paciente crea su cuenta e inicia sesión (ver
   "Registro de pacientes").
3. **Constancia de todo lo que el usuario hace** y **recibo de toda
   transacción**, que el usuario pueda guardar o descargar (ver
   "Constancias").

## Página principal pública (idea del equipo de Core)

Al abrir el sitio (`/`) ya no aparece directo el login. Primero sale una
portada (`Principal/Index`) con información de la institución: quiénes
somos, misión, visión y valores (textos oficiales tomados del Informe CAF
CONAVIHSIDA 2024), qué hace CONAVIHSIDA, qué se puede hacer en el portal,
el aviso de confidencialidad y un enlace al sitio oficial. Arriba a la
derecha están **Crear cuenta** e **Iniciar sesión**; si ya hay sesión,
**Ir a mi portal**.

- **Foto opcional:** si colocan una foto en `wwwroot/img/edificio.jpg`, la
  portada la muestra sola. Si no existe, se usa el lazo rojo.
- En login y registro, el logo y el enlace "Volver a la página principal"
  llevan de vuelta a la portada.

## Constancias (nuevo)

Cada acción del paciente en la web genera una **constancia** con número
(`CON-AAAAMMDD-0001`), fecha, datos del paciente y detalle de la acción:

| Acción | Qué pasa después |
|---|---|
| Crear cuenta | Se muestra la constancia de registro (la cédula sale parcialmente oculta) |
| Agendar cita | Se muestra la constancia de la cita agendada |
| Cancelar cita | Se muestra la constancia de la cancelación |
| Pagar factura | Se muestra el recibo y factura (RN-05) y además queda una constancia de pago |

- Cada constancia se puede **descargar en PDF** (botón "Descargar PDF") o
  imprimir.
- **Mis constancias** (`Constancias/Index`, en el menú) muestra todo el
  historial, con descarga en PDF de cada una.
- El **recibo y factura** de un pago (`Facturas/Comprobante`) también se
  descarga en PDF real (`Facturas/ComprobantePdf/{id}`).
- Las constancias **no llevan información clínica** (RN-01).

Cómo está hecho:

- `Services/GeneradorPdf.cs` arma el PDF **sin librerías ni paquetes
  NuGet**. Escribe el formato PDF directamente con las fuentes estándar
  Helvetica.
- `Services/FabricaConstancias.cs` define el contenido de cada tipo de
  constancia.
- `IConstanciaService` sigue el mismo patrón mock/real que los demás
  servicios: `UsarMockConstancias` en `appsettings.json`.
- El mock (`ConstanciaMockService`) guarda las constancias en memoria y trae
  ejemplos para el paciente de prueba.
- Contrato asumido para Integración/Core (**pendiente de confirmar**). Lo
  lógico es que el CORE las guarde, porque la web no tiene base de datos
  (RNF-03):
  - `POST pacientes/{pacienteId}/constancias` → 201 con `id`, `numero`,
    `fechaEmision`
  - `GET pacientes/{pacienteId}/constancias`
  - `GET pacientes/{pacienteId}/constancias/{id}`
- Si guardar la constancia falla, la acción del paciente (cita, pago) ya
  quedó hecha y el portal sigue normal, solo que sin mostrar la constancia.

## Registro de pacientes (nuevo)

Antes solo existía el login; ahora el paciente puede crear su cuenta.

- **Formulario en 3 pasos**: Tus datos (nombre, apellidos, cédula, fecha de
  nacimiento) → Contacto (teléfono, correo) → Contraseña (con medidor de
  seguridad y consentimiento de la Ley 172-13).
- **No se pide ningún dato clínico** (diagnóstico, código serológico,
  tratamiento). Según RN-01 y RF-CORE-02 eso lo registra el personal médico
  en la aplicación de empleados del Core.
- **Validaciones** (`Models/RegistroViewModel.cs`): cédula de 11 dígitos
  (con o sin guiones), teléfono dominicano 809/829/849, correo válido,
  contraseña de 8 a 64 caracteres con letras y números, confirmación igual,
  fecha de nacimiento en el pasado y aceptación del consentimiento.
  El navegador valida cada paso con los mismos mensajes (los lee de los
  atributos `data-val-*` que genera ASP.NET) y el servidor vuelve a validar
  todo.
- La cédula y el teléfono se formatean solos mientras se escriben
  (`001-1234567-8`, `809-555-1234`).
- Al crear la cuenta se inicia sesión de inmediato y se llega a Inicio.
- **Mock** (`AutenticacionMockService`): guarda las cuentas en memoria
  (Singleton) con la contraseña en **hash PBKDF2-SHA256 con sal**, como pide
  RF-CORE-01. Rechaza correos o cédulas repetidos. El login ahora sí valida
  la contraseña (ya no entra "cualquier correo/clave").
- **Integración** (`AutenticacionIntegracionService`): asume
  `POST auth/registro` con
  `{ nombres, apellidos, cedula, fechaNacimiento, telefono, correo, clave }`
  → 201 `{ pacienteId, nombrePaciente }` o 409 `{ error }`. Hay que
  confirmarlo con Ricardo.

## Diseño

- Identidad: azul institucional `#002f6c` y rojo del lazo `#ce1126`. El rojo
  se reserva para la marca y para acciones destructivas (cancelar cita), no
  para botones normales.
- Tipografía: Bricolage Grotesque (títulos) y Figtree (texto), cargadas de
  Google Fonts. Sin internet, la página cae a Segoe UI / la fuente del
  sistema y se sigue viendo bien.
- Pantallas de acceso a dos columnas, con el lazo que se dibuja al cargar.
- Íconos SVG propios en `Views/Shared/_Iconos.cshtml` (sin librerías).
- Avisos tipo *toast* para los mensajes de éxito, diálogo propio para
  confirmar cancelaciones, vista previa de la tarjeta que se llena (y se
  voltea al escribir el código de seguridad) en Pagar, comprobante con forma
  de recibo.
- Responsivo (menú hamburguesa en móvil), foco visible con teclado y
  respeta "reducir movimiento" del sistema operativo.
- Fechas y montos en español dominicano (`es-DO`) gracias a
  `UseRequestLocalization` en `Program.cs`.
- Todo el JavaScript (`wwwroot/js/portal.js`, `agendar.js`) es mejora
  progresiva: sin JS, los formularios siguen funcionando contra el servidor.

## Estructura

```
PortalCitas/
├── Controllers/
│   ├── PrincipalController.cs      → portada pública (quiénes somos)
│   ├── CuentaController.cs         → login, registro, logout
│   ├── InicioController.cs         → panel de bienvenida
│   ├── ConstanciasController.cs    → mis constancias, ver, PDF
│   ├── CitasController.cs          → listar, agendar, cancelar, horarios (AJAX)
│   ├── ResultadosController.cs / FacturasController.cs
│   └── PacienteControllerBase.cs   → exige sesión iniciada
├── Models/
│   ├── LoginViewModel.cs / ResultadoLogin.cs
│   ├── RegistroViewModel.cs / ResultadoRegistro.cs   ← nuevo
│   ├── InicioViewModel.cs                            ← nuevo
│   ├── ConstanciaDto.cs / DocumentoPdf.cs            ← nuevo
│   ├── MedicoDto.cs / CitaDto.cs / NuevaCitaInput.cs / ResultadoAgendar.cs
│   └── FacturaDto.cs / ResultadoLabDto.cs / ...
├── Services/
│   ├── IAutenticacionService.cs (+ Mock / Integracion)  ← ahora con registro
│   ├── IAgendaService.cs, IPagoService.cs, IResultadosService.cs (+ Mock / Integracion)
│   ├── IConstanciaService.cs (+ Mock / Integracion)    ← nuevo
│   ├── GeneradorPdf.cs / FabricaConstancias.cs         ← nuevo (PDF sin librerías)
│   └── IPasarelaService.cs (+ Mock)
├── Views/
│   ├── Shared/_LayoutPaciente.cshtml, _LayoutAcceso.cshtml, _LayoutPublico.cshtml, _Iconos.cshtml
│   ├── Principal/Index.cshtml
│   ├── Constancias/Index.cshtml, Ver.cshtml
│   ├── Cuenta/Login.cshtml, Registro.cshtml
│   ├── Inicio/Index.cshtml
│   ├── Citas/Index.cshtml, Agendar.cshtml
│   ├── Resultados/Index.cshtml
│   └── Facturas/Index.cshtml, Pagar.cshtml, Comprobante.cshtml
├── wwwroot/css/portal.css
├── wwwroot/js/portal.js, agendar.js
└── Program.cs / appsettings.json
```

## Pantalla nueva: Mis Facturas / Pagar (RF-WEB-02)

Cubre lo que le faltaba a la web según tu SRS: *"pago mediante los
mecanismos habilitados por el sistema y visualización/descarga del
comprobante correspondiente."*

- **Mis facturas** (`Facturas/Index`) — lista las facturas del paciente.
  Incluye una ya subsidiada al 100% (RN-02, RD$ 0.00, ya "Pagada") y una
  pendiente de pago (consulta especializada, RD$ 250.00) — el mismo
  escenario que ya viste del lado de Caja.
- **Pagar** (`Facturas/Pagar`) — formulario de pago simulado (tarjeta).
  No procesa ningún pago real, es solo para la demo.
- **Comprobante** (`Facturas/Comprobante`) — pantalla imprimible con
  **número de recibo y número de factura generados juntos**, tal como
  pide RN-05 ("ninguna transacción se considera completada sin
  comprobante"). El botón "Imprimir / Descargar" usa `window.print()`,
  que en cualquier navegador permite guardar como PDF.

## Pantalla nueva: Mis Resultados (respetando RN-01)

Esta pantalla ya existía en tu primer prototipo en HTML/JS, pero mostraba
directamente cosas como "Carga viral" y "Conteo CD4" con sus valores.
Al pasarla a C#, noté que eso contradice tu propia **RN-01**: esos datos
clínicos solo los puede leer personal médico acreditado.

Por eso `ResultadoLabDto` **no tiene ningún campo de valor clínico** —
a propósito, para que ni por error el equipo termine exponiendo esa
información desde la Web. El paciente solo ve:

- Qué prueba se le hizo y cuándo.
- Si ya está **Lista** o sigue **Pendiente**.
- Una nota genérica ("revísalo con tu médico"), nunca el resultado.

Los valores reales viven en la aplicación de empleados del Core, para
que los vea el médico. Dejé un comentario en
`ResultadosIntegracionService.cs` recordándole a Ricardo/Emilia que el
endpoint tampoco debería devolver esos valores en el JSON, aunque el
frontend no los pinte — la confidencialidad se protege en el backend,
no ocultando cosas en la pantalla.

## Contrato acordado con el CORE (Emilia)

La web llama solo al Gateway, desde el servidor (nunca desde el navegador).
La URL base va en `IntegracionApiBaseUrl` e incluye el `/api`; las rutas de
abajo se anexan a esa base. En cada llamada `GatewayHeadersHandler` agrega
`X-Paciente-Id` (sin él el CORE responde 401; si no coincide con el paciente
de la ruta o del cuerpo, 403).

| Acción | Ruta | Respuesta |
|---|---|---|
| Médicos | `GET medicos` | `[{ id, nombreCompleto, especialidad, activo }]` (la web solo muestra los activos) |
| Horarios | `GET medicos/{id}/horarios?fecha=yyyy-MM-dd` | solo 08:00, 09:00, 10:00, 11:00, 14:00, 15:00, 16:00 |
| Mis citas | `GET pacientes/{pacienteId}/citas` | lista de citas |
| Agendar | `POST citas` (`pacienteId, medicoId, fecha, hora, motivo`) | 201 con la cita, o 409 `{ error }` si el horario ya se ocupó |
| Cancelar | `POST citas/{id}/cancelar?pacienteId={pacienteId}` | 200 sin cuerpo, o 404 si la cita no es del paciente |
| Facturas | `GET pacientes/{pacienteId}/facturas` y `.../facturas/{facturaId}` | factura(s) |
| Pagar | `POST pacientes/{pacienteId}/facturas/{facturaId}/pagar` (`formaPago`, `referenciaPasarela`) | 200 con la factura pagada (`numeroRecibo`, `numeroFactura`, `fechaPago`), o 409 `{ error }` |
| Resultados | `GET pacientes/{pacienteId}/resultados` | `id, pacienteId, prueba, fechaSolicitud, estado, nota` |
| Constancias *(propuesto)* | `POST/GET pacientes/{pacienteId}/constancias`, `GET .../constancias/{id}` | constancia(s) |

Reglas: la hora es `HH:mm`; la fecha, `yyyy-MM-dd`; todo el JSON va en camelCase
y las fechas en ISO 8601. Las citas nuevas nacen *Confirmada*; el motivo es
obligatorio (máx. 300 caracteres). Con `formaPago = "Tarjeta"` la referencia
de la pasarela es obligatoria (máx. 100 caracteres) y el CORE nunca recibe el
número de tarjeta. Si la factura es subsidiada, la forma de pago se ignora.

### Pendiente de confirmar

- **Login y registro del paciente**: el CORE no los tiene. Acordado a alto
  nivel: el Gateway (Ricardo) valida correo y contraseña y devuelve el
  `pacienteId`. Falta su contrato real; `AutenticacionIntegracionService`
  asume `POST auth/login` con `{ correo, clave }` y `POST auth/registro`
  (ver arriba), ambos con respuesta `{ pacienteId, nombrePaciente }`.
- **Pasarela de pago**: acordado que la simule el Gateway. Mientras tanto
  `PasarelaMockService` la simula dentro de la web.
- **Campos exactos de la cita y de la factura** (qué nombres y tipos trae
  cada JSON). Los DTOs de la web son suposiciones hasta ver un ejemplo real.

## Cómo conectar la API real de Ricardo

Todo vive en `Services/`, igual que en los otros dos proyectos:

1. En `appsettings.json`, cambia `"UsarMockAgenda"` a `false` (y
   `"UsarMockAutenticacion"` cuando el login real esté listo).
2. Pon la URL real en `"IntegracionApiBaseUrl"`.
3. Revisa `AgendaIntegracionService.cs`: ajusta las rutas (`/medicos`,
   `/pacientes/{id}/citas`, etc.) y los nombres de campos según lo que
   Ricardo confirme.

El Controller y las Vistas no cambian — solo conocen `IAgendaService`.

## Nota sobre el SRS que compartiste

Reviso rápido tu `SRS_CONAVIHSIDA.docx`: incorporaste muy bien las
anotaciones de CHECK vs. triggers y el control de efectivo/comprobantes
en Caja (RN-04, RN-05, RF-CAJ-02/03/07 quedaron sólidos). Una cosa que
noté: el módulo WEB todavía incluye `RF-WEB-03` (Portal Médico),
`RF-WEB-04` (Portal Farmacia) y `RF-WEB-05` (Dashboard Ejecutivo) — esos
tres, según lo que el profesor dijo explícitamente en el audio ("la
aplicación web es para los clientes de la empresa"), deberían vivir en
la aplicación de empleados del Core, no en Web. No lo cambié yo en tu
documento porque no me lo pediste esta vez, pero vale la pena resolverlo
con el equipo antes de entregarlo — dime si quieres que te arme esa
corrección también.

## Pendientes reales

- Mostrar recordatorio o confirmación por correo al agendar (fuera de
  alcance por ahora, pero fácil de añadir en `AgendaMockService`).
- La tira de días de Agendar solo ofrece lunes a viernes, pero el servidor
  todavía no rechaza fines de semana ni feriados. Si las clínicas tienen
  otro horario, ajustarlo en `Views/Citas/Agendar.cshtml` y validarlo en
  el servicio.
- Recuperar contraseña ("olvidé mi contraseña") — depende de que el
  Gateway pueda enviar correos.
- Cuando este módulo crezca hacia el pago (`RF-WEB-03` de tu SRS más
  reciente), se puede seguir exactamente el mismo patrón: una interfaz
  `IPagoService` con su mock y su implementación real.
