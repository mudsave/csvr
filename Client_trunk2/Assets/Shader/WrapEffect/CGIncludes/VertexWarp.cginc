#ifndef VS_VERTEXWARP
#define VS_VERTEXWARP

half3x3 MakeRotation( half fAngle, half3 vAxis ) {
    half fS;
    half fC;
    sincos( fAngle, fS, fC );
    half fXX       = vAxis.x * vAxis.x;
    half fYY       = vAxis.y * vAxis.y;
    half fZZ       = vAxis.z * vAxis.z;
    half fXY       = vAxis.x * vAxis.y;
    half fYZ       = vAxis.y * vAxis.z;
    half fZX       = vAxis.z * vAxis.x;
    half fXS       = vAxis.x * fS;
    half fYS       = vAxis.y * fS;
    half fZS       = vAxis.z * fS;
	half fOneC      = 1.0f - fC;
    
    half3x3 result = half3x3(       fOneC * fXX +  fC, fOneC * fXY + fZS, fOneC * fZX - fYS,
                                    fOneC * fXY - fZS, fOneC * fYY +  fC, fOneC * fYZ + fXS,
                                    fOneC * fZX + fYS, fOneC * fYZ - fXS, fOneC * fZZ +  fC
    );
    return result; 
}

struct VertexWarp
{
	half4x4 vTransform;
	half4x4 vInvTransform;
	half2   vStrengthRadius;
	float Size;
};

float3 p_vVertexWarpModelPosition;

//--------------------------------------------------------------------------------------------------
float4 ApplyWarp( float4 vVertexWS, VertexWarp warp ) {
    // Put in warp space
    float3 vVertex = mul( warp.vInvTransform, vVertexWS ) * warp.Size;;
    float3 vModelPos = mul( warp.vInvTransform, float4( p_vVertexWarpModelPosition, 1.0f ) );
    vModelPos.z = 0;

    float fModelDistance = saturate( length( vModelPos ) / warp.vStrengthRadius.y );
    float fOriginalLength = length( vVertex.xy );

    // Gradient from 0 to 1.
    float fGradient = saturate( fOriginalLength / warp.vStrengthRadius.y );
    float fSquaredGradient = 1.0f - fGradient * fGradient;    
    float fScalar = warp.vStrengthRadius.x;

    // Put R from -5 to 1
    float fR = ( 1.0f - fGradient ) * max( 0.0f, pow( 1.5f, fSquaredGradient * 6.0f - 5.0f ) ) * fScalar / 3.0f;
    
    // Apply swirl.
	half3x3 rotation = MakeRotation( fR * fR * 3.1616f * 2.0f, half3( 0.0f, 0.0f, 1.0f ) );
    vVertex = mul( rotation, vVertex );

    // Squash to a disc.
    vVertex.z = vVertex.z * pow( fGradient, 6.0f );

    // Compress towards center.
    vVertex = vVertex * fModelDistance;

    // Back in world space
    vVertexWS.xyz = mul( warp.vTransform, half4( vVertex, 1.0f ) );

    return vVertexWS;
}

#endif  // VS_VERTEXWARP