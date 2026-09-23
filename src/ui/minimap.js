// Minimap vectorial: mismo trazado y misma posición usados por el coche 3D.
// En modo OSM dibuja geometría recibida, no una ruta oficial de examen.
export function createMinimap(canvas) {
  const ctx=canvas.getContext('2d');
  let roads=[], mode='loading', center={x:0,z:0}, heading=0, lastDraw=0, place='';
  function setRoute({graph=null,demoPoints=null,label=''}={}){
    roads=graph ? graph.paths.flatMap(path=>{
      const segments=[];
      for(let i=1;i<path.ns.length;i++){
        const a=graph.nodes.get(path.ns[i-1]),b=graph.nodes.get(path.ns[i]);
        if(a&&b)segments.push([a,b]);
      }
      return segments;
    }) : Array.isArray(demoPoints)?demoPoints.slice(1).map((q,i)=>[demoPoints[i],q]):[];
    mode=graph?'osm':demoPoints?'demo':'loading';place=label;
    lastDraw=0;
  }
  function update({position,course=0,force=false}={}){
    if(position)center={x:position.x,z:position.z};
    heading=course;
    const now=performance.now();
    if(!force&&now-lastDraw<145)return;
    lastDraw=now;
    const rect=canvas.getBoundingClientRect();
    const width=Math.max(140,Math.round(rect.width)),height=Math.max(120,Math.round(rect.height));
    const dpr=Math.min(window.devicePixelRatio||1,2);
    if(canvas.width!==Math.round(width*dpr)||canvas.height!==Math.round(height*dpr)){
      canvas.width=Math.round(width*dpr);canvas.height=Math.round(height*dpr);
    }
    ctx.setTransform(dpr,0,0,dpr,0,0);
    ctx.clearRect(0,0,width,height);
    ctx.fillStyle='#0a1827';ctx.fillRect(0,0,width,height);
    ctx.strokeStyle='#203b50';ctx.lineWidth=1;
    for(let x=0;x<width;x+=22){ctx.beginPath();ctx.moveTo(x,0);ctx.lineTo(x,height);ctx.stroke();}
    for(let y=0;y<height;y+=22){ctx.beginPath();ctx.moveTo(0,y);ctx.lineTo(width,y);ctx.stroke();}
    // Norte arriba, coche gira. 170 m en la dimensión más corta del mapa.
    const scale=Math.min(width,height)/170;
    ctx.save();ctx.translate(width/2,height/2);
    ctx.strokeStyle='#a9c0d2';ctx.lineWidth=4;ctx.lineCap='round';ctx.lineJoin='round';
    ctx.beginPath();
    for(const [a,b] of roads){
      const ax=(a.x-center.x)*scale,ay=(a.z-center.z)*scale;
      const bx=(b.x-center.x)*scale,by=(b.z-center.z)*scale;
      if(Math.max(Math.abs(ax),Math.abs(ay),Math.abs(bx),Math.abs(by))>650)continue;
      ctx.moveTo(ax,ay);ctx.lineTo(bx,by);
    }
    ctx.stroke();
    ctx.strokeStyle='#596e81';ctx.lineWidth=1;ctx.stroke();
    // Proyección: z positivo apunta al sur en el mapa.\n    ctx.rotate(Math.PI-heading);
    ctx.fillStyle='#42ddaa';ctx.strokeStyle='#07251d';ctx.lineWidth=2;
    ctx.beginPath();ctx.moveTo(0,-12);ctx.lineTo(8,10);ctx.lineTo(0,6);ctx.lineTo(-8,10);ctx.closePath();ctx.fill();ctx.stroke();
    ctx.restore();
    ctx.fillStyle='#d9e9f5';ctx.font='bold 10px system-ui';ctx.fillText('N ↑',9,15);
    ctx.fillStyle='#9ab3c9';ctx.font='10px system-ui';ctx.fillText(mode==='osm'?'OSM · vías reales':mode==='demo'?'DEMO · circuito ficticio':'CARGANDO…',9,height-8);
    if(place){ctx.fillStyle='#78ddb6';ctx.font='10px system-ui';ctx.fillText(place.slice(0,28),9,30);}
  }
  return {setRoute,update};
}
