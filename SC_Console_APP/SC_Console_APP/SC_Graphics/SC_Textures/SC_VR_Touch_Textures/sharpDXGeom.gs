struct VS_INPUT
{
    float4 Pos : POSITION;
	//float4 Col : COLOR;
    float2 tex: TEXCOORD0;
};

struct GS_INPUT
{
    float4 Pos : SV_POSITION;
	//float4 Col : COLOR;
    float2 tex: TEXCOORD0;
};

struct PS_INPUT
{
    float4 Pos : SV_POSITION;
    //float2 tex:TEXCOORD;
	//float4 Col : COLOR;
    float2 tex: TEXCOORD0;
};

cbuffer data :register(b0)
{
	//float4x4 world;
	//float4x4 viewProj;

	float4x4 worldMatrix;
	float4x4 viewMatrix;
	float4x4 projectionMatrix;
}

Texture2D diffuseMap;
SamplerState textureSampler;

GS_INPUT VS( VS_INPUT input )
{
    
    GS_INPUT output = (GS_INPUT)0;
    output.Pos = input.Pos;        
    return output;
}

[maxvertexcount(72)]
void GS( point GS_INPUT input[1], inout TriangleStream<PS_INPUT> TriStream )
{
	
	float size=5.0f;	
	
	//create a cube from every vertex
	//crea un cubo da ogni vertice
	PS_INPUT	vertices[36];
	//1
	vertices[0].Pos=float4(-size, -size, -size,1);vertices[0].tex=float2(0,1);
	vertices[1].Pos=float4(-size, size, -size,1);vertices[1].tex=float2(0,0);
	vertices[2].Pos=float4(size, size, -size,1);vertices[2].tex=float2(1,0);
	vertices[3].Pos=float4(size, size, -size,1);vertices[3].tex=float2(1,0);
	vertices[4].Pos=float4(size, -size, -size,1);vertices[4].tex=float2(1,1);
	vertices[5].Pos=float4(-size, -size, -size,1);vertices[5].tex=float2(0,1);
	//'2
	vertices[6].Pos=float4(-size, -size, size,1);vertices[6].tex=float2(1,1);
	vertices[7].Pos=float4(size, -size, size,1);vertices[7].tex=float2(0,1);
	vertices[8].Pos=float4(size, size, size,1);vertices[8].tex=float2(0,0);
	vertices[9].Pos=float4(size, size, size,1);vertices[9].tex=float2(0,0);
	vertices[10].Pos=float4(-size, size, size,1);vertices[10].tex=float2(1,0);
	vertices[11].Pos=float4(-size, -size, size,1);vertices[11].tex=float2(1,1);
	//'3
	vertices[12].Pos=float4(-size, -size, -size,1);vertices[12].tex=float2(1,1);
	vertices[13].Pos=float4(size, -size, -size,1);vertices[13].tex=float2(0,1);
	vertices[14].Pos=float4(size, -size, size,1);vertices[14].tex=float2(0,0);
	vertices[15].Pos=float4(size, -size, size,1);vertices[15].tex=float2(0,0);
	vertices[16].Pos=float4(-size, -size, size,1);vertices[16].tex=float2(1,0);
	vertices[17].Pos=float4(-size, -size, -size,1);vertices[17].tex=float2(1,1);
	//'4
	vertices[18].Pos=float4(size, -size, -size,1);vertices[18].tex=float2(0,1);
	vertices[19].Pos=float4(size, size, -size,1);vertices[19].tex=float2(0,0);
	vertices[20].Pos=float4(size, size, size,1);vertices[20].tex=float2(1,0);
	vertices[21].Pos=float4(size, size, size,1);vertices[21].tex=float2(1,0);
	vertices[22].Pos=float4(size, -size, size,1);vertices[22].tex=float2(1,1);
	vertices[23].Pos=float4(size, -size, -size,1);vertices[23].tex=float2(0,1);
	//5
	vertices[24].Pos=float4(size, size, -size,1);vertices[24].tex=float2(0,1);
	vertices[25].Pos=float4(-size, size, -size,1);vertices[25].tex=float2(0,0);
	vertices[26].Pos=float4(-size, size, size,1);vertices[26].tex=float2(1,0);
	vertices[27].Pos=float4(-size, size, size,1);vertices[27].tex=float2(1,0);
	vertices[28].Pos=float4(size, size, size,1);vertices[28].tex=float2(1,1);
	vertices[29].Pos=float4(size, size, -size,1);vertices[29].tex=float2(0,1);
	//6
	vertices[30].Pos=float4(-size, size, -size,1);vertices[30].tex=float2(1,0);
	vertices[31].Pos=float4(-size, -size, -size,1);vertices[31].tex=float2(1,1);
	vertices[32].Pos=float4(-size, -size, size,1);vertices[32].tex=float2(0,1);
	vertices[33].Pos=float4(-size, -size, size,1);vertices[33].tex=float2(0,1);
	vertices[34].Pos=float4(-size, size, size,1);vertices[34].tex=float2(0,0);
    vertices[35].Pos=float4(-size, size, -size,1);vertices[35].tex=float2(1,0);
	
	for(int i=0;i<12;i++)
	{
		float4 P=float4(input[0].Pos.xyz,1);

		//vertices[i*3].Pos=mul(world, vertices[i*3].Pos ) ;
		//vertices[i*3+1].Pos=mul(world , vertices[i*3+1].Pos );
		//vertices[i*3+2].Pos=mul(world, vertices[i*3+2].Pos );
		
		//vertices[i*3].Pos=mul(viewProj , vertices[i*3].Pos + P);
		//vertices[i*3+1].Pos=mul(viewProj , vertices[i*3+1].Pos + P);
		//vertices[i*3+2].Pos=mul(viewProj, vertices[i*3+2].Pos + P);
		
		vertices[i*3].Pos=mul(worldMatrix, vertices[i*3].Pos ) ;
		vertices[i*3+1].Pos=mul(worldMatrix , vertices[i*3+1].Pos );
		vertices[i*3+2].Pos=mul(worldMatrix, vertices[i*3+2].Pos );
		
		vertices[i*3].Pos=mul(viewMatrix, vertices[i*3].Pos + P);
		vertices[i*3+1].Pos=mul(viewMatrix , vertices[i*3+1].Pos + P);
		vertices[i*3+2].Pos=mul(viewMatrix, vertices[i*3+2].Pos + P);

		vertices[i*3].Pos=mul(projectionMatrix, vertices[i*3].Pos + P);
		vertices[i*3+1].Pos=mul(projectionMatrix , vertices[i*3+1].Pos + P);
		vertices[i*3+2].Pos=mul(projectionMatrix, vertices[i*3+2].Pos + P);





		//vertices[i*3+0].Col = float4 (0.5,0.5,0.5,1);
		//vertices[i*3+1].Col = float4 (0.5,0.5,0.5,1);
		//vertices[i*3+2].Col = float4 (0.5,0.5,0.5,1);


		vertices[i*3+0].tex = input[0].tex; 
		vertices[i*3+1].tex = input[0].tex; 
		vertices[i*3+2].tex = input[0].tex; 


		TriStream.Append(vertices[i*3]);
		TriStream.Append(vertices[i*3+1]);
		TriStream.Append(vertices[i*3+2]);
		
		TriStream.RestartStrip();		
	}
	









	
}

float4 PS( PS_INPUT input) : SV_Target
{   
    return diffuseMap.Sample( textureSampler, input.tex );
}


