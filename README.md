# SimpracTest — prototipo 0.1

Simulador interactivo de entrenamiento para el examen práctico del permiso B en España. **Móstoles es la primera zona de desarrollo**, no el nombre del juego.

Prototipo jugable para entrenar el **práctico** del permiso B desde el ordenador: cámara desde el coche, trayectos animados según decisiones, cambios de marcha simplificados, voz del examinador, seis escenarios encadenados, evaluación orientativa, cronómetro y exportación de resultados.

## Vista inmersiva y controles

La interfaz ocupa toda la ventana: carretera y habitáculo simplificado (volante y palanca animados), instrucciones de voz en la parte superior y **preguntas junto a un minimapa en la pantalla central del coche**. Durante el avance las preguntas desaparecen para dejar ver la carretera. El cuadro presenta velocidad, marcha, cronómetro y faltas.

- Selecciona una respuesta pulsando **A, B, C o D**, o haciendo clic.
- Pulsa **⛶ Pantalla completa** para ampliar la vista y el mismo botón para salir (también puedes usar Esc).
- Usa **↻ Volver a DGT** para volver a cargar el punto de salida; **Circuito ficticio** ofrece la demo separada y **↓ Resultados** exporta el registro.
- El botón **Explorar calles reales (experimental)** permite cargar otra ubicación sin cambiar el punto de inicio predeterminado de Móstoles.

### Interior del coche y retrovisores (fase Clio)

El habitáculo se ha sustituido por una **recreación geométrica inspirada en un Renault Clio 2026 de acabado sencillo**, con salpicadero, volante, consola, cuadro digital, palanca manual y puertas. No se ha importado un modelo 3D oficial ni se garantiza una reproducción exacta del acabado o las dimensiones de fábrica: el fotorrealismo y las texturas PBR detalladas siguen pendientes.

Los **tres retrovisores son funcionales en el renderizador**: cada uno utiliza una cámara independiente orientada hacia atrás, dibuja una textura dinámica del entorno y excluye el interior para evitar reflejos recursivos. En la demo podrás ver la carretera y edificios que queden detrás del coche; **todavía no hay tráfico IA** que aparezca en ellos. El izquierdo y derecho se actualizan a la mitad de la frecuencia del espejo central para reducir el coste de renderizado.

Código modular: `src/vehicle/clioInterior.js` (habitáculo, animaciones e instrumentación) y `src/vehicle/mirrorSystem.js` (tres cámaras y texturas). Los antiguos espejos CSS decorativos se han eliminado.

Las marchas y la velocidad son todavía simplificadas: no existe física de motor, embrague o estacionamiento de precisión. La ruta demostrativa sigue siendo ficticia.

## Inicio en Móstoles y minimapa

El juego intenta iniciar automáticamente en la coordenada **40.344103, -3.863962**, señalada por el usuario como punto de salida junto al centro DGT de Móstoles. Descarga la red vial de OpenStreetMap próxima a esa coordenada, conserva la posición indicada como inicio visible y dibuja la conexión aproximada hasta un nodo transitable próximo (como máximo 70 m); el punto exacto de entrada/salida vial, el sentido permitido de maniobra y las señales están **pendientes de verificación presencial**. No se afirma que exista una ruta oficial de examen.

Si el proveedor geográfico no responde, aparece un error y dos opciones explícitas: volver a intentar la carga o abrir el circuito demostrativo **ficticio**. Nunca se cambia silenciosamente a una ruta inventada bajo el nombre de DGT. El minimapa refleja el mismo recorrido y posición que se visualizan en 3D, con norte arriba y una flecha de rumbo. En modo demo se etiqueta como ficticio.

Las preguntas de la demo didáctica se muestran junto al minimapa, dentro de la pantalla del coche. En el modo de red real solo se ofrecen decisiones de navegación geométrica; **no se califican señales ni maniobras reales no verificadas**.

## IMPORTANTE: qué es real y qué no

- El **circuito de examen demostrativo es ficticio**. No representa las calles ni las señales reales de Móstoles.
- El botón **Explorar calles reales (experimental)** consulta OpenStreetMap vía Nominatim y Overpass, genera vías 3D simples y deja recorrer sus conexiones mediante botones; NO valida el punto exacto de salida del centro ni dispone aún de señales, carriles, sentido de marcha en cada situación, rutas oficiales, tráfico IA o evaluación de circulación en calles reales.
- El coche se traslada con animaciones sobre un trazado; no existe todavía simulación física de embrague y caja de cambios ni estacionamiento preciso. La elección de marcha aparece representada en el cuadro y cambia la velocidad de la animación; la relación RPM/velocidad y el calado quedan pendientes.
- Las faltas de los escenarios ficticios son **orientativas**: la gravedad en un examen real depende de hechos observables y del baremo de la DGT.
- Se necesita conexión a Internet para instalar dependencias y para cargar calles reales; la demo ficticia, tras instalar las dependencias, puede ejecutarse sin acceso al servidor de mapas.

## Ejecutar en Windows 11

Instala Node.js LTS y abre PowerShell en la carpeta que contiene este README:

```powershell
npm.cmd install
npm.cmd run dev
```

Abre la dirección que indique Vite (habitualmente `http://localhost:5173/`).

Pruebas: `npm test`. Compilación de producción: `npm run build`.

## Procedencia de los mapas

Los datos geográficos se solicitan directamente a Nominatim y a Overpass; se muestra atribución **© OpenStreetMap contributors**, licencia de base de datos **ODbL**. No añadas scraping ni peticiones masivas a los servicios públicos. Antes de usarlo con muchos alumnos, despliega un servicio propio o un proveedor geográfico con capacidad y licencia adecuadas. Para desplegar el juego de forma comercial hay que atender los requisitos de atribución y, cuando proceda, de distribución de bases de datos derivadas.

- [Copyright de OpenStreetMap](https://www.openstreetmap.org/copyright)
- [Uso de la API pública de Nominatim](https://operations.osmfoundation.org/policies/nominatim/)
- [Uso de la API pública de Overpass](https://wiki.openstreetmap.org/wiki/Overpass_API)
- [Criterios del examen práctico de la DGT](https://www.dgt.es/nuestros-servicios/permisos-de-conducir/obtener-un-nuevo-permiso-de-conducir/requisitos-preparacion-y-presentacion-a-examen/)

## Próximas fases antes de llamarlo «examen de Móstoles»

1. Verificar manualmente la posición y salida del Centro de Exámenes; importar un recorrido transitable completo y guardar una versión local reproducible con la atribución y licencia correspondientes.
2. Editor de cruces con confirmación de señales, prioridades, carriles, visibilidad y fotografías propias o referencias legales; no deducir un STOP de la forma geométrica de la calle.
3. Motor de decisión que vincule un escenario comprobado con su posición, orientación y acción ejecutable; examen realista y movimientos de tráfico coherentes.
4. Sustituir el renderizador web del prototipo por Unity cuando la lógica del examen y la primera ruta estén validadas, reutilizando los escenarios JSON y la red geográfica. Se requiere calibrar embrague y velocidades por coche.
5. Plantilla por localidades y catálogo de zonas revisadas por instructores antes de ampliar al resto de España.

## GitHub

Código fuente publicado en [toninow/SimpracTest](https://github.com/toninow/SimpracTest). Para actualizar tu copia local ejecuta `git pull origin main` (si tienes cambios locales sin guardar, consérvalos antes de actualizar). No subas `node_modules`, `dist` ni claves privadas. No se requieren claves API en el prototipo.
