// Agendar cita: al elegir médico y día, pide al servidor (sin recargar)
// las horas libres a GET /Citas/HorariosDisponibles y las pinta como
// botones. El resumen lateral se va llenando con cada elección.
(function () {
    const form = document.getElementById("form-agendar");
    if (!form) return;

    const urlHorarios = form.dataset.urlHorarios || "/Citas/HorariosDisponibles";
    const contenedor = document.getElementById("horarios-disponibles");
    const inputHora = document.getElementById("input-hora");
    const motivo = document.getElementById("input-motivo");
    const btnAgendar = document.getElementById("btn-agendar");
    const falta = document.getElementById("resumen-falta");

    // Si el servidor devolvió el formulario con error (por ejemplo, la hora
    // se ocupó), intentamos dejar marcada la hora que la persona había elegido.
    let horaPrevia = inputHora.value;
    let peticionActual = 0;

    const medicoElegido = () => form.querySelector('input[name="Form.MedicoId"]:checked');
    const fechaElegida = () => form.querySelector('input[name="Form.Fecha"]:checked');

    function pista(texto) {
        contenedor.innerHTML = "";
        const p = document.createElement("p");
        p.className = "pista";
        p.innerHTML = '<svg class="icono" aria-hidden="true"><use href="#i-info" /></svg>';
        p.appendChild(document.createTextNode(texto));
        contenedor.appendChild(p);
    }

    function fantasmas() {
        contenedor.innerHTML = '<div class="horarios" aria-label="Cargando horas">' +
            '<span class="horario-fantasma"></span>'.repeat(6) + "</div>";
    }

    async function cargarHorarios() {
        const medico = medicoElegido();
        const fecha = fechaElegida();

        inputHora.value = "";
        actualizarResumen();

        if (!medico || !fecha) {
            pista("Elige médico y día para ver las horas libres.");
            return;
        }

        const idPeticion = ++peticionActual;
        fantasmas();

        try {
            const url = `${urlHorarios}?medicoId=${encodeURIComponent(medico.value)}&fecha=${encodeURIComponent(fecha.value)}`;
            const respuesta = await fetch(url, { headers: { Accept: "application/json" } });
            if (!respuesta.ok) throw new Error("HTTP " + respuesta.status);
            const horarios = await respuesta.json();

            // Si la persona cambió de médico o día mientras esto cargaba,
            // se descarta esta respuesta vieja.
            if (idPeticion !== peticionActual) return;

            if (!horarios.length) {
                pista("No quedan horas libres ese día. Prueba con otro día.");
                return;
            }
            pintarHorarios(horarios);
        } catch (error) {
            if (idPeticion !== peticionActual) return;
            pista("No se pudieron cargar las horas. Revisa tu conexión e intenta de nuevo.");
        }
    }

    function pintarHorarios(horarios) {
        contenedor.innerHTML = "";
        const grupos = [
            { titulo: "Mañana", horas: horarios.filter((h) => parseInt(h, 10) < 12) },
            { titulo: "Tarde", horas: horarios.filter((h) => parseInt(h, 10) >= 12) },
        ];

        grupos.filter((g) => g.horas.length).forEach((grupo) => {
            const bloque = document.createElement("div");
            bloque.className = "horarios-grupo";
            const titulo = document.createElement("h3");
            titulo.textContent = grupo.titulo;
            const fila = document.createElement("div");
            fila.className = "horarios";

            grupo.horas.forEach((hora, i) => {
                const boton = document.createElement("button");
                boton.type = "button";
                boton.className = "horario";
                boton.textContent = hora;
                boton.style.animationDelay = `${i * 35}ms`;
                boton.setAttribute("aria-pressed", "false");
                boton.addEventListener("click", () => elegirHora(boton, hora));
                fila.appendChild(boton);
                if (hora === horaPrevia) elegirHora(boton, hora);
            });

            bloque.append(titulo, fila);
            contenedor.appendChild(bloque);
        });
        horaPrevia = "";
    }

    function elegirHora(boton, hora) {
        contenedor.querySelectorAll(".horario").forEach((b) => {
            b.classList.remove("elegido");
            b.setAttribute("aria-pressed", "false");
        });
        boton.classList.add("elegido");
        boton.setAttribute("aria-pressed", "true");
        inputHora.value = hora;
        actualizarResumen();
    }

    function ponerDato(clave, texto) {
        const fila = form.querySelector(`[data-resumen="${clave}"]`);
        const dd = fila.querySelector("dd");
        dd.textContent = texto || "Sin elegir";
        dd.classList.toggle("vacio-dato", !texto);
        fila.classList.toggle("completo", !!texto);
    }

    function marcarBloque(clave, listo) {
        const bloque = form.querySelector(`[data-bloque="${clave}"]`);
        if (bloque) bloque.classList.toggle("listo", listo);
    }

    function actualizarResumen() {
        const medico = medicoElegido();
        const fecha = fechaElegida();
        const hora = inputHora.value;
        const tieneMotivo = motivo.value.trim().length > 0;

        ponerDato("medico", medico ? `${medico.dataset.nombre}, ${medico.dataset.especialidad}` : "");
        ponerDato("fecha", fecha ? fecha.dataset.texto : "");
        ponerDato("hora", hora);

        marcarBloque("medico", !!medico);
        marcarBloque("fecha", !!fecha);
        marcarBloque("hora", !!hora);
        marcarBloque("motivo", tieneMotivo);

        const faltantes = [];
        if (!medico) faltantes.push("el médico");
        if (!fecha) faltantes.push("el día");
        if (!hora) faltantes.push("la hora");
        if (!tieneMotivo) faltantes.push("el motivo");

        btnAgendar.disabled = faltantes.length > 0;
        falta.textContent = faltantes.length
            ? `Falta ${faltantes.length > 1 ? faltantes.slice(0, -1).join(", ") + " y " + faltantes[faltantes.length - 1] : faltantes[0]}.`
            : "Todo listo. Revisa y confirma.";
    }

    form.addEventListener("change", (e) => {
        if (e.target.name === "Form.MedicoId" || e.target.name === "Form.Fecha") {
            horaPrevia = "";
            cargarHorarios();
        }
    });
    motivo.addEventListener("input", actualizarResumen);

    // Si al cargar la página ya hay un día marcado, que se vea en la tira.
    const fechaInicial = fechaElegida();
    if (fechaInicial) {
        const tira = form.querySelector(".dias");
        const dia = fechaInicial.closest(".dia");
        tira.scrollLeft = Math.max(0, dia.offsetLeft - tira.offsetLeft - 8);
    }

    if (medicoElegido() && fechaElegida()) {
        cargarHorarios();
    } else {
        actualizarResumen();
    }
})();
