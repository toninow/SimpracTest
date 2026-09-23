import * as THREE from 'three';
import {scenes} from './scenarios.js';
import {loadRealRoads,loadMostolesRoads,MOSTOLES_DGT_REFERENCE} from './geo.js';
import {createMinimap} from './ui/minimap.js';
import {junctionOptions,evaluate,outcome,totalFaults} from './core.js';
import './style.css';
import {createClioInterior} from './vehicle/clioInterior.js';
import {createMirrorSystem} from './vehicle/mirrorSystem.js';
import {createOccupants} from './vehicle/occupants.js';
const el=id=>document.getElementById(id);
const demoCoords=[[0,0],[0,42],[0,88],[0,130],[0,171],[0,206],[0,242],[3,270],[15,288],[35,296],[56,288],[69,269],[65,248],[52,233],[39,227],[30,209],[32,181],[35,152],[36,115],[41,89],[56,67],[76,59],[95,59],[112,64],[124,79],[128,98],[128,122],[128,150],[128,179],[129,206],[129,235],[129,257]].map(([x,z],i)=>({id:i,x,z}));
const world=new THREE.Scene();world.background=new THREE.Color('#a9cce9');world.fog=new THREE.Fog('#a9cce9',70,270);
const camera=new THREE.PerspectiveCamera(74,1,.08,550),renderer=new THREE.WebGLRenderer({antialias:true});renderer.setPixelRatio(Math.min(devicePixelRatio,2));renderer.shadowMap.enabled=false;el('three').appendChild(renderer.domElement);
world.add(new THREE.HemisphereLight(0xffffff,0x788b67,2));const sun=new THREE.DirectionalLight(0xffffff,2.0);sun.position.set(35,100,30);world.add(sun);
const mats={ground:new THREE.MeshStandardMaterial({color:0x768a68}),asphalt:new THREE.MeshStandardMaterial({color:0x343c47,roughness:1}),walk:new THREE.MeshStandardMaterial({color:0xb1ad9d}),white:new THREE.MeshBasicMaterial({color:0xf2efe6}),yellow:new THREE.MeshBasicMaterial({color:0xe9b753}),green:new THREE.MeshStandardMaterial({color:0x476d46}),building:new THREE.MeshStandardMaterial({color:0xb7b6ab}),window:new THREE.MeshStandardMaterial({color:0x6e8997}),hood:new THREE.MeshStandardMaterial({color:0x1f92bd,metalness:.3,roughness:.4})};
const ground=new THREE.Mesh(new THREE.PlaneGeometry(2500,2500),mats.ground);ground.rotation.x=-Math.PI/2;ground.position.y=-.14;world.add(ground);
const roadRoot=new THREE.Group();world.add(roadRoot);
function segment(p,q,width=7,material=mats.asphalt,y=0){const dx=q.x-p.x,dz=q.z-p.z,len=Math.hypot(dx,dz);if(len<.1)return;const mesh=new THREE.Mesh(new THREE.BoxGeometry(len,.035,width),material);mesh.position.set((p.x+q.x)/2,y,(p.z+q.z)/2);mesh.rotation.y=-Math.atan2(dz,dx);roadRoot.add(mesh);return mesh;}
function building(x,z,index){const h=5+(index%5)*2.3;const b=new THREE.Mesh(new THREE.BoxGeometry(10,h,9),mats.building);b.position.set(x,h/2-.1,z);roadRoot.add(b);const roof=new THREE.Mesh(new THREE.BoxGeometry(10.4,.22,9.4),mats.walk);roof.position.set(x,h+.04,z);roadRoot.add(roof);}
function clearRoad(){while(roadRoot.children.length){const obj=roadRoot.children.pop();obj.parent=null;obj.geometry?.dispose();}}
function drawPolyline(points,real=false){if(!real){for(let i=1;i<points.length;i++){const p=points[i-1],q=points[i];segment(p,q,12,mats.walk,-.09);segment(p,q,7,mats.asphalt,0);const delta=Math.hypot(q.x-p.x,q.z-p.z),dx=(q.x-p.x)/delta,dz=(q.z-p.z)/delta;for(let k=7;k<delta-3;k+=11){const a={x:p.x+dx*k,z:p.z+dz*k},b={x:a.x+dx*3,z:a.z+dz*3};segment(a,b,.13,mats.white,.043);}if(i%3===0){const mid={x:(p.x+q.x)/2,z:(p.z+q.z)/2},nx=-dz,nz=dx;building(mid.x+nx*13,mid.z+nz*13,i);building(mid.x-nx*13,mid.z-nz*13,i+2);}}return;}
 const drawn=new Set();let n=0;for(const p of points.paths){for(let i=1;i<p.ns.length;i++){const a=points.nodes.get(p.ns[i-1]),b=points.nodes.get(p.ns[i]);if(!a||!b)continue;const id=Math.min(a.id,b.id)+'-'+Math.max(a.id,b.id);if(drawn.has(id))continue;drawn.add(id);const w=['primary','secondary','tertiary'].includes(p.tags.highway)?9:6;segment(a,b,w+4,mats.walk,-.075);segment(a,b,w,mats.asphalt,0);if(++n>2800)break;}if(n>2800)break;}
}
const car=new THREE.Group();const shell=new THREE.Mesh(new THREE.BoxGeometry(1.6,.7,3.7),mats.hood);shell.position.y=.64;car.add(shell);const glass=new THREE.Mesh(new THREE.BoxGeometry(1.42,.53,1.55),mats.window);glass.position.set(0,1.17,-.27);car.add(glass);world.add(car);
// El coche simplificado solo mantiene la posición para la navegación; el jugador ve
// un interior independiente. Evitamos que la carrocería tape la visión de los espejos.
car.traverse(obj=>obj.layers.set(2));
world.add(camera);
const interior=createClioInterior(camera);
const occupants=createOccupants({camera,car});
const mirrorSystem=createMirrorSystem({renderer,scene:world,mirrors:interior.mirrors});
const minimap=createMinimap(el('minimap'));
const screenCorners=[new THREE.Vector3(),new THREE.Vector3()];
const screenBox=el('infotainment');
function placeInfotainment(){
 camera.updateMatrixWorld(true);interior.screenAnchor.updateWorldMatrix(true,false);
 const w=el('three').clientWidth,h=el('three').clientHeight;
 if(!w||!h)return;
 const positions=[[-.535,.285],[.535,-.285]];
 for(let i=0;i<2;i++)screenCorners[i].set(...positions[i],.006).applyMatrix4(interior.screenAnchor.matrixWorld).project(camera);
 const x1=(screenCorners[0].x+1)*w/2,y1=(1-screenCorners[0].y)*h/2;
 const x2=(screenCorners[1].x+1)*w/2,y2=(1-screenCorners[1].y)*h/2;
 if(![x1,x2,y1,y2].every(Number.isFinite))return;
 // En pantallas estrechas la proyección del GPS 3D puede quedar fuera del viewport.
 // Se utiliza una vista flotante de respaldo para que las respuestas siempre sean accesibles.
 if(w<850||x1<6||x2>w-6||y2>h-44){
  screenBox.style.left=(w<850?8:Math.max(8,w-430))+'px';
  screenBox.style.top=Math.round(h*.40)+'px';
  screenBox.style.width=Math.min(w-16,420)+'px';
  screenBox.style.height=Math.min(Math.round(h*.46),340)+'px';
  return;
 }
 screenBox.style.left=x1+'px';screenBox.style.top=y1+'px';
 screenBox.style.width=Math.max(100,x2-x1)+'px';screenBox.style.height=Math.max(75,y2-y1)+'px';
}
let steeringTarget=0,lastCarHeading=null,carHeading=0,loadSerial=0,spokenUntil=0;
function setCabinGear(gear){ /* la palanca y el cuadro se actualizan desde interior.update */ }
const state={mode:'loading',step:0,log:[],elapsed:0,answerStarted:0,gear:0,speed:0,motion:null,origin:null,graph:null,node:null,previous:null,current:null,destination:null,startPosition:null,started:false,finished:false};
const LANE_OFFSET=1.45; // mitad aproximada de la calzada española de dos sentidos (no apto para todas las vías)
function positionCar(p,heading=0,{exactStart=false}={}){
 if(lastCarHeading!==null){
  const diff=Math.atan2(Math.sin(heading-lastCarHeading),Math.cos(heading-lastCarHeading));
  steeringTarget=THREE.MathUtils.clamp(-diff*6,-.95,.95);
 }
 lastCarHeading=heading;carHeading=heading;
 const direction=new THREE.Vector3(Math.sin(heading),0,Math.cos(heading));
 const right=new THREE.Vector3(Math.cos(heading),0,-Math.sin(heading));
 const shift=exactStart?0:LANE_OFFSET;
 const c=new THREE.Vector3(p.x,.05,p.z).addScaledVector(right,shift);
 car.position.copy(c);car.rotation.y=heading;
 camera.position.copy(c).addScaledVector(direction,-.70).addScaledVector(right,-.30).add(new THREE.Vector3(0,1.42,0));
 camera.lookAt(c.clone().addScaledVector(direction,26).add(new THREE.Vector3(0,1.42,0)));
}
function updateHUD(){el('gear').textContent=state.gear===-1?'R':state.gear===0?'N':state.gear+'ª';el('speed').innerHTML=Math.round(state.speed)+' <small>km/h</small>';el('faults').textContent=totalFaults(state.log);el('clock').textContent=String(Math.floor(state.elapsed/60)).padStart(2,'0')+':'+String(Math.floor(state.elapsed%60)).padStart(2,'0');}
function say(text){spokenUntil=performance.now()+Math.max(1600,text.length*75);try{speechSynthesis.cancel();const u=new SpeechSynthesisUtterance(text);u.lang='es-ES';u.rate=.95;speechSynthesis.speak(u);}catch{}}
function setPhase(phase){el('viewer').dataset.phase=phase;}
function buttons(items,onChoose){
 const list=el('choices');list.replaceChildren();
 if(items.length)setPhase('question');
 for(const [index,item] of items.entries()){
  const b=document.createElement('button');b.className='choice';b.type='button';
  const key=document.createElement('kbd');key.textContent=String.fromCharCode(65+index);
  const label=document.createElement('span');label.textContent=item.text.replace(/^[A-Z]\.\s*/, '');
  b.append(key,label);b.setAttribute('aria-label',String.fromCharCode(65+index)+'. '+label.textContent);
  b.onclick=()=>{if(b.disabled)return;for(const other of list.querySelectorAll('button'))other.disabled=true;onChoose(item);};
  list.appendChild(b);
 }
}
document.addEventListener('keydown',event=>{
 if(!el('modal').hidden||event.altKey||event.ctrlKey||event.metaKey||event.repeat||event.target instanceof HTMLInputElement)return;
 const index=event.key.toUpperCase().charCodeAt(0)-65;
 if(index>=0&&index<4&&el('viewer').dataset.phase==='question'){
  const option=el('choices').querySelectorAll('button')[index];if(option&&!option.disabled){event.preventDefault();option.click();}
 }
});
function info(title,voice,details){el('description').querySelector('h2').textContent=title;el('instruction').textContent='«'+voice+'»';el('details').textContent=details;el('details').title=details;el('feedback').replaceChildren();state.answerStarted=performance.now();say(voice);}
function startDemo(){loadSerial++;state.mode='demo';state.started=true;state.finished=false;state.step=0;state.log=[];state.elapsed=0;state.gear=0;state.speed=0;state.motion=null;state.graph=null;state.startPosition=null;lastCarHeading=null;setCabinGear(0);setPhase('question');el('start-screen').hidden=true;el('screen-mode').textContent='DEMO · NO ES DGT';minimap.setRoute({demoPoints:demoCoords,label:'Circuito ficticio',origin:demoCoords[0]});el('source').textContent='Circuito ficticio de entrenamiento (NO calles reales)';el('badge').textContent='MODO DEMOSTRACIÓN';clearRoad();drawPolyline(demoCoords);positionCar(demoCoords[0],0);renderDemo();}
function renderDemo(){if(state.step>=scenes.length)return finish();const s=scenes[state.step];el('progress').textContent='Escena '+(state.step+1)+' de '+scenes.length;info(s.name,s.voice,s.details+' · Situación hipotética diseñada para entrenar decisiones, no una señal verificada de Móstoles.');buttons(s.choices.map((c,i)=>({...c,text:String.fromCharCode(65+i)+'. '+c.text})),choice=>{if(state.motion)return;const result=evaluate(choice);state.log.push({...result,scene:s.id,decision:choice.text,seconds:Math.round((performance.now()-state.answerStarted)/1000)});state.gear=choice.gear;state.speed=choice.speed;setCabinGear(state.gear);updateHUD();el('feedback').textContent='Decisión registrada. Ejecutando la maniobra…';el('choices').replaceChildren();setPhase('moving');const beginMove=()=>{state.motion={kind:'demo',points:demoCoords.slice(state.step*5,Math.min(demoCoords.length,(state.step+1)*5+1)),index:0,fraction:0,speed:Math.max(8,state.speed)/3.6,done:()=>{state.motion=null;state.step++;renderDemo();}};};if(state.speed===0){el('feedback').textContent='El vehículo se ha detenido. Continúe cuando considere que puede reanudar la marcha.';buttons([{text:'Reanudar la marcha desde la detención'}],()=>{state.speed=8;el('choices').replaceChildren();setPhase('moving');beginMove();});}else beginMove();});}
function finish(){state.finished=true;state.speed=0;state.gear=0;setCabinGear(0);setPhase('result');const o=outcome(state.log);el('progress').textContent='Examen finalizado';el('description').querySelector('h2').textContent='Resultado de entrenamiento';el('instruction').textContent=(o.passed?'Resultado simulado: APTO':'Resultado simulado: NO APTO')+' · L: '+o.l+' · D: '+o.d+' · E: '+o.e;el('details').textContent='Calificación didáctica, NO oficial. Cada gravedad depende del escenario descrito y no sustituye el juicio de un examinador.';el('choices').replaceChildren();const review=document.createElement('div');review.className='review';review.textContent=state.log.map((e,i)=>(i+1)+'. '+e.code+' · '+e.decision+'\n'+e.explanation).join('\n\n');el('choices').append(review);el('feedback').textContent='Puedes reiniciar o exportar las decisiones para revisarlas con tu profesor.';}
function followEdge(from,edge){
 if(!edge||!state.graph?.nodes.has(edge.to))return;
 setPhase('moving');
 state.destination=state.graph.nodes.get(edge.to);
 const points=state.startPosition?[state.startPosition,from,state.destination]:[from,state.destination];
 state.startPosition=null;
 state.motion={kind:'real',points,index:0,fraction:0,speed:7,
  done:()=>{state.previous=from;state.node=state.destination;state.motion=null;showJunction();}};
 state.speed=25;state.gear=2;setCabinGear(2);
}
function showJunction(){if(state.mode!=='real'||state.motion)return;const options=junctionOptions(state.graph,state.node.id,state.previous.id);if(!options.length){el('progress').textContent='Fin de esta vía';info('Fin del tramo','Cuando pueda, continúe por una vía permitida.','El grafo descargado no contiene una conexión válida sin retroceder. Puedes reiniciar desde otra posición.');buttons([{text:'Volver a modo demostración'}],startDemo);return;}
 if(options.length===1){followEdge(state.node,options[0]);return;}
 el('progress').textContent='Intersección de geometría real';info('Intersección · elegir dirección','En la próxima intersección, continúe por una vía permitida.','Se muestran únicamente conexiones dibujadas en OpenStreetMap. NO se han validado semáforos, señales, número de carriles ni prioridad. Este módulo NO califica las decisiones de circulación.');
 buttons(options.map((o,i)=>({text:String.fromCharCode(65+i)+'. '+({izquierda:'Girar a la izquierda',derecha:'Girar a la derecha',recto:'Continuar de frente'}[o.turn])+' → '+o.name,edge:o})),choice=>{followEdge(state.node,choice.edge);el('choices').replaceChildren();el('feedback').textContent='Avanzando por la vía descargada…';});}
function enterReal(data,{atDgt=false}={}){
 if(!data.graph.edges.get(data.start.id)?.length)throw Error('El punto cercano no tiene conexiones transitables.');
 state.mode='real';state.started=true;state.finished=false;
 state.graph=data.graph;state.origin=data.origin;state.node=data.start;state.previous=null;
 state.motion=null;state.log=[];state.elapsed=0;state.speed=0;state.gear=0;
 state.startPosition=atDgt?data.startPosition:null;
 lastCarHeading=null;setCabinGear(0);clearRoad();drawPolyline(data.graph,true);
 minimap.setRoute({graph:data.graph,label:atDgt?'Salida indicada DGT':'Red OSM',origin:atDgt?data.startPosition:data.start});
 el('modal').hidden=true;el('start-screen').hidden=true;
 el('badge').textContent='CALLES OSM · EXPLORACIÓN';
 el('source').textContent='© OpenStreetMap · '+data.place;
 el('progress').textContent=atDgt?'Salida indicada · sin ruta oficial':'Navegación libre · sin verificar';
 el('screen-mode').textContent=atDgt?'DGT · PUNTO INDICADO':'CALLES OSM';
 const edges=data.graph.edges.get(data.start.id);
 const first=edges.find(e=>!['service','track'].includes(e.highway))||edges[0];
 const initial=data.graph.nodes.get(first.to);
 const heading=atDgt?Math.atan2(data.start.x-data.startPosition.x,data.start.z-data.startPosition.z):Math.atan2(initial.x-data.start.x,initial.z-data.start.z);
 positionCar(atDgt?data.startPosition:data.start,heading,{exactStart:atDgt});
 const warning=atDgt?'La posición inicial corresponde a tus coordenadas ('+MOSTOLES_DGT_REFERENCE.lat+', '+MOSTOLES_DGT_REFERENCE.lon+'). El trazado OSM queda a '+Math.round(data.distanceToRoad)+' m. La conexión inicial es ilustrativa: revisa sobre el terreno sentido, acceso y señales; NO es ruta oficial.':'El punto inicial es un nodo vial próximo a la referencia consultada. No se han verificado señales, carriles ni prioridades.';
 info(atDgt?'Salida DGT · Móstoles':'Geometría de calles','Prepárese. Seleccione la dirección para iniciar la marcha.',warning);
 buttons(edges.map((e,i)=>({text:String.fromCharCode(65+i)+'. '+e.name,edge:e})),c=>{
  el('choices').replaceChildren();followEdge(state.node,c.edge);
 });
 minimap.update({position:car.position,course:carHeading,force:true});
}
async function startMostoles(){
 const request=++loadSerial;
 el('start-screen').hidden=false;el('retry-dgt').hidden=true;el('try-demo').hidden=true;
 el('start-message').textContent='Cargando calles próximas al punto indicado: 40.344103, -3.863962…';
 el('restart').disabled=true;
 state.mode='loading';state.started=false;state.motion=null;state.speed=0;
 el('choices').replaceChildren();el('modal').hidden=true;
 try{
  const data=await loadMostolesRoads();
  if(request!==loadSerial)return;
  enterReal(data,{atDgt:true});
 }catch(err){
  if(request!==loadSerial)return;
  el('start-message').textContent='No se pudo iniciar desde la salida indicada: '+err.message+' Puedes reintentar o abrir explícitamente el circuito ficticio.';
  el('retry-dgt').hidden=false;el('try-demo').hidden=false;
  el('badge').textContent='SIN DATOS DE CALLES';
 }finally{if(request===loadSerial)el('restart').disabled=false;}
}
async function startReal(){
 const request=++loadSerial;
 el('load').disabled=true;
 el('geoStatus').textContent='Buscando el lugar y descargando la red vial…';
 try{const data=await loadRealRoads(el('location').value);if(request!==loadSerial)return;enterReal(data);}
 catch(err){if(request===loadSerial)el('geoStatus').textContent='No se ha podido cargar la red vial: '+err.message;}
 finally{if(request===loadSerial)el('load').disabled=false;}
}
let last=performance.now();function frame(now){const dt=Math.min(.05,(now-last)/1000);last=now;if(state.started&&!state.finished)state.elapsed+=dt;const m=state.motion;if(m){const p=m.points[m.index],q=m.points[m.index+1];if(q){const len=Math.hypot(q.x-p.x,q.z-p.z)||1;m.fraction+=m.speed*dt/len;const t=Math.min(1,m.fraction);positionCar({x:p.x+(q.x-p.x)*t,z:p.z+(q.z-p.z)*t},Math.atan2(q.x-p.x,q.z-p.z));if(m.fraction>=1){m.index++;m.fraction=0;}}if(m.index>=m.points.length-1)m.done();}
 // Los espejos consultan las posiciones actuales de la carretera antes de renderizar el ojo del conductor.
 camera.updateMatrixWorld(true);
 mirrorSystem.update({position:car.position,heading:carHeading});
 interior.update(dt,{speed:state.speed,gear:state.gear,steering:steeringTarget});
 occupants.update(dt,{speaking:now<spokenUntil});
 steeringTarget*=Math.max(0,1-dt*3);
 updateHUD();placeInfotainment();minimap.update({position:car.position,course:carHeading});renderer.render(world,camera);requestAnimationFrame(frame);}function resize(){const w=el('three').clientWidth,h=el('three').clientHeight;renderer.setSize(w,h);camera.aspect=w/h;camera.updateProjectionMatrix();}window.addEventListener('resize',resize);resize();requestAnimationFrame(frame);
el('fullscreen').onclick=async()=>{
 try{if(document.fullscreenElement)await document.exitFullscreen();else await el('app').requestFullscreen();}catch(error){el('feedback').textContent='No se ha podido activar pantalla completa: '+error.message;}
};
document.addEventListener('fullscreenchange',()=>{el('fullscreen').textContent=document.fullscreenElement?'⛶ Salir de pantalla completa':'⛶ Pantalla completa';resize();});
el('restart').onclick=startMostoles;el('demo').onclick=()=>{el('restart').disabled=false;startDemo();};
el('retry-dgt').onclick=startMostoles;el('try-demo').onclick=startDemo;el('export').onclick=()=>{const blob=new Blob([JSON.stringify({mode:state.mode,date:new Date().toISOString(),log:state.log,result:outcome(state.log)},null,2)],{type:'application/json'});const a=document.createElement('a');a.href=URL.createObjectURL(blob);a.download='simulacro-mostoles-'+Date.now()+'.json';a.click();setTimeout(()=>URL.revokeObjectURL(a.href),1000);};
const geoBtn=document.createElement('button');geoBtn.id='geoBtn';geoBtn.textContent='🌍 Explorar calles reales (experimental)';el('restart').before(geoBtn);geoBtn.onclick=()=>{el('modal').hidden=false;};el('cancel').onclick=()=>{loadSerial++;el('load').disabled=false;el('modal').hidden=true;};el('load').onclick=startReal;startMostoles();
