import test from 'node:test';
import assert from 'node:assert/strict';
import {loadMostolesRoads,MOSTOLES_DGT_REFERENCE} from '../src/geo.js';

test('La salida indicada se usa como origen exacto y no se sustituye por una geocodificación',async()=>{
 const previous=globalThis.fetch;
 let request='';
 globalThis.fetch=async(url,options)=>{
  request=new URLSearchParams(options.body).get('data');
  return {ok:true,json:async()=>({elements:[
   {type:'node',id:1,lat:40.344103,lon:-3.863962},
   {type:'node',id:2,lat:40.34418,lon:-3.863962},
   {type:'way',id:3,nodes:[1,2],tags:{highway:'residential',name:'Vía de prueba'}}
  ]})};
 };
 try{
  const result=await loadMostolesRoads();
  assert.deepEqual(MOSTOLES_DGT_REFERENCE,{lat:40.344103,lon:-3.863962});
  assert.deepEqual(result.origin,MOSTOLES_DGT_REFERENCE);
  assert.deepEqual(result.startPosition,{x:0,z:0});
  assert.equal(result.start.id,1);
  assert.equal(result.verifiedDeparture,false);
  assert.ok(request.includes('way["highway"]'),'se solicitan las vías próximas por coordenadas');
 }finally{globalThis.fetch=previous;}
});

test('Un fallo de descarga geográfica se comunica y no provoca inicio en un circuito ficticio',async()=>{
 const previous=globalThis.fetch;
 globalThis.fetch=async()=>({ok:false,status:503});
 try{await assert.rejects(loadMostolesRoads(),/OpenStreetMap/);}
 finally{globalThis.fetch=previous;}
});
