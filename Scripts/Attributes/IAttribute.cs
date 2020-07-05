namespace ButtonGame.Attributes
{
    public interface IAttribute
    {
        void GainAttribute(float amount);
        float GetPercentage();
        float GetFraction();
        float GetAttributeValue();
        float GetMaxAttributeValue();
    }
}