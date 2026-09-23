import {geoBounds,parseRoads,nearestNode,metres} from './core.js';
export async function loadRealRoads(query){const q=query.trim();if(!q)throw Error('Indica una ubicación.');
 const geoRes=await fetch('https://nominatim.openstreetmap.org/search?'+new URLSearchParams({q,format:'json',limit:'5',addressdetails:'1'}),{headers:{'Accept':'application/json'}});
 if(!geoRes.ok)throw Error('El servicio de búsqueda no está disponible ('+geoRes.status+').');const matches=await geoRes.json();
 const m=matches.find(x=>x.display_name?.toLowerCase().includes('móstoles'))||matches[0];if(!m)throw Error('No se ha localizado el centro. Puedes especificar una dirección más precisa.');
 const origin={lat:+m.lat,lon:+m.lon};const bbox=geoBounds(origin.lat,origin.lon,.008);
 const queryOSM=`[out:json][timeout:25];(way["highway"](${bbox}););(._;>;);out body;`;
 const response=await fetch('https://overpass-api.de/api/interpreter',{method:'POST',body:new URLSearchParams({data:queryOSM})});if(!response.ok)throw Error('No se han podido descargar las calles ('+response.status+').');
 const raw=await response.json(),graph=parseRoads(raw,origin);if(graph.paths.length===0)throw Error('No se han encontrado calles transitables.');
 const start=nearestNode(graph,origin,350);if(!start)throw Error('No hay vías transitables cerca del punto encontrado. Comprueba el resultado geográfico.');
 return {graph,origin,start,place:m.display_name,osmSource:'© OpenStreetMap contributors · ODbL'};
}

// Punto indicado por el usuario como salida para iniciar la práctica.
// Es una coordenada proporcionada por el usuario; NO implica validación de carriles, señales o recorrido oficial.
export const MOSTOLES_DGT_REFERENCE=Object.freeze({lat:40.344103,lon:-3.863962});
export async function loadMostolesRoads(){
 const origin=MOSTOLES_DGT_REFERENCE;
 const bbox=geoBounds(origin.lat,origin.lon,.0044);
 const queryOSM=`[out:json][timeout:25];(way["highway"](${bbox}););(._;>;);out body;`;
 const response=await fetch('https://overpass-api.de/api/interpreter',{
  method:'POST',body:new URLSearchParams({data:queryOSM}),signal:AbortSignal.timeout(30000)
 });
 if(!response.ok)throw Error('OpenStreetMap no ha entregado la red vial ('+response.status+').');
 const graph=parseRoads(await response.json(),origin);
 const viable=new Set(['service','residential','living_street','unclassified','tertiary','secondary']);
 // Distancia al eje de un tramo, no al nodo: los nodos OSM pueden estar separados
 // decenas de metros aunque el carril pase junto a las coordenadas indicadas.
 let nearest=null;
 for(const path of graph.paths){
  if(!viable.has(path.tags.highway))continue;
  for(let i=1;i<path.ns.length;i++){
   const a=graph.nodes.get(path.ns[i-1]),b=graph.nodes.get(path.ns[i]);
   if(!a||!b)continue;
   const dx=b.x-a.x,dz=b.z-a.z,denom=dx*dx+dz*dz;
   if(denom<.01)continue;
   const t=Math.max(0,Math.min(1,(-a.x*dx-a.z*dz)/denom));
   const snap={x:a.x+t*dx,z:a.z+t*dz};
   const distance=Math.hypot(snap.x,snap.z);
   // Si la vía es de sentido único, desde un punto interior se continúa hacia
   // su extremo permitido; nunca se traza una salida inicial a contramano.
   const forward=(graph.edges.get(a.id)||[]).some(edge=>edge.to===b.id);
   const backward=(graph.edges.get(b.id)||[]).some(edge=>edge.to===a.id);
   for(const candidate of [forward?b:null,backward?a:null]){
    if(!candidate)continue;
    const edges=graph.edges.get(candidate.id)||[];
    if(!edges.some(edge=>viable.has(edge.highway)))continue;
    const score=distance+Math.hypot(candidate.x-snap.x,candidate.z-snap.z)*.002;
    if(!nearest||score<nearest.score)nearest={score,distance,snap,candidate};
   }
  }
 }
 if(!nearest||nearest.distance>12)throw Error('No hay una vía de circulación representada en OSM a 12 m o menos del punto indicado. Comprueba si la salida está mapeada antes de empezar.');
 const start=nearest.candidate;
 // Mantener la posición solicitada. El primer tramo hasta OSM es una aproximación
 // geográfica, NO una ruta autorizada ni una geometría de carril verificada.
 return {graph,origin,start,startPosition:{x:0,z:0},distanceToRoad:nearest.distance,
  place:'Salida indicada · DGT Móstoles (40.344103, -3.863962)',
  verifiedDeparture:false,osmSource:'© OpenStreetMap contributors · ODbL'};
}
