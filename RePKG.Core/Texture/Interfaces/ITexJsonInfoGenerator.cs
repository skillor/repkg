namespace RePKG.Core.Texture
{
    public interface ITexJsonInfoGenerator
    {
        string GenerateFullInfo(ITex tex);
        string GenerateInfo(ITex tex);
    }
}