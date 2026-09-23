import test from 'node:test';
import assert from 'node:assert/strict';
import {loadMostolesRoads,MOSTOLES_DGT_REFERENCE} from '../src/geo.js';

function roadAt(latitudeOffset){
 const o=MOSTOLES_DGT_REFERENCE;
 return {elements:[
  {type:'node',id:1,lat:o.lat+latitudeOffset,lon:o.lon-.0005},
  {type:'node',id:2,lat:o.lat+latitudeOffset,lon:o.lon+.0005},
  {type:'way',id:11,nodes:[1,2],tags:{highway:'residential',name:'Vía OSM de prueba'}}
 ]};
}
test('sale exactamente de las coordenadas indicadas y encuentra un tramo cercano aunque sus nodos estén lejos',async()=>{
 const old=globalThis.fetch;
 globalThis.fetch=async()=>({ok:true,json:async()=>roadAt(.000012)});
 try{
  const out=await loadMostolesRoads();
  assert.deepEqual(out.origin,{lat:40.344103,lon:-3.863962});
  assert.deepEqual(out.startPosition,{x:0,z:0});
  assert.ok(out.distanceToRoad<3,'tramo a unos metros del punto');
  assert.ok(out.graph.edges.get(out.start.id)?.length);
  assert.equal(out.verifiedDeparture,false);
 }finally{globalThis.fetch=old;}
});
test('no inventa una salida si la carretera está a más de doce metros',async()=>{
 const old=globalThis.fetch;
 globalThis.fetch=async()=>({ok:true,json:async()=>roadAt(.00025)});
 try{
  await assert.rejects(loadMostolesRoads(),/12 m/);
 }finally{globalThis.fetch=old;}
});
