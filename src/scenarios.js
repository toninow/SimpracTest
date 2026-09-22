// El circuito de demostración es ficticio. Cada consecuencia es didáctica y no un dictamen oficial.
export const scenes=[
{id:'prepare',name:'01 · Preparación',voice:'Buenos días. Prepare el vehículo e inicie la marcha cuando pueda hacerlo con seguridad.',details:'Está estacionado. El coche está apagado y tiene una caja manual.',choices:[
{text:'Ajusto asiento y espejos, cinturón, compruebo el entorno, arranco y preparo primera.',grade:'safe',gear:1,speed:12,explanation:'Preparación y observación adecuadas.'},
{text:'Arranco, pongo primera y salgo sin comprobar el entorno.',grade:'major',gear:1,speed:12,explanation:'La salida sin observación puede obstaculizar o poner en peligro: la gravedad real depende de lo sucedido.'},
{text:'Inicio la marcha sin abrocharme el cinturón.',grade:'critical',gear:1,speed:12,explanation:'No utilizar el cinturón en la prueba constituye un incumplimiento grave; verificar el criterio aplicable.'},
{text:'Reviso todo y me quedo parado, aunque puedo salir sin peligro.',grade:'minor',gear:1,speed:8,explanation:'Demora injustificada en este escenario didáctico.'}]},
{id:'gear',name:'02 · Cambio de marcha',voice:'Continúe de frente por esta vía.',details:'Circula a 36 km/h en segunda marcha; no hay cruce inmediato. La carretera está despejada.',choices:[
{text:'Compruebo el entorno y selecciono tercera de manera suave.',grade:'safe',gear:3,speed:40,explanation:'Transición de marcha razonable para el escenario, dependiendo del vehículo.'},
{text:'Reduzco bruscamente a primera a 36 km/h.',grade:'major',gear:1,speed:15,explanation:'Reducción inadecuada que puede provocar retención brusca o daño mecánico.'},
{text:'Mantengo segunda sin necesidad y acelero excesivamente.',grade:'minor',gear:2,speed:42,explanation:'Gestión de marchas mejorable; el resultado depende del vehículo.'},
{text:'Desembrago y circulo en punto muerto durante un tramo largo.',grade:'major',gear:0,speed:34,explanation:'Se pierde capacidad de respuesta y freno motor; valoración orientativa.'}]},
{id:'stop',name:'03 · STOP de visibilidad reducida',voice:'En el próximo cruce, gire a la derecha.',details:'STOP simulado: hay una línea de detención, pero los vehículos estacionados impiden ver el tráfico transversal desde ella.',choices:[
{text:'Detengo el vehículo en la línea, avanzo con precaución hasta ver y vuelvo a detenerme si es necesario.',grade:'safe',gear:1,speed:7,explanation:'Secuencia compatible con un STOP sin visibilidad suficiente.'},
{text:'Reduzco mucho la velocidad, pero atravieso el STOP sin detenerme.',grade:'critical',gear:2,speed:22,explanation:'Se simula atravesar un STOP sin detención y con conflicto de prioridad: peligro grave.'},
{text:'Me detengo en la línea y giro directamente sin comprobar el tráfico oculto.',grade:'critical',gear:1,speed:12,explanation:'La incorporación sin visibilidad se plantea aquí como situación de peligro real.'},
{text:'Me detengo correctamente, espero sin motivo durante bastante tiempo y después reanudo la marcha cuando es seguro.',grade:'minor',gear:1,speed:0,explanation:'Demora injustificada en este escenario; en una situación real prevalece la seguridad.'}]},
{id:'round',name:'04 · Rotonda',voice:'En la rotonda, tome la segunda salida.',details:'Glorieta simulada de dos carriles. Circula un vehículo por el carril interior, próximo al punto de incorporación. El exterior está despejado, pero podría cambiar de carril.',choices:[
{text:'Reduzco, observo trayectoria y velocidad, y entro cuando exista un hueco seguro.',grade:'safe',gear:2,speed:18,explanation:'Incorporación segura condicionada al tráfico real.'},
{text:'Entro de inmediato porque el carril exterior parece libre, obligando al otro coche a frenar con fuerza.',grade:'critical',gear:2,speed:24,explanation:'El conflicto generado representa peligro para otro conductor.'},
{text:'Me detengo innecesariamente a pesar de disponer de un hueco suficiente y después reanudo la marcha.',grade:'minor',gear:1,speed:0,explanation:'Detención innecesaria en el supuesto descrito.'},
{text:'Cruzo desde el interior a la salida sin comprobar el exterior y obligo a otro vehículo a esquivarme.',grade:'critical',gear:2,speed:20,explanation:'Salida insegura que genera peligro en este escenario.'}]},
{id:'signal',name:'05 · Señalización',voice:'En la siguiente calle, gire a la derecha.',details:'Dos carriles. Circula una motocicleta por el carril derecho, ligeramente detrás de usted. Actualmente va por el izquierdo.',choices:[
{text:'Observo espejos y ángulo muerto, señalizo con antelación y cambio cuando existe espacio seguro.',grade:'safe',gear:2,speed:22,explanation:'Observación y señalización previas al desplazamiento lateral.'},
{text:'Pongo el intermitente y cambio de carril forzando a la motocicleta a frenar bruscamente.',grade:'critical',gear:2,speed:22,explanation:'El intermitente no otorga prioridad, y aquí se genera un conflicto grave.'},
{text:'Cambio de carril con espacio suficiente, pero olvido señalizar.',grade:'minor',gear:2,speed:22,explanation:'Omisión de señalización sin interferencia simulada; podría agravarse en otras circunstancias.'},
{text:'Me detengo sin motivo en el carril izquierdo para esperar a girar.',grade:'major',gear:1,speed:0,explanation:'Obstaculización importante en el supuesto descrito.'}]},
{id:'park',name:'06 · Estacionamiento',voice:'Cuando pueda, estacione el vehículo en un lugar adecuado.',details:'Hay una plaza señalizada libre, sin vehículos próximos. El estacionamiento se representa con una animación aproximada.',choices:[
{text:'Compruebo tráfico y peatones, señalizo, selecciono marcha atrás y estaciono lentamente.',grade:'safe',gear:-1,speed:3,explanation:'Secuencia básica adecuada; no se evalúan distancias reales con este prototipo.'},
{text:'Inicio marcha atrás sin observar y obligo a un peatón a apartarse.',grade:'critical',gear:-1,speed:5,explanation:'La maniobra genera peligro para un peatón.'},
{text:'Estaciono sin señalizar, pero sin afectar a nadie.',grade:'minor',gear:-1,speed:3,explanation:'Omisión de señalización sin interferencia en esta escena.'},
{text:'Ocupo dos plazas y obstaculizo el paso de otros coches.',grade:'major',gear:-1,speed:3,explanation:'Obstaculización importante en el supuesto simulado.'}]}];
