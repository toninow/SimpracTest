import {geoBounds,parseRoads,nearestNode} from './core.js';
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
 const {metres}=await import('./core.js');
 const candidates=[...graph.nodes.values()].filter(node=>
   (graph.edges.get(node.id)||[]).some(edge=>viable.has(edge.highway)));
 candidates.sort((a,b)=>metres(a,origin)-metres(b,origin));
 const start=candidates[0];
 if(!start||metres(start,origin)>70)throw Error('No hay un nodo de vía transitable a menos de 70 m del punto de salida indicado. No se moverá el coche a otra zona.');
 return {graph,origin,start,startPosition:{x:0,z:0},distanceToRoad:metres(start,origin),
  place:'Salida indicada · DGT Móstoles (40.344103, -3.863962)',
  verifiedDeparture:false,osmSource:'© OpenStreetMap contributors · ODbL'};
}
