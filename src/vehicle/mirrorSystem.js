import * as THREE from 'three';

// Cámaras de retrovisor independientes: miran a la geometría real de la escena,
// no a una imagen precargada ni a los espejos HTML anteriores.
export function createMirrorSystem({renderer,scene,mirrors,quality='medium'}) {
  const size = quality==='low' ? [192,96] : quality==='high' ? [512,256] : [320,160];
  const specs=[
    {id:'center',x:0,y:1.65,z:-.24,side:0,interval:1,fov:51},
    {id:'left',x:-1.07,y:1.26,z:.45,side:-.22,interval:2,fov:64},
    {id:'right',x:1.07,y:1.26,z:.45,side:.22,interval:2,fov:64}
  ];
  const rigs=specs.map(spec=>{
    const target=new THREE.WebGLRenderTarget(size[0],size[1],{depthBuffer:true,stencilBuffer:false});
    target.texture.colorSpace=THREE.SRGBColorSpace;
    // Un espejo invierte izquierda y derecha respecto a una cámara normal.
    target.texture.wrapS=THREE.RepeatWrapping;
    target.texture.repeat.x=-1;target.texture.offset.x=1;
    const camera=new THREE.PerspectiveCamera(spec.fov,size[0]/size[1],.08,440);
    camera.layers.set(0); // mundo; excluye habitáculo 1 y carrocería ficticia 2
    if(spec.id==='center')camera.layers.enable(3); // examinador físicamente detrás, en el espejo central
    mirrors[spec.id].material.dispose();
    mirrors[spec.id].material=new THREE.MeshBasicMaterial({map:target.texture,toneMapped:false,side:THREE.DoubleSide});
    return {...spec,camera,target};
  });
  const up=new THREE.Vector3(0,1,0);
  const scratchPosition=new THREE.Vector3();
  const scratchTarget=new THREE.Vector3();
  const forward=new THREE.Vector3();
  const side=new THREE.Vector3();
  let frame=0;
  let paused=false;
  function update({position,heading=0}={}){
    if(!position||paused)return;
    frame++;
    forward.set(Math.sin(heading),0,Math.cos(heading));
    side.set(Math.cos(heading),0,-Math.sin(heading));
    for(const rig of rigs){
      if(frame%rig.interval!==0)continue;
      scratchPosition.set(position.x,position.y??0,position.z)
        .addScaledVector(side,rig.x).addScaledVector(forward,rig.z);
      scratchPosition.y+=rig.y;
      rig.camera.position.copy(scratchPosition);
      scratchTarget.copy(scratchPosition).addScaledVector(forward,-20)
        .addScaledVector(side,rig.side*20);
      rig.camera.up.copy(up);
      rig.camera.lookAt(scratchTarget);
      rig.camera.updateProjectionMatrix();
      const old=renderer.getRenderTarget();
      try {
        renderer.setRenderTarget(rig.target);
        renderer.clear(true,true,true);
        renderer.render(scene,rig.camera);
      } finally {
        renderer.setRenderTarget(old);
      }
    }
  }
  function dispose(){
    for(const rig of rigs){rig.target.dispose();rig.camera.clear();}
  }
  return {update,dispose,setPaused(value){paused=Boolean(value);}};
}
