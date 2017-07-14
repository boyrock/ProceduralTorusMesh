#include "ClassicNoise3D.cginc"

float3 noiseVec3(float3 x) {
	float s = cnoise(float3(x));
	float s1 = cnoise(float3(x.y - 19.1, x.z + 33.4, x.x + 47.2));
	float s2 = cnoise(float3(x.z + 74.2, x.x - 124.5, x.y + 99.4));
	float3 c = float3(s, s1, s2);
	return c;
}

float3 curlNoise(float3 p) {
	const float e = 0.01001;// 0.0009765625;

	float3 dx = float3(e, 0.0, 0.0);
	float3 dy = float3(0.0, e, 0.0);
	float3 dz = float3(0.0, 0.0, e);

	float3 p_x0 = noiseVec3(p - dx);
	float3 p_x1 = noiseVec3(p + dx);
	float3 p_y0 = noiseVec3(p - dy);
	float3 p_y1 = noiseVec3(p + dy);
	float3 p_z0 = noiseVec3(p - dz);
	float3 p_z1 = noiseVec3(p + dz);

	float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
	float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
	float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

	const float divisor = 1.0 / (2.0 * e);
	return normalize(float3(x, y, z) * divisor);
}