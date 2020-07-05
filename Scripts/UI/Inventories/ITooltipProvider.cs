namespace ButtonGame.Inventories
{
    public interface ITooltipProvider
    {
        string GetDisplayName();
        string GetCategoryName();
        TooltipDescriptionField[] GetDescriptionFields();
    }
}
