/*
	PROTOTIPOS
*/
float3 ComputeForces(RWStructuredBuffer<float3> forcesBuffer, RWStructuredBuffer<int> auxBuffer,int index);
void EulerSemi(RWStructuredBuffer<float4> positionBuffer, RWStructuredBuffer<float3> forcesBuffer, RWStructuredBuffer<float3> velocitiesBuffer, int index);


void Integrate(RWStructuredBuffer<float3> forcesBuffer, RWStructuredBuffer<float4> positionBuffer,
	RWStructuredBuffer<float3> velocitiesBuffer, RWStructuredBuffer<int> auxBuffer, int index) {
		//ComputeForces(forcesBuffer, auxBuffer, index);
		EulerSemi(positionBuffer, forcesBuffer, velocitiesBuffer, index);
}

float3 ComputeForces(RWStructuredBuffer<float3> forcesBuffer, RWStructuredBuffer<int> auxBuffer,
	int index) {
	float3 acceleration = float3(0,0,0);

	acceleration = forcesBuffer[index]; // fuerza del sistema 
	auxBuffer[index] = (int)acceleration.x;
	//acceleration += float3(0, -9.8,0); // Gravedad

	forcesBuffer[index] = float3(0,0,0); // limpiamos las fuerzas para que no se acumulen una vez computadas las aceleraciones

	return acceleration * 0.01;
}

float3 ComputeForces(RWStructuredBuffer<float3> forcesBuffer,
	int index) {
	float3 acceleration = float3(0,0,0);

	acceleration = forcesBuffer[index]; // fuerza del sistema 
	//auxBuffer[index] = (int)acceleration.x;
	acceleration += float3(0, -9.8,0); // Gravedad

	forcesBuffer[index] = float3(0,0,0); // limpiamos las fuerzas para que no se acumulen una vez computadas las aceleraciones

	return acceleration * 0.02;
}

void EulerSemi(RWStructuredBuffer<float4> positionBuffer, RWStructuredBuffer<float3> forcesBuffer,
	RWStructuredBuffer<float3> velocitiesBuffer, int index) {
	float3 v = velocitiesBuffer[index] + ComputeForces(forcesBuffer, index);
	velocitiesBuffer[index] = v;
	v *= 0.02;
	float3 pAux = float3(positionBuffer[index].x, positionBuffer[index].y, positionBuffer[index].z);
	float4 p = float4(pAux.x + v.x, pAux.y + v.y, pAux.z + v.z, positionBuffer[index].w);
	
	positionBuffer[index] = p; 
}
