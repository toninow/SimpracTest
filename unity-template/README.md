# SimpracTest · Primera escena en Unity 6

El proyecto web **no se elimina**. Esta carpeta contiene un primer módulo C# que se importa
en un proyecto Unity 6 creado con la plantilla **High Definition 3D (HDRP)** de Unity Hub.

## Estado exacto

Al pulsar Play se construye una cabina procedural, en metros, de un utilitario de acceso
(negro y gris, plástico mate). La foto del habitáculo es solo referencia de encuadre:
el coche no es un Renault. En el centro del volante se lee **Spt**. La cámara está en
el asiento delantero izquierdo. El volante tiene aro, radios y manos que giran con él;
los antebrazos usan dos huesos y siguen las empuñaduras.
La palanca manual anima el paso de punto muerto a 2ª cuando el coche avanza. El instructor
va en el asiento derecho y el examinador detrás; este último puede aparecer en el
retrovisor interior. Los tres espejos usan cámaras y RenderTexture, sin dibujar el
habitáculo para no reflejarse a sí mismos.

El cuadro, detrás del volante, muestra velocidad, marcha, revoluciones y un minimapa del
circuito de demostración con la posición y el rumbo del coche. La pantalla central solo
lleva las preguntas y las respuestas A/B/C/D. Fuera de las pantallas del coche solo queda
una línea corta con la voz del instructor. Responder sigue moviendo el coche; R reinicia;
la evaluación por gravedades no cambia.

La calzada de esta escena mide 7,4 m y tiene el eje en la polilínea. El coche circula por
el centro del carril derecho. Ese desplazamiento no debe copiarse a una calle de sentido
único. El circuito sigue siendo ficticio. La coordenada `40.344103, -3.863962` es solo
la referencia geográfica del futuro punto de salida.

**Aún no es un vehículo controlable mediante embrague/pedales, no tiene modelo 3D
fotorrealista y NO reproduce las calles reales de Móstoles ni un examen oficial**.
La coordenada `40.344103, -3.863962`, indicada por el usuario, se conserva como
referencia del futuro punto de salida geográfico. El mapa y las calles de esta fase
pertenecen exclusivamente a un circuito ficticio diseñado para comprobar el habitáculo,
la cámara, los espejos y la interacción. No descargamos OSM durante esta primera escena.

## Instalar y abrir en el ordenador Windows de la empresa

Requisitos: Windows 10/11 de 64 bits, controlador gráfico actualizado, Unity Hub y
un editor Unity 6. Para HDRP conviene una GPU dedicada compatible; la calidad de esta
primera maqueta no requiere assets de terceros. El usuario debe tener autorización
para instalar software en el equipo de la empresa.

1. Instala Unity Hub desde https://unity.com/download e inicia sesión con tu Unity ID.
2. En Unity Hub > Installs > Install Editor, instala un editor **Unity 6** compatible
   con la plantilla HDRP. Si deseas obtener un ejecutable Windows más adelante, añade
   el módulo `Windows Build Support (IL2CPP)`; para pulsar Play en el editor no hace
   falta dicho módulo.
3. En PowerShell, actualiza o clona el mismo repositorio:

   ```powershell
   cd C:\Proyectos\SimpracTest
   git pull origin main
   ```

   Si todavía no existe esa carpeta:

   ```powershell
   New-Item -ItemType Directory -Force -Path C:\Proyectos | Out-Null
   cd C:\Proyectos
   git clone https://github.com/toninow/SimpracTest.git
   ```

4. En Unity Hub elige **New Project > High Definition 3D (HDRP)**.
   Nombre: `UnityProject`. Carpeta principal (Location): `C:\Proyectos\SimpracTest`.
   La carpeta final del nuevo proyecto debe ser
   `C:\Proyectos\SimpracTest\UnityProject` (contendrá `Assets`,
   `Packages` y `ProjectSettings`). **No elijas `unity-template` como proyecto
   de Unity**, porque es el código de arranque y no un proyecto completo del editor.
5. Cierra Unity mientras copias los dos scripts, ejecutando en PowerShell:

   ```powershell
   Copy-Item -Path "C:\Proyectos\SimpracTest\unity-template\Assets\SimpracTest" `
     -Destination "C:\Proyectos\SimpracTest\UnityProject\Assets\" -Recurse -Force
   ```

6. Abre `UnityProject` desde Hub. Abre una escena normal (la de la plantilla está bien).
   No hace falta colocar componentes: los scripts arrancan automáticamente al pulsar
   el botón **▶ Play**. Selecciona la pestaña **Game** y maximízala si deseas una vista
   amplia. Si el editor pide guardar una escena modificada, puedes hacerlo.
7. Teclado: A, B, C o D = responder. También puedes hacer clic en una respuesta
   de la pantalla central. R = reiniciar. Después de seleccionar una respuesta el
   coche recorre un tramo automáticamente y presenta el escenario siguiente.
8. Comprueba, en la pestaña Game: ojos en el asiento izquierdo; volante y manos
   que giran juntos en las curvas; palanca que sale de punto muerto al avanzar;
   cuadro con velocidad, marcha, rpm y minimapa; preguntas solo en la pantalla
   central; tres retrovisores con la vía detrás; instructor a la derecha y
   examinador visible, al menos en parte, en el espejo interior.

## Compartir también el proyecto de Unity por GitHub

Una vez creado `UnityProject` mediante Unity Hub, los scripts de
`unity-template` ya están en GitHub y el repositorio tendrá también la
estructura real que Unity haya creado, si confirmas esos cambios con:

```powershell
cd C:\Proyectos\SimpracTest
git add UnityProject/Assets UnityProject/Packages UnityProject/ProjectSettings
git commit -m "Crear proyecto Unity HDRP desde la plantilla e importar SimpracTest"
git push origin main
```

El `.gitignore` del repositorio excluye Library/Temp/Logs y archivos temporales.
No subas a GitHub modelos, fotos, texturas o herramientas de terceros sin licencia
para redistribuirlos. El proyecto web se mantiene en la raíz y seguirá ejecutándose
con Vite hasta completar la migración de navegación y escenarios.

## Qué está terminado y qué es provisional

Terminado en esta escena: distribución de pantallas, cámara del conductor, giro del
volante ligado a las manos, palanca que reacciona al cambio de marcha, tres retrovisores
funcionales, ocupantes sentados con cinturón, preguntas, avance, reinicio y evaluación.

Provisional, a la espera de assets con licencia: la piel del salpicadero (malla generada,
no escaneada), los dedos (cajas articuladas, no un rig de mano), el instructor y el
examinador (cápsulas), la carrocería exterior y las revoluciones (relación didáctica
con la velocidad, no un motor real).

## Modelo 3D que haría falta para acercarse a la foto

Para sustituir la malla procedural sin reescribir la conducción:

- FBX a escala 1 unidad = 1 metro, volante a la izquierda, origen en el suelo y en el
  centro del vehículo.
- Mallas separadas: salpicadero, volante (pivote en el centro del aro y eje local de
  giro documentado), palanca, asientos, paneles de puerta y carcasas de espejo.
- Texturas PBR (albedo, normal, rugosidad/metalicidad) reasignables a `HDRP/Lit`.
  Sin shaders de otro motor.
- Conductor con huesos de hombro, codo, muñeca y dedos, o clips de giro del volante.
  `DriverArmRig` se puede retirar cuando esas manos cubran el aro sin atravesarlo.
- Licencia que permita usarlo en un producto de formación. El volante del simulador
  lleva la marca Spt; no hace falta el emblema de ningún fabricante.

## Limitaciones y diagnóstico

- El proyecto solo se ha creado como **código fuente**; no se ha ejecutado todavía
  con un editor Unity ni se ha generado un `.exe` desde esta sesión.
- Si la pantalla Game está negra o rosa, revisa que el proyecto se haya creado con
  la plantilla **HDRP**, no 2D ni un proyecto con renderizador distinto. Abre
  `Window > General > Console` y examina errores de compilación.
- Si el editor comunica que no puede compilar `SimpracUnityPrototype.cs`, no sustituyas
  archivos al azar: copia el **primer error completo**, con archivo y línea.
- La imagen hiperrealista aprobada es una **referencia visual**, no un modelo Unity
  importable. La segunda fase sustituirá la maqueta geométrica con un modelo 3D
  legalmente utilizable, texturas e iluminación afinadas.
- La siguiente fase geográfica debe importar y validar un trazado OSM cercano a
  `40.344103, -3.863962`; no es correcto tratar un trazado imaginado como oficial.
