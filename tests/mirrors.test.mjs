import test from 'node:test';
import assert from 'node:assert/strict';
import * as THREE from 'three';
import {createMirrorSystem} from '../src/vehicle/mirrorSystem.js';

test('tres espejos utilizan cámaras y texturas independientes, detrás del coche',()=>{
  const scene=new THREE.Scene();
  const mirrors=Object.fromEntries(['left','center','right'].map(id=>[id,new THREE.Mesh(
    new THREE.PlaneGeometry(1,1),new THREE.MeshBasicMaterial({color:0xffffff})
  )]));
  const calls=[];
  const renderer={
    getRenderTarget:()=>null,
    setRenderTarget:t=>{calls.push(t?'target':'restore');},
    clear:()=>{},
    render:(_,camera)=>{calls.push({camera,position:camera.position.clone(),direction:camera.getWorldDirection(new THREE.Vector3())});}
  };
  const system=createMirrorSystem({renderer,scene,mirrors});
  const car={position:new THREE.Vector3(4,0,10),heading:0};
  system.update(car);
  assert.equal(calls.filter(c=>typeof c==='object').length,1,'primer fotograma: espejo central');
  system.update(car);
  const rendered=calls.filter(c=>typeof c==='object');
  assert.equal(rendered.length,4,'segundo fotograma: central y dos laterales');
  for(const m of Object.values(mirrors)){
    assert.ok(m.material.map?.isTexture,'el espejo utiliza una textura renderizada');
    assert.equal(m.material.map.repeat.x,-1,'reflejo horizontal');
  }
  assert.ok(rendered[0].direction.z<-.8,'el espejo central apunta hacia atrás');
  assert.ok(rendered[1].direction.z<-.8,'los espejos laterales apuntan hacia atrás');
  assert.equal(rendered[1].camera.layers.isEnabled(1),false,'los espejos no incluyen el propio habitáculo');
  system.dispose();
  for(const m of Object.values(mirrors)){m.material.dispose();m.geometry.dispose();}
});
