import * as THREE from 'three';

// Recreación geométrica inspirada en un utilitario Clio de acabado sencillo.
// No es un modelo CAD oficial ni reproduce exactamente sus cotas interiores.
export function createClioInterior(camera) {
  const root = new THREE.Group();
  root.name = 'clio-inspired-cockpit';
  camera.add(root);
  // Mundo: capa 0; habitáculo: capa 1. Las cámaras de los espejos solo verán 0.
  const dark = new THREE.MeshStandardMaterial({color:0x11151c, roughness:0.91});
  const soft = new THREE.MeshStandardMaterial({color:0x23262b, roughness:0.91});
  const mid = new THREE.MeshStandardMaterial({color:0x3a4047, roughness:0.76});
  const trim = new THREE.MeshStandardMaterial({color:0x5e6972, metalness:0.35, roughness:0.48});
  const fabric = new THREE.MeshStandardMaterial({color:0x25282d, roughness:1});
  const glass = new THREE.MeshPhysicalMaterial({color:0x131a21, roughness:0.19, metalness:0.1, clearcoat:0.8});
  const lcd = new THREE.MeshBasicMaterial({color:0x090e15});
  const red = new THREE.MeshBasicMaterial({color:0xc13d36});
  const materials = [dark,soft,mid,trim,fabric,glass,lcd,red];

  const add=(geometry,material,parent=root,x=0,y=0,z=0)=>{
    const mesh=new THREE.Mesh(geometry,material);
    mesh.position.set(x,y,z); parent.add(mesh);
    mesh.castShadow=false; mesh.receiveShadow=false;
    return mesh;
  };
  const box=(w,h,d,x,y,z,material=soft,parent=root)=>{
    return add(new THREE.BoxGeometry(w,h,d),material,parent,x,y,z);
  };
  const rounded=(w,h,r,material,x,y,z)=>{
    const shape=new THREE.Shape();
    shape.moveTo(-w/2+r,-h/2);
    shape.lineTo(w/2-r,-h/2);shape.quadraticCurveTo(w/2,-h/2,w/2,-h/2+r);
    shape.lineTo(w/2,h/2-r);shape.quadraticCurveTo(w/2,h/2,w/2-r,h/2);
    shape.lineTo(-w/2+r,h/2);shape.quadraticCurveTo(-w/2,h/2,-w/2,h/2-r);
    shape.lineTo(-w/2,-h/2+r);shape.quadraticCurveTo(-w/2,-h/2,-w/2+r,-h/2);
    return add(new THREE.ExtrudeGeometry(shape,{depth:0.035,bevelEnabled:true,bevelSegments:2,steps:1,bevelSize:0.012,bevelThickness:0.012,curveSegments:6}),material,root,x,y,z);
  };
  // La carretera se ve por el parabrisas. La geometría se queda en los bordes de la visión.
  box(3.4,.28,.39,0,-.84,-1.08,soft); // tablero inferior
  box(3.45,.12,.51,0,-.68,-1.33,dark); // visera superior del tablero
  box(3.39,.025,.50,0,-.605,-1.33,soft);
  box(3.42,.10,.11,0,-.70,-1.00,trim);
  box(.16,1.7,.13,-1.63,.16,-1.37,soft).rotation.z=-.10;
  box(.16,1.7,.13,1.63,.16,-1.37,soft).rotation.z=.10;
  box(3.37,.105,.14,0,.97,-1.37,soft); // travesaño techo
  box(3.25,.065,.21,0,-1.02,-.66,dark);
  // Molduras y puertas; la puerta izquierda se percibe desde la cámara del conductor.
  for(const side of [-1,1]){
    const door=box(.50,.47,.52,side*1.38,-.82,-.78,fabric);
    door.rotation.y=side*.10;
    box(.35,.045,.08,side*1.47,-.72,-.54,trim);
    box(.18,.05,.035,side*1.47,-.70,-.49,mid);
    // Rejillas de aire laterales con lamas.
    rounded(.23,.15,.025,dark,side*1.27,-.57,-1.04);
    for(let i=0;i<3;i++)box(.19,.012,.018,side*1.27,-.62+i*.052,-.988,trim);
  }
  // Cuadro digital compacto y visera.
  rounded(.66,.31,.045,dark,-.47,-.49,-1.01);
  rounded(.59,.255,.02,glass,-.47,-.49,-.955);
  // Pantalla central sobria (sin apariencia deportiva).
  rounded(.48,.31,.038,dark,.43,-.41,-1.09);
  rounded(.405,.235,.015,lcd,.43,-.41,-1.042);
  for(let i=0;i<4;i++)box(.045,.025,.015,.28+i*.10,-.57,-1.004,mid);
  // Difusores de ventilación y controles de climatización.
  for(const x of [.15,.72]){
    rounded(.18,.115,.025,dark,x,-.65,-.994);
    for(let i=0;i<3;i++)box(.14,.012,.012,x,-.687+i*.036,-.955,trim);
  }
  for(const x of [.23,.42,.61]){
    const knob=add(new THREE.CylinderGeometry(.043,.043,.022,24),mid,root,x,-.84,-.77);
    knob.rotation.x=Math.PI/2;
    box(.006,.028,.006,x,-.82,-.749,trim);
  }
  // Consola y túnel; cambio manual.
  const consoleBody=box(.43,.33,.67,.42,-1.04,-.58,dark);
  consoleBody.rotation.x=-.16;
  rounded(.28,.33,.04,mid,.43,-.92,-.47);
  const shifter=new THREE.Group();shifter.position.set(.43,-.91,-.50);root.add(shifter);
  box(.15,.012,.10,0,.01,0,dark,shifter);
  const shaft=add(new THREE.CylinderGeometry(.012,.018,.20,12),trim,shifter,0,.11,0);
  const knob=add(new THREE.SphereGeometry(.059,20,12),dark,shifter,0,.22,0);
  knob.scale.set(1.1,.85,1);
  box(.038,.006,.016,0,.253,-.01,trim,shifter);
  // Columna de dirección y volante con textura rugosa y brazos en ángulo.
  box(.35,.31,.35,-.47,-.70,-.93,dark);
  const wheel=new THREE.Group();wheel.position.set(-.47,-.57,-.66);
  wheel.rotation.x=-.15;root.add(wheel);
  const rim=add(new THREE.TorusGeometry(.277,.037,14,64),dark,wheel);
  const innerRim=add(new THREE.TorusGeometry(.218,.006,8,48),trim,wheel,0,0,.005);
  for(const a of [Math.PI/6,Math.PI*5/6,Math.PI*3/2]){
    const spoke=box(.18,.06,.05,Math.cos(a)*.13,Math.sin(a)*.13,.015,mid,wheel);
    spoke.rotation.z=a;
  }
  const hub=add(new THREE.CylinderGeometry(.106,.106,.083,28),soft,wheel,0,0,.056);
  hub.rotation.x=Math.PI/2;
  rounded(.075,.085,.015,trim,-.47,-.57,-.558); // emblema geométrico simplificado
  for(const x of [-.145,.145]){
    for(let i=0;i<3;i++)box(.025,.012,.008,x,-.025+i*.025,.08,trim,wheel);
  }
  // Los tres marcos se sitúan dentro del campo de visión del conductor.
  const mirrorSpecs=[
    {id:'left',x:-1.29,y:-.07,z:-1.18,w:.32,h:.16},
    {id:'center',x:0,y:.60,z:-1.18,w:.37,h:.13},
    {id:'right',x:1.29,y:-.07,z:-1.18,w:.32,h:.16}
  ];
  const mirrors={};
  for(const spec of mirrorSpecs){
    rounded(spec.w+.035,spec.h+.035,.025,dark,spec.x,spec.y,spec.z-.015);
    const surface=add(new THREE.PlaneGeometry(spec.w,spec.h),new THREE.MeshBasicMaterial({color:0x7695a5,side:THREE.DoubleSide}),root,spec.x,spec.y,spec.z+.045);
    mirrors[spec.id]=surface;
  }
  box(.045,.22,.045,0,.68,-1.28,dark); // soporte del espejo central
  // Pantalla real con tipografía nítida renderizada sobre un canvas local.
  const canvas=document.createElement('canvas');canvas.width=512;canvas.height=256;
  const ctx=canvas.getContext('2d');
  const texture=new THREE.CanvasTexture(canvas);
  const readout=new THREE.MeshBasicMaterial({map:texture,toneMapped:false});
  const screen=add(new THREE.PlaneGeometry(.56,.235),readout,root,-.47,-.49,-.918);
  const drawDashboard=(speed,gear,seconds)=>{
    ctx.fillStyle='#0a111c';ctx.fillRect(0,0,512,256);
    ctx.strokeStyle='#39506b';ctx.lineWidth=3;ctx.strokeRect(5,5,502,246);
    ctx.fillStyle='#9cc5e5';ctx.font='22px system-ui';ctx.fillText('km/h',163,60);
    ctx.font='bold 110px system-ui';ctx.fillStyle='#f6f9ff';
    ctx.fillText(String(Math.round(speed)).padStart(2,'0'),138,169);
    ctx.fillStyle='#93c9fa';ctx.font='bold 52px system-ui';
    ctx.fillText(gear===-1?'R':gear===0?'N':String(gear),375,158);
    ctx.fillStyle='#90a9bf';ctx.font='18px system-ui';ctx.fillText('SIMPR ACTEST',20,235);
    texture.needsUpdate=true;
  };
  drawDashboard(0,0,0);
  root.traverse(obj=>{obj.layers.set(1);});
  camera.layers.enable(1);
  const update=(dt,{speed=0,gear=0,steering=0}={})=>{
    wheel.rotation.z+=(-steering-wheel.rotation.z)*Math.min(1,dt*7);
    const shifterTarget=gear===-1?-.22:gear===0?0:gear%2?-.15:.15;
    shifter.rotation.z+=(shifterTarget-shifter.rotation.z)*Math.min(1,dt*7);
    const key=Math.round(speed)+'|'+gear;
    if(update.previous!==key){drawDashboard(speed,gear);update.previous=key;}
  };
  const dispose=()=>{
    root.traverse(obj=>{if(obj.isMesh){obj.geometry.dispose();if(obj.material&&!materials.includes(obj.material))obj.material.dispose();}});
    texture.dispose();materials.forEach(m=>m.dispose());root.removeFromParent();
  };
  return {root,wheel,shifter,mirrors,update,dispose};
}
