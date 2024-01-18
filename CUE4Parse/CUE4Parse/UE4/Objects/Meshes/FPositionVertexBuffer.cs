using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.Meshes
{
    [JsonConverter(typeof(FPositionVertexBufferConverter))]
    public class FPositionVertexBuffer
    {
        public readonly FVector[] Verts;
        public readonly int Stride;
        public readonly int NumVertices;

        public FPositionVertexBuffer(FArchive Ar)
        {
            Stride = Ar.Read<int>();
            NumVertices = Ar.Read<int>();
            if (Ar.Game == EGame.GAME_Valorant)
            {
                bool bUseFullPrecisionPositions = Ar.ReadBoolean();
                var bounds = new FBoxSphereBounds(Ar);
                if (!bUseFullPrecisionPositions)
                {
                    var vertsHalf = Ar.ReadBulkArray<FVector3SignedShortScale>();
                    Verts = new FVector[vertsHalf.Length];
                    for (int i = 0; i < vertsHalf.Length; i++)
                        Verts[i] = vertsHalf[i] * bounds.BoxExtent + bounds.Origin;
                    return;
                }
            }
            if (Ar.Game == EGame.GAME_Gollum) Ar.Position += 25;
            Verts = Ar.ReadBulkArray<FVector>();
        }
    }
}
