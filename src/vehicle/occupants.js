import * as THREE from 'three';

// Figuras provisionales de referencia espacial para el puesto del profesor
// y el asiento trasero del examinador. No son modelos humanos fotorrealistas.
export function createOccupants({camera,car}) {
 const cloth=new THREE.MeshStandardMaterial({color:0x29415b,roughness:.94});
 const examinerCloth=new THREE.MeshStandardMaterial({color:0x293039,roughness:.9});
 const skin=new THREE.MeshStandardMaterial({color:0xb98c70,roughness:.96});
 const hair=new THREE.MeshStandardMaterial({color:0x25211d,roughness:1});
 const seat=new THREE.MeshStandardMaterial({color:0x1b2027,roughness:.98});
 const belt=new THREE.MeshStandardMaterial({color:0x6a737b,roughness:.85});
 const shared=[cloth,examinerCloth,skin,hair,seat,belt];
 function part(parent,geometry,material,x,y,z){
  const m=new THREE.Mesh(geometry,material);m.position.set(x,y,z);parent.add(m);return m;
 }
 function strap(root,from,to,r=.010){
  const a=new THREE.Vector3(...from),b=new THREE.Vector3(...to);
  const delta=new THREE.Vector3().subVectors(b,a);
  const mesh=part(root,new THREE.CylinderGeometry(r,r,delta.length(),8),belt,...a.clone().add(b).multiplyScalar(.5).toArray());
  mesh.quaternion.setFromUnitVectors(new THREE.Vector3(0,1,0),delta.normalize());
 }
 function person(parent,{x,y,z,shirt,scale=1}){
  const g=new THREE.Group();g.position.set(x,y,z);g.scale.setScalar(scale);parent.add(g);
  part(g,new THREE.BoxGeometry(.40,.62,.29),seat,0,-.47,.12); // respaldo
  part(g,new THREE.BoxGeometry(.42,.11,.43),seat,0,-.77,-.09);
  part(g,new THREE.CapsuleGeometry(.17,.27,5,12),shirt,0,-.36,0);
  part(g,new THREE.CylinderGeometry(.055,.057,.10,12),skin,0,-.115,-.01);
  part(g,new THREE.SphereGeometry(.115,16,14),skin,0,.03,-.015);
  const hairCap=part(g,new THREE.SphereGeometry(.116,16,12,0,Math.PI*2,0,Math.PI*.47),hair,0,.059,-.019);
  for(const side of [-1,1]){
   const arm=part(g,new THREE.CapsuleGeometry(.050,.26,5,10),shirt,side*.22,-.41,-.12);
   arm.rotation.z=side*.10;
   const hand=part(g,new THREE.SphereGeometry(.043,10,10),skin,side*.22,-.66,-.12);
   hand.scale.set(.75,1.2,.75);
  }
  strap(g,[-.17,-.12,-.21],[.17,-.66,-.23]);
  return g;
 }
 // El profesor se ve a la derecha del conductor. La carrocería oculta parte del asiento.
 const instructor=person(camera,{x:1.28,y:-.13,z:-1.52,shirt:cloth,scale:1.0});
 instructor.traverse(o=>o.layers.set(1));
 camera.layers.enable(1);
 // El examinador va REALMENTE detrás, en coordenadas del vehículo; se ve en
 // el retrovisor interior, cuya cámara también muestra la capa 3.
 const examiner=person(car,{x:.34,y:1.40,z:-1.18,shirt:examinerCloth,scale:.96});
 examiner.traverse(o=>o.layers.set(3));
 let tick=0;
 function update(dt,{speaking=false}={}){
  tick+=dt;
  instructor.rotation.y+=( (speaking?Math.sin(tick*3)*.045:0)-instructor.rotation.y)*Math.min(1,dt*3);
  instructor.position.y=-.13+Math.sin(tick*1.6)*.003;
 }
 function dispose(){
  for(const root of [instructor,examiner]){
   root.traverse(o=>o.isMesh&&o.geometry.dispose());
   root.removeFromParent();
  }
  shared.forEach(m=>m.dispose());
 }
 return {instructor,examiner,update,dispose};
}
