/*
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
	PROTOTIPOS
*/
int Hash(RWStructuredBuffer<float4> positionBuffer, int index, float l);
int Hash(float4 pos);

void GetNeighbors(out int v[40], float3 pos);
int3 DiscretizePos(float4 pos);
int3 DiscretizePos(float3 pos);
// void InitTHash(uint nBuckets, uint sizeBuckets, float supportRadius);
uint RemapIndex(uint index);
/*
	Numeros primos empleados para el hashing
*/
#define p1 73856093
#define p2 19349663
#define p3 83492791

/*
	Parametros de la tabla
*/
uint nBuckets;
uint sizeBuckets;
float l;

int Hash(RWStructuredBuffer<float4> positionBuffer, int index) {
	//float3 r = float3(floor(positionBuffer[index].x / l), floor(positionBuffer[index].y / l), floor(positionBuffer[index].z / l));
	int3 r = DiscretizePos(positionBuffer[index]);
	int rX, rY, rZ;
	rX = (int)r.x*p1;
	rY = (int)r.y*p2;
	rZ = (int)r.z*p3;
	
	return (rX ^ rY ^ rZ) % nBuckets;
}

int Hash(int3 pos) {
	int rX, rY, rZ;

	rX = (int)pos.x*p1;
	rY = (int)pos.y*p2;
	rZ = (int)pos.z*p3;

	return (rX ^ rY ^ rZ) % nBuckets;	
}

void GetNeighbors(RWStructuredBuffer<int> hTable, out int v[40], float3 pos) {
	int3 bbMin = DiscretizePos(pos - float3(l,l,l)); 
	int3 bbMax = DiscretizePos(pos + float3(l,l,l));

	int auxCounter = 0;

	for (int i = bbMin.x; i < bbMax.x; i++) {
		for (int j = bbMin.y; j < bbMax.y; j++) {
			for (int k = bbMin.z; k < bbMax.z; k++) {
				int hash = Hash(int3(i,j,k));
				
					for (uint index = hash * sizeBuckets; index < hash * sizeBuckets + sizeBuckets; index++) {
						int value = hTable[index];
						if (auxCounter < 40 && value != -1) {
							v[auxCounter] = value;
						}
						auxCounter++;
					}

			}
		}
	}
}
/*
	SETTERS Y GETTERS
*/

uint RemapIndex(uint index) {
	return index / nBuckets;
}

int3 DiscretizePos(float4 pos) {
	return float3(floor(pos.x / l), floor(pos.y / l), floor(pos.z / l));
}

int3 DiscretizePos(float3 pos) {
	return float3(floor(pos.x / l), floor(pos.y / l), floor(pos.z / l));
}

// void InitTHash(RWStructuredBuffer<int> hTable, uint index , uint nBuckets, uint sizeBuckets, float supportRadius) {

// 	uint i = 0;
// 	uint bucketIndex = RemapIndex(index);
// 	while (i < sizeBuckets) {
// 		if (hTable[bucketIndex + i] != -1) {
// 			hTable[bucketIndex + i] = -1;
// 			break;
// 		}
// 		i++;
// 	}
// }