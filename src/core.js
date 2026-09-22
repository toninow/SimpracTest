export const DEG=Math.PI/180;
export function toLocal(p,origin){return {x:(p.lon-origin.lon)*111320*Math.cos(origin.lat*DEG),z:-(p.lat-origin.lat)*111320};}
export function metres(a,b){const dy=(a.lat-b.lat)*111320,dx=(a.lon-b.lon)*111320*Math.cos(a.lat*DEG);return Math.hypot(dx,dy);}
export function parseRoads(data,origin){if(!data||!Array.isArray(data.elements))throw Error('Respuesta geográfica inválida');const nodes=new Map(),edges=new Map(),paths=[];
 for(const e of data.elements){if(e.type==='node')nodes.set(e.id,{id:e.id,lat:e.lat,lon:e.lon,...toLocal(e,origin),tags:e.tags||{}});}
 for(const w of data.elements){if(w.type!=='way'||!w.tags?.highway||!Array.isArray(w.nodes))continue;
 const allowed=!['motorway','motorway_link','trunk','trunk_link','construction','proposed','path','footway','cycleway','steps','pedestrian','bridleway','raceway','platform'].includes(w.tags.highway)&&w.tags.access!=='private'&&w.tags.motor_vehicle!=='no';if(!allowed)continue;
 const ns=w.nodes.filter(id=>nodes.has(id));if(ns.length<2)continue;paths.push({id:w.id,ns,tags:w.tags});const forwardOnly=w.tags.oneway==='yes'||w.tags.oneway==='1'||w.tags.junction==='roundabout';const backwardOnly=w.tags.oneway==='-1';
 for(let i=1;i<ns.length;i++){const a=ns[i-1],b=ns[i];if(!backwardOnly)add(a,b,w);if(!forwardOnly)add(b,a,w);}
 }
 function add(a,b,w){const n1=nodes.get(a),n2=nodes.get(b);if(metres(n1,n2)<.1)return;const list=edges.get(a)||[];list.push({from:a,to:b,way:w.id,name:w.tags.name||w.tags.ref||'vía sin nombre',highway:w.tags.highway});edges.set(a,list);}
 return {nodes,edges,paths};}
export function nearestNode(graph,target,maxM=500){let best=null,d=Infinity;for(const n of graph.nodes.values()){if(!graph.edges.has(n.id))continue;const v=metres(n,target);if(v<d){d=v;best=n;}}return d<=maxM?best:null;}
export function relativeTurn(prev,current,next){const a=Math.atan2(current.z-prev.z,current.x-prev.x),b=Math.atan2(next.z-current.z,next.x-current.x);let diff=(b-a)*180/Math.PI;while(diff>180)diff-=360;while(diff< -180)diff+=360;return diff>35?'derecha':diff< -35?'izquierda':'recto';}
export function junctionOptions(graph,nodeId,prevId){const a=graph.nodes.get(nodeId),p=graph.nodes.get(prevId);if(!a||!p)return[];const options=(graph.edges.get(nodeId)||[]).filter(e=>e.to!==prevId).map(e=>({ ...e,turn:relativeTurn(p,a,graph.nodes.get(e.to))}));return options.filter((e,i)=>options.findIndex(x=>x.to===e.to)===i);}
export const grading={safe:{code:'OK',name:'Correcta'},minor:{code:'L',name:'Leve (orientativa)'},major:{code:'D',name:'Deficiente (orientativa)'},critical:{code:'E',name:'Eliminatoria (orientativa)'}};
export function evaluate(answer){if(!grading[answer.grade])throw Error('Gravedad desconocida');return {code:grading[answer.grade].code,description:grading[answer.grade].name,explanation:answer.explanation||'',tag:answer.tag||'',at:new Date().toISOString()};}
export function totalFaults(log){return log.filter(x=>x.code!=='OK').length;}
export function outcome(log){const e=log.filter(x=>x.code==='E').length,d=log.filter(x=>x.code==='D').length,l=log.filter(x=>x.code==='L').length;return {e,d,l,passed:!e&&d<2&&l<10&&!(d===1&&l>=5)};}
export function geoBounds(lat,lon,r=.008){return [lat-r,lon-r,lat+r,lon+r].map(v=>v.toFixed(6)).join(',');}
