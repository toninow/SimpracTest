import * as THREE from 'three';
import {scenes} from './scenarios.js';
import {loadRealRoads} from './geo.js';
import {junctionOptions,evaluate,outcome,totalFaults} from './core.js';
import './style.css';
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
// Habitáculo simplificado anclado a la cámara: las calles siguen siendo el mundo 3D.
// Es visual y no representa aún una caja de cambios ni retrovisores físicos.
world.add(camera);
const cabin=new THREE.Group();camera.add(cabin);
const cabinDark=new THREE.MeshStandardMaterial({color:0x131923,roughness:.9,metalness:.07});
const cabinTrim=new THREE.MeshStandardMaterial({color:0x283342,roughness:.65,metalness:.2});
function cabinBox(w,h,d,x,y,z,material=cabinDark){const mesh=new THREE.Mesh(new THREE.BoxGeometry(w,h,d),material);mesh.position.set(x,y,z);cabin.add(mesh);return mesh;}
cabinBox(3.25,.38,.55,0,-.83,-1.25);
cabinBox(3.1,.13,.35,0,-.60,-1.49,cabinTrim);
cabinBox(.095,2,.12,-1.4,.28,-1.21);
cabinBox(.095,2,.12,1.4,.28,-1.21);
cabinBox(2.88,.08,.12,0,.93,-1.21);
// Volante, radios, columna y palanca. Sus movimientos se sincronizan con las maniobras.
const wheel=new THREE.Group();wheel.position.set(-.58,-.55,-.86);cabin.add(wheel);
const rim=new THREE.Mesh(new THREE.TorusGeometry(.27,.036,12,48),cabinDark);wheel.add(rim);
const hub=new THREE.Mesh(new THREE.CylinderGeometry(.085,.085,.09,16),cabinTrim);hub.rotation.x=Math.PI/2;hub.position.z=.025;wheel.add(hub);
for(const angle of [0,Math.PI*2/3,Math.PI*4/3]){const spoke=new THREE.Mesh(new THREE.BoxGeometry(.25,.035,.035),cabinTrim);spoke.position.set(Math.cos(angle)*.13,Math.sin(angle)*.13,0);spoke.rotation.z=angle;wheel.add(spoke);}
cabinBox(.76,.24,.12,-.58,-.60,-1.13,cabinTrim);
const shifter=new THREE.Group();shifter.position.set(.62,-.71,-1.05);cabin.add(shifter);
const stick=new THREE.Mesh(new THREE.CylinderGeometry(.015,.02,.27,12),cabinTrim);stick.position.y=.11;shifter.add(stick);
const knob=new THREE.Mesh(new THREE.SphereGeometry(.065,12,10),cabinDark);knob.position.y=.24;shifter.add(knob);
let steeringTarget=0,gearPosition=0,lastCarHeading=null;
function setCabinGear(gear){gearPosition=gear===-1?-.26:gear===0?0:(gear%2?-.20:.20);}
const state={mode:'demo',step:0,log:[],elapsed:0,answerStarted:0,gear:0,speed:0,motion:null,origin:null,graph:null,node:null,previous:null,current:null,destination:null,started:false,finished:false};
function positionCar(p,heading=0){
 if(lastCarHeading!==null){const diff=Math.atan2(Math.sin(heading-lastCarHeading),Math.cos(heading-lastCarHeading));steeringTarget=THREE.MathUtils.clamp(-diff*6,-.95,.95);}
 lastCarHeading=heading;
 car.position.set(p.x,.05,p.z);car.rotation.y=heading;const direction=new THREE.Vector3(Math.sin(heading),0,Math.cos(heading));const c=new THREE.Vector3(p.x,.05,p.z);camera.position.copy(c).addScaledVector(direction,-.7).add(new THREE.Vector3(0,1.8,0));camera.lookAt(c.clone().addScaledVector(direction,27).add(new THREE.Vector3(0,1.5,0)));}
function updateHUD(){el('gear').textContent=state.gear===-1?'R':state.gear===0?'N':state.gear+'ª';el('speed').innerHTML=Math.round(state.speed)+' <small>km/h</small>';el('faults').textContent=totalFaults(state.log);el('clock').textContent=String(Math.floor(state.elapsed/60)).padStart(2,'0')+':'+String(Math.floor(state.elapsed%60)).padStart(2,'0');}
function say(text){try{speechSynthesis.cancel();const u=new SpeechSynthesisUtterance(text);u.lang='es-ES';u.rate=.95;speechSynthesis.speak(u);}catch{}}
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
function info(title,voice,details){el('description').querySelector('h2').textContent=title;el('instruction').textContent='EXAMINADOR: «'+voice+'»';el('details').textContent=details;el('feedback').replaceChildren();state.answerStarted=performance.now();say(voice);}
function startDemo(){state.mode='demo';state.started=true;state.finished=false;state.step=0;state.log=[];state.elapsed=0;state.gear=0;state.speed=0;state.motion=null;state.graph=null;lastCarHeading=null;setPhase('question');el('source').textContent='Circuito ficticio de entrenamiento (NO calles reales)';el('badge').textContent='MODO DEMOSTRACIÓN';clearRoad();drawPolyline(demoCoords);positionCar(demoCoords[0],0);renderDemo();}
function renderDemo(){if(state.step>=scenes.length)return finish();const s=scenes[state.step];el('progress').textContent='Escena '+(state.step+1)+' de '+scenes.length;info(s.name,s.voice,s.details+' · Situación hipotética diseñada para entrenar decisiones, no una señal verificada de Móstoles.');buttons(s.choices.map((c,i)=>({...c,text:String.fromCharCode(65+i)+'. '+c.text})),choice=>{if(state.motion)return;const result=evaluate(choice);state.log.push({...result,scene:s.id,decision:choice.text,seconds:Math.round((performance.now()-state.answerStarted)/1000)});state.gear=choice.gear;state.speed=choice.speed;setCabinGear(state.gear);updateHUD();el('feedback').textContent='Decisión registrada. Ejecutando la maniobra…';el('choices').replaceChildren();setPhase('moving');const beginMove=()=>{state.motion={kind:'demo',points:demoCoords.slice(state.step*5,Math.min(demoCoords.length,(state.step+1)*5+1)),index:0,fraction:0,speed:Math.max(8,state.speed)/3.6,done:()=>{state.motion=null;state.step++;renderDemo();}};};if(state.speed===0){el('feedback').textContent='El vehículo se ha detenido. Continúe cuando considere que puede reanudar la marcha.';buttons([{text:'Reanudar la marcha desde la detención'}],()=>{state.speed=8;el('choices').replaceChildren();beginMove();});}else beginMove();});}
function finish(){state.finished=true;state.speed=0;state.gear=0;setCabinGear(0);setPhase('result');const o=outcome(state.log);el('progress').textContent='Examen finalizado';el('description').querySelector('h2').textContent='Resultado de entrenamiento';el('instruction').textContent=(o.passed?'Resultado simulado: APTO':'Resultado simulado: NO APTO')+' · L: '+o.l+' · D: '+o.d+' · E: '+o.e;el('details').textContent='Calificación didáctica, NO oficial. Cada gravedad depende del escenario descrito y no sustituye el juicio de un examinador.';el('choices').replaceChildren();const review=document.createElement('div');review.className='review';review.textContent=state.log.map((e,i)=>(i+1)+'. '+e.code+' · '+e.decision+'\n'+e.explanation).join('\n\n');el('choices').append(review);el('feedback').textContent='Puedes reiniciar o exportar las decisiones para revisarlas con tu profesor.';}
function followEdge(from,edge){setPhase('moving');state.destination=state.graph.nodes.get(edge.to);state.motion={kind:'real',points:[from,state.destination],index:0,fraction:0,speed:7,done:()=>{state.previous=from;state.node=state.destination;state.motion=null;showJunction();}};state.speed=25;state.gear=2;setCabinGear(2);}
function showJunction(){if(state.mode!=='real'||state.motion)return;const options=junctionOptions(state.graph,state.node.id,state.previous.id);if(!options.length){el('progress').textContent='Fin de esta vía';info('Fin del tramo','Cuando pueda, continúe por una vía permitida.','El grafo descargado no contiene una conexión válida sin retroceder. Puedes reiniciar desde otra posición.');buttons([{text:'Volver a modo demostración'}],startDemo);return;}
 if(options.length===1){followEdge(state.node,options[0]);return;}
 el('progress').textContent='Intersección de geometría real';info('Intersección · elegir dirección','En la próxima intersección, continúe por una vía permitida.','Se muestran únicamente conexiones dibujadas en OpenStreetMap. NO se han validado semáforos, señales, número de carriles ni prioridad. Este módulo NO califica las decisiones de circulación.');
 buttons(options.map((o,i)=>({text:String.fromCharCode(65+i)+'. '+({izquierda:'Girar a la izquierda',derecha:'Girar a la derecha',recto:'Continuar de frente'}[o.turn])+' → '+o.name,edge:o})),choice=>{followEdge(state.node,choice.edge);el('choices').replaceChildren();el('feedback').textContent='Avanzando por la vía descargada…';});}
async function startReal(){el('load').disabled=true;el('geoStatus').textContent='Buscando el lugar y descargando la red vial…';try{const data=await loadRealRoads(el('location').value);if(!data.graph.edges.get(data.start.id)?.length)throw Error('El punto encontrado no tiene conexiones transitables.');state.mode='real';state.started=true;state.finished=false;state.graph=data.graph;state.node=data.start;state.previous=null;state.motion=null;state.log=[];state.elapsed=0;state.speed=0;state.gear=1;lastCarHeading=null;setCabinGear(1);clearRoad();drawPolyline(data.graph,true);el('modal').hidden=true;el('badge').textContent='MAPA REAL · EN PRUEBAS';el('source').textContent='Red vial OSM · '+data.place;el('progress').textContent='Validación pendiente';const edges=data.graph.edges.get(state.node.id);const first=edges.find(e=>!['service','track'].includes(e.highway))||edges[0];const initial=data.graph.nodes.get(first.to);positionCar(state.node,Math.atan2(initial.x-state.node.x,initial.z-state.node.z));el('progress').textContent='Calles reales · navegación libre';info('Geometría real descargada','Seleccione una vía para comenzar.','AVISO: el punto seleccionado es el nodo de carretera más cercano a la ubicación geocodificada. No está verificado que sea la salida del centro de exámenes ni que las señales visibles en el juego reproduzcan las reales.');buttons(edges.map((e,i)=>({text:String.fromCharCode(65+i)+'. '+e.name,edge:e})),c=>{followEdge(state.node,c.edge);el('choices').replaceChildren();});}catch(err){el('geoStatus').textContent='No se ha podido cargar la red vial: '+err.message+' Prueba con una dirección más precisa o vuelve al circuito de demostración.';}finally{el('load').disabled=false;}}
let last=performance.now();function frame(now){const dt=Math.min(.05,(now-last)/1000);last=now;if(state.started&&!state.finished)state.elapsed+=dt;const m=state.motion;if(m){const p=m.points[m.index],q=m.points[m.index+1];if(q){const len=Math.hypot(q.x-p.x,q.z-p.z)||1;m.fraction+=m.speed*dt/len;const t=Math.min(1,m.fraction);positionCar({x:p.x+(q.x-p.x)*t,z:p.z+(q.z-p.z)*t},Math.atan2(q.x-p.x,q.z-p.z));if(m.fraction>=1){m.index++;m.fraction=0;}}if(m.index>=m.points.length-1)m.done();}
 wheel.rotation.z+=(steeringTarget-wheel.rotation.z)*Math.min(1,dt*6);
 steeringTarget*=Math.max(0,1-dt*3);
 shifter.rotation.z+=(gearPosition*.4-shifter.rotation.z)*Math.min(1,dt*6);
 updateHUD();renderer.render(world,camera);requestAnimationFrame(frame);}function resize(){const w=el('three').clientWidth,h=el('three').clientHeight;renderer.setSize(w,h);camera.aspect=w/h;camera.updateProjectionMatrix();}window.addEventListener('resize',resize);resize();requestAnimationFrame(frame);
el('fullscreen').onclick=async()=>{
 try{if(document.fullscreenElement)await document.exitFullscreen();else await el('viewer').requestFullscreen();}catch(error){el('feedback').textContent='No se ha podido activar pantalla completa: '+error.message;}
};
document.addEventListener('fullscreenchange',()=>{el('fullscreen').textContent=document.fullscreenElement?'⛶ Salir de pantalla completa':'⛶ Pantalla completa';resize();});
el('restart').onclick=startDemo;el('export').onclick=()=>{const blob=new Blob([JSON.stringify({mode:state.mode,date:new Date().toISOString(),log:state.log,result:outcome(state.log)},null,2)],{type:'application/json'});const a=document.createElement('a');a.href=URL.createObjectURL(blob);a.download='simulacro-mostoles-'+Date.now()+'.json';a.click();setTimeout(()=>URL.revokeObjectURL(a.href),1000);};
const geoBtn=document.createElement('button');geoBtn.id='geoBtn';geoBtn.textContent='🌍 Explorar calles reales (experimental)';el('restart').before(geoBtn);geoBtn.onclick=()=>{el('modal').hidden=false;};el('cancel').onclick=()=>{el('modal').hidden=true;};el('load').onclick=startReal;startDemo();
