// Comportamiento compartido del portal. Todo es mejora progresiva: si el
// JavaScript no carga, los formularios igual funcionan contra el servidor.
(function () {
    "use strict";

    const $ = (sel, raiz = document) => raiz.querySelector(sel);
    const $$ = (sel, raiz = document) => Array.from(raiz.querySelectorAll(sel));

    /* ---------------- Menú en móvil ---------------- */
    const barra = $("[data-barra]");
    const botonMenu = $("[data-menu]");
    if (barra && botonMenu) {
        botonMenu.addEventListener("click", () => {
            const abierto = barra.classList.toggle("abierta");
            botonMenu.setAttribute("aria-expanded", String(abierto));
        });
        document.addEventListener("keydown", (e) => {
            if (e.key === "Escape" && barra.classList.contains("abierta")) {
                barra.classList.remove("abierta");
                botonMenu.setAttribute("aria-expanded", "false");
                botonMenu.focus();
            }
        });
    }

    /* ---------------- Aviso (toast) ---------------- */
    const aviso = $("[data-aviso]");
    if (aviso) {
        const cerrar = () => {
            aviso.classList.add("saliendo");
            setTimeout(() => aviso.remove(), 320);
        };
        let temporizador = setTimeout(cerrar, 6500);
        aviso.addEventListener("mouseenter", () => clearTimeout(temporizador));
        aviso.addEventListener("mouseleave", () => { temporizador = setTimeout(cerrar, 3000); });
        $("[data-aviso-cerrar]", aviso)?.addEventListener("click", cerrar);
    }

    /* ---------------- Confirmación con diálogo ---------------- */
    // <form data-confirmar="Título" data-confirmar-texto="..." data-confirmar-boton="Sí, ...">
    const dialogo = $("#dialogo-confirmar");
    $$("form[data-confirmar]").forEach((form) => {
        form.addEventListener("submit", (e) => {
            if (form.dataset.confirmado === "1") return;
            e.preventDefault();

            const titulo = form.dataset.confirmar;
            const texto = form.dataset.confirmarTexto || "";

            if (!dialogo || typeof dialogo.showModal !== "function") {
                if (window.confirm(`${titulo}\n\n${texto}`)) enviarConfirmado(form);
                return;
            }

            $("[data-dialogo-titulo]", dialogo).textContent = titulo;
            $("[data-dialogo-texto]", dialogo).textContent = texto;
            $("[data-dialogo-aceptar]", dialogo).textContent = form.dataset.confirmarBoton || "Sí, continuar";
            dialogo.returnValue = "";
            dialogo.showModal();
            dialogo.addEventListener("close", function alCerrar() {
                dialogo.removeEventListener("close", alCerrar);
                if (dialogo.returnValue === "si") enviarConfirmado(form);
            });
        });
    });

    function enviarConfirmado(form) {
        form.dataset.confirmado = "1";
        form.submit();
    }

    /* ---------------- Botón "enviando…" ---------------- */
    // Evita doble envío y muestra que algo está pasando.
    document.addEventListener("submit", (e) => {
        const form = e.target;
        if (e.defaultPrevented) return;
        const boton = $("[data-enviando]", form);
        if (!boton) return;
        // Se deja un instante para que el navegador tome el valor del botón.
        setTimeout(() => {
            boton.classList.add("enviando");
            boton.dataset.textoOriginal = boton.innerHTML;
            boton.textContent = boton.dataset.enviando;
        }, 0);
    });
    // Si el usuario vuelve con el botón "atrás", restaurar el botón.
    window.addEventListener("pageshow", () => {
        $$("[data-enviando].enviando").forEach((b) => {
            b.classList.remove("enviando");
            if (b.dataset.textoOriginal) b.innerHTML = b.dataset.textoOriginal;
        });
    });

    /* ---------------- Mostrar / ocultar contraseña ---------------- */
    $$("[data-ver-clave]").forEach((boton) => {
        const input = boton.parentElement.querySelector("input");
        const uso = boton.querySelector("use");
        const etiqueta = boton.querySelector(".sr");
        boton.addEventListener("click", () => {
            const mostrar = input.type === "password";
            input.type = mostrar ? "text" : "password";
            uso.setAttribute("href", mostrar ? "#i-ojo-off" : "#i-ojo");
            etiqueta.textContent = mostrar ? "Ocultar contraseña" : "Mostrar contraseña";
            boton.setAttribute("aria-pressed", String(mostrar));
            input.focus();
        });
    });

    /* ---------------- Máscaras de entrada ---------------- */
    const soloDigitos = (v) => v.replace(/\D/g, "");
    const mascaras = {
        // 001-1234567-8
        cedula(v) {
            const d = soloDigitos(v).slice(0, 11);
            if (d.length <= 3) return d;
            if (d.length <= 10) return `${d.slice(0, 3)}-${d.slice(3)}`;
            return `${d.slice(0, 3)}-${d.slice(3, 10)}-${d.slice(10)}`;
        },
        // 809-555-1234
        telefono(v) {
            const d = soloDigitos(v).slice(0, 10);
            if (d.length <= 3) return d;
            if (d.length <= 6) return `${d.slice(0, 3)}-${d.slice(3)}`;
            return `${d.slice(0, 3)}-${d.slice(3, 6)}-${d.slice(6)}`;
        },
        // 1234 5678 9012 3456
        tarjeta(v) {
            return soloDigitos(v).slice(0, 16).replace(/(\d{4})(?=\d)/g, "$1 ");
        },
        // MM/AA
        vencimiento(v) {
            const d = soloDigitos(v).slice(0, 4);
            return d.length > 2 ? `${d.slice(0, 2)}/${d.slice(2)}` : d;
        },
        digitos(v) {
            return soloDigitos(v);
        },
    };

    $$("[data-mascara]").forEach((input) => {
        const aplicar = mascaras[input.dataset.mascara];
        if (!aplicar) return;
        input.addEventListener("input", (e) => {
            // Al borrar no se reformatea, para no "pelear" con el cursor.
            if (e.inputType && e.inputType.startsWith("delete")) return;
            const alFinal = input.selectionStart === input.value.length;
            input.value = aplicar(input.value);
            if (alFinal) input.setSelectionRange(input.value.length, input.value.length);
        });
        if (input.value) input.value = aplicar(input.value);
    });

    /* ---------------- Contador de caracteres ---------------- */
    $$("[data-contador]").forEach((contador) => {
        const campo = document.getElementById(contador.dataset.contador);
        if (!campo) return;
        const max = parseInt(campo.getAttribute("maxlength"), 10) || 0;
        const actualizar = () => {
            contador.textContent = `${campo.value.length} / ${max}`;
            contador.classList.toggle("cerca", max && campo.value.length > max * 0.85);
        };
        campo.addEventListener("input", actualizar);
        actualizar();
    });

    /* ---------------- Pestañas ---------------- */
    $$("[data-pestanas]").forEach((lista) => {
        const pestanas = $$('[role="tab"]', lista);
        const elegir = (pestana) => {
            pestanas.forEach((p) => {
                const activa = p === pestana;
                p.setAttribute("aria-selected", String(activa));
                p.tabIndex = activa ? 0 : -1;
                document.getElementById(p.getAttribute("aria-controls")).hidden = !activa;
            });
        };
        pestanas.forEach((p, i) => {
            p.addEventListener("click", () => elegir(p));
            p.addEventListener("keydown", (e) => {
                if (e.key !== "ArrowRight" && e.key !== "ArrowLeft") return;
                const siguiente = pestanas[(i + (e.key === "ArrowRight" ? 1 : -1) + pestanas.length) % pestanas.length];
                elegir(siguiente);
                siguiente.focus();
            });
        });
        elegir(pestanas.find((p) => p.getAttribute("aria-selected") === "true") || pestanas[0]);
    });

    /* ---------------- Validación en el navegador ---------------- */
    // Lee los mismos atributos data-val-* que ASP.NET genera a partir de las
    // DataAnnotations del modelo, así los mensajes son idénticos a los del
    // servidor. El servidor siempre vuelve a validar.
    function errorDe(input, form) {
        const d = input.dataset;
        if (d.val !== "true") return "";
        const valor = input.type === "checkbox" ? input.checked : input.value.trim();

        if (input.type === "checkbox") {
            return d.valRange && !valor ? d.valRange : "";
        }
        if (d.valRequired && !valor) return d.valRequired;
        if (!valor) return "";
        if (d.valLength) {
            const min = parseInt(d.valLengthMin || "0", 10);
            const max = parseInt(d.valLengthMax || "100000", 10);
            if (valor.length < min || valor.length > max) return d.valLength;
        }
        if (d.valEmail && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(valor)) return d.valEmail;
        if (d.valRegex && d.valRegexPattern) {
            try {
                if (!new RegExp(`^(?:${d.valRegexPattern})$`).test(valor)) return d.valRegex;
            } catch (_) { /* patrón no compatible con JS: lo valida el servidor */ }
        }
        if (d.valEqualto && d.valEqualtoOther) {
            const otro = d.valEqualtoOther.replace("*.", "");
            const campoOtro = form.querySelector(`[name="${otro}"]`);
            if (campoOtro && campoOtro.value !== input.value) return d.valEqualto;
        }
        return "";
    }

    function mostrarError(input, mensaje) {
        const span = input.form.querySelector(`[data-valmsg-for="${input.name}"]`);
        if (span) {
            span.textContent = mensaje;
            span.classList.toggle("field-validation-error", !!mensaje);
            span.classList.toggle("field-validation-valid", !mensaje);
        }
        input.classList.toggle("input-validation-error", !!mensaje);
        if (mensaje) input.setAttribute("aria-invalid", "true");
        else input.removeAttribute("aria-invalid");
    }

    function validarCampos(campos, form) {
        let primero = null;
        campos.forEach((input) => {
            const mensaje = errorDe(input, form);
            mostrarError(input, mensaje);
            if (mensaje && !primero) primero = input;
        });
        if (primero) primero.focus();
        return !primero;
    }

    const camposValidables = (raiz) =>
        $$("input[data-val='true'], textarea[data-val='true'], select[data-val='true']", raiz)
            .filter((i) => i.type !== "hidden");

    // Al corregir un campo con error, el mensaje se quita al salir de él.
    document.addEventListener("focusout", (e) => {
        const input = e.target;
        if (!input.matches || !input.matches(".input-validation-error")) return;
        mostrarError(input, errorDe(input, input.form));
    });

    /* ---------------- Registro por pasos ---------------- */
    const registro = $("[data-registro]");
    if (registro) {
        const pasos = $$("[data-paso]", registro);
        const marcas = $$("[data-marca]", registro);
        let actual = 0;

        const ir = (indice, haciaAtras = false) => {
            actual = indice;
            pasos.forEach((p, i) => {
                p.classList.toggle("activo", i === indice);
                p.classList.toggle("hacia-atras", i === indice && haciaAtras);
            });
            marcas.forEach((m, i) => {
                m.classList.toggle("actual", i === indice);
                m.classList.toggle("hecho", i < indice);
                if (i === indice) m.setAttribute("aria-current", "step");
                else m.removeAttribute("aria-current");
            });
            const primerCampo = $("input:not([type=hidden])", pasos[indice]);
            if (primerCampo && document.activeElement !== document.body) primerCampo.focus({ preventScroll: true });
        };

        $$("[data-siguiente]", registro).forEach((b) =>
            b.addEventListener("click", () => {
                if (validarCampos(camposValidables(pasos[actual]), registro)) ir(actual + 1);
            }));
        $$("[data-anterior]", registro).forEach((b) =>
            b.addEventListener("click", () => ir(actual - 1, true)));

        // Enter en un paso intermedio avanza en vez de enviar el formulario.
        registro.addEventListener("keydown", (e) => {
            if (e.key !== "Enter" || e.target.tagName !== "INPUT" || e.target.type === "checkbox") return;
            if (actual < pasos.length - 1) {
                e.preventDefault();
                $("[data-siguiente]", pasos[actual])?.click();
            }
        });

        registro.addEventListener("submit", (e) => {
            // Revisa todo; si hay error, abre el paso donde está.
            for (let i = 0; i < pasos.length; i++) {
                if (!validarCampos(camposValidables(pasos[i]), registro)) {
                    e.preventDefault();
                    if (i !== actual) ir(i, i < actual);
                    return;
                }
            }
        });

        // Si el servidor devolvió errores, empezar en el primer paso que los tenga.
        const pasoConError = pasos.findIndex((p) => $(".field-validation-error, .input-validation-error", p));
        const hayResumen = $(".validation-summary-errors", registro);
        ir(pasoConError >= 0 ? pasoConError : (hayResumen ? pasos.length - 1 : 0));
    }

    /* ---------------- Medidor de contraseña ---------------- */
    $$("[data-medidor]").forEach((input) => {
        const medidor = document.getElementById(input.dataset.medidor);
        const texto = $(".medidor-texto", medidor);
        const textoInicial = texto.textContent;
        const etiquetas = ["", "Débil", "Aceptable", "Buena", "Fuerte"];
        input.addEventListener("input", () => {
            const v = input.value;
            let nivel = 0;
            if (v.length > 0) {
                nivel = 1;
                const basica = v.length >= 8 && /[A-Za-zÁÉÍÓÚÑáéíóúñ]/.test(v) && /\d/.test(v);
                if (basica) nivel = 2;
                if (basica && (/[a-z]/.test(v) && /[A-Z]/.test(v) || /[^A-Za-z0-9]/.test(v))) nivel = 3;
                if (basica && v.length >= 12 && /[^A-Za-z0-9]/.test(v)) nivel = 4;
            }
            medidor.dataset.nivel = String(nivel);
            texto.textContent = nivel === 0
                ? textoInicial
                : nivel === 1 ? "Débil: usa al menos 8 caracteres, con letras y números." : etiquetas[nivel];
        });
    });

    /* ---------------- Vista previa de la tarjeta ---------------- */
    const pago = $("[data-pago]");
    const tarjeta = $("[data-tarjeta]");
    if (pago && tarjeta) {
        const vistas = {};
        $$("[data-vista]", tarjeta).forEach((el) => {
            vistas[el.dataset.vista] = { el, inicial: el.textContent };
        });
        const formatos = {
            numero: (v) => {
                const d = soloDigitos(v).padEnd(16, "•").slice(0, 16);
                return d.replace(/(.{4})(?=.)/g, "$1 ");
            },
            titular: (v) => v.trim(),
            vencimiento: (v) => v,
            cvv: (v) => "•".repeat(soloDigitos(v).length) || "",
        };
        $$("[data-hacia]", pago).forEach((input) => {
            const clave = input.dataset.hacia;
            const pintar = () => {
                const vista = vistas[clave];
                if (!vista) return;
                const valor = formatos[clave](input.value);
                vista.el.textContent = valor || vista.inicial;
            };
            input.addEventListener("input", pintar);
            if (clave === "cvv") {
                input.addEventListener("focus", () => tarjeta.classList.add("volteada"));
                input.addEventListener("blur", () => tarjeta.classList.remove("volteada"));
            }
            pintar();
        });
    }
})();
