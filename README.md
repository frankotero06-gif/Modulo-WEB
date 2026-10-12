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
cd PortalCitas
dotnet run
```

Entra con cualquier correo/clave no vacíos (ej. `paciente@correo.com` /
`123456`). Ya viene con una cita de ejemplo cargada para que veas la lista
funcionando desde el primer momento.

## Qué incluye

- **Login** (`Cuenta/Login`) — misma idea que el portal HTML/JS anterior,
  ahora en Razor.
- **Mis citas** (`Citas/Index`) — lista las citas del paciente con estado
  (Confirmada / Pendiente / Cancelada) y permite cancelar.
- **Agendar cita** (`Citas/Agendar`) — lo más importante de esta entrega:
  1. El paciente elige un médico.
  2. Elige una fecha.
  3. El formulario llama, **sin recargar la página**, al endpoint
     `GET /Citas/HorariosDisponibles?medicoId=&fecha=` (ver
     `wwwroot/js/agendar.js`) y pinta los horarios libres como botones.
  4. Al elegir un horario y escribir el motivo, confirma la cita.
  5. El servicio mock valida que el horario siga libre (evita choques si
     dos personas agendan a la vez) antes de guardar.

## Estructura

```
PortalCitas/
├── Controllers/
│   ├── CuentaController.cs         → login/logout
│   ├── CitasController.cs          → listar, agendar, cancelar, horarios (AJAX)
│   └── PacienteControllerBase.cs   → exige sesión iniciada
├── Models/
│   ├── LoginViewModel.cs / ResultadoLogin.cs
│   ├── MedicoDto.cs / CitaDto.cs
│   ├── NuevaCitaInput.cs (+ AgendarCitaViewModel)
│   └── ResultadoAgendar.cs
├── Services/
│   ├── IAutenticacionService.cs (+ Mock / Integracion)
│   └── IAgendaService.cs        (+ Mock / Integracion)  ← lo nuevo
├── Views/
│   ├── Shared/_LayoutPaciente.cshtml
│   ├── Cuenta/Login.cshtml
│   └── Citas/Index.cshtml, Agendar.cshtml
├── wwwroot/css/portal.css
├── wwwroot/js/agendar.js
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

Reglas: la hora es `HH:mm`; la fecha, `yyyy-MM-dd`; todo el JSON va en camelCase
y las fechas en ISO 8601. Las citas nuevas nacen *Confirmada*; el motivo es
obligatorio (máx. 300 caracteres). Con `formaPago = "Tarjeta"` la referencia
de la pasarela es obligatoria (máx. 100 caracteres) y el CORE nunca recibe el
número de tarjeta. Si la factura es subsidiada, la forma de pago se ignora.

### Pendiente de confirmar

- **Login del paciente**: el CORE no lo tiene. Acordado a alto nivel: el
  Gateway (Ricardo) valida correo y contraseña y devuelve el `pacienteId`.
  Falta su contrato real; `AutenticacionIntegracionService` asume
  `POST auth/login` con `{ correo, clave }` y respuesta
  `{ pacienteId, nombrePaciente }`.
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
- Validación de que la fecha elegida no sea un fin de semana o feriado,
  si aplica a las clínicas de CONAVIHSIDA.
- Cuando este módulo crezca hacia el pago (`RF-WEB-03` de tu SRS más
  reciente), se puede seguir exactamente el mismo patrón: una interfaz
  `IPagoService` con su mock y su implementación real.
