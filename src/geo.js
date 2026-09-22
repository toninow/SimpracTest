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
