using CUE4Parse.UE4.Assets.Exports.Component.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;

namespace CUE4Parse.UE4.Assets.Exports.Component.SkeletalMesh;

public class USkeletalMeshComponent : UObject
{
    public override void Deserialize(FAssetArchive Ar, long validPos)
    {
        base.Deserialize(Ar, validPos);
        
    }

    public FPackageIndex GetSkeletalMesh()
    {
        var skeletalMesh = GetSkeletalMesh("SkeletalMesh"); // deprecated in 5.1 so fallback below
        if (skeletalMesh.IsNull) skeletalMesh = GetSkeletalMesh("SkinnedAsset");
        
        return skeletalMesh;
    }
    
    public FPackageIndex GetSkeletalMesh(string parameterName)
    {
        var mesh = new FPackageIndex();
        var current = this;
        while (true)
        {
            mesh = current.GetOrDefault(parameterName, new FPackageIndex());
            if (!mesh.IsNull || current.Template == null)
                break;
            current = current.Template.Load<USkeletalMeshComponent>();
        }

        return mesh;
    }
}