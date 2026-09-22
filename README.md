# Autoescuela Virtual · Móstoles — prototipo 0.1

Prototipo jugable para entrenar el **práctico** del permiso B desde el ordenador: cámara desde el coche, trayectos animados según decisiones, cambios de marcha simplificados, voz del examinador, seis escenarios encadenados, evaluación orientativa, cronómetro y exportación de resultados.

## IMPORTANTE: qué es real y qué no

- El **circuito de examen demostrativo es ficticio**. No representa las calles ni las señales reales de Móstoles.
- El botón **Explorar calles reales (experimental)** consulta OpenStreetMap vía Nominatim y Overpass, genera vías 3D simples y deja recorrer sus conexiones mediante botones; NO valida el punto exacto de salida del centro ni dispone aún de señales, carriles, sentido de marcha en cada situación, rutas oficiales, tráfico IA o evaluación de circulación en calles reales.
- El coche se traslada con animaciones sobre un trazado; no existe todavía simulación física de embrague y caja de cambios ni estacionamiento preciso. La elección de marcha aparece representada en el cuadro y cambia la velocidad de la animación; la relación RPM/velocidad y el calado quedan pendientes.
- Las faltas de los escenarios ficticios son **orientativas**: la gravedad en un examen real depende de hechos observables y del baremo de la DGT.
- Se necesita conexión a Internet para instalar dependencias y para cargar calles reales; la demo ficticia, tras instalar las dependencias, puede ejecutarse sin acceso al servidor de mapas.

## Ejecutar en Windows 11

Instala Node.js LTS y abre PowerShell en la carpeta que contiene este README:

```powershell
npm install
npm run dev
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

Este ZIP no se ha publicado en GitHub. Crea un repositorio nuevo privado y sube estos archivos (sin `node_modules` ni `dist`). No se requieren claves API en el prototipo.
