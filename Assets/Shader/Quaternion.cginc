
float4 LookAt(float3 forward, float3 up)
{
	forward = normalize(forward);

	float3 v = normalize(forward);
	float3 v2 = cross(up, v);
	float3 v3 = cross(v, v2);

	float m00 = v2.x;
	float m01 = v2.y;
	float m02 = v2.z;
	float m10 = v3.x;
	float m11 = v3.y;
	float m12 = v3.z;
	float m20 = v.x;
	float m21 = v.y;
	float m22 = v.z;

	float num8 = (m00 + m11) + m22;
	float4 quaternion = float4(0, 0, 0, 0);
	if (num8 > 0)
	{
		float num = sqrt(num8 + 1.0);
		quaternion.w = num * 0.5;
		num = 0.5 / num;
		quaternion.x = (m12 - m21) * num;
		quaternion.y = (m20 - m02) * num;
		quaternion.z = (m01 - m10) * num;
		return quaternion;
	}
	if ((m00 >= m11) && (m00 >= m22))
	{
		float num7 = sqrt(((1 + m00) - m11) - m22);
		float num4 = 0.5 / num7;
		quaternion.x = 0.5 * num7;
		quaternion.y = (m01 + m10) * num4;
		quaternion.z = (m02 + m20) * num4;
		quaternion.w = (m12 - m21) * num4;
		return quaternion;
	}
	if (m11 > m22)
	{
		float num6 = sqrt(((1 + m11) - m00) - m22);
		float num3 = 0.5 / num6;
		quaternion.x = (m10 + m01) * num3;
		quaternion.y = 0.5 * num6;
		quaternion.z = (m21 + m12) * num3;
		quaternion.w = (m20 - m02) * num3;
		return quaternion;
	}
	float num5 = sqrt(((1 + m22) - m00) - m11);
	float num2 = 0.5 / num5;
	quaternion.x = (m20 + m02) * num2;
	quaternion.y = (m21 + m12) * num2;
	quaternion.z = 0.5 * num5;
	quaternion.w = (m01 - m10) * num2;
	return quaternion;
}

float4 Mul(float4 q1, float4 q2)
{
	return float4(
		q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
		q1.w * q2.w - dot(q1.xyz, q2.xyz)
		);
}

float3 RotateWithQuaternion(float3 v, float4 r)
{
	float4 r_c = r * float4(-1.0, -1.0, -1.0, 1.0);
	return Mul(r, Mul(float4(v, 0.0), r_c)).xyz;
}

float3 RotateAroundPoint(float3 position, float3 axis, float4 angle)
{
	return RotateWithQuaternion((position - axis), angle) + axis;
}

float4 AngleAxis(float3 axis, float degrees) {

	float4 result;
	float angle = degrees * 3.14159 / 180.0;
	angle *= 0.5;

	axis = normalize(axis);
	axis = axis * sin(angle);
	result.x = axis.x;
	result.y = axis.y;
	result.z = axis.z;
	result.w = cos(angle);

	return normalize(result);
}
