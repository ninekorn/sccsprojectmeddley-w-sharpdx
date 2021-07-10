using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Windows.Forms;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;


using SharpDX.WIC;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using System.Windows.Media;
using System.Collections;

using System.Threading;
using System.Windows.Media.Imaging;

using Matrix = SharpDX.Matrix;

using SharpDX.DirectInput;

namespace SC_Console_APP
{
    public class SC_DirectX
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DMatrixBuffer
        {
            public Matrix world;
            public Matrix view;
            public Matrix proj;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DLightBuffer
        {
            public Vector4 ambientColor;
            public Vector4 diffuseColor;
            public Vector3 lightDirection;
            public float padding; // Added extra padding so structure is a multiple of 16.
        }



        public SC_VR_Chunk.DInstanceType[] instances { get; set; }
        //[StructLayout(LayoutKind.Sequential)]
        /*public struct DInstanceType
        {
            public Vector3 position;
        };*/


        public static DShaderManager shaderManager { get; set; }

        public const int mapWidth = 20;
        public const int mapHeight = 1;
        public const int mapDepth = 20;

        public const int tinyChunkWidth = 20;
        public const int tinyChunkHeight = 20;
        public const int tinyChunkDepth = 20;

        public const int mapObjectInstanceWidth = 2;
        public const int mapObjectInstanceHeight = 1;
        public const int mapObjectInstanceDepth = 2;

        float _planeSize = 0.1f;

        SC_VR_Chunk[] arrayOfChunks;

        public static Device device;

        public static DeviceContext context;
        Buffer contantBuffer;

        int InstanceCount;
        public static SharpDX.DirectInput.Keyboard _Keyboard;// { get; set; }

        Vector3 VRPos;

        public VertexShader VertexShader;
        public PixelShader PixelShader;

        public InputLayout Layout;
        SC_ThreadPool threadPool;
        public static System.Windows.Forms.Control MainControl;

        public SC_DirectX()
        {


            var form = new RenderForm("SharpDX - MiniCube Direct3D11 Sample");

            form.CreateControl();
            form.Activate();
            //form.
            //MainControl = form.ActiveControl;


            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Used for debugging dispose object references
            // Configuration.EnableObjectTracking = true;

            // Disable throws on shader compilation errors
            //Configuration.ThrowOnShaderCompileError = false;

            // Create Device and SwapChain
            SwapChain swapChain;
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            context = device.ImmediateContext;





            var contextPerThread = new DeviceContext[7];

            var deferredContexts = new DeviceContext[7];
            for (int i = 0; i < deferredContexts.Length; i++)
            {
                deferredContexts[i] = new DeviceContext(device);
                contextPerThread[i] = context;
            }



            var commandLists = new CommandList[7];
            CommandList[] frozenCommandLists = null;

            // Check if driver is supporting natively CommandList
            bool supportConcurentResources;
            bool supportCommandList;
            device.CheckThreadingSupport(out supportConcurentResources, out supportCommandList);




            // Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);



            //shaderManager = new DShaderManager();
            //shaderManager.Initialize(device, form.Handle);

            //Console.WriteLine("start");

            arrayOfChunks = new SC_VR_Chunk[mapObjectInstanceWidth * mapObjectInstanceHeight * mapObjectInstanceDepth];



            InstanceCount = mapWidth * mapHeight * mapDepth; //(widther * heigther * depther)
            instances = new SC_VR_Chunk.DInstanceType[mapWidth * mapHeight * mapDepth]; //(widther * heigther * depther)


            int counter = 0;




            Func<int> formatDelegate0 = () =>
            {
                for (int x = 0; x < mapObjectInstanceWidth; x++)
                {
                    for (int y = 0; y < mapObjectInstanceHeight; y++)
                    {
                        for (int z = 0; z < mapObjectInstanceDepth; z++)
                        {
                            Vector3 chunkPos = new Vector3(x, y, z);

                            chunkPos.X = chunkPos.X * (mapWidth * tinyChunkWidth * _planeSize);// * (mapWidth * tinyChunkWidth * _planeSize); //4
                            chunkPos.Y = chunkPos.Y * (mapHeight * tinyChunkHeight * _planeSize);// * (mapHeight * tinyChunkHeight * _planeSize); //1
                            chunkPos.Z = chunkPos.Z * (mapDepth * tinyChunkDepth * _planeSize);// * (mapDepth * tinyChunkDepth * _planeSize); //4

                            //instances[x + mapObjectInstanceWidth * (y + mapObjectInstanceHeight * z)] = new SC_VR_Chunk.DInstanceType[InstanceCount];
                            arrayOfChunks[x + mapObjectInstanceWidth * (y + mapObjectInstanceHeight * z)] = new SC_VR_Chunk(1f, 1f, 1f, new Vector4(0.1f, 0.1f, 0.1f, 1), mapWidth, mapHeight, mapDepth, chunkPos); //, instances[x + mapObjectInstanceWidth * (y + mapObjectInstanceHeight * z)]

                            //arrayOfChunks[x + mapObjectInstanceWidth * (y + mapObjectInstanceHeight * z)].Initialize(device, 1f, 1f, 1f, new Vector4(0.1f, 0.1f, 0.1f, 1), mapWidth, mapHeight, mapDepth, position);                
                            //arrayOfChunks[x + mapObjectInstanceWidth * (y + mapObjectInstanceHeight * z)].shaderOfChunk = shaderOfChunk;
                        }
                    }
                }
                return 1;
            };
            var t1 = new Task<int>(formatDelegate0);
            t1.RunSynchronously();
            t1.Dispose();











            /*for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int z = 0; z < mapDepth; z++)
                    {
                        var xx = x;
                        var yy = y;//  (mapHeight - 1) - y;
                        var zz = z;


                        Vector3 position = new Vector3(x, y, z);

                        position.X *= ((tinyChunkWidth * _planeSize));
                        position.Y *= ((tinyChunkHeight * _planeSize));
                        position.Z *= ((tinyChunkDepth * _planeSize));

                        //position.X = position.X + (_chunkPos.X); //*1.05f
                        //position.Y = position.Y + (_chunkPos.Y);
                        //position.Z = position.Z + (_chunkPos.Z);

                        instances[xx + mapWidth * (yy + mapHeight * zz)] = new SC_VR_Chunk.DInstanceType()
                        {
                            position = position,
                        };
                    }
                }
            }*/



















            var vsFileNameByteArray = SC_Console_APP.Properties.Resources.textureTrig;
            var psFileNameByteArray = SC_Console_APP.Properties.Resources.textureTrig1;
            ShaderBytecode vertexShaderByteCode = ShaderBytecode.Compile(vsFileNameByteArray, "TextureVertexShader", "vs_5_0", ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None);
            ShaderBytecode pixelShaderByteCode = ShaderBytecode.Compile(psFileNameByteArray, "TexturePixelShader", "ps_5_0", ShaderFlags.None, SharpDX.D3DCompiler.EffectFlags.None);

            // Create the vertex shader from the buffer.
            VertexShader = new VertexShader(device, vertexShaderByteCode);
            // Create the pixel shader from the buffer.
            PixelShader = new PixelShader(device, pixelShaderByteCode);



            //new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            //new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
            //new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
            //new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 36, 0),
            //new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0)


            InputElement[] inputElements = new InputElement[]
            {
                    new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                        Slot = 0,
                        AlignedByteOffset = 0,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "COLOR",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "NORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    /* new InputElement()
                    {
                        SemanticName = "TANGENT",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "BINORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },*/
                    new InputElement()
                    {
                        SemanticName = "TEXCOORD",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 1,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 1,
                        AlignedByteOffset = 0,
                        Classification = InputClassification.PerInstanceData,
                        InstanceDataStepRate = 1,
                    },

                    /*new InputElement()
                    {
                        SemanticName = "COLOR",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "NORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "TEXCOORD",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification =InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },


                   new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 1,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 1,
                        AlignedByteOffset = 0,
                        Classification = InputClassification.PerInstanceData,
                        InstanceDataStepRate = 1,
                    },*/
            };

            // Create the vertex input the layout. Kin dof like a Vertex Declaration.
            Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);




            /*var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                              {
                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
                                      new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                                      new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // BACK
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                                      new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
                                      new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

                                      new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
                                      new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
                                      new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

                                      new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                            });*/






            // Create Constant Buffer








            //var contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);


            /*context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.InputLayout = Layout;
            context.VertexShader.Set(VertexShader);
            context.PixelShader.Set(PixelShader);
            context.GeometryShader.Set(null);*/

            //context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>() * 2, 0));
            //context.VertexShader.SetConstantBuffer(0, contantBuffer);


































            /*// Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            var layout = new InputLayout(device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                    });*/

            /*// Instantiate Vertex buiffer from vertex data
            var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
                                      new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                                      new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // BACK
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                                      new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
                                      new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

                                      new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
                                      new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
                                      new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

                                      new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                            });*/






            // Create Constant Buffer
            //var contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);



            /*// Prepare All the stages
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>() * 2, 0));
            //context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);*/

            // Prepare matrices




            Matrix _WorldMatrix = Matrix.Identity;
            var view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix proj = Matrix.Identity;

            // Use clock
            var clock = new Stopwatch();
            clock.Start();

            // Declare texture for rendering
            bool userResized = true;
            Texture2D backBuffer = null;
            RenderTargetView renderView = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;

            // Setup handler on resize form
            form.UserResized += (sender, args) => userResized = true;

            // Setup full screen mode change F5 (Full) F4 (Window)
            form.KeyUp += (sender, args) =>
            {
                if (args.KeyCode == Keys.F5)
                    swapChain.SetFullscreenState(true, null);
                else if (args.KeyCode == Keys.F4)
                    swapChain.SetFullscreenState(false, null);
                else if (args.KeyCode == Keys.Escape)
                    form.Close();
            };


            DMatrixBuffer[] arrayOfMatrixBuff = new DMatrixBuffer[1];

            var contantBuffer = new Buffer(device, Utilities.SizeOf<DMatrixBuffer>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);




            var InstanceBuffer = new Buffer(device, Utilities.SizeOf<SC_VR_Chunk.DInstanceType>() * instances.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            //InstanceBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, instances, Utilities.SizeOf<DInstanceType>() * instances.Length, ResourceUsage.Dynamic, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            //var vertexBuffer = new Buffer(device, Utilities.SizeOf<SC_VR_Chunk.DVertex>() * arrayOfChunks[0].arrayOfDVertex.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);








            Vector4 ambientColor = new Vector4(0.15f, 0.15f, 0.15f, 1.0f);
            Vector4 diffuseColour = new Vector4(1, 1, 1, 1);
            Vector3 lightDirection = new Vector3(1, 0, 0);


            DLightBuffer[] lightBuffer = new DLightBuffer[1];


            // Copy the lighting variables into the constant buffer.
            lightBuffer[0] = new DLightBuffer()
            {
                ambientColor = ambientColor,
                diffuseColor = diffuseColour,
                lightDirection = lightDirection,
                padding = 0
            };


            BufferDescription lightBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<DLightBuffer>(), // Must be divisable by 16 bytes, so this is equated to 32.
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
            var ConstantLightBuffer = new SharpDX.Direct3D11.Buffer(device, lightBufferDesc);














            SC_VR_Chunk_Shader shaderOfChunk = new SC_VR_Chunk_Shader(device, contantBuffer, Layout, VertexShader, PixelShader, InstanceBuffer, ConstantLightBuffer); //InstanceBuffer
            //Console.WriteLine("start");


            Matrix _worldMatrix = _WorldMatrix;
            Matrix _viewMatrix;
            Matrix _projectionMatrix;

            SharpDX.Direct3D11.Buffer[] vertBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];
            SharpDX.Direct3D11.Buffer[] colorBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];
            SharpDX.Direct3D11.Buffer[] indexBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];
            SharpDX.Direct3D11.Buffer[] instanceBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];

            SharpDX.Direct3D11.Buffer[] normalBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];
            SharpDX.Direct3D11.Buffer[] texBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];

            SharpDX.Direct3D11.Buffer[] dVertBuffers = new SharpDX.Direct3D11.Buffer[mapWidth * mapHeight * mapDepth];

            


            var directInput = new DirectInput();

            _Keyboard = new SharpDX.DirectInput.Keyboard(directInput);

            _Keyboard.Properties.BufferSize = 128;
            _Keyboard.Acquire();


            float ratio = (float)form.ClientSize.Width / (float)form.ClientSize.Height;
            proj = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 1000);
            view = Matrix.LookAtLH(new Vector3(0, 3, -10), new Vector3(), Vector3.UnitY);

            int startOnce = 1;
            threadPool = new SC_ThreadPool();
            threadPool.startPool(1);



            //Vector3 from = new Vector3(0, 70, -150);
            //Vector3 to = new Vector3(0, 50, 0);
            //Vector3 lightDirection = new Vector3(0.5f, 0, -1);
            //lightDirection.Normalize();





            RenderLoop.Run(form, () =>
            {
                if (startOnce == 1)
                {

                    startOnce = 0;
                }


                // Clear views
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(renderView, SharpDX.Color.CornflowerBlue);

                // If Form resized
                if (userResized)
                {
                    // Dispose all previous allocated resources
                    Utilities.Dispose(ref backBuffer);
                    Utilities.Dispose(ref renderView);
                    Utilities.Dispose(ref depthBuffer);
                    Utilities.Dispose(ref depthView);

                    // Resize the backbuffer
                    swapChain.ResizeBuffers(desc.BufferCount, form.ClientSize.Width, form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

                    // Get the backbuffer from the swapchain
                    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

                    // Renderview on the backbuffer
                    renderView = new RenderTargetView(device, backBuffer);

                    // Create the depth buffer
                    depthBuffer = new Texture2D(device, new Texture2DDescription()
                    {
                        Format = Format.D32_Float_S8X24_UInt,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = form.ClientSize.Width,
                        Height = form.ClientSize.Height,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    // Create the depth buffer view
                    depthView = new DepthStencilView(device, depthBuffer);

                    // Setup targets and viewport for rendering
                    context.Rasterizer.SetViewport(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));
                    context.OutputMerger.SetTargets(depthView, renderView);

                    // Setup new projection matrix with correct aspect ratio
                    proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, form.ClientSize.Width / (float)form.ClientSize.Height, 0.1f, 100.0f);
                    // We are done resizing
                    userResized = false;
                }


                _worldMatrix.M41 += VRPos.X;
                _worldMatrix.M42 += VRPos.Y;
                _worldMatrix.M43 += VRPos.Z;


                //var time = clock.ElapsedMilliseconds / 1000.0f;
                //var viewProj = Matrix.Multiply(view, proj);


                // Update WorldViewProj Matrix
                //var worldViewProj = Matrix.RotationX(time) * Matrix.RotationY(time * 2) * Matrix.RotationZ(time * .7f) * viewProj;
                //worldViewProj.Transpose();
                //context.UpdateSubresource(ref worldViewProj, contantBuffer);

                //_worldMatrix = _worldMatrix;
                _viewMatrix = view;
                _projectionMatrix = proj;

                _worldMatrix.Transpose();
                _viewMatrix.Transpose();
                _projectionMatrix.Transpose();

                float bias = 0.005f;

                arrayOfMatrixBuff[0] = new DMatrixBuffer()
                {
                    world = _worldMatrix,
                    view = _viewMatrix,
                    proj = _projectionMatrix,
                };






                _worldMatrix = _WorldMatrix;

                timeWatch.Stop();
                timeWatch.Reset();
                timeWatch.Start();

                /*for (int c = 0; c < arrayOfChunks.Length; c++)
                {
                    chunkData chunkDat = new chunkData();
                    //ShaderManager.test();
                    chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                    chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                    chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;
                    chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;
                    //chunkDat.dVertexData = arrayOfChunks[c].arrayOfDVertexData;
                    chunkDat.Device = device;
                    chunkDat.worldMatrix = _worldMatrix;
                    chunkDat.viewMatrix = view;
                    chunkDat.projectionMatrix = proj;
                    chunkDat.chunkShader = shaderOfChunk;
                    chunkDat.matrixBuffer = arrayOfMatrixBuff;
                    chunkDat.vertBuffers = vertBuffers;
                    chunkDat.colorBuffers = colorBuffers;
                    chunkDat.indexBuffers = indexBuffers;

                    shaderOfChunk.Renderer(chunkDat);
                    //threadPool.AddToQueue(chunkDat);

                }*/

                /*Func<bool> formatDelegate = () =>
                {
                    for (int c = 0; c < arrayOfChunks.Length; c++)
                    {

                        chunkData chunkDat = new chunkData();
                        chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                        chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                        chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;
                        chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;
                        chunkDat.Device = device;
                        chunkDat.worldMatrix = _worldMatrix;
                        chunkDat.viewMatrix = view;
                        chunkDat.projectionMatrix = proj;
                        chunkDat.chunkShader = shaderOfChunk;
                        chunkDat.matrixBuffer = arrayOfMatrixBuff;
                        chunkDat.vertBuffers = vertBuffers;
                        chunkDat.colorBuffers = colorBuffers;
                        chunkDat.indexBuffers = indexBuffers;

                        shaderOfChunk.Renderer(chunkDat);
                    }
                    return true;
                };

                var t2 = new Task<bool>(formatDelegate);
                t2.RunSynchronously();
                t2.Dispose();*/



                for (int c = 0; c < arrayOfChunks.Length; c++)
                {
                    Func<bool> formatDelegate = () =>
                    {
                        chunkData chunkDat = new chunkData();
                        chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                        chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                        chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;//arrayOfChunks[c].instances;
                        chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;

                        chunkDat.arrayOfInstanceNormals = arrayOfChunks[c].arrayOfInstanceNormals;
                        chunkDat.arrayOfInstanceTextureCoordinates = arrayOfChunks[c].arrayOfInstanceTexturesCoordinates;
                        //arrayOfInstanceColors

                        chunkDat.dVertexData = arrayOfChunks[c].arrayOfDVertex;

                        chunkDat.Device = device;
                        chunkDat.worldMatrix = _worldMatrix;
                        chunkDat.viewMatrix = view;
                        chunkDat.projectionMatrix = proj;
                        chunkDat.chunkShader = shaderOfChunk;
                        chunkDat.matrixBuffer = arrayOfMatrixBuff;
                        chunkDat.vertBuffers = vertBuffers;
                        chunkDat.colorBuffers = colorBuffers;
                        chunkDat.indexBuffers = indexBuffers;     
                        chunkDat.instanceBuffers = instanceBuffers;
                        chunkDat.dVertBuffers = dVertBuffers;
                        chunkDat.texBuffers = texBuffers;
                        chunkDat.normalBuffers = normalBuffers;
                        chunkDat.lightBuffer = lightBuffer;


                        shaderOfChunk.Renderer(chunkDat);
                        return true;
                    };
                    var t2 = new Task<bool>(formatDelegate);
                    t2.RunSynchronously();
                    t2.Dispose();
                }



















                /*if (queueOfFunctions.Count > 0)
                {
                    try
                    {
                        Func<bool> someTask;
                        queueOfFunctions.TryPop(out someTask);
                        var t3 = new Task<bool>(someTask);
                        t3.RunSynchronously();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    //someTask.RunSynchronously();
                }*/

                /*int contextCount = 0;
                int c = 0;
                for (; c < arrayOfChunks.Length; c++)
                {
                    if (contextCount >= 7)
                    {
                        contextCount = 0;
                    }
                    chunkData chunkDat = new chunkData();
                    //ShaderManager.test();
                    chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                    chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                    chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;
                    chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;
                    //chunkDat.dVertexData = arrayOfChunks[c].arrayOfDVertexData;
                    chunkDat.Device = device;
                    chunkDat.worldMatrix = _WorldMatrix;
                    chunkDat.viewMatrix = view;
                    chunkDat.projectionMatrix = proj;
                    chunkDat.chunkShader = shaderOfChunk;
                    chunkDat.matrixBuffer = arrayOfMatrixBuff;
                    chunkDat.vertBuffers = vertBuffers;
                    chunkDat.colorBuffers = colorBuffers;
                    chunkDat.indexBuffers = indexBuffers;
                    chunkDat._renderingContext = contextPerThread[contextCount];

                    //shaderOfChunk.Renderer(chunkDat);
                    //var tasker =
                    //tasker.RunSynchronously();

                    Func<int> formatDelegate = () =>
                    {
                        var chunkShader = chunkDat.chunkShader;
                        chunkShader.Renderer(chunkDat);
                        return 1;
                    };

                    var t2 = new Task<int>(formatDelegate);
                    t2.RunSynchronously();
                    t2.Dispose();


                    /*Action<int> RenderDeferred = (int threadCount) =>
                    {
                        var renderingContext = contextPerThread[0];
                        var tasks = new Task[1];
                        for (int i = 0; i < threadCount; i++)
                        {
                            tasks[i] = new Task(() => shaderOfChunk.Render(chunkDat));
                            tasks[i].Start();
                        }
                        Task.WaitAll(tasks);
                    };

                    RenderDeferred(1);*/

                //chunkDat.chunkShader.Render(chunkDat);
                //threadPool.AddToQueue(chunkDat);

                //queueOfFunctions.Enqueue(chunkDat);
                /*Func<int> formatDelegate = () =>
                {
                    var chunkShader = chunkDat.chunkShader;
                    chunkShader.Render(chunkDat);
                    return 1;
                };

                //var d = new SafeCallDelegate(formatDelegate);
                //callFunctionSafe(formatDelegate,form);

                //form.Invoke(d);
                //queueOfFunctions.Add(chunkDat);
                //contextCount++;
            }*/





                /*if(arrayOfChunks.Length > 0)
                {
                    int lengther = (int)(arrayOfChunks.Length / 4);

                    var switchForArray = 1;
                    var switchForTasks = 0;
                    var index0 = 0;
                    int contextCount = 0;

                    arrayOfSmallerChunks0 = new SC_VR_Chunk[lengther];
                    int ch = 0;

                    //_loopStuff:

                    for (ch = 0; ch < arrayOfChunks.Length; ch++)
                    {
                        if (switchForArray == 1)
                        {
                            index0 = 0;
                            arrayOfSmallerChunks0 = new SC_VR_Chunk[lengther];
                            switchForArray = 0;
                        }

                        if (index0 <= lengther - 1)
                        {
                            arrayOfSmallerChunks0[index0] = arrayOfChunks[ch];
                        }
                        else
                        {
                            switchForTasks = 1;
                        }

                        if (switchForTasks == 1)
                        {
                            Func<int> formatDelegate0 = () =>
                            {
                                for (int c = 0; c < arrayOfSmallerChunks0.Length; c++)
                                {
                                   //if (contextCount >= 7)
                                    //{
                                    //    contextCount = 0;
                                    //}
                                    chunkData chunkDat = new chunkData();
                                    //ShaderManager.test();
                                    chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                                    chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                                    chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;
                                    chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;
                                    //chunkDat.dVertexData = arrayOfChunks[c].arrayOfDVertexData;
                                    chunkDat.Device = device;
                                    chunkDat.worldMatrix = _WorldMatrix;
                                    chunkDat.viewMatrix = view;
                                    chunkDat.projectionMatrix = proj;
                                    chunkDat.chunkShader = shaderOfChunk;
                                    chunkDat.matrixBuffer = arrayOfMatrixBuff;
                                    chunkDat.vertBuffers = vertBuffers;
                                    chunkDat.colorBuffers = colorBuffers;
                                    chunkDat.indexBuffers = indexBuffers;
                                    //chunkDat._renderingContext = contextPerThread[contextCount];
                                    var chunkShader = chunkDat.chunkShader;
                                    chunkShader.Renderer(chunkDat);
                                    //contextCount++;
                                }
                                //ch++;
                                return 1;
                            };
                            var t0 = new Task<int>(formatDelegate0);
                            t0.RunSynchronously();

                            switchForArray = 1;
                            switchForTasks = 0;
                        }

                        index0++;
                    }
                    //Console.WriteLine(ch);
                    /*if (ch >= arrayOfChunks.Length-1 )
                    {

                        goto _endLoop;
                    }
                    goto _loopStuff;

                    _endLoop:
                }
                else*/
                /*{
                    Func<bool> formatDelegate = () =>
                    {
                        for (int c = 0; c < arrayOfChunks.Length; c++)
                        {

                            chunkData chunkDat = new chunkData();
                            chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                            chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                            chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;
                            chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;
                            chunkDat.Device = device;
                            chunkDat.worldMatrix = _worldMatrix;
                            chunkDat.viewMatrix = view;
                            chunkDat.projectionMatrix = proj;
                            chunkDat.chunkShader = shaderOfChunk;
                            chunkDat.matrixBuffer = arrayOfMatrixBuff;
                            chunkDat.vertBuffers = vertBuffers;
                            chunkDat.colorBuffers = colorBuffers;
                            chunkDat.indexBuffers = indexBuffers;

                            shaderOfChunk.Renderer(chunkDat);
                        }
                        return true;
                    };

                    var t2 = new Task<bool>(formatDelegate);
                    t2.RunSynchronously();
                    t2.Dispose();
                }*/


























                /*if (queueOfFunctions.Count > 0)
                {
                    //Console.WriteLine("test");
                    var test = queueOfFunctions.Dequeue();

                    /*Action<int> RenderDeferred = (int threadCount) =>
                    {
                        var renderingContext = contextPerThread[0];
                        var tasks = new Task[1];
                        for (int i = 0; i < threadCount; i++)
                        {
                            tasks[i] = new Task(() => test());
                            tasks[i].Start();
                        }
                        Task.WaitAll(tasks);
                    };

                    RenderDeferred(1);
                }*/













                /*for (int i = 0; i < 7; i++)
                {
                    var commandList = commandLists[i];
                    context.ExecuteCommandList(commandList, false);

                    //Console.WriteLine(commandList);
                    // Execute the deferred command list on the immediate context
                    //context.ExecuteCommandList(commandList, false);

                    // For classic deferred we release the command list. Not for frozen
                    /*if (currentState.Type == TestType.Deferred)
                    {
                        // Release the command list
                        commandList.Dispose();
                        commandLists[i] = null;
                    }*
                }*/



























                /*if (queueOfFunctions.Count > 0)
                {
                    try
                    {
                        //Console.WriteLine("test");
                        //var formatDelegate = queueOfFunctions[0];
                        chunkData someTask = queueOfFunctions.Dequeue();

                        Func<int> formatDelegate = () =>
                        {
                            var chunkShader = someTask.chunkShader;
                            chunkShader.Render(someTask);
                            return 1;
                        };

                        form.Invoke(formatDelegate);

                        /*chunkData someTask = queueOfFunctions[0];

                        Func<int> formatDelegate = () =>
                        {
                            var chunkShader = someTask.chunkShader;
                            chunkShader.Render(someTask);
                            return 1;
                        };

                        var t2 = new Task<int>(formatDelegate);
                        t2.RunSynchronously();
                        t2.Dispose();
                        queueOfFunctions.Remove(queueOfFunctions[0]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    //someTask.RunSynchronously();
                }*/





























                /*if (queueOfFunctions.Count > 0)
                {
                    try
                    {
                        //chunkData someTask;

                       Func<int> someTask = queueOfFunctions.Dequeue(); //out someTask

                        /*Func<bool> formatDelegate = () =>
                        {
                            var chunkShader = someTask.chunkShader;
                            chunkShader.Render(someTask);
                            return true;
                        };

                        var t2 = new Task<int>(someTask);
                        t2.RunSynchronously();
                        t2.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    //someTask.RunSynchronously();
                }*/


                /*Func<bool> formatDelegate = () =>
                {
                    for (int c = 0; c < arrayOfChunks.Length; c++)
                    {
                        chunkData chunkDat = new chunkData();
                        //ShaderManager.test();
                        chunkDat.instanceBuffer = arrayOfChunks[c].InstanceBuffer;
                        chunkDat.arrayOfInstanceVertex = arrayOfChunks[c].arrayOfInstanceVertex;
                        chunkDat.arrayOfInstancePos = arrayOfChunks[c].instances;
                        chunkDat.arrayOfInstanceIndices = arrayOfChunks[c].arrayOfInstanceIndices;
                        //chunkDat.dVertexData = arrayOfChunks[c].arrayOfDVertexData;
                        chunkDat.Device = device;
                        chunkDat.worldMatrix = _worldMatrix;
                        chunkDat.viewMatrix = view;
                        chunkDat.projectionMatrix = proj;
                        chunkDat.chunkShader = shaderOfChunk;
                        chunkDat.matrixBuffer = arrayOfMatrixBuff;
                        chunkDat.vertBuffers = vertBuffers;
                        chunkDat.colorBuffers = colorBuffers;
                        chunkDat.indexBuffers = indexBuffers;

                        shaderOfChunk.Renderer(chunkDat);
                    }
                    return true;
                };

                var t2 = new Task<bool>(formatDelegate);
                t2.RunSynchronously();
                t2.Dispose();*/












                //Console.WriteLine(timeWatch.Elapsed.Milliseconds);


                float speed = 0.1f;

                ReadKeyboard();

                if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Up))
                {
                    VRPos.Z -= speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Down))
                {
                    VRPos.Z += speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Q))
                {
                    VRPos.Y -= speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Z))
                {
                    VRPos.Y += speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Left))
                {
                    VRPos.X += speed;
                }
                else if (_KeyboardState != null && _KeyboardState.PressedKeys.Contains(Key.Right))
                {
                    VRPos.X -= speed;
                }








                // Present!
                swapChain.Present(0, PresentFlags.None);
            });

            // Release all resources
            //signature.Dispose();
            vertexShaderByteCode.Dispose();
            VertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            PixelShader.Dispose();
            //vertices.Dispose();
            Layout.Dispose();
            contantBuffer.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
            context.ClearState();
            context.Flush();
            device.Dispose();
            context.Dispose();
            swapChain.Dispose();
            factory.Dispose();
        }

        SC_VR_Chunk[] arrayOfSmallerChunks0;
        private void callFunctionSafe(Func<int> text, RenderForm form)
        {
            var test = new SafeCallDelegate(callFunctionSafe);

            var result = form.BeginInvoke(text);
            form.EndInvoke(result);

        }
        private delegate void SafeCallDelegate(Func<int> someFunction, RenderForm form);

        //public static List<chunkData> queueOfFunctions = new List<chunkData>();
        //public static List<Func<int>> queueOfFunctions = new List<Func<int>>();

        //public static Queue<chunkData> queueOfFunctions = new Queue<chunkData>();

        public static Queue<Func<int>> queueOfFunctions = new Queue<Func<int>>();

        Stopwatch timeWatch = new Stopwatch();



        public struct chunkData
        {
            public SharpDX.Direct3D11.Buffer instanceBuffer;
            public Vector4[][] arrayOfInstanceVertex;
            public SC_VR_Chunk.DInstanceType[] arrayOfInstancePos;
            public int[][] arrayOfInstanceIndices;
            public Vector3[][] arrayOfInstanceNormals;
            public Vector2[][] arrayOfInstanceTextureCoordinates;
            public Vector4[][] arrayOfInstanceColors;
            public SC_VR_Chunk.DVertex[][] dVertexData;

            public SharpDX.Direct3D11.Device Device;
            public Matrix worldMatrix;
            public Matrix viewMatrix;
            public Matrix projectionMatrix;
            //public DShaderManager shaderManager;
            public SC_VR_Chunk_Shader chunkShader;
            public DMatrixBuffer[] matrixBuffer;
            public DLightBuffer[] lightBuffer;
            public SharpDX.Direct3D11.Buffer[] vertBuffers;
            public SharpDX.Direct3D11.Buffer[] colorBuffers;
            public SharpDX.Direct3D11.Buffer[] indexBuffers;
            public SharpDX.Direct3D11.Buffer[] normalBuffers;
            public SharpDX.Direct3D11.Buffer[] texBuffers;
            public SharpDX.Direct3D11.Buffer[] dVertBuffers;


            public DeviceContext _renderingContext;
            public SharpDX.Direct3D11.Buffer[] instanceBuffers; 

        }

        KeyboardState _KeyboardState;
        private bool ReadKeyboard()
        {
            var resultCode = SharpDX.DirectInput.ResultCode.Ok;
            _KeyboardState = new KeyboardState();

            try
            {
                // Read the keyboard device.
                _Keyboard.GetCurrentState(ref _KeyboardState);
            }
            catch (SharpDX.SharpDXException ex)
            {
                resultCode = ex.Descriptor; // ex.ResultCode;
            }
            catch (Exception)
            {
                return false;
            }

            // If the mouse lost focus or was not acquired then try to get control back.
            if (resultCode == SharpDX.DirectInput.ResultCode.InputLost || resultCode == SharpDX.DirectInput.ResultCode.NotAcquired)
            {
                try
                {
                    _Keyboard.Acquire();
                }
                catch
                { }

                return true;
            }

            if (resultCode == SharpDX.DirectInput.ResultCode.Ok)
                return true;

            return false;
        }

    }
}
