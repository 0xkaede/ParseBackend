namespace CUE4Parse_Conversion.UEFormat.Enums;

public enum EUEFormatVersion
{
    BeforeCustomVersionWasAdded = 0,
    SerializeBinormalSign = 1,
    AddMultipleVertexColors = 2,
        
    VersionPlusOne,
    LatestVersion = VersionPlusOne - 1
}